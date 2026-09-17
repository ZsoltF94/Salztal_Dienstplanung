using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Planning.Tests.Validation;

namespace Salztal.Dienstplanung.Planning.Tests.Candidates;

internal static class CandidateScenarioFactory
{
    public static readonly DateOnly Saturday = new(2026, 9, 26);
    public static readonly Guid NormalEmployeeId = Guid.Parse("11000000-0000-0000-0000-000000000001");
    public static readonly Guid AuxiliaryEmployeeId = Guid.Parse("11000000-0000-0000-0000-000000000002");
    public static readonly Guid IneligibleEmployeeId = Guid.Parse("11000000-0000-0000-0000-000000000003");
    public static readonly Guid ServiceEmployeeId = Guid.Parse("11000000-0000-0000-0000-000000000004");
    public static readonly Guid NormalTypeId = Guid.Parse("21000000-0000-0000-0000-000000000001");
    public static readonly Guid AuxiliaryTypeId = Guid.Parse("21000000-0000-0000-0000-000000000002");
    public static readonly Guid IneligibleTypeId = Guid.Parse("21000000-0000-0000-0000-000000000003");
    public static readonly Guid ServiceTypeId = Guid.Parse("21000000-0000-0000-0000-000000000004");
    public static readonly Guid RestaurantId = Guid.Parse("31000000-0000-0000-0000-000000000001");
    public static readonly Guid CafeteriaId = Guid.Parse("31000000-0000-0000-0000-000000000002");
    public static readonly Guid EarlyShiftId = Guid.Parse("41000000-0000-0000-0000-000000000001");
    public static readonly Guid LateShiftId = Guid.Parse("41000000-0000-0000-0000-000000000002");
    public static readonly Guid CafeteriaBShiftId = Guid.Parse("41000000-0000-0000-0000-000000000003");
    public static readonly Guid SplitPatternId = Guid.Parse("51000000-0000-0000-0000-000000000001");
    public static readonly Guid ReliefPatternId = Guid.Parse("51000000-0000-0000-0000-000000000002");

    public static PlanningInputSnapshot Create(
        IEnumerable<PlanningEmployeeSnapshot>? employees = null,
        IEnumerable<PlanningEmployeeTypeSnapshot>? employeeTypes = null,
        IEnumerable<PlanningAvailabilityEntrySnapshot>? availabilityEntries = null,
        IEnumerable<ScheduleDemandSlotSnapshot>? demandSlots = null,
        IEnumerable<ScheduleAssignmentSnapshot>? protectedAssignments = null,
        bool enableAuxiliaryReliefShift = false)
    {
        return new PlanningInputSnapshot(
            Guid.Parse("71000000-0000-0000-0000-000000000001"),
            Guid.Parse("81000000-0000-0000-0000-000000000001"),
            1,
            PlanningInputTestFactory.PeriodMonday,
            PlanningInputTestFactory.PeriodMonday.AddDays(20),
            employees ?? CreateEmployees(),
            employeeTypes ?? CreateEmployeeTypes(),
            CreateServiceCatalog(),
            availabilityEntries ?? [],
            demandSlots ?? CreateDemandSlots(),
            protectedAssignments ?? [],
            PlanningInputTestFactory.CreateRuleCatalog(),
            new PlanningRunOptions(enableAuxiliaryReliefShift),
            PlanningInputTestFactory.CreateHistory());
    }

    public static PlanningEmployeeSnapshot[] CreateEmployees() =>
    [
        new PlanningEmployeeSnapshot(NormalEmployeeId, "Nora", "Normal", NormalTypeId),
        new PlanningEmployeeSnapshot(AuxiliaryEmployeeId, "Ada", "Aushilfe", AuxiliaryTypeId),
        new PlanningEmployeeSnapshot(IneligibleEmployeeId, "Ina", "Ohnefreigabe", IneligibleTypeId),
        new PlanningEmployeeSnapshot(ServiceEmployeeId, "Sina", "Service", ServiceTypeId),
    ];

    public static PlanningEmployeeTypeSnapshot[] CreateEmployeeTypes() =>
    [
        CreateType(
            NormalTypeId,
            "N",
            EmployeeTypePlanningRole.Normal,
            [
                Shift(EarlyShiftId),
                Shift(LateShiftId),
                Shift(CafeteriaBShiftId),
                Pattern(SplitPatternId),
                Pattern(ReliefPatternId),
            ]),
        CreateType(
            AuxiliaryTypeId,
            "AH",
            EmployeeTypePlanningRole.Auxiliary,
            [
                Shift(LateShiftId),
                Shift(CafeteriaBShiftId),
                Pattern(SplitPatternId),
                Pattern(
                    ReliefPatternId,
                    ShiftEligibilityActivation.ExplicitPlanningRunOption),
            ]),
        CreateType(
            IneligibleTypeId,
            "O",
            EmployeeTypePlanningRole.Normal,
            []),
        CreateType(
            ServiceTypeId,
            "T1",
            EmployeeTypePlanningRole.ServiceManagement,
            [
                Shift(EarlyShiftId),
                Shift(LateShiftId),
                Shift(CafeteriaBShiftId),
                Pattern(SplitPatternId),
                Pattern(ReliefPatternId),
            ]),
    ];

    public static ScheduleDemandSlotSnapshot[] CreateDemandSlots() =>
    [
        CreateSlot(
            Guid.Parse("91000000-0000-0000-0000-000000000001"),
            RestaurantId,
            EarlyShiftId,
            1,
            new TimeOnly(6, 30),
            new TimeOnly(13, 30)),
        CreateSlot(
            Guid.Parse("91000000-0000-0000-0000-000000000002"),
            RestaurantId,
            LateShiftId,
            1,
            new TimeOnly(16, 30),
            new TimeOnly(19, 30)),
        CreateSlot(
            Guid.Parse("91000000-0000-0000-0000-000000000002"),
            RestaurantId,
            LateShiftId,
            2,
            new TimeOnly(16, 30),
            new TimeOnly(19, 30)),
        CreateSlot(
            Guid.Parse("91000000-0000-0000-0000-000000000003"),
            CafeteriaId,
            CafeteriaBShiftId,
            1,
            new TimeOnly(13, 30),
            new TimeOnly(17, 30)),
    ];

    public static ScheduleAssignmentSnapshot CreateProtectedNormal(
        ScheduleDemandSlotSnapshot slot,
        int assignmentNumber = 1,
        bool officeTime = false)
    {
        return new ScheduleAssignmentSnapshot(
            Guid.Parse($"a1000000-0000-0000-0000-{assignmentNumber:D12}"),
            ServiceEmployeeId,
            slot.Date,
            officeTime
                ? ScheduleAssignmentKindSnapshot.OfficeTime
                : ScheduleAssignmentKindSnapshot.NormalDemand,
            ScheduleAssignmentOriginSnapshot.ServiceManagement,
            null,
            slot.DurationMinutes,
            true,
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
            officeTime
                ? []
                :
                [
                    CreateCoverage(
                        slot,
                        slot.ActualStart,
                        slot.ActualEnd,
                        ScheduleDemandCoverageKindSnapshot.Full),
                ]);
    }

    public static ScheduleAssignmentSnapshot CreateProtectedRelief(
        ScheduleDemandSlotSnapshot cafeteriaSlot,
        ScheduleDemandSlotSnapshot lateSlot)
    {
        int secondMinutes = MinutesBetween(cafeteriaSlot.ActualEnd, lateSlot.ActualEnd);
        return new ScheduleAssignmentSnapshot(
            Guid.Parse("a1000000-0000-0000-0000-000000000010"),
            ServiceEmployeeId,
            cafeteriaSlot.Date,
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
            ScheduleAssignmentOriginSnapshot.ServiceManagement,
            ReliefPatternId,
            cafeteriaSlot.DurationMinutes + secondMinutes,
            true,
            [
                new ScheduleAssignmentSegmentSnapshot(
                    cafeteriaSlot.SourceId,
                    cafeteriaSlot.Date,
                    cafeteriaSlot.WorkLocationId,
                    cafeteriaSlot.ShiftTypeId,
                    cafeteriaSlot.ActualStart,
                    cafeteriaSlot.ActualEnd,
                    cafeteriaSlot.DurationMinutes),
                new ScheduleAssignmentSegmentSnapshot(
                    lateSlot.SourceId,
                    lateSlot.Date,
                    lateSlot.WorkLocationId,
                    lateSlot.ShiftTypeId,
                    cafeteriaSlot.ActualEnd,
                    lateSlot.ActualEnd,
                    secondMinutes),
            ],
            [
                CreateCoverage(
                    cafeteriaSlot,
                    cafeteriaSlot.ActualStart,
                    cafeteriaSlot.ActualEnd,
                    ScheduleDemandCoverageKindSnapshot.Full),
                CreateCoverage(
                    lateSlot,
                    cafeteriaSlot.ActualEnd,
                    lateSlot.ActualEnd,
                    ScheduleDemandCoverageKindSnapshot.PartialReliefShift),
            ]);
    }

    private static PlanningEmployeeTypeSnapshot CreateType(
        Guid id,
        string code,
        EmployeeTypePlanningRole role,
        IEnumerable<PlanningEmployeeTypeEligibilitySnapshot> eligibilities)
    {
        bool serviceManagement = role == EmployeeTypePlanningRole.ServiceManagement;
        return new PlanningEmployeeTypeSnapshot(
            id,
            code,
            $"Synthetischer Typ {code}",
            serviceManagement ? 2_400 : 1_200,
            !serviceManagement,
            serviceManagement ? null : 240,
            role,
            !serviceManagement,
            serviceManagement,
            serviceManagement,
            serviceManagement
                ? ManualSuggestionPriority.LastResort
                : ManualSuggestionPriority.Standard,
            eligibilities);
    }

    private static PlanningEmployeeTypeEligibilitySnapshot Shift(Guid id) => new(
        ShiftEligibilityTargetKind.ShiftType,
        id,
        ShiftEligibilityMode.Regular,
        ShiftEligibilityActivation.Always);

    private static PlanningEmployeeTypeEligibilitySnapshot Pattern(
        Guid id,
        ShiftEligibilityActivation activation = ShiftEligibilityActivation.Always) => new(
            ShiftEligibilityTargetKind.ShiftPattern,
            id,
            ShiftEligibilityMode.Regular,
            activation);

    private static PlanningServiceCatalogSnapshot CreateServiceCatalog() => new(
        [
            new PlanningWorkLocationSnapshot(RestaurantId, "Restaurant Test", "#AA0000"),
            new PlanningWorkLocationSnapshot(CafeteriaId, "Cafeteria Test", "#AAAA00"),
        ],
        [
            CreateShiftType(EarlyShiftId, RestaurantId, "Früh Test", new TimeOnly(6, 30), new TimeOnly(13, 30)),
            CreateShiftType(LateShiftId, RestaurantId, "Spät Test", new TimeOnly(16, 30), new TimeOnly(19, 30)),
            CreateShiftType(CafeteriaBShiftId, CafeteriaId, "Cafeteria B Test", new TimeOnly(13, 30), new TimeOnly(17, 30)),
        ],
        new PlanningSplitShiftPatternSnapshot(
            SplitPatternId,
            RestaurantId,
            EarlyShiftId,
            LateShiftId,
            180,
            600),
        new PlanningReliefShiftPatternSnapshot(
            ReliefPatternId,
            DayOfWeek.Saturday,
            CafeteriaId,
            CafeteriaBShiftId,
            RestaurantId,
            LateShiftId,
            ReliefShiftSwitchRule.EndOfFirstActualDemand,
            false));

    private static PlanningShiftTypeSnapshot CreateShiftType(
        Guid id,
        Guid locationId,
        string name,
        TimeOnly start,
        TimeOnly end) => new(
            id,
            name,
            locationId,
            ShiftTypeDisplayKind.ActualTime,
            null,
            start,
            end);

    private static ScheduleDemandSlotSnapshot CreateSlot(
        Guid sourceId,
        Guid locationId,
        Guid shiftTypeId,
        int ordinal,
        TimeOnly start,
        TimeOnly end) => new(
            sourceId,
            ScheduleDemandSourceKindSnapshot.Standard,
            Saturday,
            locationId,
            "Synthetischer Ort",
            shiftTypeId,
            "Synthetischer Dienst",
            ordinal,
            start,
            end,
            MinutesBetween(start, end));

    private static ScheduleDemandCoverageSnapshot CreateCoverage(
        ScheduleDemandSlotSnapshot slot,
        TimeOnly start,
        TimeOnly end,
        ScheduleDemandCoverageKindSnapshot kind) => new(
            slot.SourceId,
            slot.Date,
            slot.WorkLocationId,
            slot.ShiftTypeId,
            slot.Ordinal,
            start,
            end,
            MinutesBetween(start, end),
            kind);

    private static int MinutesBetween(TimeOnly start, TimeOnly end) =>
        (int)(end - start).TotalMinutes;
}
