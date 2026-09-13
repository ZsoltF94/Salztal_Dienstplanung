using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.ShiftTypes;

public sealed class ShiftTypeTests
{
    [Fact]
    public void CreateWhenAbbreviationDisplayIsValidReturnsShiftType()
    {
        ShiftStandardTime standardTime = CreateStandardTime(9, 0, 12, 0);

        ShiftTypeValidationResult result = ShiftType.Create(
            new Guid("07f29d2f-12c7-4606-863f-ad12803bd09e"),
            "Mittagsdienst",
            InitialWorkLocationCatalog.Restaurant.Id,
            ShiftTypeDisplayKind.Abbreviation,
            "  M  ",
            standardTime);

        ShiftType shiftType = Assert.IsType<ShiftType>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal("Mittagsdienst", shiftType.Name.Value);
        Assert.Equal(InitialWorkLocationCatalog.Restaurant.Id, shiftType.WorkLocationId);
        Assert.Equal(ShiftTypeDisplayKind.Abbreviation, shiftType.Display.Kind);
        Assert.Equal("M", shiftType.Display.Abbreviation);
        Assert.Equal(standardTime, shiftType.StandardTime);
    }

    [Fact]
    public void CreateWhenActualTimeDisplayIsValidDoesNotStoreAbbreviation()
    {
        ShiftTypeValidationResult result = ShiftType.Create(
            new Guid("d3ef221f-1129-4254-bb18-b3ea9e956a64"),
            "Terrassen-Dienst",
            InitialWorkLocationCatalog.Cafeteria.Id,
            ShiftTypeDisplayKind.ActualTime,
            null,
            CreateStandardTime(10, 0, 14, 0));

        ShiftType shiftType = Assert.IsType<ShiftType>(result.Value);
        Assert.Equal(ShiftTypeDisplayKind.ActualTime, shiftType.Display.Kind);
        Assert.Null(shiftType.Display.Abbreviation);
    }

    [Fact]
    public void CreateWhenRequiredValuesAreMissingReturnsAllValidationCodes()
    {
        ShiftTypeValidationResult result = ShiftType.Create(
            Guid.Empty,
            " ",
            null,
            ShiftTypeDisplayKind.Abbreviation,
            null,
            null);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(ShiftTypeValidationCode.IdentifierRequired, error.Code),
            error => Assert.Equal(ShiftTypeValidationCode.NameRequired, error.Code),
            error => Assert.Equal(ShiftTypeValidationCode.WorkLocationRequired, error.Code),
            error => Assert.Equal(ShiftTypeValidationCode.AbbreviationRequired, error.Code),
            error => Assert.Equal(ShiftTypeValidationCode.StandardTimeRequired, error.Code));
    }

    [Fact]
    public void CreateWhenActualTimeDisplayHasAbbreviationReturnsError()
    {
        ShiftTypeValidationResult result = ShiftType.Create(
            Guid.NewGuid(),
            "Terrassen-Dienst",
            InitialWorkLocationCatalog.Cafeteria.Id,
            ShiftTypeDisplayKind.ActualTime,
            "T",
            CreateStandardTime(10, 0, 14, 0));

        ShiftTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            ShiftTypeValidationCode.AbbreviationNotAllowedForActualTime,
            error.Code);
    }

    [Fact]
    public void CreateWhenDisplayKindIsUnknownReturnsError()
    {
        ShiftTypeValidationResult result = ShiftType.Create(
            Guid.NewGuid(),
            "Terrassen-Dienst",
            InitialWorkLocationCatalog.Cafeteria.Id,
            (ShiftTypeDisplayKind)999,
            null,
            CreateStandardTime(10, 0, 14, 0));

        ShiftTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(ShiftTypeValidationCode.UnsupportedDisplayKind, error.Code);
    }

    [Fact]
    public void WithStandardTimeWhenTimeIsValidPreservesIdentityAndDefinition()
    {
        ShiftType original = Assert.IsType<ShiftType>(
            ShiftType.Create(
                new Guid("d70dcb6d-6494-48ec-8b55-b5f2e8fc406c"),
                "Mittagsdienst",
                InitialWorkLocationCatalog.Restaurant.Id,
                ShiftTypeDisplayKind.Abbreviation,
                "M",
                CreateStandardTime(9, 0, 12, 0)).Value);

        ShiftTypeValidationResult result = original.WithStandardTime(
            CreateStandardTime(9, 30, 12, 30));

        ShiftType changed = Assert.IsType<ShiftType>(result.Value);
        Assert.Equal(original.Id, changed.Id);
        Assert.Equal(original.Name, changed.Name);
        Assert.Equal(original.WorkLocationId, changed.WorkLocationId);
        Assert.Equal(original.Display, changed.Display);
        Assert.Equal(new TimeOnly(9, 30), changed.StandardTime.Start);
        Assert.Equal(new TimeOnly(12, 30), changed.StandardTime.End);
        Assert.Equal(new TimeOnly(9, 0), original.StandardTime.Start);
    }

    [Fact]
    public void WithStandardTimeWhenTimeIsMissingReturnsError()
    {
        ShiftType original = Assert.IsType<ShiftType>(
            ShiftType.Create(
                new Guid("32db40e0-c59a-44bd-8788-a72147a2b9fc"),
                "Mittagsdienst",
                InitialWorkLocationCatalog.Restaurant.Id,
                ShiftTypeDisplayKind.Abbreviation,
                "M",
                CreateStandardTime(9, 0, 12, 0)).Value);

        ShiftTypeValidationResult result = original.WithStandardTime(null);

        Assert.False(result.IsSuccess);
        ShiftTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(ShiftTypeValidationCode.StandardTimeRequired, error.Code);
        Assert.Equal(new TimeOnly(9, 0), original.StandardTime.Start);
    }

    private static ShiftStandardTime CreateStandardTime(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        return Assert.IsType<ShiftStandardTime>(
            ShiftStandardTime.Create(
                new TimeOnly(startHour, startMinute),
                new TimeOnly(endHour, endMinute)).Value);
    }
}
