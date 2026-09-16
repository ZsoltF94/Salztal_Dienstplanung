using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class PlanningInputSnapshotBuilder
{
    public static PlanningInputSnapshotBuildResult Build(
        ScheduleDayCommandContext context,
        IEnumerable<PlanningHistoryDayReadItem> historyDays,
        RuleCatalogSnapshot ruleCatalog,
        PlanningRunOptions runOptions)
    {
        PlanningHistoryBuildResult historyResult = CreateHistory(
            context.Draft.Period.StartMonday,
            historyDays);
        if (historyResult.Error is not null)
        {
            return PlanningInputSnapshotBuildResult.Failure(historyResult.Error);
        }

        IReadOnlyList<InvalidServiceManagementAssignment> invalidAssignments =
            ServiceManagementAssignmentCurrentValidator.Validate(context);
        if (invalidAssignments.Count > 0)
        {
            return PlanningInputSnapshotBuildResult.Failure(
                new PreparePlanningInputError(
                    PreparePlanningInputErrorCode.InvalidServiceManagementAssignment,
                    "Mindestens eine Typ1-Einteilung passt nicht mehr zu den aktuellen Eingaben.",
                    invalidAssignments: invalidAssignments));
        }

        PreparePlanningInputError? readinessError = ValidateReadiness(context);
        if (readinessError is not null)
        {
            return PlanningInputSnapshotBuildResult.Failure(readinessError);
        }

        Employee[] activeEmployees = context.ReadData.Availability.Employees
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.Id.Value)
            .ToArray();
        HashSet<EmployeeTypeId> usedTypeIds = activeEmployees
            .Select(employee => employee.EmployeeTypeId)
            .ToHashSet();
        PlanningEmployeeSnapshot[] employees = activeEmployees
            .Select(employee => new PlanningEmployeeSnapshot(
                employee.Id.Value,
                employee.FirstName.Value,
                employee.LastName.Value,
                employee.EmployeeTypeId.Value))
            .ToArray();
        PlanningEmployeeTypeSnapshot[] employeeTypes =
            context.ReadData.Availability.EmployeeTypes
                .Where(type => usedTypeIds.Contains(type.Id))
                .OrderBy(type => type.Id.Value)
                .Select(CreateEmployeeType)
                .ToArray();
        HashSet<Guid> activeEmployeeIds = employees
            .Select(employee => employee.Id)
            .ToHashSet();
        PlanningAvailabilityEntrySnapshot[] entries =
            context.ReadData.Availability.Entries
                .Where(item => activeEmployeeIds.Contains(item.Entry.EmployeeId.Value))
                .Where(item => context.Draft.Period.Contains(item.Entry.Date))
                .OrderBy(item => item.Entry.EmployeeId.Value)
                .ThenBy(item => item.Entry.Date)
                .Select(item => new PlanningAvailabilityEntrySnapshot(
                    item.Entry.EmployeeId.Value,
                    item.Entry.Date,
                    item.Entry.Kind,
                    item.ChangeVersion))
                .ToArray();
        ScheduleAssignmentSnapshot[] serviceManagementAssignments =
            ScheduleSnapshotMapper.CreateAssignments(
                context.Draft.Assignments.Where(assignment =>
                    assignment.Origin == AssignmentOrigin.ServiceManagement));

        PlanningInputSnapshot snapshot = new(
            Guid.NewGuid(),
            context.Draft.Id.Value,
            context.Draft.Version.Value,
            context.Draft.Period.StartMonday,
            context.Draft.Period.EndSunday,
            employees,
            employeeTypes,
            CreateServiceCatalog(context),
            entries,
            ScheduleSnapshotMapper.CreateDemandSlots(
                context.Workspace.DemandSlots,
                context.ReadData.StaffingDemands.ServiceCatalog),
            serviceManagementAssignments,
            ruleCatalog,
            runOptions,
            historyResult.Value!);

        return PlanningInputSnapshotBuildResult.Success(snapshot);
    }

    private static PlanningEmployeeTypeSnapshot CreateEmployeeType(EmployeeType type)
    {
        EmployeeTypePlanningPolicy policy = type.PlanningPolicy;
        return new PlanningEmployeeTypeSnapshot(
            type.Id.Value,
            type.Code.Value,
            type.Name.Value,
            type.WeeklyWorkTarget.Minutes,
            type.AbsencePolicy.AllowsVacationAndSickness,
            type.AbsencePolicy.DayValue?.Minutes,
            policy.Role,
            policy.AllowsAutomaticAssignment,
            policy.RequiresWeeklyManualAssignment,
            policy.PreservesManualAssignmentsOnGeneration,
            policy.ManualSuggestionPriority,
            type.ShiftEligibilities
                .OrderBy(eligibility => eligibility.TargetKind)
                .ThenBy(eligibility =>
                    eligibility.ShiftTypeId?.Value
                    ?? eligibility.ShiftPatternId!.Value)
                .ThenBy(eligibility => eligibility.Mode)
                .ThenBy(eligibility => eligibility.Activation)
                .Select(eligibility => new PlanningEmployeeTypeEligibilitySnapshot(
                    eligibility.TargetKind,
                    eligibility.ShiftTypeId?.Value
                        ?? eligibility.ShiftPatternId!.Value,
                    eligibility.Mode,
                    eligibility.Activation)));
    }

    private static PlanningServiceCatalogSnapshot CreateServiceCatalog(
        ScheduleDayCommandContext context)
    {
        var catalog = context.ReadData.StaffingDemands.ServiceCatalog;
        return new PlanningServiceCatalogSnapshot(
            catalog.WorkLocations
                .OrderBy(location => location.Id.Value)
                .Select(location => new PlanningWorkLocationSnapshot(
                    location.Id.Value,
                    location.Name.Value,
                    location.Color.Code)),
            catalog.ShiftTypes
                .OrderBy(shiftType => shiftType.Id.Value)
                .Select(shiftType => new PlanningShiftTypeSnapshot(
                    shiftType.Id.Value,
                    shiftType.Name.Value,
                    shiftType.WorkLocationId.Value,
                    shiftType.Display.Kind,
                    shiftType.Display.Abbreviation,
                    shiftType.StandardTime.Start,
                    shiftType.StandardTime.End)),
            new PlanningSplitShiftPatternSnapshot(
                catalog.SplitShiftPattern.Id.Value,
                catalog.SplitShiftPattern.WorkLocationId.Value,
                catalog.SplitShiftPattern.FirstShiftTypeId.Value,
                catalog.SplitShiftPattern.SecondShiftTypeId.Value,
                catalog.SplitShiftPattern.StandardBreakMinutes,
                catalog.SplitShiftPattern.StandardWorkMinutes),
            new PlanningReliefShiftPatternSnapshot(
                catalog.ReliefShiftPattern.Id.Value,
                catalog.ReliefShiftPattern.AllowedDay,
                catalog.ReliefShiftPattern.FirstWorkLocationId.Value,
                catalog.ReliefShiftPattern.FirstShiftTypeId.Value,
                catalog.ReliefShiftPattern.SecondWorkLocationId.Value,
                catalog.ReliefShiftPattern.SecondShiftTypeId.Value,
                catalog.ReliefShiftPattern.SwitchRule,
                catalog.ReliefShiftPattern.HasInterruption));
    }

    private static PreparePlanningInputError? ValidateReadiness(
        ScheduleDayCommandContext context)
    {
        Employee[] serviceManagementEmployees = context.ReadData.Availability.Employees
            .Where(employee => employee.IsActive)
            .Where(employee =>
                context.Workspace.EmployeeTypes[employee.EmployeeTypeId]
                    .PlanningPolicy.Role == EmployeeTypePlanningRole.ServiceManagement)
            .ToArray();

        foreach (Employee employee in serviceManagementEmployees)
        {
            WeeklyAvailability[] weeks = context.Workspace.WeeklyAvailabilities
                .Where(item => item.Key.EmployeeId == employee.Id)
                .OrderBy(item => item.Key.WeekMonday)
                .Select(item => item.Value)
                .ToArray();
            ServiceManagementReadiness readiness =
                ServiceManagementReadiness.Evaluate(
                    context.Draft,
                    employee.Id,
                    weeks).Value!;
            DateOnly[] missingWeeks = readiness.Weeks
                .Where(week =>
                    week.Status == ServiceManagementWeekReadinessStatus.MissingAssignment)
                .Select(week => week.WeekMonday)
                .ToArray();
            if (missingWeeks.Length > 0)
            {
                return new PreparePlanningInputError(
                    PreparePlanningInputErrorCode.ServiceManagementNotReady,
                    "Typ1 benötigt in jeder nicht vollständig abwesenden Woche mindestens einen vorgetragenen Dienst.",
                    employeeId: employee.Id.Value,
                    weekMondays: missingWeeks);
            }
        }

        return null;
    }

    private static PlanningHistoryBuildResult CreateHistory(
        DateOnly periodMonday,
        IEnumerable<PlanningHistoryDayReadItem> historyDays)
    {
        PlanningHistoryDayReadItem[] supplied = historyDays.ToArray();
        if (supplied.GroupBy(day => day.Date).Any(group => group.Count() > 1)
            || supplied.Any(day =>
                day.Assignments.Any(assignment =>
                    assignment.EmployeeId == Guid.Empty
                    || assignment.WorkMinutes <= 0)
                || day.Assignments.GroupBy(assignment => assignment.EmployeeId)
                    .Any(group => group.Count() > 1)))
        {
            return PlanningHistoryBuildResult.Failure(
                new PreparePlanningInputError(
                    PreparePlanningInputErrorCode.StoredDataInvalid,
                    "Die gespeicherte Vorgeschichte ist widersprüchlich."));
        }

        DateOnly firstDate;
        try
        {
            firstDate = periodMonday.AddDays(-7);
        }
        catch (ArgumentOutOfRangeException)
        {
            return PlanningHistoryBuildResult.Failure(
                new PreparePlanningInputError(
                    PreparePlanningInputErrorCode.PeriodInvalid,
                    "Für diesen Zeitraum können keine sieben vorhergehenden Kalendertage dargestellt werden."));
        }

        DateOnly lastDate = periodMonday.AddDays(-1);
        if (supplied.Any(day => day.Date < firstDate || day.Date > lastDate))
        {
            return PlanningHistoryBuildResult.Failure(
                new PreparePlanningInputError(
                    PreparePlanningInputErrorCode.StoredDataInvalid,
                    "Die geladene Vorgeschichte liegt außerhalb der sieben vorhergehenden Kalendertage."));
        }

        Dictionary<DateOnly, PlanningHistoryDayReadItem> daysByDate =
            supplied.ToDictionary(day => day.Date);
        PlanningHistoryDaySnapshot[] days = Enumerable.Range(0, 7)
            .Select(index => firstDate.AddDays(index))
            .Select(date => daysByDate.TryGetValue(date, out PlanningHistoryDayReadItem? day)
                ? new PlanningHistoryDaySnapshot(
                    date,
                    PlanningHistoryDayStatus.Available,
                    day.Assignments
                        .OrderBy(assignment => assignment.EmployeeId)
                        .Select(assignment => new PlanningHistoryAssignmentSnapshot(
                            assignment.EmployeeId,
                            assignment.WorkMinutes)))
                : new PlanningHistoryDaySnapshot(
                    date,
                    PlanningHistoryDayStatus.Missing,
                    []))
            .ToArray();
        int availableCount = days.Count(day =>
            day.Status == PlanningHistoryDayStatus.Available);
        PlanningHistoryCompleteness completeness = availableCount switch
        {
            0 => PlanningHistoryCompleteness.Missing,
            7 => PlanningHistoryCompleteness.Complete,
            _ => PlanningHistoryCompleteness.Partial,
        };

        return PlanningHistoryBuildResult.Success(
            new PlanningHistorySnapshot(completeness, days));
    }
}

internal sealed record PlanningInputSnapshotBuildResult(
    PlanningInputSnapshot? Value,
    PreparePlanningInputError? Error)
{
    public static PlanningInputSnapshotBuildResult Success(
        PlanningInputSnapshot value) => new(value, null);

    public static PlanningInputSnapshotBuildResult Failure(
        PreparePlanningInputError error) => new(null, error);
}

internal sealed record PlanningHistoryBuildResult(
    PlanningHistorySnapshot? Value,
    PreparePlanningInputError? Error)
{
    public static PlanningHistoryBuildResult Success(
        PlanningHistorySnapshot value) => new(value, null);

    public static PlanningHistoryBuildResult Failure(
        PreparePlanningInputError error) => new(null, error);
}
