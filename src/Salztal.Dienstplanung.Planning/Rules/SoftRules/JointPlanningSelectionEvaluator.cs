using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal sealed record JointPlanningSelectionEvaluation(
    AuxiliaryWeeklyMinimumObjective AuxiliaryMinimum,
    RelativeWeeklyTargetObjective RelativeWeeklyTarget,
    IReadOnlyList<RuleEvaluationResult> ObjectiveRuleResults,
    IReadOnlyList<RuleEvaluationResult> NoticeResults,
    IReadOnlyDictionary<(Guid EmployeeId, DateOnly WeekMonday), int> WeeklyMinutes);

internal static class JointPlanningSelectionEvaluator
{
    public static JointPlanningSelectionEvaluation Evaluate(
        HardRulePlanningContext context,
        IEnumerable<string> selectedCandidateKeys)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedCandidateKeys);
        HashSet<string> selectedKeys = selectedCandidateKeys.ToHashSet(
            StringComparer.Ordinal);
        PlanningAssignmentCandidate[] selected = context.StructuralModel.CandidateSet
            .Candidates.Where(candidate => selectedKeys.Contains(candidate.TechnicalKey))
            .ToArray();
        WeeklyMinutesMinimumParameters minimumParameters =
            context.GetParameters<WeeklyMinutesMinimumParameters>(
                CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum);
        RelativeWeeklyTargetParameters targetParameters =
            context.GetParameters<RelativeWeeklyTargetParameters>(
                CurrentSoftRuleDefinitions.RelativeWeeklyTarget);
        WeeklyMinutesBelowNoticeParameters lowParameters =
            context.GetParameters<WeeklyMinutesBelowNoticeParameters>(
                InitialNoticeRuleDefinitions.AuxiliaryWeeklyLow);
        WeeklyMinutesRangeNoticeParameters highParameters =
            context.GetParameters<WeeklyMinutesRangeNoticeParameters>(
                InitialNoticeRuleDefinitions.AuxiliaryWeeklyHigh);
        Dictionary<(Guid EmployeeId, DateOnly WeekMonday), int> weeklyMinutes = [];
        List<AuxiliaryWeeklyMinimumCase> minimumCases = [];
        List<RelativeWeeklyTargetCase> relativeCases = [];
        List<EmployeeWeekMinutesResult> lowNotices = [];
        List<EmployeeWeekMinutesResult> highNotices = [];
        int auxiliaryEmployeeCount = 0;

        foreach (var employee in context.Snapshot.Employees.OrderBy(value => value.Id))
        {
            EmployeeTypePlanningRole role = context.GetType(employee.Id).PlanningRole;
            if (role is not EmployeeTypePlanningRole.Normal
                and not EmployeeTypePlanningRole.Auxiliary)
            {
                continue;
            }

            if (role == EmployeeTypePlanningRole.Auxiliary)
            {
                auxiliaryEmployeeCount++;
            }

            for (int weekIndex = 0; weekIndex < 3; weekIndex++)
            {
                DateOnly monday = context.Snapshot.PeriodMonday.AddDays(weekIndex * 7);
                int minutes = context.ProtectedWorkMinutes(employee.Id, monday)
                    + selected.Where(candidate => candidate.EmployeeId == employee.Id
                            && HardRulePlanningContext.WeekMonday(candidate.Date) == monday)
                        .Sum(candidate => candidate.WorkMinutes);
                weeklyMinutes.Add((employee.Id, monday), minutes);
                int target = role == EmployeeTypePlanningRole.Auxiliary
                    ? targetParameters.AuxiliaryTargetMinutes
                    : context.EffectiveWeeklyTargetMinutes(employee.Id, monday);
                relativeCases.Add(new RelativeWeeklyTargetCase(
                    employee.Id,
                    monday,
                    minutes,
                    target));

                if (role != minimumParameters.Role)
                {
                    continue;
                }

                bool hasEligibleDemand = (context.CandidatesByEmployeeWeek
                    .GetValueOrDefault((employee.Id, monday))?.Count ?? 0) > 0;
                minimumCases.Add(new AuxiliaryWeeklyMinimumCase(
                    employee.Id,
                    monday,
                    minutes,
                    hasEligibleDemand));
                EmployeeWeekMinutesResult notice = new(employee.Id, monday, minutes);
                if (minutes < lowParameters.ExclusiveUpperMinutes)
                {
                    lowNotices.Add(notice);
                }

                if (minutes > highParameters.ExclusiveLowerMinutes
                    && minutes <= highParameters.InclusiveUpperMinutes)
                {
                    highNotices.Add(notice);
                }
            }
        }

        AuxiliaryWeeklyMinimumObjective minimum = new(minimumCases);
        RelativeWeeklyTargetObjective relative = new(relativeCases);
        int reliefCount = selected.Count(candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        int splitCount = selected.Count(candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
        bool hasComparableTargets = relative.ComparableCases.Count > 0;
        bool allTargetsMet = relative.ComparableCases.All(value =>
            value.AbsoluteDeviationMinutes == 0);

        return new JointPlanningSelectionEvaluation(
            minimum,
            relative,
            [
                PatternResult(CurrentSoftRuleDefinitions.MinimizeReliefShifts, reliefCount),
                PatternResult(CurrentSoftRuleDefinitions.MinimizeSplitShifts, splitCount),
                RuleEvaluationResult.Create(
                    CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum.Id,
                    minimum.Cases.Count == 0
                        ? RuleEvaluationStatus.NotApplicable
                        : minimum.ViolatedWeekCount == 0
                            ? RuleEvaluationStatus.Satisfied
                            : RuleEvaluationStatus.Violated,
                    NoRuleResultParameters.Instance),
                RuleEvaluationResult.Create(
                    CurrentSoftRuleDefinitions.RelativeWeeklyTarget.Id,
                    !hasComparableTargets
                        ? RuleEvaluationStatus.NotApplicable
                        : allTargetsMet
                            ? RuleEvaluationStatus.Satisfied
                            : RuleEvaluationStatus.Violated,
                    NoRuleResultParameters.Instance),
            ],
            [
                NoticeResult(
                    InitialNoticeRuleDefinitions.AuxiliaryWeeklyLow,
                    auxiliaryEmployeeCount,
                    lowNotices),
                NoticeResult(
                    InitialNoticeRuleDefinitions.AuxiliaryWeeklyHigh,
                    auxiliaryEmployeeCount,
                    highNotices),
            ],
            weeklyMinutes);
    }

    private static RuleEvaluationResult PatternResult(
        RuleDefinition definition,
        int assignmentCount) => RuleEvaluationResult.Create(
        definition.Id,
        assignmentCount == 0
            ? RuleEvaluationStatus.Satisfied
            : RuleEvaluationStatus.Violated,
        NoRuleResultParameters.Instance);

    private static RuleEvaluationResult NoticeResult(
        RuleDefinition definition,
        int auxiliaryEmployeeCount,
        IEnumerable<EmployeeWeekMinutesResult> notices)
    {
        EmployeeWeekMinutesResult[] values = notices.ToArray();
        return RuleEvaluationResult.Create(
            definition.Id,
            auxiliaryEmployeeCount == 0
                ? RuleEvaluationStatus.NotApplicable
                : values.Length == 0
                    ? RuleEvaluationStatus.Satisfied
                    : RuleEvaluationStatus.Violated,
            new WeeklyMinutesNoticeResultParameters(values));
    }
}
