using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.ShiftPatterns;

public sealed class SplitShiftPatternTests
{
    [Fact]
    public void CreateWhenEarlyAndLateShiftAreValidReturnsConfirmedPattern()
    {
        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            new Guid("21be2d8d-02f0-4f55-8341-8daf73593e96"),
            InitialShiftTypeCatalog.EarlyShift,
            InitialShiftTypeCatalog.LateShift);

        SplitShiftPattern pattern = Assert.IsType<SplitShiftPattern>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal("D", pattern.DisplayCode);
        Assert.Equal(InitialShiftTypeCatalog.EarlyShift.Id, pattern.FirstShiftTypeId);
        Assert.Equal(InitialShiftTypeCatalog.LateShift.Id, pattern.SecondShiftTypeId);
        Assert.Equal(InitialWorkLocationCatalog.Restaurant.Id, pattern.WorkLocationId);
        Assert.Equal(180, pattern.StandardBreakMinutes);
        Assert.Equal(600, pattern.StandardWorkMinutes);
    }

    [Fact]
    public void CreateWhenShiftsUseCafeteriaReturnsRestaurantRequired()
    {
        ShiftType earlyShift = CreateShiftType(
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            InitialWorkLocationCatalog.Cafeteria.Id,
            "F",
            6,
            30,
            13,
            30);
        ShiftType lateShift = CreateShiftType(
            InitialShiftTypeCatalog.LateShift.Id.Value,
            InitialWorkLocationCatalog.Cafeteria.Id,
            "S",
            16,
            30,
            19,
            30);

        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            Guid.NewGuid(),
            earlyShift,
            lateShift);

        SplitShiftPatternValidationError error = Assert.Single(result.Errors);
        Assert.Equal(SplitShiftPatternValidationCode.RestaurantRequired, error.Code);
    }

    [Fact]
    public void CreateWhenShiftsUseDifferentLocationsReturnsLocationErrors()
    {
        ShiftType lateShiftAtCafeteria = CreateShiftType(
            InitialShiftTypeCatalog.LateShift.Id.Value,
            InitialWorkLocationCatalog.Cafeteria.Id,
            "S",
            16,
            30,
            19,
            30);

        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            Guid.NewGuid(),
            InitialShiftTypeCatalog.EarlyShift,
            lateShiftAtCafeteria);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                SplitShiftPatternValidationCode.SameWorkLocationRequired,
                error.Code),
            error => Assert.Equal(
                SplitShiftPatternValidationCode.RestaurantRequired,
                error.Code));
    }

    [Fact]
    public void CreateWhenShiftsOverlapReturnsOverlapError()
    {
        ShiftType overlappingEarlyShift = CreateShiftType(
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            InitialWorkLocationCatalog.Restaurant.Id,
            "F",
            6,
            30,
            17,
            0);

        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            Guid.NewGuid(),
            overlappingEarlyShift,
            InitialShiftTypeCatalog.LateShift);

        SplitShiftPatternValidationError error = Assert.Single(result.Errors);
        Assert.Equal(SplitShiftPatternValidationCode.SegmentsMustNotOverlap, error.Code);
    }

    [Fact]
    public void CreateWhenShiftsAreReversedReturnsChronologicalOrderError()
    {
        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            Guid.NewGuid(),
            InitialShiftTypeCatalog.LateShift,
            InitialShiftTypeCatalog.EarlyShift);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == SplitShiftPatternValidationCode.SegmentsMustBeInChronologicalOrder);
    }

    [Fact]
    public void CreateWhenSegmentsAreAdjacentReturnsBreakRequired()
    {
        ShiftType adjacentEarlyShift = CreateShiftType(
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            InitialWorkLocationCatalog.Restaurant.Id,
            "F",
            6,
            30,
            16,
            30);

        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            Guid.NewGuid(),
            adjacentEarlyShift,
            InitialShiftTypeCatalog.LateShift);

        SplitShiftPatternValidationError error = Assert.Single(result.Errors);
        Assert.Equal(SplitShiftPatternValidationCode.BreakRequired, error.Code);
    }

    [Fact]
    public void CreateWhenComponentsAreMissingReturnsStructuredErrors()
    {
        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            Guid.Empty,
            null,
            null);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(SplitShiftPatternValidationCode.IdentifierRequired, error.Code),
            error => Assert.Equal(SplitShiftPatternValidationCode.FirstShiftRequired, error.Code),
            error => Assert.Equal(SplitShiftPatternValidationCode.SecondShiftRequired, error.Code));
    }

    private static ShiftType CreateShiftType(
        Guid id,
        WorkLocationId workLocationId,
        string abbreviation,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        ShiftStandardTime standardTime = Assert.IsType<ShiftStandardTime>(
            ShiftStandardTime.Create(
                new TimeOnly(startHour, startMinute),
                new TimeOnly(endHour, endMinute)).Value);

        return Assert.IsType<ShiftType>(
            ShiftType.Create(
                id,
                "Synthetischer Restaurantdienst",
                workLocationId,
                ShiftTypeDisplayKind.Abbreviation,
                abbreviation,
                standardTime).Value);
    }
}
