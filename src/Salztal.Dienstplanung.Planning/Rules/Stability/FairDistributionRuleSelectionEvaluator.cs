using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.Stability;

internal sealed record FairDistributionRuleSelectionEvaluation(
    RuleViolationSet Violations,
    RuleEvaluationResult Result);

internal static class FairDistributionRuleSelectionEvaluator
{
    public static FairDistributionRuleSelectionEvaluation Evaluate(
        HardRulePlanningContext context,
        IEnumerable<string> selectedCandidateKeys)
    {
        HashSet<string> keys = selectedCandidateKeys.ToHashSet(StringComparer.Ordinal);
        PlanningAssignmentCandidate[] selected = context.StructuralModel.CandidateSet.Candidates
            .Where(candidate => keys.Contains(candidate.TechnicalKey))
            .ToArray();
        List<RuleViolationCase> violations = [];
        foreach (FairDistributionSubject subject in Enum.GetValues<FairDistributionSubject>())
        {
            var opportunities = context.Snapshot.Employees.ToDictionary(
                employee => employee.Id,
                employee => context.StructuralModel.CandidateSet.Candidates
                    .Where(candidate => candidate.EmployeeId == employee.Id
                        && FairDistributionRuleModelBuilder.Matches(
                            context.Snapshot,
                            candidate,
                            subject))
                    .ToArray());
            foreach (var group in opportunities.Where(item => item.Value.Length > 0)
                         .GroupBy(item => FairDistributionRuleModelBuilder.OpportunityCount(
                             item.Value,
                             subject)))
            {
                Guid[] employeeIds = group.Select(item => item.Key).Order().ToArray();
                if (employeeIds.Length < 2)
                {
                    continue;
                }

                int[] counts = employeeIds.Select(employeeId => selected.Count(candidate =>
                    candidate.EmployeeId == employeeId
                    && FairDistributionRuleModelBuilder.Matches(
                        context.Snapshot,
                        candidate,
                        subject))).ToArray();
                int spread = counts.Max() - counts.Min();
                if (spread > 0)
                {
                    violations.Add(new RuleViolationCase(
                        InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution,
                        $"{subject}:{group.Key}",
                        new RuleViolationMagnitude(spread)));
                }
            }
        }

        RuleViolationSet set = new(violations);
        return new FairDistributionRuleSelectionEvaluation(
            set,
            RuleEvaluationResult.Create(
                InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution.Id,
                set.Count == 0
                    ? RuleEvaluationStatus.Satisfied
                    : RuleEvaluationStatus.Violated,
                NoRuleResultParameters.Instance));
    }
}
