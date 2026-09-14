using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public static class InitialStaffingDemandCatalog
{
    private static readonly DateOnly InitialEffectiveMonday = DateOnly.MinValue;

    private static readonly ReadOnlyCollection<InitialStandardDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                Definition("10000000-0000-4000-8000-000000000001", DayOfWeek.Monday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000002", DayOfWeek.Monday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000003", DayOfWeek.Monday, InitialShiftTypeCatalog.LateShift, 4),
                Definition("10000000-0000-4000-8000-000000000004", DayOfWeek.Tuesday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000005", DayOfWeek.Tuesday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000006", DayOfWeek.Tuesday, InitialShiftTypeCatalog.LateShift, 4),
                Definition("10000000-0000-4000-8000-000000000007", DayOfWeek.Wednesday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000008", DayOfWeek.Wednesday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000009", DayOfWeek.Wednesday, InitialShiftTypeCatalog.LateShift, 4),
                Definition("10000000-0000-4000-8000-000000000010", DayOfWeek.Thursday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000011", DayOfWeek.Thursday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000012", DayOfWeek.Thursday, InitialShiftTypeCatalog.LateShift, 4),
                Definition("10000000-0000-4000-8000-000000000013", DayOfWeek.Friday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000014", DayOfWeek.Friday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000015", DayOfWeek.Friday, InitialShiftTypeCatalog.LateShift, 4),
                Definition("10000000-0000-4000-8000-000000000016", DayOfWeek.Saturday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000017", DayOfWeek.Saturday, InitialShiftTypeCatalog.CafeteriaShiftB, 1),
                Definition("10000000-0000-4000-8000-000000000018", DayOfWeek.Saturday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000019", DayOfWeek.Saturday, InitialShiftTypeCatalog.LateShift, 4),
                Definition("10000000-0000-4000-8000-000000000020", DayOfWeek.Sunday, InitialShiftTypeCatalog.CafeteriaShiftA, 1),
                Definition("10000000-0000-4000-8000-000000000021", DayOfWeek.Sunday, InitialShiftTypeCatalog.CafeteriaShiftB, 1),
                Definition("10000000-0000-4000-8000-000000000022", DayOfWeek.Sunday, InitialShiftTypeCatalog.EarlyShift, 4),
                Definition("10000000-0000-4000-8000-000000000023", DayOfWeek.Sunday, InitialShiftTypeCatalog.LateShift, 4),
            });

    private static readonly StandardStaffingDemandRevisionSet InitialRevisionSet =
        CreateFromCurrentShiftTypes(InitialShiftTypeCatalog.All);

    public static IReadOnlyList<StandardStaffingDemandRevision> All =>
        InitialRevisionSet.Revisions;

    public static StandardStaffingDemandRevisionSet CreateFromCurrentShiftTypes(
        IReadOnlyCollection<ShiftType> currentShiftTypes)
    {
        ArgumentNullException.ThrowIfNull(currentShiftTypes);

        Dictionary<ShiftTypeId, ShiftType> shiftTypesById = currentShiftTypes
            .ToDictionary(shiftType => shiftType.Id);
        List<StandardStaffingDemandRevision> revisions = [];

        foreach (InitialStandardDefinition definition in Definitions)
        {
            if (!shiftTypesById.TryGetValue(definition.ShiftTypeId, out ShiftType? shiftType))
            {
                throw new InvalidOperationException(
                    $"The required initial shift type {definition.ShiftTypeId} is missing.");
            }

            if (shiftType.WorkLocationId != definition.WorkLocationId)
            {
                throw new InvalidOperationException(
                    $"The initial shift type {definition.ShiftTypeId} has an unexpected work location.");
            }

            StandardStaffingDemandRevisionValidationResult result =
                StandardStaffingDemandRevision.CreateAddition(
                    definition.Id,
                    definition.DayOfWeek,
                    definition.WorkLocationId.Value,
                    definition.ShiftTypeId.Value,
                    InitialEffectiveMonday,
                    shiftType.StandardTime.Start,
                    shiftType.StandardTime.End,
                    definition.RequiredEmployeeCount);

            revisions.Add(
                result.Value
                ?? throw new InvalidOperationException(
                    "An initial staffing-demand revision is invalid."));
        }

        StandardStaffingDemandRevisionSetValidationResult revisionSetResult =
            StandardStaffingDemandRevisionSet.Create(revisions);

        return revisionSetResult.Value
            ?? throw new InvalidOperationException(
                "The initial staffing-demand revision set is invalid.");
    }

    private static InitialStandardDefinition Definition(
        string id,
        DayOfWeek dayOfWeek,
        ShiftType shiftType,
        int requiredEmployeeCount)
    {
        return new InitialStandardDefinition(
            new Guid(id),
            dayOfWeek,
            shiftType.WorkLocationId,
            shiftType.Id,
            requiredEmployeeCount);
    }

    private sealed record InitialStandardDefinition(
        Guid Id,
        DayOfWeek DayOfWeek,
        WorkLocationId WorkLocationId,
        ShiftTypeId ShiftTypeId,
        int RequiredEmployeeCount);
}
