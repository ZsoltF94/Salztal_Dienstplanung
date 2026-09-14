using System.Diagnostics.CodeAnalysis;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StandardStaffingDemandKey
{
    private StandardStaffingDemandKey(
        DayOfWeek dayOfWeek,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId)
    {
        DayOfWeek = dayOfWeek;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
    }

    public DayOfWeek DayOfWeek { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeId ShiftTypeId { get; }

    internal static IReadOnlyList<StandardStaffingDemandRevisionValidationCode> TryCreate(
        DayOfWeek dayOfWeek,
        Guid workLocationId,
        Guid shiftTypeId,
        [NotNullWhen(true)] out StandardStaffingDemandKey? key)
    {
        List<StandardStaffingDemandRevisionValidationCode> errors = [];

        if (!Enum.IsDefined(dayOfWeek))
        {
            errors.Add(StandardStaffingDemandRevisionValidationCode.UnsupportedDayOfWeek);
        }

        if (!WorkLocationId.TryCreate(
                workLocationId,
                out WorkLocationId? validatedWorkLocationId))
        {
            errors.Add(StandardStaffingDemandRevisionValidationCode.WorkLocationRequired);
        }

        if (!ShiftTypeId.TryCreate(shiftTypeId, out ShiftTypeId? validatedShiftTypeId))
        {
            errors.Add(StandardStaffingDemandRevisionValidationCode.ShiftTypeRequired);
        }

        if (errors.Count > 0)
        {
            key = null;
            return Array.AsReadOnly(errors.ToArray());
        }

        key = new StandardStaffingDemandKey(
            dayOfWeek,
            validatedWorkLocationId!,
            validatedShiftTypeId!);
        return Array.AsReadOnly(
            Array.Empty<StandardStaffingDemandRevisionValidationCode>());
    }
}
