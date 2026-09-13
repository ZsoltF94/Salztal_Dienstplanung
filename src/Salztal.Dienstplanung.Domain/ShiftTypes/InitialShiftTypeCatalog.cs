using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public static class InitialShiftTypeCatalog
{
    private static readonly ReadOnlyCollection<ShiftType> InitialShiftTypes = Array.AsReadOnly(
        new[]
        {
            CreateInitial(
                new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"),
                "Frühdienst",
                InitialWorkLocationCatalog.Restaurant.Id,
                ShiftTypeDisplayKind.Abbreviation,
                "F",
                new TimeOnly(6, 30),
                new TimeOnly(13, 30)),
            CreateInitial(
                new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"),
                "Spätdienst",
                InitialWorkLocationCatalog.Restaurant.Id,
                ShiftTypeDisplayKind.Abbreviation,
                "S",
                new TimeOnly(16, 30),
                new TimeOnly(19, 30)),
            CreateInitial(
                new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"),
                "Cafeteria-Dienst A",
                InitialWorkLocationCatalog.Cafeteria.Id,
                ShiftTypeDisplayKind.ActualTime,
                null,
                new TimeOnly(13, 30),
                new TimeOnly(20, 30)),
            CreateInitial(
                new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"),
                "Cafeteria-Dienst B",
                InitialWorkLocationCatalog.Cafeteria.Id,
                ShiftTypeDisplayKind.ActualTime,
                null,
                new TimeOnly(13, 30),
                new TimeOnly(17, 30)),
        });

    public static ShiftType EarlyShift => InitialShiftTypes[0];

    public static ShiftType LateShift => InitialShiftTypes[1];

    public static ShiftType CafeteriaShiftA => InitialShiftTypes[2];

    public static ShiftType CafeteriaShiftB => InitialShiftTypes[3];

    public static IReadOnlyList<ShiftType> All => InitialShiftTypes;

    private static ShiftType CreateInitial(
        Guid id,
        string name,
        WorkLocationId workLocationId,
        ShiftTypeDisplayKind displayKind,
        string? abbreviation,
        TimeOnly start,
        TimeOnly end)
    {
        ShiftStandardTime standardTime = ShiftStandardTime.Create(start, end).Value
            ?? throw new InvalidOperationException("An initial shift standard time is invalid.");

        ShiftTypeValidationResult result = ShiftType.Create(
            id,
            name,
            workLocationId,
            displayKind,
            abbreviation,
            standardTime);

        return result.Value
            ?? throw new InvalidOperationException("An initial shift type is invalid.");
    }
}
