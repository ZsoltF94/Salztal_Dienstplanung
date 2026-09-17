using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.Rules.HardRules;

internal static class AutomaticHardRuleTranslators
{
    public static IReadOnlyList<IAutomaticHardRuleTranslator> CreateAll() =>
    [
        new ConstructiveHardRuleTranslator(InitialAutomaticHardRuleDefinitions.ActiveEmployeesOnly),
        new ConstructiveHardRuleTranslator(InitialAutomaticHardRuleDefinitions.ShiftEligibilityRequired),
        new ConstructiveHardRuleTranslator(InitialAutomaticHardRuleDefinitions.ExplicitRunOptionRequired),
        new ConstructiveHardRuleTranslator(InitialAutomaticHardRuleDefinitions.ServiceManagementManualOnly),
        new ConstructiveHardRuleTranslator(InitialAutomaticHardRuleDefinitions.ServiceManagementWeeklyPrerequisite),
        new NormalWeeklyMaximumTranslator(),
        new AuxiliaryWeeklyMaximumTranslator(),
        new MaximumConsecutiveWorkdaysTranslator(),
        new PostVacationWeekendFreeTranslator(),
        new ConstructiveHardRuleTranslator(InitialAutomaticHardRuleDefinitions.AutomaticNoOverstaffing),
        new ReliefShiftEmergencyOnlyTranslator(),
    ];

    private sealed class ConstructiveHardRuleTranslator(RuleDefinition definition)
        : IAutomaticHardRuleTranslator
    {
        public RuleId RuleId => definition.Id;

        public void Apply(HardRulePlanningContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
        }
    }

    private sealed class NormalWeeklyMaximumTranslator : IAutomaticHardRuleTranslator
    {
        public RuleId RuleId => InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum.Id;

        public void Apply(HardRulePlanningContext context)
        {
            EffectiveWeeklyTargetMaximumParameters parameters =
                context.GetParameters<EffectiveWeeklyTargetMaximumParameters>(
                    InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum);
            AddWeeklyLimits(
                context,
                parameters.Role,
                (employeeId, weekMonday) => checked(
                    context.EffectiveWeeklyTargetMinutes(employeeId, weekMonday)
                    + parameters.MaximumMinutesAboveTarget));
        }
    }

    private sealed class AuxiliaryWeeklyMaximumTranslator : IAutomaticHardRuleTranslator
    {
        public RuleId RuleId => InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum.Id;

        public void Apply(HardRulePlanningContext context)
        {
            WeeklyMinutesMaximumParameters parameters =
                context.GetParameters<WeeklyMinutesMaximumParameters>(
                    InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum);
            AddWeeklyLimits(context, parameters.Role, (_, _) => parameters.MaximumMinutes);
        }
    }

    private sealed class MaximumConsecutiveWorkdaysTranslator
        : IAutomaticHardRuleTranslator
    {
        public RuleId RuleId =>
            InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays.Id;

        public void Apply(HardRulePlanningContext context)
        {
            MaximumConsecutiveWorkdaysParameters parameters =
                context.GetParameters<MaximumConsecutiveWorkdaysParameters>(
                    InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays);
            DateOnly firstDate = context.Snapshot.PeriodMonday.AddDays(-parameters.MaximumDays);
            DateOnly lastStart = context.Snapshot.PeriodSunday.AddDays(-parameters.MaximumDays);
            Dictionary<DateOnly, PlanningHistoryDayStatus> historyStatuses =
                context.Snapshot.History.Days.ToDictionary(day => day.Date, day => day.Status);

            foreach (PlanningEmployeeSnapshot employee in context.Snapshot.Employees)
            {
                for (DateOnly start = firstDate; start <= lastStart; start = start.AddDays(1))
                {
                    DateOnly[] dates = Enumerable.Range(0, parameters.MaximumDays + 1)
                        .Select(start.AddDays)
                        .ToArray();
                    if (dates.Any(date => date < context.Snapshot.PeriodMonday
                        && (!historyStatuses.TryGetValue(date, out PlanningHistoryDayStatus status)
                            || status == PlanningHistoryDayStatus.Missing)))
                    {
                        continue;
                    }

                    List<LinearExpr> workTerms = [];
                    int fixedWorkdays = 0;
                    foreach (DateOnly date in dates)
                    {
                        if (date < context.Snapshot.PeriodMonday)
                        {
                            if (context.Snapshot.History.Days.Single(day => day.Date == date)
                                .Assignments.Any(item => item.EmployeeId == employee.Id))
                            {
                                fixedWorkdays++;
                            }

                            continue;
                        }

                        if (context.HasProtectedWork(employee.Id, date))
                        {
                            fixedWorkdays++;
                        }

                        if (context.CandidatesByEmployeeDate.TryGetValue(
                                (employee.Id, date),
                                out IReadOnlyList<PlanningAssignmentCandidate>? candidates)
                            && candidates is not null)
                        {
                            workTerms.Add(LinearExpr.Sum(candidates.Select(candidate =>
                                context.StructuralModel.CandidateVariables[candidate.TechnicalKey])));
                        }
                    }

                    context.StructuralModel.Model.Add(
                        LinearExpr.Sum(workTerms) + fixedWorkdays <= parameters.MaximumDays);
                }
            }
        }
    }

    private sealed class PostVacationWeekendFreeTranslator
        : IAutomaticHardRuleTranslator
    {
        public RuleId RuleId =>
            InitialAutomaticHardRuleDefinitions.PostVacationWeekendFree.Id;

        public void Apply(HardRulePlanningContext context)
        {
            VacationBoundaryWeekendParameters parameters =
                context.GetParameters<VacationBoundaryWeekendParameters>(
                    InitialAutomaticHardRuleDefinitions.PostVacationWeekendFree);
            foreach (IGrouping<Guid, PlanningAvailabilityEntrySnapshot> group in
                     context.Snapshot.AvailabilityEntries
                         .Where(entry => entry.Kind == AvailabilityEntryKind.Vacation)
                         .GroupBy(entry => entry.EmployeeId))
            {
                HashSet<DateOnly> vacationDates = group.Select(entry => entry.Date).ToHashSet();
                foreach (DateOnly boundary in vacationDates.Where(date =>
                             date.DayOfWeek == parameters.BoundaryDay
                             && !vacationDates.Contains(date.AddDays(1))))
                {
                    AddDayBlock(context, group.Key, boundary.AddDays(1));
                    AddDayBlock(context, group.Key, boundary.AddDays(2));
                }
            }
        }
    }

    private sealed class ReliefShiftEmergencyOnlyTranslator
        : IAutomaticHardRuleTranslator
    {
        public RuleId RuleId =>
            InitialAutomaticHardRuleDefinitions.ReliefShiftEmergencyOnly.Id;

        public void Apply(HardRulePlanningContext context)
        {
            foreach (PlanningAssignmentCandidate candidate in
                     context.StructuralModel.CandidateSet.Candidates.Where(candidate =>
                         candidate.Kind == PlanningCandidateKind.ReliefShiftPattern
                         && !context.ReliefShiftEmergencyGate.Allows(candidate)))
            {
                context.StructuralModel.Model.Add(
                    context.StructuralModel.CandidateVariables[candidate.TechnicalKey] == 0);
            }
        }
    }

    private static void AddWeeklyLimits(
        HardRulePlanningContext context,
        EmployeeTypePlanningRole role,
        Func<Guid, DateOnly, int> maximumMinutes)
    {
        foreach (KeyValuePair<
                     (Guid EmployeeId, DateOnly WeekMonday),
                     IReadOnlyList<PlanningAssignmentCandidate>> group in
                 context.CandidatesByEmployeeWeek)
        {
            if (context.GetType(group.Key.EmployeeId).PlanningRole != role)
            {
                continue;
            }

            int fixedMinutes = context.ProtectedWorkMinutes(
                group.Key.EmployeeId,
                group.Key.WeekMonday);
            LinearExpr candidateMinutes = LinearExpr.WeightedSum(
                group.Value.Select(candidate =>
                    context.StructuralModel.CandidateVariables[candidate.TechnicalKey]),
                group.Value.Select(candidate => (long)candidate.WorkMinutes));
            context.StructuralModel.Model.Add(
                candidateMinutes + fixedMinutes <= maximumMinutes(
                    group.Key.EmployeeId,
                    group.Key.WeekMonday));
        }
    }

    private static void AddDayBlock(
        HardRulePlanningContext context,
        Guid employeeId,
        DateOnly date)
    {
        if (context.CandidatesByEmployeeDate.TryGetValue(
                (employeeId, date),
                out IReadOnlyList<PlanningAssignmentCandidate>? candidates)
            && candidates is not null)
        {
            context.StructuralModel.Model.Add(LinearExpr.Sum(candidates.Select(candidate =>
                context.StructuralModel.CandidateVariables[candidate.TechnicalKey])) == 0);
        }
    }
}
