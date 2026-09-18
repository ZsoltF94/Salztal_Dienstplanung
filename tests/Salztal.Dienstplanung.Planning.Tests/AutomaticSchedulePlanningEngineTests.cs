using System.Security.Cryptography;
using System.Text;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Mapping;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Validation;

namespace Salztal.Dienstplanung.Planning.Tests;

[Collection(nameof(AutomaticSchedulePlanningEngineExecutionGroup))]
public sealed class AutomaticSchedulePlanningEngineTests
{
    [Fact]
    public void SimpleScenarioCanBeOptimizedWithinProductiveBoundary()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();

        AutomaticScheduleOptimizationRun result = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot),
            TimeSpan.FromSeconds(30),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsProvenOptimal);
    }

    [Fact]
    public async Task RealPlannerReturnsOptimalProposalWithCompleteCanonicalMetadata()
    {
        AutomaticSchedulePlanningRequest request = new(
            PlanningInputTestFactory.Create(),
            TimeSpan.FromSeconds(30));

        AutomaticSchedulePlanningResult result = await new AutomaticSchedulePlanner(
            new AutomaticSchedulePlanningEngine()).PlanAsync(
                request,
                CancellationToken.None);

        Assert.Equal(AutomaticSchedulePlanningStatus.Optimal, result.Status);
        AutomaticScheduleRunMetadata metadata = Assert.IsType<AutomaticScheduleRunMetadata>(
            result.Preview?.Proposal.Metadata);
        Assert.Equal(request.TimeLimit, metadata.TimeLimit);
        Assert.Equal(AutomaticSchedulePlanningStatus.Optimal, metadata.ResultStatus);
        Assert.Equal("Google OR-Tools CP-SAT", metadata.SolverName);
        Assert.NotEqual("unknown", metadata.SolverVersion);
        Assert.Contains(metadata.Settings, item =>
            item.Key == "num_search_workers" && item.Value == "1");
        Assert.Contains(metadata.Settings, item =>
            item.Key == "random_seed" && item.Value == "0");
        Assert.Contains(metadata.Settings, item =>
            item.Key == "optimization_stages"
            && item.Value.StartsWith("joint:", StringComparison.Ordinal)
            && item.Value.Contains("auxiliary_minimum", StringComparison.Ordinal));
        Assert.Equal(
            [
                AutomaticSchedulePhaseKind.InputValidation,
                AutomaticSchedulePhaseKind.ModelBuilding,
                AutomaticSchedulePhaseKind.HardRules,
                AutomaticSchedulePhaseKind.RegularCoverage,
                AutomaticSchedulePhaseKind.ReliefCoverage,
                AutomaticSchedulePhaseKind.HighPriorityRules,
                AutomaticSchedulePhaseKind.ReliefShiftMinimization,
                AutomaticSchedulePhaseKind.SplitShiftMinimization,
                AutomaticSchedulePhaseKind.AuxiliaryMinimum,
                AutomaticSchedulePhaseKind.RelativeWeeklyTarget,
                AutomaticSchedulePhaseKind.MediumPriorityRules,
                AutomaticSchedulePhaseKind.LowPriorityRules,
                AutomaticSchedulePhaseKind.Stability,
                AutomaticSchedulePhaseKind.TechnicalTieBreak,
                AutomaticSchedulePhaseKind.ResultMapping,
            ],
            metadata.Phases.Select(value => value.Kind));
        Assert.Equal(metadata.Phases, result.Phases);
        Assert.Equal(
            AutomaticSchedulePhaseTraceCompleteness.Complete,
            metadata.PhaseTraceCompleteness);
        Assert.All(
            metadata.Phases.Where(value => value.Kind
                != AutomaticSchedulePhaseKind.LowPriorityRules),
            value => Assert.Equal(
                AutomaticSchedulePhaseStatus.Completed,
                value.Status));
        Assert.Equal(
            AutomaticSchedulePhaseStatus.NotApplicable,
            Assert.Single(metadata.Phases, value => value.Kind
                == AutomaticSchedulePhaseKind.LowPriorityRules).Status);
        AutomaticSchedulePhaseSnapshot relief = Assert.Single(
            metadata.Phases,
            value => value.Kind
                == AutomaticSchedulePhaseKind.ReliefShiftMinimization);
        Assert.Contains(
            relief.Values,
            value => value.Key == "initial_assignment_count");
        Assert.Contains(
            relief.Values,
            value => value.Key == "assignment_count");
        AutomaticSchedulePhaseSnapshot split = Assert.Single(
            metadata.Phases,
            value => value.Kind
                == AutomaticSchedulePhaseKind.SplitShiftMinimization);
        Assert.Contains(
            split.Values,
            value => value.Key == "initial_assignment_count");
        Assert.Contains(
            split.Values,
            value => value.Key == "assignment_count");
        AutomaticSchedulePhaseSnapshot reliefCoverage = Assert.Single(
            metadata.Phases,
            value => value.Kind == AutomaticSchedulePhaseKind.ReliefCoverage);
        Assert.Contains(
            reliefCoverage.Values,
            value => value.Key == "initial_covered_minutes");
        Assert.Contains(
            reliefCoverage.Values,
            value => value.Key == "additional_covered_minutes");
        AutomaticSchedulePhaseSnapshot regularCoverage = Assert.Single(
            metadata.Phases,
            value => value.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Contains(
            regularCoverage.Values,
            value => value.Key == "uncovered_minutes");
        Assert.Contains(
            regularCoverage.Values,
            value => value.Key == "fully_uncovered_demand_count");
        Assert.Contains(
            reliefCoverage.Values,
            value => value.Key == "initial_uncovered_minutes");
        Assert.Contains(
            reliefCoverage.Values,
            value => value.Key == "fully_uncovered_demand_count");
    }

    [Fact]
    public async Task FeasibleRunReturnsPreviewWithoutClaimingOptimality()
    {
        AutomaticSchedulePlanningRequest request = CreateRequest();
        AutomaticSchedulePlanningEngine engine = CreateEngine(new DelegateRunner(
            (snapshot, candidates, _, _) => CreateRun(snapshot, candidates, false)));

        AutomaticSchedulePlanningResult result = await engine.PlanAsync(
            request,
            CancellationToken.None);

        Assert.True(
            result.Status == AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
            string.Join(';', result.Errors.Select(error =>
                $"{error.Code}:{error.Parameter}")));
        Assert.NotNull(result.Preview);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task TimeLimitWithoutFeasibleSelectionHasNoPreview()
    {
        AutomaticSchedulePlanningRequest request = new(
            CreateSnapshot(),
            TimeSpan.FromTicks(1));
        ScheduleAssignmentSnapshot[] originalAssignments = request.Snapshot
            .ServiceManagementAssignments
            .ToArray();

        AutomaticSchedulePlanningResult result = await new AutomaticSchedulePlanningEngine()
            .PlanAsync(
            request,
            CancellationToken.None);

        Assert.Equal(
            AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult,
            result.Status);
        Assert.Null(result.Preview);
        Assert.Empty(result.Errors);
        Assert.Equal(originalAssignments, request.Snapshot.ServiceManagementAssignments);
        Assert.NotEmpty(result.Phases);
        Assert.Equal(
            AutomaticSchedulePhaseStatus.Interrupted,
            result.Phases[^1].Status);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection,
            result.Phases[^1].Termination?.Reason);
        Assert.Equal(
            AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes,
            result.Phases[^1].Termination?.ActiveTarget);
        Assert.Equal(request.TimeLimit, result.Phases[^1].Termination?.TimeLimit);
        Assert.True(result.Phases[^1].Termination?.BudgetElapsed >= request.TimeLimit);
    }

    [Fact]
    public async Task CancellationDuringSolveStopsWithoutReturningIncumbent()
    {
        using ManualResetEventSlim entered = new();
        AutomaticSchedulePlanningEngine engine = CreateEngine(new DelegateRunner(
            (snapshot, candidates, _, cancellationToken) =>
            {
                entered.Set();
                cancellationToken.WaitHandle.WaitOne();
                cancellationToken.ThrowIfCancellationRequested();
                return CreateRun(snapshot, candidates, false);
            }));
        using CancellationTokenSource cancellation = new();
        Task<AutomaticSchedulePlanningResult> task = engine.PlanAsync(
            CreateRequest(),
            cancellation.Token);
        Assert.True(entered.Wait(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken));

        cancellation.Cancel();
        AutomaticSchedulePlanningResult result = await task;

        Assert.Equal(AutomaticSchedulePlanningStatus.Cancelled, result.Status);
        Assert.Null(result.Preview);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task CancellationAfterRunnerResultDiscardsFeasiblePreview()
    {
        using CancellationTokenSource cancellation = new();
        AutomaticSchedulePlanningEngine engine = CreateEngine(new DelegateRunner(
            (snapshot, candidates, _, _) =>
            {
                AutomaticScheduleOptimizationRun run = CreateRun(
                    snapshot,
                    candidates,
                    false);
                cancellation.Cancel();
                return run;
            }));

        AutomaticSchedulePlanningResult result = await engine.PlanAsync(
            CreateRequest(),
            cancellation.Token);

        Assert.Equal(AutomaticSchedulePlanningStatus.Cancelled, result.Status);
        Assert.Null(result.Preview);
    }

    [Fact]
    public async Task ConcurrentRunIsRejectedWithoutEnteringSecondRunner()
    {
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim release = new();
        int calls = 0;
        AutomaticSchedulePlanningEngine engine = CreateEngine(new DelegateRunner(
            (snapshot, candidates, _, cancellationToken) =>
            {
                Interlocked.Increment(ref calls);
                entered.Set();
                release.Wait(cancellationToken);
                return CreateRun(snapshot, candidates, true);
            }));
        Task<AutomaticSchedulePlanningResult> first = engine.PlanAsync(
            CreateRequest(),
            CancellationToken.None);
        Assert.True(entered.Wait(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken));

        AutomaticSchedulePlanningResult second = await engine.PlanAsync(
            CreateRequest(),
            CancellationToken.None);
        release.Set();
        AutomaticSchedulePlanningResult completedFirst = await first;

        Assert.Equal(AutomaticSchedulePlanningStatus.Optimal, completedFirst.Status);
        Assert.Equal(AutomaticSchedulePlanningStatus.TechnicalFailure, second.Status);
        Assert.Equal(AutomaticScheduleErrorCode.ConcurrentRun, Assert.Single(second.Errors).Code);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task UnexpectedSolverStatusUsesStablePrivacySafeError()
    {
        AutomaticSchedulePlanningEngine engine = CreateEngine(new DelegateRunner(
            (_, _, _, _) => throw new UnexpectedPlanningSolverStatusException(
                Google.OrTools.Sat.CpSolverStatus.ModelInvalid)));

        AutomaticSchedulePlanningResult result = await engine.PlanAsync(
            CreateRequest(),
            CancellationToken.None);

        AutomaticScheduleError error = Assert.Single(result.Errors);
        Assert.Equal(AutomaticSchedulePlanningStatus.TechnicalFailure, result.Status);
        Assert.Equal(AutomaticScheduleErrorCode.UnexpectedSolverStatus, error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
        Assert.Equal(typeof(UnexpectedPlanningSolverStatusException).FullName, error.Parameter);
        Assert.Equal(
            AutomaticScheduleTechnicalStage.Solving,
            error.TechnicalDetails?.Stage);
    }

    [Fact]
    public async Task MappingFailureUsesStableCodeWithoutExceptionMessage()
    {
        const string SensitiveMessage = "Mitarbeiter Erika Mustermann kompletter Dienstplan";
        AutomaticSchedulePlanningRequest request = CreateRequest();
        AutomaticSchedulePlanningEngine engine = new(
            PlanningCandidateBuilder.Build,
            new DelegateRunner((snapshot, candidates, _, _) =>
                CreateRun(snapshot, candidates, true)),
            new ThrowingMapper(SensitiveMessage));

        AutomaticSchedulePlanningResult result = await engine.PlanAsync(
            request,
            CancellationToken.None);

        AutomaticScheduleError error = Assert.Single(result.Errors);
        Assert.Equal(AutomaticScheduleErrorCode.ResultValidationFailed, error.Code);
        Assert.Equal(typeof(InvalidOperationException).FullName, error.Parameter);
        Assert.Equal(
            AutomaticScheduleTechnicalStage.Mapping,
            error.TechnicalDetails?.Stage);
        Assert.DoesNotContain(SensitiveMessage, string.Join('|', result.Errors));
        Assert.Null(result.Preview);
    }

    [Fact]
    public async Task TechnicalFailureDoesNotExposeNamesOrPlanContents()
    {
        const string SensitiveMessage = "Mitarbeiter Max Beispiel: kompletter Dienstplan";
        AutomaticSchedulePlanningEngine engine = CreateEngine(new DelegateRunner(
            (_, _, _, _) => throw new InvalidDataException(SensitiveMessage)));

        AutomaticSchedulePlanningResult result = await engine.PlanAsync(
            CreateRequest(),
            CancellationToken.None);

        AutomaticScheduleError error = Assert.Single(result.Errors);
        Assert.Equal(AutomaticScheduleErrorCode.TechnicalFailure, error.Code);
        Assert.Equal(typeof(InvalidDataException).FullName, error.Parameter);
        Assert.Equal(
            AutomaticScheduleTechnicalStage.Solving,
            error.TechnicalDetails?.Stage);
        Assert.DoesNotContain(SensitiveMessage, string.Join('|', result.Errors));
    }

    [Fact]
    public async Task RepeatedRunsAndFreshAdapterReturnIdenticalProposalContent()
    {
        AutomaticSchedulePlanningRequest request = CreateRequest();
        AutomaticSchedulePlanningEngine firstEngine = new();

        AutomaticSchedulePlanningResult first = await firstEngine.PlanAsync(
            request,
            CancellationToken.None);
        AutomaticSchedulePlanningResult repeated = await firstEngine.PlanAsync(
            request,
            CancellationToken.None);
        AutomaticSchedulePlanningResult fresh = await new AutomaticSchedulePlanningEngine()
            .PlanAsync(request, CancellationToken.None);

        string firstSignature = Signature(Assert.IsType<AutomaticScheduleProposal>(
            first.Preview?.Proposal));
        string signatureHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(firstSignature)));
        Assert.Equal("8CE119FA0F60DA06C1AEA2C2C4628FFA", signatureHash[..32]);
        Assert.Equal("BBAF39C6E6C9F2CFF233968F53EF5963", signatureHash[32..]);
        Assert.Equal(firstSignature, Signature(Assert.IsType<AutomaticScheduleProposal>(
            repeated.Preview?.Proposal)));
        Assert.Equal(firstSignature, Signature(Assert.IsType<AutomaticScheduleProposal>(
            fresh.Preview?.Proposal)));
    }

    [Fact]
    public void ProductiveDefaultTimeLimitIsCentralAndNotUserEditable()
    {
        Assert.Equal(TimeSpan.FromSeconds(120), AutomaticSchedulePlanningEngine.DefaultTimeLimit);
    }

    private static AutomaticSchedulePlanningRequest CreateRequest() => new(
        CreateSnapshot(),
        TimeSpan.FromSeconds(30));

    private static PlanningInputSnapshot CreateSnapshot()
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        ScheduleDemandSlotSnapshot slot = DemandCoverageOptimizerTests.Slot(
            CandidateScenarioFactory.Saturday,
            1,
            CandidateScenarioFactory.LateShiftId,
            180,
            startHour: 16,
            startMinute: 30);
        return DemandCoverageOptimizerTests.Clone(
            CandidateScenarioFactory.Create(),
            [employee],
            [type],
            [slot],
            []);
    }

    private static AutomaticSchedulePlanningEngine CreateEngine(
        IAutomaticScheduleOptimizationRunner runner) => new(
            PlanningCandidateBuilder.Build,
            runner,
            new AutomaticScheduleProposalMapper(
                new DeterministicAutomaticScheduleAssignmentIdFactory()));

    private static AutomaticScheduleOptimizationRun CreateRun(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidates,
        bool isProvenOptimal) => new(
            DemandCoverageOptimizer.Optimize(snapshot, candidates),
            isProvenOptimal,
            TimeSpan.FromMilliseconds(2));

    private static string Signature(AutomaticScheduleProposal proposal) => string.Join(
        '|',
        proposal.Assignments.Select(assignment =>
            $"A:{assignment.AssignmentId:N}:{assignment.EmployeeId:N}:{assignment.Date:yyyyMMdd}:"
            + $"{assignment.Kind}:{assignment.PatternId}")
        .Concat(proposal.GeneratedDayOffs.Select(dayOff =>
            $"X:{dayOff.EmployeeId:N}:{dayOff.Date:yyyyMMdd}"))
        .Concat(proposal.OpenDemands.Select(demand =>
            $"O:{demand.SourceId:N}:{demand.Date:yyyyMMdd}:{demand.Ordinal}:"
            + $"{demand.UncoveredStart:HHmm}:{demand.UncoveredEnd:HHmm}"))
        .Append(
            $"V:{proposal.ObjectiveVector.UncoveredEmployeeMinutes}:"
            + $"{proposal.ObjectiveVector.FullyUncoveredDemandSlotCount}:"
            + $"{proposal.ObjectiveVector.HighPriorityViolations.Count}:"
            + $"{proposal.ObjectiveVector.MediumPriorityViolations.Count}:"
            + $"{proposal.ObjectiveVector.StabilityViolations.Count}"));

    private sealed class DelegateRunner(
        Func<PlanningInputSnapshot, PlanningCandidateSet, TimeSpan, CancellationToken,
            AutomaticScheduleOptimizationRun> implementation)
        : IAutomaticScheduleOptimizationRunner
    {
        public AutomaticScheduleOptimizationRun Optimize(
            PlanningInputSnapshot snapshot,
            PlanningCandidateSet candidateSet,
            TimeSpan timeLimit,
            CancellationToken cancellationToken) => implementation(
                snapshot,
                candidateSet,
                timeLimit,
                cancellationToken);
    }

    private sealed class ThrowingMapper(string message) : IAutomaticScheduleProposalMapper
    {
        public PreparedAutomaticScheduleProposal Prepare(
            PlanningInputSnapshot snapshot,
            DemandCoverageOptimizationResult optimization) =>
            throw new InvalidOperationException(message);

        public AutomaticScheduleProposal Complete(
            PlanningInputSnapshot snapshot,
            DemandCoverageOptimizationResult optimization,
            PreparedAutomaticScheduleProposal prepared,
            AutomaticScheduleRunMetadata metadata) =>
            throw new NotSupportedException();
    }
}

[CollectionDefinition(
    nameof(AutomaticSchedulePlanningEngineExecutionGroup),
    DisableParallelization = true)]
public sealed class AutomaticSchedulePlanningEngineExecutionGroup;
