using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling;

public sealed class ScheduleAssignmentTests
{
    private static readonly DateOnly PeriodMonday = new(2026, 9, 14);
    private static readonly DateOnly Saturday = PeriodMonday.AddDays(5);
    private static readonly EmployeeId EmployeeId = CreateEmployeeId();

    [Theory]
    [InlineData(AssignmentOrigin.ServiceManagement, true)]
    [InlineData(AssignmentOrigin.AutomaticGeneration, false)]
    [InlineData(AssignmentOrigin.ManualEdit, false)]
    public void CreateNormalCoversExactlyOneCompleteSlot(
        AssignmentOrigin origin,
        bool expectedProtection)
    {
        DemandSlot slot = FindSlot(
            ResolveInitialSlots(),
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);

        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                EmployeeId,
                slot,
                origin).Value);

        AssignmentSegment segment = Assert.Single(assignment.Segments);
        DemandCoverage coverage = Assert.Single(assignment.Coverages);
        Assert.Equal(ScheduleAssignmentKind.NormalDemand, assignment.Kind);
        Assert.Equal(origin, assignment.Origin);
        Assert.Equal(slot.Id, segment.AnchorSlotId);
        Assert.Equal(slot.ActualTime, segment.ActualTime);
        Assert.Equal(slot.Id, coverage.SlotId);
        Assert.Equal(slot.ActualTime, coverage.CoveredTime);
        Assert.Equal(DemandCoverageKind.Full, coverage.Kind);
        Assert.Equal(slot.DurationMinutes, assignment.WorkMinutes);
        Assert.Equal(expectedProtection, assignment.IsProtectedFromAutomaticGeneration);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateOfficeTimeCountsWorkButDoesNotCoverDemand(bool useEarlyShift)
    {
        ShiftType shiftType = useEarlyShift
            ? InitialShiftTypeCatalog.EarlyShift
            : InitialShiftTypeCatalog.LateShift;
        DemandSlot slot = FindSlot(ResolveInitialSlots(), PeriodMonday, shiftType);

        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateOfficeTime(
                Guid.NewGuid(),
                EmployeeId,
                slot).Value);

        Assert.Equal(ScheduleAssignmentKind.OfficeTime, assignment.Kind);
        Assert.Equal(AssignmentOrigin.ServiceManagement, assignment.Origin);
        Assert.True(assignment.IsProtectedFromAutomaticGeneration);
        Assert.Equal(slot.DurationMinutes, assignment.WorkMinutes);
        Assert.Single(assignment.Segments);
        Assert.Empty(assignment.Coverages);
    }

    [Fact]
    public void CreateOfficeTimeForCafeteriaReturnsStructuredError()
    {
        DemandSlot slot = FindSlot(
            ResolveInitialSlots(),
            PeriodMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateOfficeTime(
                Guid.NewGuid(),
                EmployeeId,
                slot);

        ScheduleAssignmentValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            ScheduleAssignmentValidationCode.OfficeTimeRequiresRestaurantEarlyOrLate,
            error.Code);
        Assert.Equal(slot.Id, error.SlotId);
    }

    [Fact]
    public void CreateSplitShiftUsesTwoFullRestaurantSegmentsWithBreak()
    {
        DemandSlotSet slots = ResolveInitialSlots();
        DemandSlot early = FindSlot(slots, PeriodMonday, InitialShiftTypeCatalog.EarlyShift);
        DemandSlot late = FindSlot(slots, PeriodMonday, InitialShiftTypeCatalog.LateShift);

        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateSplitShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.SplitShift,
                early,
                late,
                AssignmentOrigin.AutomaticGeneration).Value);

        Assert.Equal(ScheduleAssignmentKind.SplitShiftPattern, assignment.Kind);
        Assert.Equal(InitialShiftPatternCatalog.SplitShift.Id, assignment.PatternId);
        Assert.Equal(2, assignment.Segments.Count);
        Assert.Equal(2, assignment.Coverages.Count);
        Assert.All(
            assignment.Coverages,
            coverage => Assert.Equal(DemandCoverageKind.Full, coverage.Kind));
        Assert.True(
            assignment.Segments[0].ActualTime.End
            < assignment.Segments[1].ActualTime.Start);
        Assert.Equal(600, assignment.WorkMinutes);
    }

    [Fact]
    public void CreateSplitShiftWithWrongFirstSlotReturnsStructuredError()
    {
        DemandSlotSet slots = ResolveInitialSlots();
        DemandSlot cafeteria = FindSlot(
            slots,
            PeriodMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);
        DemandSlot late = FindSlot(slots, PeriodMonday, InitialShiftTypeCatalog.LateShift);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateSplitShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.SplitShift,
                cafeteria,
                late,
                AssignmentOrigin.AutomaticGeneration);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleAssignmentValidationCode.SlotDoesNotMatchPattern
                && error.SlotId == cafeteria.Id);
    }

    [Fact]
    public void CreateSplitShiftWithOverlappingActualDemandsReturnsStructuredError()
    {
        StaffingDemandDateException replacement = CreateReplacement(
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            new TimeOnly(6, 30),
            new TimeOnly(17, 0));
        DemandSlotSet slots = ResolveSlots(replacement);
        DemandSlot early = FindSlot(slots, PeriodMonday, InitialShiftTypeCatalog.EarlyShift);
        DemandSlot late = FindSlot(slots, PeriodMonday, InitialShiftTypeCatalog.LateShift);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateSplitShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.SplitShift,
                early,
                late,
                AssignmentOrigin.AutomaticGeneration);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleAssignmentValidationCode.SplitShiftBreakRequired);
    }

    [Fact]
    public void CreateReliefShiftUsesActualSwitchAndOnlyPartialRestaurantCoverage()
    {
        DemandSlotSet slots = ResolveInitialSlots();
        DemandSlot cafeteria = FindSlot(
            slots,
            Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB);
        DemandSlot late = FindSlot(slots, Saturday, InitialShiftTypeCatalog.LateShift);

        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateReliefShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.ReliefShift,
                cafeteria,
                late,
                AssignmentOrigin.ServiceManagement).Value);

        Assert.Equal(ScheduleAssignmentKind.ReliefShiftPattern, assignment.Kind);
        Assert.Equal(InitialShiftPatternCatalog.ReliefShift.Id, assignment.PatternId);
        Assert.Equal(2, assignment.Segments.Count);
        Assert.Equal(cafeteria.ActualTime.End, assignment.Segments[1].ActualTime.Start);
        Assert.Equal(assignment.Segments[0].ActualTime.End, assignment.Segments[1].ActualTime.Start);
        Assert.Equal(360, assignment.WorkMinutes);
        Assert.Equal(DemandCoverageKind.Full, assignment.Coverages[0].Kind);
        Assert.Equal(DemandCoverageKind.PartialReliefShift, assignment.Coverages[1].Kind);
        Assert.Equal(new TimeOnly(17, 30), assignment.Coverages[1].CoveredTime.Start);
        Assert.Equal(new TimeOnly(19, 30), assignment.Coverages[1].CoveredTime.End);
        Assert.Equal(120, assignment.Coverages[1].CoveredMinutes);
    }

    [Fact]
    public void CreateReliefShiftOutsideSaturdayReturnsStructuredError()
    {
        DateOnly friday = Saturday.AddDays(-1);
        StaffingDemandDateException cafeteriaAddition = CreateAddition(
            friday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            new TimeOnly(13, 30),
            new TimeOnly(17, 30));
        DemandSlotSet slots = ResolveSlots(cafeteriaAddition);
        DemandSlot cafeteria = FindSlot(
            slots,
            friday,
            InitialShiftTypeCatalog.CafeteriaShiftB);
        DemandSlot late = FindSlot(slots, friday, InitialShiftTypeCatalog.LateShift);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateReliefShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.ReliefShift,
                cafeteria,
                late,
                AssignmentOrigin.AutomaticGeneration);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleAssignmentValidationCode.ReliefShiftWrongDay);
    }

    [Fact]
    public void CreateReliefShiftWhenSwitchIsOutsideLateDemandReturnsStructuredError()
    {
        StaffingDemandDateException cafeteriaReplacement = CreateReplacement(
            Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            new TimeOnly(13, 30),
            new TimeOnly(20, 0));
        DemandSlotSet slots = ResolveSlots(cafeteriaReplacement);
        DemandSlot cafeteria = FindSlot(
            slots,
            Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB);
        DemandSlot late = FindSlot(slots, Saturday, InitialShiftTypeCatalog.LateShift);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateReliefShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.ReliefShift,
                cafeteria,
                late,
                AssignmentOrigin.AutomaticGeneration);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleAssignmentValidationCode.ReliefSwitchMustBeInsideSecondDemand);
    }

    [Fact]
    public void CreateCompositeWithSameSlotReturnsStructuredError()
    {
        DemandSlot early = FindSlot(
            ResolveInitialSlots(),
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateSplitShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.SplitShift,
                early,
                early,
                AssignmentOrigin.AutomaticGeneration);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleAssignmentValidationCode.SlotsMustBeDistinct);
    }

    [Fact]
    public void CreateCompositeWithSlotsOnDifferentDaysReturnsStructuredError()
    {
        DemandSlotSet slots = ResolveInitialSlots();
        DemandSlot early = FindSlot(
            slots,
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);
        DemandSlot lateNextDay = FindSlot(
            slots,
            PeriodMonday.AddDays(1),
            InitialShiftTypeCatalog.LateShift);

        ScheduleAssignmentValidationResult result =
            ScheduleAssignment.CreateSplitShift(
                Guid.NewGuid(),
                EmployeeId,
                InitialShiftPatternCatalog.SplitShift,
                early,
                lateNextDay,
                AssignmentOrigin.AutomaticGeneration);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleAssignmentValidationCode.SlotsMustShareDate);
    }

    [Fact]
    public void CreateManualAdditionalUsesActualDutyButDoesNotCoverItsSlot()
    {
        DemandSlot slot = FindSlot(
            ResolveInitialSlots(),
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);

        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateManualAdditional(
                Guid.NewGuid(),
                EmployeeId,
                slot).Value);

        Assert.Equal(ScheduleAssignmentKind.ManualAdditional, assignment.Kind);
        Assert.Equal(AssignmentOrigin.ManualEdit, assignment.Origin);
        Assert.Equal(slot.ActualTime, Assert.Single(assignment.Segments).ActualTime);
        Assert.Empty(assignment.Coverages);
    }

    [Fact]
    public void CreateWithInvalidIdentityEmployeeSlotAndOriginReturnsAllLocalErrors()
    {
        ScheduleAssignmentValidationResult result = ScheduleAssignment.CreateNormal(
            Guid.Empty,
            null,
            null,
            (AssignmentOrigin)999);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                ScheduleAssignmentValidationCode.IdentifierRequired,
                ScheduleAssignmentValidationCode.EmployeeRequired,
                ScheduleAssignmentValidationCode.OriginInvalid,
                ScheduleAssignmentValidationCode.DemandSlotRequired,
            ],
            result.Errors.Select(error => error.Code));
    }

    [Fact]
    public void GeneratedDayOffAndAssignmentLockKeepTypedReferences()
    {
        DemandSlot slot = FindSlot(
            ResolveInitialSlots(),
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);
        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                EmployeeId,
                slot,
                AssignmentOrigin.AutomaticGeneration).Value);

        GeneratedDayOffMarker marker = new(EmployeeId, PeriodMonday.AddDays(1));
        AssignmentLock assignmentLock = new(assignment.Id);

        Assert.Equal(EmployeeId, marker.EmployeeId);
        Assert.Equal(PeriodMonday.AddDays(1), marker.Date);
        Assert.Equal(assignment.Id, assignmentLock.AssignmentId);
    }

    private static EmployeeId CreateEmployeeId()
    {
        EmployeeId.TryCreate(
            new Guid("30000000-0000-4000-8000-000000000001"),
            out EmployeeId? employeeId);
        return employeeId!;
    }

    private static DemandSlotSet ResolveInitialSlots()
    {
        return ResolveSlots();
    }

    private static DemandSlotSet ResolveSlots(
        params StaffingDemandDateException[] dateExceptions)
    {
        SchedulePeriod period = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(PeriodMonday).Value);
        StandardStaffingDemandRevisionSet revisions =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    InitialStaffingDemandCatalog.All).Value);
        StaffingDemandDateExceptionSet exceptions =
            Assert.IsType<StaffingDemandDateExceptionSet>(
                StaffingDemandDateExceptionSet.Create(dateExceptions).Value);
        StaffingDemandWeek week = Assert.IsType<StaffingDemandWeek>(
            StaffingDemandWeek.Resolve(
                PeriodMonday,
                revisions,
                exceptions).Value);

        return Assert.IsType<DemandSlotSet>(
            DemandSlotSet.Create(
                period,
                week.Demands,
                InitialWorkLocationCatalog.All.Select(location => location.Id),
                InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id)).Value);
    }

    private static DemandSlot FindSlot(
        DemandSlotSet slots,
        DateOnly date,
        ShiftType shiftType)
    {
        return slots.Slots.First(
            slot => slot.Id.Date == date
                && slot.Id.ShiftTypeId == shiftType.Id
                && slot.Id.Ordinal == 1);
    }

    private static StaffingDemandDateException CreateReplacement(
        DateOnly date,
        ShiftType shiftType,
        TimeOnly start,
        TimeOnly end)
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateReplacement(
                Guid.NewGuid(),
                date,
                shiftType.WorkLocationId.Value,
                shiftType.Id.Value,
                start,
                end,
                1).Value);
    }

    private static StaffingDemandDateException CreateAddition(
        DateOnly date,
        ShiftType shiftType,
        TimeOnly start,
        TimeOnly end)
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateAddition(
                Guid.NewGuid(),
                date,
                shiftType.WorkLocationId.Value,
                shiftType.Id.Value,
                start,
                end,
                1).Value);
    }
}
