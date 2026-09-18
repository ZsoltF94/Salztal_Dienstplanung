using System.Diagnostics;
using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Mapping;
using Salztal.Dienstplanung.Planning.Optimization;

namespace Salztal.Dienstplanung.Planning;

internal sealed class AutomaticSchedulePlanningEngine : IAutomaticSchedulePlanningEngine
{
    private const string OptimizationStages =
        "joint:regular_coverage>relief_coverage>high>relief_count>split_count>"
        + "auxiliary_minimum>relative_weekly_target>medium>low_empty>stability>technical_tie";
    private static readonly SemaphoreSlim ActiveGeneration = new(1, 1);
    private readonly Func<PlanningInputSnapshot, PlanningCandidateSet> candidateBuilder;
    private readonly IAutomaticScheduleOptimizationRunner optimizationRunner;
    private readonly IAutomaticScheduleProposalMapper proposalMapper;

    public AutomaticSchedulePlanningEngine()
        : this(
            PlanningCandidateBuilder.Build,
            new AutomaticScheduleOptimizationRunner(),
            new AutomaticScheduleProposalMapper(
                new DeterministicAutomaticScheduleAssignmentIdFactory()))
    {
    }

    internal AutomaticSchedulePlanningEngine(
        Func<PlanningInputSnapshot, PlanningCandidateSet> candidateBuilder,
        IAutomaticScheduleOptimizationRunner optimizationRunner,
        IAutomaticScheduleProposalMapper proposalMapper)
    {
        ArgumentNullException.ThrowIfNull(candidateBuilder);
        ArgumentNullException.ThrowIfNull(optimizationRunner);
        ArgumentNullException.ThrowIfNull(proposalMapper);
        this.candidateBuilder = candidateBuilder;
        this.optimizationRunner = optimizationRunner;
        this.proposalMapper = proposalMapper;
    }

    internal static TimeSpan DefaultTimeLimit =>
        AutomaticSchedulePlanningRequest.ProductiveTimeLimit;

    public async Task<AutomaticSchedulePlanningResult> PlanAsync(
        AutomaticSchedulePlanningRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.Cancelled);
        }

        if (!await ActiveGeneration.WaitAsync(0, CancellationToken.None))
        {
            return Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                AutomaticScheduleErrorCode.ConcurrentRun);
        }

        try
        {
            return await Task.Run(
                () => PlanCore(request, cancellationToken),
                CancellationToken.None);
        }
        finally
        {
            ActiveGeneration.Release();
        }
    }

    private AutomaticSchedulePlanningResult PlanCore(
        AutomaticSchedulePlanningRequest request,
        CancellationToken cancellationToken)
    {
        Stopwatch totalStopwatch = Stopwatch.StartNew();
        PlanningEngineStage stage = PlanningEngineStage.ModelBuilding;
        List<AutomaticSchedulePhaseSnapshot> phases = [];
        Stopwatch stageStopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            PlanningCandidateSet candidates = candidateBuilder(request.Snapshot);
            TimeSpan modelBuildDuration = stageStopwatch.Elapsed;
            phases.Add(new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.ModelBuilding,
                AutomaticSchedulePhaseStatus.Completed,
                modelBuildDuration,
                [
                    new AutomaticSchedulePhaseValue(
                        "candidate_count",
                        candidates.Candidates.Count),
                    new AutomaticSchedulePhaseValue(
                        "remaining_demand_count",
                        candidates.RemainingDemands.Count),
                ]));

            stage = PlanningEngineStage.Solving;
            AutomaticScheduleOptimizationRun run = optimizationRunner.Optimize(
                request.Snapshot,
                candidates,
                request.TimeLimit,
                cancellationToken);
            phases.AddRange(run.Phases);
            cancellationToken.ThrowIfCancellationRequested();

            stage = PlanningEngineStage.Mapping;
            stageStopwatch.Restart();
            PreparedAutomaticScheduleProposal prepared = proposalMapper.Prepare(
                request.Snapshot,
                run.Result);
            TimeSpan mappingDuration = stageStopwatch.Elapsed;
            phases.Add(new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.ResultMapping,
                AutomaticSchedulePhaseStatus.Completed,
                mappingDuration,
                [
                    new AutomaticSchedulePhaseValue(
                        "assignment_count",
                        prepared.Assignments.Count),
                ]));
            AutomaticScheduleRunMetadata metadata = CreateMetadata(
                request.TimeLimit,
                modelBuildDuration,
                run,
                run.IsProvenOptimal
                    ? AutomaticSchedulePlanningStatus.Optimal
                    : AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
                mappingDuration,
                totalStopwatch.Elapsed,
                phases);
            AutomaticScheduleProposal proposal = proposalMapper.Complete(
                request.Snapshot,
                run.Result,
                prepared,
                metadata);
            return AutomaticSchedulePlanningResult.Success(
                run.IsProvenOptimal
                    ? AutomaticSchedulePlanningStatus.Optimal
                    : AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
                proposal);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            AddTerminalPhase(
                phases,
                stage,
                AutomaticSchedulePhaseStatus.Interrupted,
                stageStopwatch.Elapsed,
                AutomaticSchedulePhaseTerminationReason.CancellationRequested);
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.Cancelled,
                phases: phases);
        }
        catch (AutomaticScheduleOptimizationFailureException exception)
        {
            phases.AddRange(exception.Phases);
            Exception cause = exception.InnerException ?? exception;
            if (cause is OperationCanceledException
                && cancellationToken.IsCancellationRequested)
            {
                return AutomaticSchedulePlanningResult.Failure(
                    AutomaticSchedulePlanningStatus.Cancelled,
                    phases: phases);
            }

            if (cause is PlanningTimeLimitWithoutFeasibleSelectionException)
            {
                return AutomaticSchedulePlanningResult.Failure(
                    AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult,
                    phases: phases);
            }

            AutomaticScheduleErrorCode code = cause is UnexpectedPlanningSolverStatusException
                ? AutomaticScheduleErrorCode.UnexpectedSolverStatus
                : AutomaticScheduleErrorCode.TechnicalFailure;
            return Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                code,
                cause,
                stage,
                phases);
        }
        catch (PlanningTimeLimitWithoutFeasibleSelectionException)
        {
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult);
        }
        catch (UnexpectedPlanningSolverStatusException exception)
        {
            return Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                AutomaticScheduleErrorCode.UnexpectedSolverStatus,
                exception,
                stage,
                phases);
        }
        catch (InvalidOperationException exception)
            when (stage == PlanningEngineStage.Mapping)
        {
            return Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                AutomaticScheduleErrorCode.ResultValidationFailed,
                exception,
                stage,
                phases);
        }
        catch (Exception exception)
        {
            return Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                AutomaticScheduleErrorCode.TechnicalFailure,
                exception,
                stage,
                phases);
        }
    }

    private static AutomaticScheduleRunMetadata CreateMetadata(
        TimeSpan timeLimit,
        TimeSpan modelBuildDuration,
        AutomaticScheduleOptimizationRun run,
        AutomaticSchedulePlanningStatus resultStatus,
        TimeSpan mappingDuration,
        TimeSpan elapsed,
        IEnumerable<AutomaticSchedulePhaseSnapshot> phases)
    {
        TimeSpan measured = modelBuildDuration
            + run.OptimizationDuration
            + mappingDuration;
        return new AutomaticScheduleRunMetadata(
            "Google OR-Tools CP-SAT",
            typeof(CpSolver).Assembly.GetName().Version?.ToString() ?? "unknown",
            resultStatus,
            timeLimit,
            modelBuildDuration,
            run.OptimizationDuration,
            mappingDuration,
            elapsed > measured ? elapsed : measured,
            [
                new AutomaticScheduleSetting("num_search_workers", "1"),
                new AutomaticScheduleSetting("random_seed", "0"),
                new AutomaticScheduleSetting("search_branching", "FIXED_SEARCH_FOR_TIE_BREAK"),
                new AutomaticScheduleSetting("optimization_stages", OptimizationStages),
            ],
            phases);
    }

    private static AutomaticSchedulePlanningResult Failure(
        AutomaticSchedulePlanningStatus status,
        AutomaticScheduleErrorCode code) => AutomaticSchedulePlanningResult.Failure(
            status,
            [new AutomaticScheduleError(
                code,
                CorrelationId: Guid.NewGuid().ToString("N"))]);

    private static AutomaticSchedulePlanningResult Failure(
        AutomaticSchedulePlanningStatus status,
        AutomaticScheduleErrorCode code,
        Exception exception,
        PlanningEngineStage stage,
        List<AutomaticSchedulePhaseSnapshot> phases)
    {
        AddTerminalPhase(
            phases,
            stage,
            AutomaticSchedulePhaseStatus.Failed,
            TimeSpan.Zero,
            AutomaticSchedulePhaseTerminationReason.TechnicalFailure);
        return AutomaticSchedulePlanningResult.Failure(
            status,
            [new AutomaticScheduleError(
                code,
                CorrelationId: Guid.NewGuid().ToString("N"),
                Parameter: exception.GetType().FullName,
                TechnicalDetails:
                    AutomaticScheduleTechnicalFailureDetails.FromException(
                        MapTechnicalStage(stage),
                        exception))],
            phases);
    }

    private static void AddTerminalPhase(
        List<AutomaticSchedulePhaseSnapshot> phases,
        PlanningEngineStage stage,
        AutomaticSchedulePhaseStatus status,
        TimeSpan duration,
        AutomaticSchedulePhaseTerminationReason terminationReason)
    {
        AutomaticSchedulePhaseKind kind = stage switch
        {
            PlanningEngineStage.ModelBuilding => AutomaticSchedulePhaseKind.ModelBuilding,
            PlanningEngineStage.Solving => AutomaticSchedulePhaseKind.HardRules,
            PlanningEngineStage.Mapping => AutomaticSchedulePhaseKind.ResultMapping,
            _ => throw new ArgumentOutOfRangeException(nameof(stage)),
        };
        if (phases.Any(value => value.Kind == kind))
        {
            return;
        }

        phases.Add(new AutomaticSchedulePhaseSnapshot(
            kind,
            status,
            duration,
            termination: new AutomaticSchedulePhaseTerminationSnapshot(
                terminationReason)));
    }

    private static AutomaticScheduleTechnicalStage MapTechnicalStage(
        PlanningEngineStage stage) => stage switch
        {
            PlanningEngineStage.ModelBuilding =>
                AutomaticScheduleTechnicalStage.ModelBuilding,
            PlanningEngineStage.Solving => AutomaticScheduleTechnicalStage.Solving,
            PlanningEngineStage.Mapping => AutomaticScheduleTechnicalStage.Mapping,
            _ => throw new ArgumentOutOfRangeException(nameof(stage)),
        };

    private enum PlanningEngineStage
    {
        ModelBuilding,
        Solving,
        Mapping,
    }
}
