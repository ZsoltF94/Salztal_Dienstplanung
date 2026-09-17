using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

internal static class ExhaustiveScheduleReferenceSolver
{
    internal static ReferenceScheduleCandidate<TValue>? SelectBest<TValue>(
        IEnumerable<ReferenceScheduleCandidate<TValue>> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        ReferenceScheduleCandidate<TValue>? best = null;
        foreach (ReferenceScheduleCandidate<TValue> candidate in candidates
                     .Where(candidate => candidate.IsFeasible)
                     .OrderBy(candidate => candidate.Key, StringComparer.Ordinal))
        {
            if (best is null)
            {
                best = candidate;
                continue;
            }

            ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(
                candidate.ObjectiveVector,
                best.ObjectiveVector);
            if (comparison.Outcome == ScheduleObjectiveComparisonOutcome.Better)
            {
                best = candidate;
            }
        }

        return best;
    }
}
