using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.ShiftTypes;

public sealed class InitialShiftTypeCatalogTests
{
    [Fact]
    public void AllWhenReadContainsConfirmedNormalShiftTypes()
    {
        Assert.Collection(
            InitialShiftTypeCatalog.All,
            earlyShift => AssertShiftType(
                earlyShift,
                new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"),
                "Frühdienst",
                InitialWorkLocationCatalog.Restaurant.Id,
                ShiftTypeDisplayKind.Abbreviation,
                "F",
                new TimeOnly(6, 30),
                new TimeOnly(13, 30)),
            lateShift => AssertShiftType(
                lateShift,
                new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"),
                "Spätdienst",
                InitialWorkLocationCatalog.Restaurant.Id,
                ShiftTypeDisplayKind.Abbreviation,
                "S",
                new TimeOnly(16, 30),
                new TimeOnly(19, 30)),
            cafeteriaShiftA => AssertShiftType(
                cafeteriaShiftA,
                new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"),
                "Cafeteria-Dienst A",
                InitialWorkLocationCatalog.Cafeteria.Id,
                ShiftTypeDisplayKind.ActualTime,
                null,
                new TimeOnly(13, 30),
                new TimeOnly(20, 30)),
            cafeteriaShiftB => AssertShiftType(
                cafeteriaShiftB,
                new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"),
                "Cafeteria-Dienst B",
                InitialWorkLocationCatalog.Cafeteria.Id,
                ShiftTypeDisplayKind.ActualTime,
                null,
                new TimeOnly(13, 30),
                new TimeOnly(17, 30)));
    }

    [Fact]
    public void AllWhenReadContainsUniqueStableIdentifiers()
    {
        Guid[] identifiers = InitialShiftTypeCatalog.All
            .Select(shiftType => shiftType.Id.Value)
            .ToArray();

        Assert.Equal(4, identifiers.Distinct().Count());
        Assert.DoesNotContain(Guid.Empty, identifiers);
    }

    [Fact]
    public void InitialShiftTypeWhenStandardTimeChangesLeavesCatalogValueUnchanged()
    {
        ShiftStandardTime changedTime = Assert.IsType<ShiftStandardTime>(
            ShiftStandardTime.Create(
                new TimeOnly(7, 0),
                new TimeOnly(13, 0)).Value);

        ShiftTypeValidationResult result = InitialShiftTypeCatalog.EarlyShift
            .WithStandardTime(changedTime);

        ShiftType changed = Assert.IsType<ShiftType>(result.Value);
        Assert.Equal(new TimeOnly(7, 0), changed.StandardTime.Start);
        Assert.Equal(
            new TimeOnly(6, 30),
            InitialShiftTypeCatalog.EarlyShift.StandardTime.Start);
    }

    private static void AssertShiftType(
        ShiftType shiftType,
        Guid expectedId,
        string expectedName,
        WorkLocationId expectedWorkLocationId,
        ShiftTypeDisplayKind expectedDisplayKind,
        string? expectedAbbreviation,
        TimeOnly expectedStart,
        TimeOnly expectedEnd)
    {
        Assert.Equal(expectedId, shiftType.Id.Value);
        Assert.Equal(expectedName, shiftType.Name.Value);
        Assert.Equal(expectedWorkLocationId, shiftType.WorkLocationId);
        Assert.Equal(expectedDisplayKind, shiftType.Display.Kind);
        Assert.Equal(expectedAbbreviation, shiftType.Display.Abbreviation);
        Assert.Equal(expectedStart, shiftType.StandardTime.Start);
        Assert.Equal(expectedEnd, shiftType.StandardTime.End);
    }
}
