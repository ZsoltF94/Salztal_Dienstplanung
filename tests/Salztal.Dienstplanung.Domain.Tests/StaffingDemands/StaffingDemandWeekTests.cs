using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.StaffingDemands;

public sealed class StaffingDemandWeekTests
{
    private static readonly WorkLocationId CafeteriaId =
        InitialWorkLocationCatalog.Cafeteria.Id;
    private static readonly WorkLocationId RestaurantId =
        InitialWorkLocationCatalog.Restaurant.Id;

    [Fact]
    public void ResolveInitialWeekReturnsConfirmedDemandsAndMinuteSummaries()
    {
        DateOnly weekMonday = new(2026, 9, 14);

        StaffingDemandWeek week = ResolveWeek(weekMonday);

        Assert.Equal(weekMonday, week.WeekMonday);
        Assert.Equal(23, week.Demands.Count);
        Assert.Equal(14, week.DaySummaries.Count);
        Assert.Equal(2, week.WorkLocationSummaries.Count);
        Assert.Equal(57 * 60, FindWorkLocationSummary(week, CafeteriaId));
        Assert.Equal(280 * 60, FindWorkLocationSummary(week, RestaurantId));
        Assert.Equal(337 * 60, week.TotalRequiredWorkMinutes);
        Assert.Equal(
            420,
            FindDaySummary(week, weekMonday, CafeteriaId));
        Assert.Equal(
            660,
            FindDaySummary(week, weekMonday.AddDays(5), CafeteriaId));
        Assert.Equal(
            2_400,
            FindDaySummary(week, weekMonday, RestaurantId));
        Assert.All(
            week.Demands,
            demand => Assert.Equal(StaffingDemandSourceKind.Standard, demand.SourceKind));
    }

    [Fact]
    public void ResolveWithReplacementUsesCompleteDateExceptionSnapshot()
    {
        DateOnly weekMonday = new(2026, 9, 14);
        StaffingDemandDateException replacement = CreateDateException(
            StaffingDemandDateExceptionKind.Replace,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA,
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            2);

        StaffingDemandWeek week = ResolveWeek(weekMonday, replacement);

        EffectiveStaffingDemand demand = FindDemand(
            week,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);
        Assert.Equal(StaffingDemandSourceKind.DateException, demand.SourceKind);
        Assert.Equal(replacement.Id, demand.SourceId);
        Assert.Equal(new TimeOnly(10, 0), demand.ActualTime.Start);
        Assert.Equal(new TimeOnly(12, 0), demand.ActualTime.End);
        Assert.Equal(2, demand.RequiredEmployeeCount.Value);
        Assert.Equal(240L, demand.RequiredWorkMinutes);
        Assert.Equal((337 * 60) - 420 + 240, week.TotalRequiredWorkMinutes);
    }

    [Fact]
    public void ResolveWithAdditionAllowsOverlappingDifferentShiftTypes()
    {
        DateOnly weekMonday = new(2026, 9, 14);
        StaffingDemandDateException addition = CreateDateException(
            StaffingDemandDateExceptionKind.Add,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            new TimeOnly(13, 30),
            new TimeOnly(17, 30),
            1);

        StaffingDemandWeek week = ResolveWeek(weekMonday, addition);

        EffectiveStaffingDemand cafeteriaShiftA = FindDemand(
            week,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);
        EffectiveStaffingDemand cafeteriaShiftB = FindDemand(
            week,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftB);
        Assert.Equal(cafeteriaShiftA.ActualTime.Start, cafeteriaShiftB.ActualTime.Start);
        Assert.True(cafeteriaShiftB.ActualTime.End < cafeteriaShiftA.ActualTime.End);
        Assert.Equal(24, week.Demands.Count);
        Assert.Equal((337 * 60) + 240, week.TotalRequiredWorkMinutes);
    }

    [Fact]
    public void ResolveAfterExistingRemovalExceptionIsRemovedReturnsToCurrentStandard()
    {
        DateOnly weekMonday = new(2026, 9, 14);
        StaffingDemandDateException removal = CreateDateException(
            StaffingDemandDateExceptionKind.Remove,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);

        StaffingDemandWeek weekWithRemoval = ResolveWeek(weekMonday, removal);
        StaffingDemandWeek weekAfterExceptionRemoval = ResolveWeek(weekMonday);

        Assert.DoesNotContain(
            weekWithRemoval.Demands,
            demand => demand.Date == weekMonday
                && demand.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id);
        EffectiveStaffingDemand restoredStandard = FindDemand(
            weekAfterExceptionRemoval,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);
        Assert.Equal(StaffingDemandSourceKind.Standard, restoredStandard.SourceKind);
        Assert.Equal(new TimeOnly(13, 30), restoredStandard.ActualTime.Start);
        Assert.Equal(new TimeOnly(20, 30), restoredStandard.ActualTime.End);
    }

    [Fact]
    public void ResolveWithDateExceptionIgnoresLaterStandardAndCurrentShiftTimeChanges()
    {
        DateOnly weekMonday = new(2026, 9, 14);
        StaffingDemandDateException replacementException = CreateDateException(
            StaffingDemandDateExceptionKind.Replace,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA,
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            3);
        ShiftStandardTime changedShiftTime = Assert.IsType<ShiftStandardTime>(
            ShiftStandardTime.Create(
                new TimeOnly(14, 0),
                new TimeOnly(21, 0)).Value);
        ShiftType changedShiftType = Assert.IsType<ShiftType>(
            InitialShiftTypeCatalog.CafeteriaShiftA
                .WithStandardTime(changedShiftTime).Value);
        ShiftType[] currentShiftTypes = InitialShiftTypeCatalog.All
            .Select(
                shiftType => shiftType.Id == changedShiftType.Id
                    ? changedShiftType
                    : shiftType)
            .ToArray();
        StandardStaffingDemandRevisionSet currentStandards =
            InitialStaffingDemandCatalog.CreateFromCurrentShiftTypes(currentShiftTypes);
        StandardStaffingDemandRevision laterStandard = CreateStandardReplacement(
            weekMonday,
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.CafeteriaShiftA,
            new TimeOnly(15, 0),
            new TimeOnly(20, 0),
            4);
        StandardStaffingDemandRevisionSet standardsWithLaterRevision =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    currentStandards.Revisions.Append(laterStandard)).Value);

        StaffingDemandWeek week = ResolveWeek(
            weekMonday,
            standardsWithLaterRevision,
            CreateExceptionSet(replacementException));

        EffectiveStaffingDemand demand = FindDemand(
            week,
            weekMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA);
        Assert.Equal(StaffingDemandSourceKind.DateException, demand.SourceKind);
        Assert.Equal(new TimeOnly(10, 0), demand.ActualTime.Start);
        Assert.Equal(new TimeOnly(12, 0), demand.ActualTime.End);
        Assert.Equal(3, demand.RequiredEmployeeCount.Value);
    }

    [Theory]
    [InlineData(StaffingDemandDateExceptionKind.Add, true)]
    [InlineData(StaffingDemandDateExceptionKind.Replace, false)]
    [InlineData(StaffingDemandDateExceptionKind.Remove, false)]
    public void ResolveWhenExceptionKindDoesNotMatchStandardStateReturnsError(
        StaffingDemandDateExceptionKind kind,
        bool useExistingStandard)
    {
        DateOnly weekMonday = new(2026, 9, 14);
        ShiftType shiftType = useExistingStandard
            ? InitialShiftTypeCatalog.CafeteriaShiftA
            : InitialShiftTypeCatalog.CafeteriaShiftB;
        StaffingDemandDateException dateException = CreateDateException(
            kind,
            weekMonday,
            shiftType);

        StaffingDemandWeekResolutionResult result = StaffingDemandWeek.Resolve(
            weekMonday,
            CreateInitialStandards(),
            CreateExceptionSet(dateException));

        StaffingDemandWeekResolutionError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            kind switch
            {
                StaffingDemandDateExceptionKind.Add =>
                    StaffingDemandWeekResolutionCode.AdditionRequiresMissingStandard,
                StaffingDemandDateExceptionKind.Replace =>
                    StaffingDemandWeekResolutionCode.ReplacementRequiresExistingStandard,
                StaffingDemandDateExceptionKind.Remove =>
                    StaffingDemandWeekResolutionCode.RemovalRequiresExistingStandard,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            },
            error.Code);
        Assert.Equal(dateException.Key, error.Key);
    }

    [Fact]
    public void ResolveForFridayDateTreatsHolidayLikeOrdinaryManualException()
    {
        DateOnly weekMonday = new(2026, 12, 21);
        DateOnly holiday = new(2026, 12, 25);
        StaffingDemandDateException replacement = CreateDateException(
            StaffingDemandDateExceptionKind.Replace,
            holiday,
            InitialShiftTypeCatalog.EarlyShift,
            new TimeOnly(7, 0),
            new TimeOnly(11, 0),
            2);

        StaffingDemandWeek week = ResolveWeek(weekMonday, replacement);

        EffectiveStaffingDemand demand = FindDemand(
            week,
            holiday,
            InitialShiftTypeCatalog.EarlyShift);
        Assert.Equal(StaffingDemandSourceKind.DateException, demand.SourceKind);
        Assert.Equal(480L, demand.RequiredWorkMinutes);
    }

    [Fact]
    public void ResolveWhenWeekStartIsNotMondayReturnsError()
    {
        StaffingDemandWeekResolutionResult result = StaffingDemandWeek.Resolve(
            new DateOnly(2026, 9, 15),
            CreateInitialStandards(),
            CreateExceptionSet());

        StaffingDemandWeekResolutionError error = Assert.Single(result.Errors);
        Assert.Equal(StaffingDemandWeekResolutionCode.WeekStartMustBeMonday, error.Code);
    }

    [Fact]
    public void ResolveWhenCompleteWeekExceedsSupportedCalendarReturnsError()
    {
        DateOnly finalCalendarMonday = new(9999, 12, 27);

        StaffingDemandWeekResolutionResult result = StaffingDemandWeek.Resolve(
            finalCalendarMonday,
            CreateInitialStandards(),
            CreateExceptionSet());

        StaffingDemandWeekResolutionError error = Assert.Single(result.Errors);
        Assert.Equal(StaffingDemandWeekResolutionCode.WeekMustFitSevenDays, error.Code);
    }

    private static StaffingDemandWeek ResolveWeek(
        DateOnly weekMonday,
        params StaffingDemandDateException[] dateExceptions)
    {
        return ResolveWeek(
            weekMonday,
            CreateInitialStandards(),
            CreateExceptionSet(dateExceptions));
    }

    private static StaffingDemandWeek ResolveWeek(
        DateOnly weekMonday,
        StandardStaffingDemandRevisionSet standardRevisions,
        StaffingDemandDateExceptionSet dateExceptions)
    {
        return Assert.IsType<StaffingDemandWeek>(
            StaffingDemandWeek.Resolve(
                weekMonday,
                standardRevisions,
                dateExceptions).Value);
    }

    private static StandardStaffingDemandRevisionSet CreateInitialStandards()
    {
        return Assert.IsType<StandardStaffingDemandRevisionSet>(
            StandardStaffingDemandRevisionSet.Create(
                InitialStaffingDemandCatalog.All).Value);
    }

    private static StaffingDemandDateExceptionSet CreateExceptionSet(
        params StaffingDemandDateException[] dateExceptions)
    {
        return Assert.IsType<StaffingDemandDateExceptionSet>(
            StaffingDemandDateExceptionSet.Create(dateExceptions).Value);
    }

    private static StaffingDemandDateException CreateDateException(
        StaffingDemandDateExceptionKind kind,
        DateOnly date,
        ShiftType shiftType,
        TimeOnly? actualStart = null,
        TimeOnly? actualEnd = null,
        int requiredEmployeeCount = 1)
    {
        StaffingDemandDateExceptionValidationResult result = kind switch
        {
            StaffingDemandDateExceptionKind.Add =>
                StaffingDemandDateException.CreateAddition(
                    Guid.NewGuid(),
                    date,
                    shiftType.WorkLocationId.Value,
                    shiftType.Id.Value,
                    actualStart ?? shiftType.StandardTime.Start,
                    actualEnd ?? shiftType.StandardTime.End,
                    requiredEmployeeCount),
            StaffingDemandDateExceptionKind.Replace =>
                StaffingDemandDateException.CreateReplacement(
                    Guid.NewGuid(),
                    date,
                    shiftType.WorkLocationId.Value,
                    shiftType.Id.Value,
                    actualStart ?? shiftType.StandardTime.Start,
                    actualEnd ?? shiftType.StandardTime.End,
                    requiredEmployeeCount),
            StaffingDemandDateExceptionKind.Remove =>
                StaffingDemandDateException.CreateRemoval(
                    Guid.NewGuid(),
                    date,
                    shiftType.WorkLocationId.Value,
                    shiftType.Id.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        return Assert.IsType<StaffingDemandDateException>(result.Value);
    }

    private static StandardStaffingDemandRevision CreateStandardReplacement(
        DateOnly effectiveMonday,
        DayOfWeek dayOfWeek,
        ShiftType shiftType,
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        return Assert.IsType<StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateReplacement(
                Guid.NewGuid(),
                dayOfWeek,
                shiftType.WorkLocationId.Value,
                shiftType.Id.Value,
                effectiveMonday,
                actualStart,
                actualEnd,
                requiredEmployeeCount).Value);
    }

    private static EffectiveStaffingDemand FindDemand(
        StaffingDemandWeek week,
        DateOnly date,
        ShiftType shiftType)
    {
        return Assert.Single(
            week.Demands,
            demand => demand.Date == date
                && demand.WorkLocationId == shiftType.WorkLocationId
                && demand.ShiftTypeId == shiftType.Id);
    }

    private static long FindDaySummary(
        StaffingDemandWeek week,
        DateOnly date,
        WorkLocationId workLocationId)
    {
        return Assert.Single(
            week.DaySummaries,
            summary => summary.Date == date
                && summary.WorkLocationId == workLocationId).RequiredWorkMinutes;
    }

    private static long FindWorkLocationSummary(
        StaffingDemandWeek week,
        WorkLocationId workLocationId)
    {
        return Assert.Single(
            week.WorkLocationSummaries,
            summary => summary.WorkLocationId == workLocationId).RequiredWorkMinutes;
    }
}
