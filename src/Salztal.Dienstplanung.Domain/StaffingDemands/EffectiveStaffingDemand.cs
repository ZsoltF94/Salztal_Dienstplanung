using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class EffectiveStaffingDemand
{
    private EffectiveStaffingDemand(
        StaffingDemandId sourceId,
        StaffingDemandSourceKind sourceKind,
        DateOnly date,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId,
        StaffingDemandTime actualTime,
        RequiredEmployeeCount requiredEmployeeCount,
        long requiredWorkMinutes)
    {
        SourceId = sourceId;
        SourceKind = sourceKind;
        Date = date;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
        ActualTime = actualTime;
        RequiredEmployeeCount = requiredEmployeeCount;
        RequiredWorkMinutes = requiredWorkMinutes;
    }

    public StaffingDemandId SourceId { get; }

    public StaffingDemandSourceKind SourceKind { get; }

    public DateOnly Date { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeId ShiftTypeId { get; }

    public StaffingDemandTime ActualTime { get; }

    public RequiredEmployeeCount RequiredEmployeeCount { get; }

    public int DurationMinutes => ActualTime.DurationMinutes;

    public long RequiredWorkMinutes { get; }

    internal static EffectiveStaffingDemand FromStandard(
        DateOnly date,
        StandardStaffingDemandRevision revision)
    {
        return new EffectiveStaffingDemand(
            revision.Id,
            StaffingDemandSourceKind.Standard,
            date,
            revision.Key.WorkLocationId,
            revision.Key.ShiftTypeId,
            revision.ActualTime!,
            revision.RequiredEmployeeCount!,
            revision.RequiredWorkMinutes!.Value);
    }

    internal static EffectiveStaffingDemand FromDateException(
        StaffingDemandDateException dateException)
    {
        return new EffectiveStaffingDemand(
            dateException.Id,
            StaffingDemandSourceKind.DateException,
            dateException.Key.Date,
            dateException.Key.WorkLocationId,
            dateException.Key.ShiftTypeId,
            dateException.ActualTime!,
            dateException.RequiredEmployeeCount!,
            dateException.RequiredWorkMinutes!.Value);
    }
}
