using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

internal static class ExhaustiveCandidateSelectionReferenceSolver
{
    public static CandidateSelectionReferenceResult FindMaximumCoverage(
        PlanningCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(candidateSet);
        if (candidateSet.Candidates.Count > 62)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateSet),
                "The exhaustive reference is limited to 62 candidates.");
        }

        long combinationCount = 1L << candidateSet.Candidates.Count;
        int bestCoveredMinutes = -1;
        string[] bestKeys = [];
        for (long mask = 0; mask < combinationCount; mask++)
        {
            string[] selectedKeys = candidateSet.Candidates
                .Where((_, index) => (mask & (1L << index)) != 0)
                .Select(candidate => candidate.TechnicalKey)
                .ToArray();
            HashSet<string> selected = selectedKeys.ToHashSet(StringComparer.Ordinal);
            if (!IsFeasible(candidateSet, selected))
            {
                continue;
            }

            int coveredMinutes = candidateSet.CoverageChoices
                .Where(choice => choice.CandidateKeys.Any(selected.Contains))
                .Sum(choice => (int)(choice.End - choice.Start).TotalMinutes);
            string[] orderedKeys = selectedKeys.Order(StringComparer.Ordinal).ToArray();
            if (coveredMinutes > bestCoveredMinutes
                || (coveredMinutes == bestCoveredMinutes
                    && CompareKeys(orderedKeys, bestKeys) < 0))
            {
                bestCoveredMinutes = coveredMinutes;
                bestKeys = orderedKeys;
            }
        }

        return new CandidateSelectionReferenceResult(
            bestCoveredMinutes,
            bestKeys);
    }

    private static bool IsFeasible(
        PlanningCandidateSet candidateSet,
        HashSet<string> selected)
    {
        return candidateSet.EmployeeDayChoices.All(choice =>
                choice.CandidateKeys.Count(selected.Contains) <= 1)
            && candidateSet.CoverageChoices.All(choice =>
                choice.CandidateKeys.Count(selected.Contains) <= 1);
    }

    private static int CompareKeys(
        string[] left,
        string[] right)
    {
        int sharedLength = Math.Min(left.Length, right.Length);
        for (int index = 0; index < sharedLength; index++)
        {
            int comparison = string.Compare(
                left[index],
                right[index],
                StringComparison.Ordinal);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return left.Length.CompareTo(right.Length);
    }
}

internal sealed record CandidateSelectionReferenceResult(
    int CoveredMinutes,
    IReadOnlyList<string> CandidateKeys);
