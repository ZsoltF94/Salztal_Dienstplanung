using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Planning.Tests.Validation;

internal static class PlanningInputTestFactory
{
    public static readonly DateOnly PeriodMonday = new(2026, 9, 21);
    public static readonly Guid EmployeeId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid EmployeeTypeId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid WorkLocationId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid ShiftTypeId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid SplitPatternId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    public static readonly Guid ReliefPatternId = Guid.Parse("60000000-0000-0000-0000-000000000001");

    public static PlanningInputSnapshot Create(
        RuleCatalogSnapshot? ruleCatalog = null,
        IEnumerable<PlanningEmployeeSnapshot>? employees = null,
        IEnumerable<PlanningEmployeeTypeSnapshot>? employeeTypes = null,
        PlanningServiceCatalogSnapshot? serviceCatalog = null,
        IEnumerable<PlanningAvailabilityEntrySnapshot>? availabilityEntries = null,
        IEnumerable<ScheduleDemandSlotSnapshot>? demandSlots = null,
        IEnumerable<ScheduleAssignmentSnapshot>? assignments = null,
        PlanningHistorySnapshot? history = null,
        Guid? snapshotId = null,
        DateOnly? periodMonday = null,
        DateOnly? periodSunday = null)
    {
        ScheduleDemandSlotSnapshot[] defaultSlots = CreateDemandSlots();
        return new PlanningInputSnapshot(
            snapshotId ?? Guid.Parse("70000000-0000-0000-0000-000000000001"),
            Guid.Parse("80000000-0000-0000-0000-000000000001"),
            1,
            periodMonday ?? PeriodMonday,
            periodSunday ?? PeriodMonday.AddDays(20),
            employees ?? [CreateEmployee()],
            employeeTypes ?? [CreateEmployeeType()],
            serviceCatalog ?? CreateServiceCatalog(),
            availabilityEntries ?? [],
            demandSlots ?? defaultSlots,
            assignments ?? CreateAssignments(defaultSlots),
            ruleCatalog ?? CreateRuleCatalog(),
            PlanningRunOptions.Default,
            history ?? CreateHistory());
    }

    public static RuleCatalogSnapshot CreateRuleCatalog() =>
        GetRuleCatalogQuery.ExecuteAsync().GetAwaiter().GetResult();

    public static PlanningEmployeeSnapshot CreateEmployee() => new(
        EmployeeId,
        "Erika",
        "Beispiel",
        EmployeeTypeId);

    public static PlanningEmployeeTypeSnapshot CreateEmployeeType() => new(
        EmployeeTypeId,
        "T1",
        "Service-Leitung",
        2_400,
        true,
        480,
        EmployeeTypePlanningRole.ServiceManagement,
        false,
        true,
        true,
        ManualSuggestionPriority.LastResort,
        [
            new PlanningEmployeeTypeEligibilitySnapshot(
                ShiftEligibilityTargetKind.ShiftType,
                ShiftTypeId,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always),
        ]);

    public static PlanningServiceCatalogSnapshot CreateServiceCatalog() => new(
        [new PlanningWorkLocationSnapshot(WorkLocationId, "Testort", "#112233")],
        [
            new PlanningShiftTypeSnapshot(
                ShiftTypeId,
                "Testdienst",
                WorkLocationId,
                ShiftTypeDisplayKind.Abbreviation,
                "TD",
                new TimeOnly(8, 0),
                new TimeOnly(12, 0)),
        ],
        new PlanningSplitShiftPatternSnapshot(
            SplitPatternId,
            WorkLocationId,
            ShiftTypeId,
            ShiftTypeId,
            60,
            480),
        new PlanningReliefShiftPatternSnapshot(
            ReliefPatternId,
            DayOfWeek.Saturday,
            WorkLocationId,
            ShiftTypeId,
            WorkLocationId,
            ShiftTypeId,
            ReliefShiftSwitchRule.EndOfFirstActualDemand,
            false));

    public static ScheduleDemandSlotSnapshot[] CreateDemandSlots() =>
        Enumerable.Range(0, 3)
            .Select(index => CreateDemandSlot(PeriodMonday.AddDays(index * 7), index + 1))
            .ToArray();

    public static ScheduleDemandSlotSnapshot CreateDemandSlot(
        DateOnly date,
        int sourceNumber = 1,
        TimeOnly? actualEnd = null,
        int durationMinutes = 240) => new(
            Guid.Parse($"90000000-0000-0000-0000-{sourceNumber:D12}"),
            ScheduleDemandSourceKindSnapshot.Standard,
            date,
            WorkLocationId,
            "Testort",
            ShiftTypeId,
            "Testdienst",
            1,
            new TimeOnly(8, 0),
            actualEnd ?? new TimeOnly(12, 0),
            durationMinutes);

    public static ScheduleAssignmentSnapshot[] CreateAssignments(
        IReadOnlyList<ScheduleDemandSlotSnapshot>? slots = null)
    {
        IReadOnlyList<ScheduleDemandSlotSnapshot> values = slots ?? CreateDemandSlots();
        return values.Select((slot, index) => CreateAssignment(slot, index + 1)).ToArray();
    }

    public static ScheduleAssignmentSnapshot CreateAssignment(
        ScheduleDemandSlotSnapshot slot,
        int assignmentNumber = 1,
        bool isProtected = true) => new(
            Guid.Parse($"a0000000-0000-0000-0000-{assignmentNumber:D12}"),
            EmployeeId,
            slot.Date,
            ScheduleAssignmentKindSnapshot.NormalDemand,
            ScheduleAssignmentOriginSnapshot.ServiceManagement,
            null,
            slot.DurationMinutes,
            isProtected,
            [
                new ScheduleAssignmentSegmentSnapshot(
                    slot.SourceId,
                    slot.Date,
                    slot.WorkLocationId,
                    slot.ShiftTypeId,
                    slot.ActualStart,
                    slot.ActualEnd,
                    slot.DurationMinutes),
            ],
            [
                new ScheduleDemandCoverageSnapshot(
                    slot.SourceId,
                    slot.Date,
                    slot.WorkLocationId,
                    slot.ShiftTypeId,
                    slot.Ordinal,
                    slot.ActualStart,
                    slot.ActualEnd,
                    slot.DurationMinutes,
                    ScheduleDemandCoverageKindSnapshot.Full),
            ]);

    public static PlanningHistorySnapshot CreateHistory()
    {
        PlanningHistoryDaySnapshot[] days = Enumerable.Range(1, 7)
            .Select(daysBefore => new PlanningHistoryDaySnapshot(
                PeriodMonday.AddDays(-daysBefore),
                PlanningHistoryDayStatus.Missing,
                []))
            .OrderBy(day => day.Date)
            .ToArray();
        return new PlanningHistorySnapshot(PlanningHistoryCompleteness.Missing, days);
    }
}
