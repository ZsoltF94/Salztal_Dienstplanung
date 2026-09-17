using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.Rules.HardRules;

internal sealed class AutomaticHardRuleSelectionEvaluation
{
    public AutomaticHardRuleSelectionEvaluation(
        IEnumerable<RuleEvaluationResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        RuleEvaluationResult[] values = results.ToArray();
        if (values.Length != InitialAutomaticHardRuleDefinitions.All.Count
            || values.Select(result => result.RuleId).Distinct().Count() != values.Length)
        {
            throw new ArgumentException(
                "Every automatic hard rule must be evaluated exactly once.",
                nameof(results));
        }

        Results = Array.AsReadOnly(values);
    }

    public ReadOnlyCollection<RuleEvaluationResult> Results { get; }

    public bool IsValid => Results.All(result =>
        result.Status != RuleEvaluationStatus.Violated);
}

internal static class AutomaticHardRuleSelectionEvaluator
{
    public static AutomaticHardRuleSelectionEvaluation Evaluate(
        HardRulePlanningContext context,
        IEnumerable<string> selectedCandidateKeys)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(selectedCandidateKeys);
        string[] keys = selectedCandidateKeys.ToArray();
        if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
        {
            throw new ArgumentException(
                "Selected candidate keys must be unique.",
                nameof(selectedCandidateKeys));
        }

        Dictionary<string, PlanningAssignmentCandidate> available =
            context.StructuralModel.CandidateSet.Candidates.ToDictionary(
                candidate => candidate.TechnicalKey,
                StringComparer.Ordinal);
        if (keys.Any(key => !available.ContainsKey(key)))
        {
            throw new ArgumentException(
                "Every selected candidate key must exist in the candidate set.",
                nameof(selectedCandidateKeys));
        }

        PlanningAssignmentCandidate[] selected = keys
            .Select(key => available[key])
            .ToArray();
        return new AutomaticHardRuleSelectionEvaluation(
        [
            Result(
                InitialAutomaticHardRuleDefinitions.ActiveEmployeesOnly,
                EvaluateActiveEmployees(context, selected)),
            Result(
                InitialAutomaticHardRuleDefinitions.ShiftEligibilityRequired,
                EvaluateEligibility(context, selected, requireExplicitOption: false)),
            Result(
                InitialAutomaticHardRuleDefinitions.ExplicitRunOptionRequired,
                EvaluateEligibility(context, selected, requireExplicitOption: true)),
            Result(
                InitialAutomaticHardRuleDefinitions.ServiceManagementManualOnly,
                EvaluateServiceManagementManualOnly(context, selected)),
            Result(
                InitialAutomaticHardRuleDefinitions.ServiceManagementWeeklyPrerequisite,
                EvaluateServiceManagementPrerequisite(context)),
            Result(
                InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum,
                EvaluateWeeklyMaximum(context, selected, auxiliary: false)),
            Result(
                InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum,
                EvaluateWeeklyMaximum(context, selected, auxiliary: true)),
            Result(
                InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays,
                EvaluateConsecutiveWorkdays(context, selected)),
            Result(
                InitialAutomaticHardRuleDefinitions.PostVacationWeekendFree,
                EvaluatePostVacationWeekend(context, selected)),
            Result(
                InitialAutomaticHardRuleDefinitions.AutomaticNoOverstaffing,
                EvaluateOverstaffing(context, selected)),
            Result(
                InitialAutomaticHardRuleDefinitions.ReliefShiftEmergencyOnly,
                EvaluateReliefEmergency(context, selected)),
        ]);
    }

    private static RuleEvaluationResult Result(
        RuleDefinition definition,
        RuleEvaluationStatus status) => RuleEvaluationResult.Create(
            definition.Id,
            status,
            NoRuleResultParameters.Instance);

    private static RuleEvaluationStatus EvaluateActiveEmployees(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected)
    {
        if (selected.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        HashSet<Guid> employeeIds = context.Snapshot.Employees
            .Select(employee => employee.Id)
            .ToHashSet();
        return selected.All(candidate => employeeIds.Contains(candidate.EmployeeId))
            ? RuleEvaluationStatus.Satisfied
            : RuleEvaluationStatus.Violated;
    }

    private static RuleEvaluationStatus EvaluateEligibility(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        bool requireExplicitOption)
    {
        PlanningAssignmentCandidate[] applicable = selected
            .Where(candidate => requireExplicitOption
                ? CandidateUsesExplicitActivation(context, candidate)
                : true)
            .ToArray();
        if (applicable.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        return applicable.All(candidate => CandidateEligibilityIsValid(
            context,
            candidate,
            requireExplicitOption))
            ? RuleEvaluationStatus.Satisfied
            : RuleEvaluationStatus.Violated;
    }

    private static bool CandidateEligibilityIsValid(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate candidate,
        bool requireExplicitOption)
    {
        PlanningEmployeeTypeSnapshot type = context.GetType(candidate.EmployeeId);
        if (requireExplicitOption)
        {
            PlanningEmployeeTypeEligibilitySnapshot? patternEligibility = type.Eligibilities
                .SingleOrDefault(eligibility =>
                    eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
                    && eligibility.TargetId == candidate.PatternId
                    && eligibility.Mode == ShiftEligibilityMode.Regular);
            return patternEligibility?.Activation
                    == ShiftEligibilityActivation.ExplicitPlanningRunOption
                && context.Snapshot.RunOptions.EnableAuxiliaryReliefShift;
        }

        if (!type.AllowsAutomaticAssignment)
        {
            return false;
        }

        bool segmentsAreEligible = candidate.Segments.All(segment =>
            type.Eligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType
                && eligibility.TargetId == segment.AnchorDemand.ShiftTypeId
                && eligibility.Mode == ShiftEligibilityMode.Regular
                && eligibility.Activation == ShiftEligibilityActivation.Always));
        bool alwaysPatternEligibility = candidate.PatternId is not null
            && type.Eligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
                && eligibility.TargetId == candidate.PatternId
                && eligibility.Mode == ShiftEligibilityMode.Regular
                && eligibility.Activation == ShiftEligibilityActivation.Always);
        bool enabledReliefPatternEligibility = alwaysPatternEligibility
            || candidate.PatternId is not null
            && context.Snapshot.RunOptions.EnableAuxiliaryReliefShift
            && type.Eligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
                && eligibility.TargetId == candidate.PatternId
                && eligibility.Mode == ShiftEligibilityMode.Regular
                && eligibility.Activation
                    == ShiftEligibilityActivation.ExplicitPlanningRunOption);
        return candidate.Kind switch
        {
            PlanningCandidateKind.NormalDemand => segmentsAreEligible,
            PlanningCandidateKind.SplitShiftPattern => alwaysPatternEligibility,
            PlanningCandidateKind.ReliefShiftPattern =>
                segmentsAreEligible && enabledReliefPatternEligibility,
            _ => false,
        };
    }

    private static bool CandidateUsesExplicitActivation(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate candidate)
    {
        if (candidate.Kind != PlanningCandidateKind.ReliefShiftPattern)
        {
            return false;
        }

        return context.GetType(candidate.EmployeeId).Eligibilities.Any(eligibility =>
            eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
            && eligibility.TargetId == candidate.PatternId
            && eligibility.Mode == ShiftEligibilityMode.Regular
            && eligibility.Activation
                == ShiftEligibilityActivation.ExplicitPlanningRunOption);
    }

    private static RuleEvaluationStatus EvaluateServiceManagementManualOnly(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected)
    {
        if (selected.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        return selected.Any(candidate => context.GetType(candidate.EmployeeId).PlanningRole
                == EmployeeTypePlanningRole.ServiceManagement)
            ? RuleEvaluationStatus.Violated
            : RuleEvaluationStatus.Satisfied;
    }

    private static RuleEvaluationStatus EvaluateServiceManagementPrerequisite(
        HardRulePlanningContext context)
    {
        Guid[] serviceEmployees = context.Snapshot.Employees
            .Where(employee => context.GetType(employee.Id).PlanningRole
                == EmployeeTypePlanningRole.ServiceManagement)
            .Select(employee => employee.Id)
            .ToArray();
        if (serviceEmployees.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        foreach (Guid employeeId in serviceEmployees)
        {
            for (int weekIndex = 0; weekIndex < 3; weekIndex++)
            {
                DateOnly monday = context.Snapshot.PeriodMonday.AddDays(weekIndex * 7);
                bool fullyUnavailable = Enumerable.Range(0, 7)
                    .Select(monday.AddDays)
                    .All(date => context.Snapshot.AvailabilityEntries.Any(entry =>
                        entry.EmployeeId == employeeId && entry.Date == date));
                bool hasProtectedAssignment = context.Snapshot.ServiceManagementAssignments.Any(
                    assignment => assignment.EmployeeId == employeeId
                        && assignment.Date >= monday
                        && assignment.Date <= monday.AddDays(6));
                if (!fullyUnavailable && !hasProtectedAssignment)
                {
                    return RuleEvaluationStatus.Violated;
                }
            }
        }

        return RuleEvaluationStatus.Satisfied;
    }

    private static RuleEvaluationStatus EvaluateWeeklyMaximum(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected,
        bool auxiliary)
    {
        EmployeeTypePlanningRole role = auxiliary
            ? EmployeeTypePlanningRole.Auxiliary
            : EmployeeTypePlanningRole.Normal;
        PlanningAssignmentCandidate[] applicable = selected.Where(candidate =>
            context.GetType(candidate.EmployeeId).PlanningRole == role).ToArray();
        if (applicable.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        foreach (IGrouping<(Guid EmployeeId, DateOnly Monday), PlanningAssignmentCandidate>
                 group in applicable.GroupBy(candidate => (
                     candidate.EmployeeId,
                     HardRulePlanningContext.WeekMonday(candidate.Date))))
        {
            int maximum = auxiliary
                ? context.GetParameters<WeeklyMinutesMaximumParameters>(
                    InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum).MaximumMinutes
                : checked(context.EffectiveWeeklyTargetMinutes(
                        group.Key.EmployeeId,
                        group.Key.Monday)
                    + context.GetParameters<EffectiveWeeklyTargetMaximumParameters>(
                        InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum)
                        .MaximumMinutesAboveTarget);
            int minutes = context.ProtectedWorkMinutes(
                    group.Key.EmployeeId,
                    group.Key.Monday)
                + group.Sum(candidate => candidate.WorkMinutes);
            if (minutes > maximum)
            {
                return RuleEvaluationStatus.Violated;
            }
        }

        return RuleEvaluationStatus.Satisfied;
    }

    private static RuleEvaluationStatus EvaluateConsecutiveWorkdays(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected)
    {
        bool hasWork = selected.Length > 0
            || context.Snapshot.ServiceManagementAssignments.Count > 0
            || context.Snapshot.History.Days.Any(day => day.Assignments.Count > 0);
        if (!hasWork)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        MaximumConsecutiveWorkdaysParameters parameters =
            context.GetParameters<MaximumConsecutiveWorkdaysParameters>(
                InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays);
        foreach (PlanningEmployeeSnapshot employee in context.Snapshot.Employees)
        {
            HashSet<DateOnly> workingDates = context.Snapshot.History.Days
                .Where(day => day.Status == PlanningHistoryDayStatus.Available)
                .Where(day => day.Assignments.Any(item => item.EmployeeId == employee.Id))
                .Select(day => day.Date)
                .Concat(context.Snapshot.ServiceManagementAssignments
                    .Where(assignment => assignment.EmployeeId == employee.Id)
                    .Select(assignment => assignment.Date))
                .Concat(selected
                    .Where(candidate => candidate.EmployeeId == employee.Id)
                    .Select(candidate => candidate.Date))
                .ToHashSet();
            int run = 0;
            for (DateOnly date = context.Snapshot.PeriodMonday.AddDays(-parameters.MaximumDays);
                 date <= context.Snapshot.PeriodSunday;
                 date = date.AddDays(1))
            {
                PlanningHistoryDaySnapshot? historyDay = context.Snapshot.History.Days
                    .SingleOrDefault(day => day.Date == date);
                if (historyDay?.Status == PlanningHistoryDayStatus.Missing)
                {
                    run = 0;
                    continue;
                }

                run = workingDates.Contains(date) ? run + 1 : 0;
                if (run > parameters.MaximumDays)
                {
                    return RuleEvaluationStatus.Violated;
                }
            }
        }

        return context.Snapshot.History.Completeness == PlanningHistoryCompleteness.Complete
            ? RuleEvaluationStatus.Satisfied
            : RuleEvaluationStatus.NotFullyEvaluable;
    }

    private static RuleEvaluationStatus EvaluatePostVacationWeekend(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected)
    {
        bool applicable = false;
        foreach (IGrouping<Guid, PlanningAvailabilityEntrySnapshot> group in
                 context.Snapshot.AvailabilityEntries
                     .Where(entry => entry.Kind == AvailabilityEntryKind.Vacation)
                     .GroupBy(entry => entry.EmployeeId))
        {
            HashSet<DateOnly> dates = group.Select(entry => entry.Date).ToHashSet();
            foreach (DateOnly friday in dates.Where(date =>
                         date.DayOfWeek == DayOfWeek.Friday
                         && !dates.Contains(date.AddDays(1))))
            {
                applicable = true;
                if (selected.Any(candidate => candidate.EmployeeId == group.Key
                        && candidate.Date > friday
                        && candidate.Date <= friday.AddDays(2)))
                {
                    return RuleEvaluationStatus.Violated;
                }
            }
        }

        return applicable
            ? RuleEvaluationStatus.Satisfied
            : RuleEvaluationStatus.NotApplicable;
    }

    private static RuleEvaluationStatus EvaluateOverstaffing(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected)
    {
        if (selected.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        bool overlap = selected.SelectMany(candidate => candidate.Coverages)
            .GroupBy(coverage => coverage.Demand)
            .Any(group => group.Any(first => group.Any(second =>
                !ReferenceEquals(first, second)
                && first.CoveredStart < second.CoveredEnd
                && second.CoveredStart < first.CoveredEnd)));
        return overlap
            ? RuleEvaluationStatus.Violated
            : RuleEvaluationStatus.Satisfied;
    }

    private static RuleEvaluationStatus EvaluateReliefEmergency(
        HardRulePlanningContext context,
        PlanningAssignmentCandidate[] selected)
    {
        PlanningAssignmentCandidate[] relief = selected.Where(candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern).ToArray();
        if (relief.Length == 0)
        {
            return RuleEvaluationStatus.NotApplicable;
        }

        return relief.All(context.ReliefShiftEmergencyGate.Allows)
            ? RuleEvaluationStatus.Satisfied
            : RuleEvaluationStatus.Violated;
    }
}
