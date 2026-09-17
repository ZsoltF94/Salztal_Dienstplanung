using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

internal static class ExhaustiveHighPriorityReferenceSolver
{
    public static IReadOnlyList<RuleViolationSet> FindParetoFrontier(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        Func<PlanningAssignmentCandidate[], RuleViolationSet> evaluateHighRules)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(candidateSet);
        ArgumentNullException.ThrowIfNull(evaluateHighRules);
        if (candidateSet.Candidates.Count > 20
            || candidateSet.Candidates.Any(candidate =>
                candidate.Kind == PlanningCandidateKind.ReliefShiftPattern))
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateSet),
                "This exhaustive high-priority reference supports at most 20 non-relief candidates.");
        }

        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidateSet);
        HardRulePlanningContext context = new(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);
        PlanningAssignmentCandidate[][] feasible = Enumerate(candidateSet)
            .Where(selection => HasValidStructure(selection)
                && AutomaticHardRuleSelectionEvaluator.Evaluate(
                    context,
                    selection.Select(item => item.TechnicalKey)).IsValid)
            .ToArray();
        int maximumCoverage = feasible.Max(selection => selection.Sum(candidate =>
            candidate.Coverages.Sum(coverage => coverage.CoveredMinutes)));
        PlanningAssignmentCandidate[][] coverageOptimal = feasible
            .Where(selection => selection.Sum(candidate => candidate.Coverages.Sum(
                coverage => coverage.CoveredMinutes)) == maximumCoverage)
            .ToArray();
        int maximumTouched = coverageOptimal.Max(selection => selection
            .SelectMany(candidate => candidate.Coverages)
            .Select(coverage => coverage.Demand)
            .Distinct()
            .Count());
        RuleViolationSet[] highValues = coverageOptimal
            .Where(selection => selection.SelectMany(candidate => candidate.Coverages)
                .Select(coverage => coverage.Demand)
                .Distinct()
                .Count() == maximumTouched)
            .Select(evaluateHighRules)
            .ToArray();
        int minimumCases = highValues.Min(value => value.Count);
        RuleViolationSet[] caseOptimal = highValues
            .Where(value => value.Count == minimumCases)
            .ToArray();
        return caseOptimal.Where(candidate => !caseOptimal.Any(other =>
                !ReferenceEquals(candidate, other)
                && Dominates(other, candidate)))
            .ToArray();
    }

    private static bool Dominates(RuleViolationSet left, RuleViolationSet right)
    {
        RuleId[] ruleIds = left.MagnitudeByRule.Keys
            .Concat(right.MagnitudeByRule.Keys)
            .Distinct()
            .ToArray();
        return ruleIds.All(rule => left.MagnitudeByRule.GetValueOrDefault(rule)
                <= right.MagnitudeByRule.GetValueOrDefault(rule))
            && ruleIds.Any(rule => left.MagnitudeByRule.GetValueOrDefault(rule)
                < right.MagnitudeByRule.GetValueOrDefault(rule));
    }

    private static IEnumerable<PlanningAssignmentCandidate[]> Enumerate(
        PlanningCandidateSet candidateSet)
    {
        int combinations = 1 << candidateSet.Candidates.Count;
        for (int mask = 0; mask < combinations; mask++)
        {
            yield return candidateSet.Candidates
                .Where((_, index) => (mask & (1 << index)) != 0)
                .ToArray();
        }
    }

    private static bool HasValidStructure(PlanningAssignmentCandidate[] selection) =>
        !selection.GroupBy(candidate => (candidate.EmployeeId, candidate.Date))
            .Any(group => group.Count() > 1)
        && !selection.SelectMany(candidate => candidate.Coverages)
            .GroupBy(coverage => coverage.Demand)
            .Any(group => group.Count() > 1);
}
