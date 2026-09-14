using System.Diagnostics.CodeAnalysis;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StaffingDemandDateKey
{
    private StaffingDemandDateKey(
        DateOnly date,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId)
    {
        Date = date;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
    }

    public DateOnly Date { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeId ShiftTypeId { get; }

    internal static IReadOnlyList<StaffingDemandDateExceptionValidationCode> TryCreate(
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        [NotNullWhen(true)] out StaffingDemandDateKey? key)
    {
        List<StaffingDemandDateExceptionValidationCode> errors = [];

        if (!WorkLocationId.TryCreate(
                workLocationId,
                out WorkLocationId? validatedWorkLocationId))
        {
            errors.Add(StaffingDemandDateExceptionValidationCode.WorkLocationRequired);
        }

        if (!ShiftTypeId.TryCreate(shiftTypeId, out ShiftTypeId? validatedShiftTypeId))
        {
            errors.Add(StaffingDemandDateExceptionValidationCode.ShiftTypeRequired);
        }

        if (errors.Count > 0)
        {
            key = null;
            return Array.AsReadOnly(errors.ToArray());
        }

        key = new StaffingDemandDateKey(
            date,
            validatedWorkLocationId!,
            validatedShiftTypeId!);
        return Array.AsReadOnly(
            Array.Empty<StaffingDemandDateExceptionValidationCode>());
    }

    internal static StaffingDemandDateKey CreateValidated(
        DateOnly date,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId)
    {
        return new StaffingDemandDateKey(date, workLocationId, shiftTypeId);
    }
}
