using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

internal static class ScheduleWorkspaceTestContext
{
    public static readonly DateOnly PeriodMonday = new(2026, 9, 14);

    public static readonly Guid ServiceManagementEmployeeId =
        new("65f90c44-d54d-45c0-b244-674272ba89dc");

    public static readonly Guid StandardEmployeeId =
        new("3955858c-9c62-4bd6-afc3-f88a296d6907");

    public static readonly Guid InactiveEmployeeId =
        new("810a8df7-e620-4f3a-9016-cc207826a9ef");

    public static SchedulePeriod Period => Assert.IsType<SchedulePeriod>(
        SchedulePeriod.Create(PeriodMonday).Value);

    public static Employee ServiceManagementEmployee => CreateEmployee(
        ServiceManagementEmployeeId,
        "Sarah",
        "Leitung",
        InitialEmployeeTypeCatalog.Type1,
        true);

    public static Employee StandardEmployee => CreateEmployee(
        StandardEmployeeId,
        "Erika",
        "Muster",
        InitialEmployeeTypeCatalog.Type20,
        true);

    public static Employee InactiveEmployee => CreateEmployee(
        InactiveEmployeeId,
        "Ines",
        "Inaktiv",
        InitialEmployeeTypeCatalog.Type20,
        false);

    public static ServiceCatalogData ServiceCatalog => new(
        InitialWorkLocationCatalog.All,
        InitialShiftTypeCatalog.All,
        InitialShiftPatternCatalog.SplitShift,
        InitialShiftPatternCatalog.ReliefShift);

    public static ScheduleWorkspaceReadData CreateReadData(
        IEnumerable<Employee>? employees = null,
        IEnumerable<EmployeeType>? employeeTypes = null,
        IEnumerable<AvailabilityEntryReadItem>? entries = null,
        IEnumerable<StandardStaffingDemandRevision>? standardRevisions = null,
        IEnumerable<StaffingDemandDateException>? dateExceptions = null,
        ServiceCatalogData? serviceCatalog = null,
        IEnumerable<ScheduleDraftHeader>? draftHeaders = null,
        ScheduleDraft? exactDraft = null,
        IEnumerable<PlanningHistoryDayReadItem>? historyDays = null,
        PlanningInputSnapshot? preparedSnapshot = null)
    {
        return new ScheduleWorkspaceReadData(
            new AvailabilityReadData(
                employees ?? [ServiceManagementEmployee, StandardEmployee, InactiveEmployee],
                employeeTypes ?? InitialEmployeeTypeCatalog.All,
                entries ?? []),
            new StaffingDemandReadData(
                standardRevisions ?? InitialStaffingDemandCatalog.All,
                dateExceptions ?? [],
                serviceCatalog ?? ServiceCatalog),
            draftHeaders ?? [],
            exactDraft,
            historyDays,
            preparedSnapshot);
    }

    public static ScheduleDraft CreateDraft(
        SchedulePeriod? period = null,
        IEnumerable<AvailabilityEntry>? availabilityEntries = null,
        IEnumerable<ScheduleAssignment>? assignments = null,
        int version = 1)
    {
        SchedulePeriod selectedPeriod = period ?? Period;
        AvailabilityEntrySet availabilitySet = Assert.IsType<AvailabilityEntrySet>(
            AvailabilityEntrySet.Create(availabilityEntries ?? []).Value);
        DemandSlotSet demandSlots = CreateDemandSlots(selectedPeriod);

        return Assert.IsType<ScheduleDraft>(
            ScheduleDraft.Create(
                new Guid("40e0d49f-e151-4a72-99c8-af3b273e8d56"),
                version,
                selectedPeriod,
                demandSlots,
                availabilitySet,
                assignments ?? [],
                [],
                []).Value);
    }

    public static ScheduleDraftHeader CreateHeader(ScheduleDraft draft)
    {
        return new ScheduleDraftHeader(draft.Id, draft.Version, draft.Period);
    }

    public static ScheduleWorkspaceReadData CreateReadDataForDraft(
        ScheduleDraft draft,
        IEnumerable<Employee>? employees = null,
        IEnumerable<EmployeeType>? employeeTypes = null,
        IEnumerable<AvailabilityEntryReadItem>? entries = null,
        IEnumerable<PlanningHistoryDayReadItem>? historyDays = null,
        PlanningInputSnapshot? preparedSnapshot = null)
    {
        return CreateReadData(
            employees: employees,
            employeeTypes: employeeTypes,
            entries: entries,
            draftHeaders: [CreateHeader(draft)],
            exactDraft: draft,
            historyDays: historyDays,
            preparedSnapshot: preparedSnapshot);
    }

    public static ScheduleDemandSlotSelection CreateSelection(DemandSlot slot)
    {
        return new ScheduleDemandSlotSelection(
            slot.Id.SourceId.Value,
            slot.Id.SourceKind == StaffingDemandSourceKind.Standard
                ? ScheduleDemandSourceKindSnapshot.Standard
                : ScheduleDemandSourceKindSnapshot.DateException,
            slot.Id.Date,
            slot.Id.WorkLocationId.Value,
            slot.Id.ShiftTypeId.Value,
            slot.Id.Ordinal,
            slot.ActualTime.Start,
            slot.ActualTime.End);
    }

    public static AvailabilityEntryReadItem CreateEntry(
        Guid employeeId,
        DateOnly date,
        AvailabilityEntryKind kind,
        long changeVersion = 1)
    {
        AvailabilityEntry entry = Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(employeeId, date, kind).Value);
        return new AvailabilityEntryReadItem(entry, changeVersion);
    }

    private static Employee CreateEmployee(
        Guid id,
        string firstName,
        string lastName,
        EmployeeType employeeType,
        bool active)
    {
        Employee employee = Assert.IsType<Employee>(
            Employee.Create(id, firstName, lastName, employeeType.Id.Value).Value);
        return active ? employee : employee.Deactivate();
    }

    public static DemandSlotSet CreateDemandSlots(SchedulePeriod period)
    {
        StandardStaffingDemandRevisionSet revisionSet =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    InitialStaffingDemandCatalog.All).Value);
        StaffingDemandDateExceptionSet exceptionSet =
            Assert.IsType<StaffingDemandDateExceptionSet>(
                StaffingDemandDateExceptionSet.Create([]).Value);
        List<EffectiveStaffingDemand> demands = [];

        for (int weekIndex = 0; weekIndex < 3; weekIndex++)
        {
            StaffingDemandWeek week = Assert.IsType<StaffingDemandWeek>(
                StaffingDemandWeek.Resolve(
                    period.StartMonday.AddDays(weekIndex * 7),
                    revisionSet,
                    exceptionSet).Value);
            demands.AddRange(week.Demands);
        }

        return Assert.IsType<DemandSlotSet>(
            DemandSlotSet.Create(
                period,
                demands,
                InitialWorkLocationCatalog.All.Select(location => location.Id),
                InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id)).Value);
    }
}
