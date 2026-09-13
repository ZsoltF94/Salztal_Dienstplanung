using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.ShiftPatterns;

public sealed class ReliefShiftPatternTests
{
    [Fact]
    public void CreateWhenDefinitionIsValidReturnsConfirmedSaturdayPattern()
    {
        ReliefShiftPatternValidationResult result = ReliefShiftPattern.Create(
            new Guid("4987b872-fe65-417a-9751-ad4f8211c33d"),
            DayOfWeek.Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            InitialShiftTypeCatalog.LateShift);

        ReliefShiftPattern pattern = Assert.IsType<ReliefShiftPattern>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal("Spr", pattern.DisplayCode);
        Assert.Equal("blue", pattern.DisplayColorCode);
        Assert.Equal(DayOfWeek.Saturday, pattern.AllowedDay);
        Assert.Equal(InitialShiftTypeCatalog.CafeteriaShiftB.Id, pattern.FirstShiftTypeId);
        Assert.Equal(InitialWorkLocationCatalog.Cafeteria.Id, pattern.FirstWorkLocationId);
        Assert.Equal(InitialShiftTypeCatalog.LateShift.Id, pattern.SecondShiftTypeId);
        Assert.Equal(InitialWorkLocationCatalog.Restaurant.Id, pattern.SecondWorkLocationId);
        Assert.Equal(ReliefShiftSwitchRule.EndOfFirstActualDemand, pattern.SwitchRule);
        Assert.False(pattern.HasInterruption);
    }

    [Fact]
    public void CreateWhenDayIsNotSaturdayReturnsSaturdayRequired()
    {
        ReliefShiftPatternValidationResult result = ReliefShiftPattern.Create(
            Guid.NewGuid(),
            DayOfWeek.Sunday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            InitialShiftTypeCatalog.LateShift);

        ReliefShiftPatternValidationError error = Assert.Single(result.Errors);
        Assert.Equal(ReliefShiftPatternValidationCode.SaturdayRequired, error.Code);
    }

    [Fact]
    public void CreateWhenShiftTypesAreWrongReturnsBothTypeErrors()
    {
        ReliefShiftPatternValidationResult result = ReliefShiftPattern.Create(
            Guid.NewGuid(),
            DayOfWeek.Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftA,
            InitialShiftTypeCatalog.EarlyShift);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ReliefShiftPatternValidationCode.FirstShiftMustBeCafeteriaShiftB);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ReliefShiftPatternValidationCode.SecondShiftMustBeLateShift);
    }

    [Fact]
    public void CreateWhenLocationsAreWrongReturnsBothLocationErrors()
    {
        ShiftType cafeteriaShiftAtRestaurant = CreateShiftType(
            InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value,
            "Cafeteria-Dienst B",
            InitialWorkLocationCatalog.Restaurant.Id,
            ShiftTypeDisplayKind.ActualTime,
            null,
            13,
            30,
            17,
            30);
        ShiftType lateShiftAtCafeteria = CreateShiftType(
            InitialShiftTypeCatalog.LateShift.Id.Value,
            "Spätdienst",
            InitialWorkLocationCatalog.Cafeteria.Id,
            ShiftTypeDisplayKind.Abbreviation,
            "S",
            16,
            30,
            19,
            30);

        ReliefShiftPatternValidationResult result = ReliefShiftPattern.Create(
            Guid.NewGuid(),
            DayOfWeek.Saturday,
            cafeteriaShiftAtRestaurant,
            lateShiftAtCafeteria);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                ReliefShiftPatternValidationCode.FirstWorkLocationMustBeCafeteria,
                error.Code),
            error => Assert.Equal(
                ReliefShiftPatternValidationCode.SecondWorkLocationMustBeRestaurant,
                error.Code));
    }

    [Fact]
    public void CreateWhenStandardTimesOverlapStillUsesActualDemandSwitchRule()
    {
        ReliefShiftPattern pattern = Assert.IsType<ReliefShiftPattern>(
            ReliefShiftPattern.Create(
                Guid.NewGuid(),
                DayOfWeek.Saturday,
                InitialShiftTypeCatalog.CafeteriaShiftB,
                InitialShiftTypeCatalog.LateShift).Value);

        Assert.True(
            InitialShiftTypeCatalog.CafeteriaShiftB.StandardTime.End
                > InitialShiftTypeCatalog.LateShift.StandardTime.Start);
        Assert.Equal(ReliefShiftSwitchRule.EndOfFirstActualDemand, pattern.SwitchRule);
        Assert.False(pattern.HasInterruption);
    }

    [Fact]
    public void CreateWhenComponentsAreMissingReturnsStructuredErrors()
    {
        ReliefShiftPatternValidationResult result = ReliefShiftPattern.Create(
            Guid.Empty,
            DayOfWeek.Monday,
            null,
            null);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(ReliefShiftPatternValidationCode.IdentifierRequired, error.Code),
            error => Assert.Equal(ReliefShiftPatternValidationCode.SaturdayRequired, error.Code),
            error => Assert.Equal(ReliefShiftPatternValidationCode.FirstShiftRequired, error.Code),
            error => Assert.Equal(ReliefShiftPatternValidationCode.SecondShiftRequired, error.Code));
    }

    private static ShiftType CreateShiftType(
        Guid id,
        string name,
        WorkLocationId workLocationId,
        ShiftTypeDisplayKind displayKind,
        string? abbreviation,
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
                name,
                workLocationId,
                displayKind,
                abbreviation,
                standardTime).Value);
    }
}
