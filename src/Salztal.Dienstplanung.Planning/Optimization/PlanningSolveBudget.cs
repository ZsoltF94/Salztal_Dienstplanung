using System.Diagnostics;
using System.Globalization;
using Google.OrTools.Sat;
using Salztal.Dienstplanung.Planning.ModelBuilding;

namespace Salztal.Dienstplanung.Planning.Optimization;

internal sealed class PlanningSolveBudget
{
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private readonly CancellationToken cancellationToken;
    private IReadOnlyList<string>? lastSelectedCandidateKeys;

    public PlanningSolveBudget(
        TimeSpan timeLimit,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeLimit, TimeSpan.Zero);
        TimeLimit = timeLimit;
        this.cancellationToken = cancellationToken;
    }

    public TimeSpan TimeLimit { get; }

    public CancellationToken CancellationToken => cancellationToken;

    public TimeSpan Elapsed => stopwatch.Elapsed;

    public CpSolver CreateSolver(bool fixedSearch = false)
    {
        ThrowIfInterrupted();
        double remainingSeconds = Math.Max(
            0.001,
            (TimeLimit - stopwatch.Elapsed).TotalSeconds);
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
        lastSelectedCandidateKeys = model.CandidateSet.Candidates
            .Where(candidate => solver.Value(
                model.CandidateVariables[candidate.TechnicalKey]) == 1)
            .Select(candidate => candidate.TechnicalKey)
            .ToArray();
    }

    public void ThrowIfInterrupted()
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (stopwatch.Elapsed < TimeLimit)
        {
            return;
        }

        if (lastSelectedCandidateKeys is not null)
        {
            throw new PlanningTimeLimitWithFeasibleSelectionException(
                lastSelectedCandidateKeys);
        }

        throw new PlanningTimeLimitWithoutFeasibleSelectionException();
    }

    public void ThrowForStoppedSearch(CpSolverStatus status)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (status == CpSolverStatus.Feasible)
        {
            if (lastSelectedCandidateKeys is not null)
            {
                throw new PlanningTimeLimitWithFeasibleSelectionException(
                    lastSelectedCandidateKeys);
            }

            throw new PlanningTimeLimitWithoutFeasibleSelectionException();
        }

        if (status == CpSolverStatus.Unknown && stopwatch.Elapsed >= TimeLimit)
        {
            if (lastSelectedCandidateKeys is not null)
            {
                throw new PlanningTimeLimitWithFeasibleSelectionException(
                    lastSelectedCandidateKeys);
            }

            throw new PlanningTimeLimitWithoutFeasibleSelectionException();
        }

        throw new UnexpectedPlanningSolverStatusException(status);
    }
}

internal sealed class PlanningTimeLimitWithFeasibleSelectionException(
    IReadOnlyList<string> selectedCandidateKeys) : Exception
{
    public IReadOnlyList<string> SelectedCandidateKeys { get; } =
        selectedCandidateKeys;
}

internal sealed class PlanningTimeLimitWithoutFeasibleSelectionException : Exception
{
}

internal sealed class UnexpectedPlanningSolverStatusException(CpSolverStatus status)
    : Exception
{
    public CpSolverStatus Status { get; } = status;
}
