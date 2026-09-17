using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.Optimization;

internal interface IAutomaticScheduleOptimizationRunner
{
    public AutomaticScheduleOptimizationRun Optimize(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        TimeSpan timeLimit,
        CancellationToken cancellationToken);
}

internal sealed class AutomaticScheduleOptimizationRunner
    : IAutomaticScheduleOptimizationRunner
{
    public AutomaticScheduleOptimizationRun Optimize(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        TimeSpan timeLimit,
        CancellationToken cancellationToken) => DemandCoverageOptimizer.Optimize(
            snapshot,
            candidateSet,
            timeLimit,
            cancellationToken);
}
