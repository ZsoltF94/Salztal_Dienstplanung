using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal sealed record MediumPriorityRuleSelectionEvaluation(
    RuleViolationSet Violations,
    IReadOnlyList<RuleEvaluationResult> Results);

internal static class MediumPriorityRuleSelectionEvaluator
{
    public static MediumPriorityRuleSelectionEvaluation Evaluate(
        HardRulePlanningContext context,
        IEnumerable<string> selectedCandidateKeys)
    {
        HashSet<string> keys = selectedCandidateKeys.ToHashSet(StringComparer.Ordinal);
        PlanningAssignmentCandidate[] selected = context.StructuralModel.CandidateSet.Candidates
            .Where(candidate => keys.Contains(candidate.TechnicalKey))
            .ToArray();
        List<RuleViolationCase> violations = [];
        foreach (var employee in context.Snapshot.Employees)
        {
            int freeWeekends = Enumerable.Range(0, 3).Count(index =>
            {
                DateOnly monday = context.Snapshot.PeriodMonday.AddDays(index * 7);
                return IsRegularDayOff(context, selected, employee.Id, monday.AddDays(5))
                    && IsRegularDayOff(context, selected, employee.Id, monday.AddDays(6));
            });
            if (freeWeekends == 0)
            {
                violations.Add(new RuleViolationCase(
                    CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend,
                    employee.Id.ToString("N"),
                    new RuleViolationMagnitude(1)));
            }
        }

        RuleViolationSet set = new(violations);
        RuleEvaluationResult[] results =
        [
            Result(CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend, set),
        ];
        return new MediumPriorityRuleSelectionEvaluation(set, results);
    }

    private static bool IsRegularDayOff(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        Guid employeeId,
        DateOnly date)
    {
        AvailabilityEntryKind? marker = context.Snapshot.AvailabilityEntries
            .Where(entry => entry.EmployeeId == employeeId && entry.Date == date)
            .Select(entry => (AvailabilityEntryKind?)entry.Kind)
            .SingleOrDefault();
        return marker == AvailabilityEntryKind.FixedDayOff
            || (marker is null
                && !context.HasProtectedWork(employeeId, date)
                && !selected.Any(candidate =>
                    candidate.EmployeeId == employeeId && candidate.Date == date));
    }

    private static RuleEvaluationResult Result(
        RuleDefinition rule,
        RuleViolationSet violations) => RuleEvaluationResult.Create(
        rule.Id,
        violations.Violations.Any(item => item.RuleId == rule.Id)
            ? RuleEvaluationStatus.Violated
            : RuleEvaluationStatus.Satisfied,
        NoRuleResultParameters.Instance);
}
