using System.Diagnostics;
using System.Globalization;
using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.ModelBuilding;

namespace Salztal.Dienstplanung.Planning.Optimization;

internal sealed class PlanningSolveBudget
{
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private readonly CancellationToken cancellationToken;
    private readonly Func<TimeSpan>? elapsedProvider;
    private IReadOnlyList<string>? lastSelectedCandidateKeys;
    private AutomaticScheduleOptimizationTargetKind? activeTarget;

    public PlanningSolveBudget(
        TimeSpan timeLimit,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeLimit, TimeSpan.Zero);
        TimeLimit = timeLimit;
        this.cancellationToken = cancellationToken;
    }

    internal PlanningSolveBudget(
        TimeSpan timeLimit,
        Func<TimeSpan> elapsedProvider,
        CancellationToken cancellationToken)
        : this(timeLimit, cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(elapsedProvider);
        this.elapsedProvider = elapsedProvider;
    }

    public TimeSpan TimeLimit { get; }

    public CancellationToken CancellationToken => cancellationToken;

    public TimeSpan Elapsed => elapsedProvider?.Invoke() ?? stopwatch.Elapsed;

    public IReadOnlyList<string> LastSelectedCandidateKeys =>
        lastSelectedCandidateKeys
        ?? throw new InvalidOperationException(
            "No feasible planning selection has been recorded yet.");

    public AutomaticScheduleOptimizationTargetKind? ActiveTarget => activeTarget;

    internal bool HasRecordedSelection => lastSelectedCandidateKeys is not null;

    public void SetActiveTarget(AutomaticScheduleOptimizationTargetKind target)
    {
        if (!Enum.IsDefined(target))
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        activeTarget = target;
    }

    public AutomaticSchedulePhaseTerminationSnapshot CreateTermination(
        AutomaticSchedulePhaseTerminationReason reason) => new(
        reason,
        activeTarget,
        TimeLimit,
        Elapsed);

    public CpSolver CreateSolver(bool fixedSearch = false)
    {
        ThrowIfInterrupted();
        double remainingSeconds = Math.Max(
            0.001,
            (TimeLimit - Elapsed).TotalSeconds);
        string parameters = string.Join(' ', new[]
        {
            "num_search_workers:1",
            "random_seed:0",
            $"max_time_in_seconds:{remainingSeconds.ToString("R", CultureInfo.InvariantCulture)}",
            fixedSearch ? "search_branching:FIXED_SEARCH" : null,
        }.Where(value => value is not null));
        return new CpSolver
        {
            StringParameters = parameters,
        };
    }

    public CancellationTokenRegistration RegisterCancellation(CpSolver solver)
    {
        ArgumentNullException.ThrowIfNull(solver);
        return cancellationToken.Register(solver.StopSearch);
    }

    public void RecordSelection(
        StructuralPlanningModel model,
        CpSolver solver)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(solver);
        RecordSelection(model.CandidateSet.Candidates
            .Where(candidate => solver.Value(
                model.CandidateVariables[candidate.TechnicalKey]) == 1)
            .Select(candidate => candidate.TechnicalKey));
    }

    internal void RecordSelection(IEnumerable<string> selectedCandidateKeys)
    {
        ArgumentNullException.ThrowIfNull(selectedCandidateKeys);
        string[] values = selectedCandidateKeys.ToArray();
        if (values.Any(string.IsNullOrWhiteSpace)
            || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                "Selected planning candidate keys must be non-empty and unique.",
                nameof(selectedCandidateKeys));
        }

        lastSelectedCandidateKeys = values;
    }

    public void ThrowIfInterrupted()
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Elapsed < TimeLimit)
        {
            return;
        }

        if (lastSelectedCandidateKeys is not null)
        {
            throw new PlanningTimeLimitWithFeasibleSelectionException(
                lastSelectedCandidateKeys,
                CreateTermination(
                    AutomaticSchedulePhaseTerminationReason
                        .TimeLimitWithFeasibleSelection));
        }

        throw new PlanningTimeLimitWithoutFeasibleSelectionException(
            CreateTermination(
                AutomaticSchedulePhaseTerminationReason
                    .TimeLimitWithoutFeasibleSelection));
    }

    public void ThrowForStoppedSearch(CpSolverStatus status)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (status == CpSolverStatus.Feasible)
        {
            if (lastSelectedCandidateKeys is not null)
            {
                throw new PlanningTimeLimitWithFeasibleSelectionException(
                    lastSelectedCandidateKeys,
                    CreateTermination(
                        AutomaticSchedulePhaseTerminationReason
                            .TimeLimitWithFeasibleSelection));
            }

            throw new PlanningTimeLimitWithoutFeasibleSelectionException(
                CreateTermination(
                    AutomaticSchedulePhaseTerminationReason
                        .TimeLimitWithoutFeasibleSelection));
        }

        if (status == CpSolverStatus.Unknown && Elapsed >= TimeLimit)
        {
            if (lastSelectedCandidateKeys is not null)
            {
                throw new PlanningTimeLimitWithFeasibleSelectionException(
                    lastSelectedCandidateKeys,
                    CreateTermination(
                        AutomaticSchedulePhaseTerminationReason
                            .TimeLimitWithFeasibleSelection));
            }

            throw new PlanningTimeLimitWithoutFeasibleSelectionException(
                CreateTermination(
                    AutomaticSchedulePhaseTerminationReason
                        .TimeLimitWithoutFeasibleSelection));
        }

        throw new UnexpectedPlanningSolverStatusException(status);
    }
}

internal sealed class PlanningTimeLimitWithFeasibleSelectionException(
    IReadOnlyList<string> selectedCandidateKeys,
    AutomaticSchedulePhaseTerminationSnapshot termination) : Exception
{
    public IReadOnlyList<string> SelectedCandidateKeys { get; } =
        selectedCandidateKeys;

    public AutomaticSchedulePhaseTerminationSnapshot Termination { get; } =
        termination;
}

internal sealed class PlanningTimeLimitWithoutFeasibleSelectionException(
    AutomaticSchedulePhaseTerminationSnapshot termination) : Exception
{
    public AutomaticSchedulePhaseTerminationSnapshot Termination { get; } =
        termination;
}

internal sealed class UnexpectedPlanningSolverStatusException(CpSolverStatus status)
    : Exception
{
    public CpSolverStatus Status { get; } = status;
}
