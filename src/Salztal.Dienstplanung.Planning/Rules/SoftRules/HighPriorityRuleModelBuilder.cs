using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal static class HighPriorityRuleModelBuilder
{
    public static HighPriorityRuleModel Apply(HardRulePlanningContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        List<HighPriorityRuleCaseModel> cases = [];
        AddNormalWeeklyMinimum(context, cases);
        AddWeeklyConsecutiveDaysOff(context, cases);
        AddGuaranteedDayOffAdjacency(context, cases);
        AddPreVacationWeekendFree(context, cases);
        AddSplitShiftWeeklyMaximum(context, cases);
        return new HighPriorityRuleModel(cases);
    }

    private static void AddNormalWeeklyMinimum(
        HardRulePlanningContext context,
        List<HighPriorityRuleCaseModel> cases)
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
            LinearExpr work = CandidateMinutes(context, employee.Id, monday)
                + context.ProtectedWorkMinutes(employee.Id, monday);
            AddPositiveMagnitudeCase(
                context,
                cases,
                InitialSoftRuleDefinitions.NormalWeeklyMinimum,
                EmployeeWeekKey(employee.Id, monday),
                minimum,
                minimum - work);
        }
    }

    private static void AddWeeklyConsecutiveDaysOff(
        HardRulePlanningContext context,
        List<HighPriorityRuleCaseModel> cases)
    {
        WeeklyConsecutiveDaysOffParameters parameters =
            context.GetParameters<WeeklyConsecutiveDaysOffParameters>(
                InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff);
        if (parameters.MinimumConsecutiveDays != 2)
        {
            throw new InvalidOperationException(
                "The initial consecutive-days-off translator supports a two-day block.");
        }

        foreach ((PlanningEmployeeSnapshot employee, DateOnly monday) in EmployeeWeeks(context))
        {
            BoolVar[] daysOff = Enumerable.Range(0, 7)
                .Select(index => DayOffVariable(context, employee.Id, monday.AddDays(index)))
                .ToArray();
            BoolVar[] pairs = Enumerable.Range(0, 6)
                .Select(index => And(
                    context,
                    daysOff[index],
                    daysOff[index + 1],
                    $"off-pair-{employee.Id:N}-{monday:yyyyMMdd}-{index}"))
                .ToArray();
            BoolVar hasPair = context.StructuralModel.Model.NewBoolVar(
                $"has-off-pair-{employee.Id:N}-{monday:yyyyMMdd}");
            context.StructuralModel.Model.AddMaxEquality(hasPair, pairs);
            BoolVar violation = context.StructuralModel.Model.NewBoolVar(
                $"violated-off-pair-{employee.Id:N}-{monday:yyyyMMdd}");
            context.StructuralModel.Model.Add(violation + hasPair == 1);
            cases.Add(new HighPriorityRuleCaseModel(
                InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff,
                EmployeeWeekKey(employee.Id, monday),
                violation,
                violation));
        }
    }

    private static void AddGuaranteedDayOffAdjacency(
        HardRulePlanningContext context,
        List<HighPriorityRuleCaseModel> cases)
    {
        GuaranteedDayOffAdjacencyParameters parameters =
            context.GetParameters<GuaranteedDayOffAdjacencyParameters>(
                InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff);
        if (parameters.MinimumBlockLength != 2)
        {
            throw new InvalidOperationException(
                "The initial guaranteed-day-off translator supports a two-day block.");
        }

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
            if (anchors.Length == 0)
            {
                continue;
            }

            List<BoolVar> isolated = [];
            foreach (DateOnly anchor in anchors)
            {
                List<BoolVar> adjacent = [];
                if (anchor > monday)
                {
                    adjacent.Add(DayOffVariable(context, employee.Id, anchor.AddDays(-1)));
                }

                if (anchor < monday.AddDays(6))
                {
                    adjacent.Add(DayOffVariable(context, employee.Id, anchor.AddDays(1)));
                }

                BoolVar hasAdjacent = context.StructuralModel.Model.NewBoolVar(
                    $"red-x-adjacent-{employee.Id:N}-{anchor:yyyyMMdd}");
                context.StructuralModel.Model.AddMaxEquality(hasAdjacent, adjacent);
                BoolVar isIsolated = context.StructuralModel.Model.NewBoolVar(
                    $"red-x-isolated-{employee.Id:N}-{anchor:yyyyMMdd}");
                context.StructuralModel.Model.Add(isIsolated + hasAdjacent == 1);
                isolated.Add(isIsolated);
            }

            AddPositiveMagnitudeCase(
                context,
                cases,
                InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff,
                EmployeeWeekKey(employee.Id, monday),
                isolated.Count,
                LinearExpr.Sum(isolated));
        }
    }

    private static void AddPreVacationWeekendFree(
        HardRulePlanningContext context,
        List<HighPriorityRuleCaseModel> cases)
    {
        VacationBoundaryWeekendParameters parameters =
            context.GetParameters<VacationBoundaryWeekendParameters>(
                InitialSoftRuleDefinitions.PreVacationWeekendFree);
        foreach (IGrouping<Guid, PlanningAvailabilityEntrySnapshot> group in context.Snapshot
                     .AvailabilityEntries
                     .Where(entry => entry.Kind == AvailabilityEntryKind.Vacation)
                     .GroupBy(entry => entry.EmployeeId))
        {
            HashSet<DateOnly> vacationDates = group.Select(entry => entry.Date).ToHashSet();
            foreach (DateOnly monday in vacationDates.Where(date =>
                         date.DayOfWeek == parameters.BoundaryDay
                         && !vacationDates.Contains(date.AddDays(-1))))
            {
                DateOnly saturday = monday.AddDays(-2);
                DateOnly sunday = monday.AddDays(-1);
                if (!TryWorkExpression(context, group.Key, saturday, out LinearExpr? first)
                    || !TryWorkExpression(context, group.Key, sunday, out LinearExpr? second))
                {
                    continue;
                }

                AddPositiveMagnitudeCase(
                    context,
                    cases,
                    InitialSoftRuleDefinitions.PreVacationWeekendFree,
                    $"{group.Key:N}:{saturday:yyyyMMdd}",
                    2,
                    first + second);
            }
        }
    }

    private static void AddSplitShiftWeeklyMaximum(
        HardRulePlanningContext context,
        List<HighPriorityRuleCaseModel> cases)
    {
        SplitShiftWeeklyMaximumParameters parameters =
            context.GetParameters<SplitShiftWeeklyMaximumParameters>(
                InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum);
        foreach ((PlanningEmployeeSnapshot employee, DateOnly monday) in EmployeeWeeks(context))
        {
            IReadOnlyList<PlanningAssignmentCandidate> candidates = context
                .CandidatesByEmployeeWeek.GetValueOrDefault((employee.Id, monday)) ?? [];
            LinearExpr splitCount = LinearExpr.Sum(candidates
                .Where(candidate => candidate.Kind == PlanningCandidateKind.SplitShiftPattern)
                .Select(candidate => context.StructuralModel.CandidateVariables[
                    candidate.TechnicalKey]));
            AddPositiveMagnitudeCase(
                context,
                cases,
                InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum,
                EmployeeWeekKey(employee.Id, monday),
                7,
                splitCount - parameters.MaximumAssignments);
        }
    }

    private static void AddPositiveMagnitudeCase(
        HardRulePlanningContext context,
        List<HighPriorityRuleCaseModel> cases,
        RuleDefinition rule,
        string caseKey,
        int maximum,
        LinearExpr rawMagnitude)
    {
        IntVar magnitude = context.StructuralModel.Model.NewIntVar(
            0,
            maximum,
            $"magnitude-{rule.Id.Value}-{caseKey}");
        context.StructuralModel.Model.AddMaxEquality(
            magnitude,
            [rawMagnitude, LinearExpr.Constant(0)]);
        BoolVar violation = context.StructuralModel.Model.NewBoolVar(
            $"violated-{rule.Id.Value}-{caseKey}");
        context.StructuralModel.Model.Add(magnitude >= 1).OnlyEnforceIf(violation);
        context.StructuralModel.Model.Add(magnitude == 0).OnlyEnforceIf(violation.Not());
        cases.Add(new HighPriorityRuleCaseModel(rule, caseKey, violation, magnitude));
    }

    private static BoolVar DayOffVariable(
        HardRulePlanningContext context,
        Guid employeeId,
        DateOnly date)
    {
        AvailabilityEntryKind? marker = context.Snapshot.AvailabilityEntries
            .Where(entry => entry.EmployeeId == employeeId && entry.Date == date)
            .Select(entry => (AvailabilityEntryKind?)entry.Kind)
            .SingleOrDefault();
        BoolVar result = context.StructuralModel.Model.NewBoolVar(
            $"regular-day-off-{employeeId:N}-{date:yyyyMMdd}");
        if (marker == AvailabilityEntryKind.FixedDayOff)
        {
            context.StructuralModel.Model.Add(result == 1);
        }
        else if (marker is AvailabilityEntryKind.Vacation or AvailabilityEntryKind.Sickness
                 || context.HasProtectedWork(employeeId, date))
        {
            context.StructuralModel.Model.Add(result == 0);
        }
        else
        {
            context.StructuralModel.Model.Add(
                result + CandidateWork(context, employeeId, date) == 1);
        }

        return result;
    }

    private static BoolVar And(
        HardRulePlanningContext context,
        BoolVar left,
        BoolVar right,
        string name)
    {
        BoolVar result = context.StructuralModel.Model.NewBoolVar(name);
        context.StructuralModel.Model.Add(result <= left);
        context.StructuralModel.Model.Add(result <= right);
        context.StructuralModel.Model.Add(result >= left + right - 1);
        return result;
    }

    private static bool TryWorkExpression(
        HardRulePlanningContext context,
        Guid employeeId,
        DateOnly date,
        out LinearExpr? work)
    {
        if (date >= context.Snapshot.PeriodMonday)
        {
            work = context.HasProtectedWork(employeeId, date)
                ? LinearExpr.Constant(1)
                : CandidateWork(context, employeeId, date);
            return true;
        }

        PlanningHistoryDaySnapshot? history = context.Snapshot.History.Days
            .SingleOrDefault(day => day.Date == date);
        if (history is null || history.Status == PlanningHistoryDayStatus.Missing)
        {
            work = null;
            return false;
        }

        work = LinearExpr.Constant(history.Assignments.Any(item =>
            item.EmployeeId == employeeId) ? 1 : 0);
        return true;
    }

    private static LinearExpr CandidateWork(
        HardRulePlanningContext context,
        Guid employeeId,
        DateOnly date) => LinearExpr.Sum(
        context.CandidatesByEmployeeDate.GetValueOrDefault((employeeId, date))
            ?.Select(candidate => context.StructuralModel.CandidateVariables[
                candidate.TechnicalKey]) ?? []);

    private static LinearExpr CandidateMinutes(
        HardRulePlanningContext context,
        Guid employeeId,
        DateOnly monday)
    {
        IReadOnlyList<PlanningAssignmentCandidate> candidates = context
            .CandidatesByEmployeeWeek.GetValueOrDefault((employeeId, monday)) ?? [];
        return LinearExpr.WeightedSum(
            candidates.Select(candidate => context.StructuralModel.CandidateVariables[
                candidate.TechnicalKey]),
            candidates.Select(candidate => (long)candidate.WorkMinutes));
    }

    private static IEnumerable<(PlanningEmployeeSnapshot Employee, DateOnly Monday)>
        EmployeeWeeks(HardRulePlanningContext context) => context.Snapshot.Employees
            .SelectMany(employee => Enumerable.Range(0, 3).Select(index =>
                (employee, context.Snapshot.PeriodMonday.AddDays(index * 7))));

    private static string EmployeeWeekKey(Guid employeeId, DateOnly monday) =>
        $"{employeeId:N}:{monday:yyyyMMdd}";
}
