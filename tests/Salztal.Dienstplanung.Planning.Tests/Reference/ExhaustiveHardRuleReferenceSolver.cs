using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

internal static class ExhaustiveHardRuleReferenceSolver
{
    public static int FindMaximumCoveredMinutes(HardRulePlanningContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        PlanningAssignmentCandidate[] candidates = context.StructuralModel.CandidateSet
            .Candidates.ToArray();
        if (candidates.Length > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(context),
                "The exhaustive reference solver supports at most 20 candidates.");
        }

        int maximum = 0;
        int combinations = 1 << candidates.Length;
        for (int mask = 0; mask < combinations; mask++)
        {
            PlanningAssignmentCandidate[] selected = candidates
                .Where((_, index) => (mask & (1 << index)) != 0)
                .ToArray();
            if (!HasValidStructure(selected))
            {
                continue;
            }

            AutomaticHardRuleSelectionEvaluation evaluation =
                AutomaticHardRuleSelectionEvaluator.Evaluate(
                    context,
                    selected.Select(candidate => candidate.TechnicalKey));
            if (!evaluation.IsValid)
            {
                continue;
            }

            maximum = Math.Max(
                maximum,
                selected.Sum(candidate => candidate.Coverages.Sum(coverage =>
                    coverage.CoveredMinutes)));
        }

        return maximum;
    }

    private static bool HasValidStructure(
        IEnumerable<PlanningAssignmentCandidate> selected)
    {
        PlanningAssignmentCandidate[] values = selected.ToArray();
        if (values.GroupBy(candidate => (candidate.EmployeeId, candidate.Date))
            .Any(group => group.Count() > 1))
        {
            return false;
        }

        return !values.SelectMany(candidate => candidate.Coverages)
            .GroupBy(coverage => coverage.Demand)
            .Any(group => group.Any(first => group.Any(second =>
                !ReferenceEquals(first, second)
                && first.CoveredStart < second.CoveredEnd
                && second.CoveredStart < first.CoveredEnd)));
    }
}
