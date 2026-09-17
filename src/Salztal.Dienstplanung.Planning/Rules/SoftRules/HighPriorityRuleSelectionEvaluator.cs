using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal sealed record HighPriorityRuleSelectionEvaluation(
    RuleViolationSet Violations,
    IReadOnlyList<RuleEvaluationResult> Results);

internal static class HighPriorityRuleSelectionEvaluator
{
    public static HighPriorityRuleSelectionEvaluation Evaluate(
        HardRulePlanningContext context,
        IEnumerable<string> selectedCandidateKeys)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedCandidateKeys);
        HashSet<string> selectedKeys = selectedCandidateKeys.ToHashSet(StringComparer.Ordinal);
        PlanningAssignmentCandidate[] selected = context.StructuralModel.CandidateSet.Candidates
            .Where(candidate => selectedKeys.Contains(candidate.TechnicalKey))
            .ToArray();
        List<RuleViolationCase> violations = [];
        EvaluateNormalWeeklyMinimum(context, selected, violations);
        EvaluateConsecutiveDaysOff(context, selected, violations);
        EvaluateGuaranteedDayOffAdjacency(context, selected, violations);
        EvaluatePreVacationWeekend(context, selected, violations);
        EvaluateSplitShiftMaximum(context, selected, violations);
        RuleViolationSet violationSet = new(violations);
        return new HighPriorityRuleSelectionEvaluation(
            violationSet,
            CreateResults(context, violationSet));
    }

    private static void EvaluateNormalWeeklyMinimum(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        List<RuleViolationCase> violations)
    {
        EffectiveWeeklyTargetMinimumParameters parameters =
            context.GetParameters<EffectiveWeeklyTargetMinimumParameters>(
                InitialSoftRuleDefinitions.NormalWeeklyMinimum);
        foreach ((PlanningEmployeeSnapshot employee, DateOnly monday) in EmployeeWeeks(context))
        {
            if (context.GetType(employee.Id).PlanningRole != parameters.Role)
            {
                continue;
            }

            int minimum = Math.Max(
                0,
                context.EffectiveWeeklyTargetMinutes(employee.Id, monday)
                - parameters.MinimumMinutesBelowTarget);
            int work = context.ProtectedWorkMinutes(employee.Id, monday)
                + selected.Where(candidate =>
                    candidate.EmployeeId == employee.Id
                    && candidate.Date >= monday
                    && candidate.Date <= monday.AddDays(6))
                    .Sum(candidate => candidate.WorkMinutes);
            AddViolation(
                violations,
                InitialSoftRuleDefinitions.NormalWeeklyMinimum,
                EmployeeWeekKey(employee.Id, monday),
                Math.Max(0, minimum - work));
        }
    }

    private static void EvaluateConsecutiveDaysOff(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        List<RuleViolationCase> violations)
    {
        foreach ((PlanningEmployeeSnapshot employee, DateOnly monday) in EmployeeWeeks(context))
        {
            bool[] daysOff = Enumerable.Range(0, 7)
                .Select(index => IsRegularDayOff(
                    context,
                    selected,
                    employee.Id,
                    monday.AddDays(index)))
                .ToArray();
            bool hasPair = daysOff.Zip(daysOff.Skip(1)).Any(pair => pair.First && pair.Second);
            AddViolation(
                violations,
                InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff,
                EmployeeWeekKey(employee.Id, monday),
                hasPair ? 0 : 1);
        }
    }

    private static void EvaluateGuaranteedDayOffAdjacency(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        List<RuleViolationCase> violations)
    {
        HashSet<(Guid EmployeeId, DateOnly Date)> redDays = context.Snapshot
            .AvailabilityEntries
            .Where(entry => entry.Kind == AvailabilityEntryKind.FixedDayOff)
            .Select(entry => (entry.EmployeeId, entry.Date))
            .ToHashSet();
        foreach ((PlanningEmployeeSnapshot employee, DateOnly monday) in EmployeeWeeks(context))
        {
            DateOnly[] anchors = Enumerable.Range(0, 7)
                .Select(monday.AddDays)
                .Where(date => redDays.Contains((employee.Id, date)))
                .ToArray();
            int isolated = anchors.Count(anchor =>
                !(anchor > monday
                    && IsRegularDayOff(context, selected, employee.Id, anchor.AddDays(-1)))
                && !(anchor < monday.AddDays(6)
                    && IsRegularDayOff(context, selected, employee.Id, anchor.AddDays(1))));
            AddViolation(
                violations,
                InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff,
                EmployeeWeekKey(employee.Id, monday),
                isolated);
        }
    }

    private static void EvaluatePreVacationWeekend(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        List<RuleViolationCase> violations)
    {
        foreach (IGrouping<Guid, PlanningAvailabilityEntrySnapshot> group in context.Snapshot
                     .AvailabilityEntries
                     .Where(entry => entry.Kind == AvailabilityEntryKind.Vacation)
                     .GroupBy(entry => entry.EmployeeId))
        {
            HashSet<DateOnly> vacationDates = group.Select(entry => entry.Date).ToHashSet();
            foreach (DateOnly monday in vacationDates.Where(date =>
                         date.DayOfWeek == DayOfWeek.Monday
                         && !vacationDates.Contains(date.AddDays(-1))))
            {
                DateOnly saturday = monday.AddDays(-2);
                DateOnly sunday = monday.AddDays(-1);
                if (!TryIsWorking(context, selected, group.Key, saturday, out bool first)
                    || !TryIsWorking(context, selected, group.Key, sunday, out bool second))
                {
                    continue;
                }

                AddViolation(
                    violations,
                    InitialSoftRuleDefinitions.PreVacationWeekendFree,
                    $"{group.Key:N}:{saturday:yyyyMMdd}",
                    (first ? 1 : 0) + (second ? 1 : 0));
            }
        }
    }

    private static void EvaluateSplitShiftMaximum(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        List<RuleViolationCase> violations)
    {
        SplitShiftWeeklyMaximumParameters parameters =
            context.GetParameters<SplitShiftWeeklyMaximumParameters>(
                InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum);
        foreach ((PlanningEmployeeSnapshot employee, DateOnly monday) in EmployeeWeeks(context))
        {
            int splitCount = selected.Count(candidate =>
                candidate.EmployeeId == employee.Id
                && candidate.Date >= monday
                && candidate.Date <= monday.AddDays(6)
                && candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
            AddViolation(
                violations,
                InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum,
                EmployeeWeekKey(employee.Id, monday),
                Math.Max(0, splitCount - parameters.MaximumAssignments));
        }
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

    private static bool TryIsWorking(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        Guid employeeId,
        DateOnly date,
        out bool isWorking)
    {
        if (date >= context.Snapshot.PeriodMonday)
        {
            isWorking = context.HasProtectedWork(employeeId, date)
                || selected.Any(candidate =>
                    candidate.EmployeeId == employeeId && candidate.Date == date);
            return true;
        }

        PlanningHistoryDaySnapshot? history = context.Snapshot.History.Days
            .SingleOrDefault(day => day.Date == date);
        if (history is null || history.Status == PlanningHistoryDayStatus.Missing)
        {
            isWorking = false;
            return false;
        }

        isWorking = history.Assignments.Any(item => item.EmployeeId == employeeId);
        return true;
    }

    private static RuleEvaluationResult[] CreateResults(
        HardRulePlanningContext context,
        RuleViolationSet violations)
    {
        HashSet<RuleId> violatedIds = violations.Violations
            .Select(item => item.RuleId)
            .ToHashSet();
        bool hasNormalEmployee = context.Snapshot.Employees.Any(employee =>
            context.GetType(employee.Id).PlanningRole == EmployeeTypePlanningRole.Normal);
        bool hasRedDay = context.Snapshot.AvailabilityEntries.Any(entry =>
            entry.Kind == AvailabilityEntryKind.FixedDayOff);
        (bool HasBoundary, bool HasUnknownBoundary) vacation = VacationBoundaryState(context);
        return InitialSoftRuleDefinitions.All
            .Where(rule => rule.Priority == RulePriority.High)
            .Select(rule => RuleEvaluationResult.Create(
                rule.Id,
                violatedIds.Contains(rule.Id)
                    ? RuleEvaluationStatus.Violated
                    : ApplicableStatus(
                        rule,
                        hasNormalEmployee,
                        hasRedDay,
                        vacation),
                NoRuleResultParameters.Instance))
            .ToArray();
    }

    private static RuleEvaluationStatus ApplicableStatus(
        RuleDefinition rule,
        bool hasNormalEmployee,
        bool hasRedDay,
        (bool HasBoundary, bool HasUnknownBoundary) vacation) => rule.Id.Value switch
        {
            "NORMAL_WEEKLY_MINIMUM" => hasNormalEmployee
                ? RuleEvaluationStatus.Satisfied
                : RuleEvaluationStatus.NotApplicable,
            "RED_X_ADJACENT_DAY_OFF" => hasRedDay
                ? RuleEvaluationStatus.Satisfied
                : RuleEvaluationStatus.NotApplicable,
            "PRE_VACATION_WEEKEND_FREE" => vacation.HasUnknownBoundary
                ? RuleEvaluationStatus.NotFullyEvaluable
                : vacation.HasBoundary
                    ? RuleEvaluationStatus.Satisfied
                    : RuleEvaluationStatus.NotApplicable,
            _ => RuleEvaluationStatus.Satisfied,
        };

    private static (bool HasBoundary, bool HasUnknownBoundary) VacationBoundaryState(
        HardRulePlanningContext context)
    {
        bool hasBoundary = false;
        bool hasUnknown = false;
        foreach (IGrouping<Guid, PlanningAvailabilityEntrySnapshot> group in context.Snapshot
                     .AvailabilityEntries
                     .Where(entry => entry.Kind == AvailabilityEntryKind.Vacation)
                     .GroupBy(entry => entry.EmployeeId))
        {
            HashSet<DateOnly> dates = group.Select(entry => entry.Date).ToHashSet();
            foreach (DateOnly monday in dates.Where(date =>
                         date.DayOfWeek == DayOfWeek.Monday
                         && !dates.Contains(date.AddDays(-1))))
            {
                hasBoundary = true;
                if (monday == context.Snapshot.PeriodMonday
                    && Enumerable.Range(1, 2).Any(daysBefore =>
                        context.Snapshot.History.Days.SingleOrDefault(day =>
                            day.Date == monday.AddDays(-daysBefore))?.Status
                        != PlanningHistoryDayStatus.Available))
                {
                    hasUnknown = true;
                }
            }
        }

        return (hasBoundary, hasUnknown);
    }

    private static void AddViolation(
        List<RuleViolationCase> violations,
        RuleDefinition rule,
        string caseKey,
        int magnitude)
    {
        if (magnitude > 0)
        {
            violations.Add(new RuleViolationCase(
                rule,
                caseKey,
                new RuleViolationMagnitude(magnitude)));
        }
    }

    private static IEnumerable<(PlanningEmployeeSnapshot Employee, DateOnly Monday)>
        EmployeeWeeks(HardRulePlanningContext context) => context.Snapshot.Employees
            .SelectMany(employee => Enumerable.Range(0, 3).Select(index =>
                (employee, context.Snapshot.PeriodMonday.AddDays(index * 7))));

    private static string EmployeeWeekKey(Guid employeeId, DateOnly monday) =>
        $"{employeeId:N}:{monday:yyyyMMdd}";
}
