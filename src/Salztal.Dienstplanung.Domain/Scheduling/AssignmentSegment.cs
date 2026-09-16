using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record AssignmentSegment
{
    internal AssignmentSegment(
        DemandSlotId anchorSlotId,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId,
        DateOnly date,
        StaffingDemandTime actualTime)
    {
        AnchorSlotId = anchorSlotId;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
        Date = date;
        ActualTime = actualTime;
    }

    public DemandSlotId AnchorSlotId { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeId ShiftTypeId { get; }

    public DateOnly Date { get; }

    public StaffingDemandTime ActualTime { get; }

    public int WorkMinutes => ActualTime.DurationMinutes;

    internal static AssignmentSegment FromFullSlot(DemandSlot slot)
    {
        return new AssignmentSegment(
            slot.Id,
            slot.Id.WorkLocationId,
            slot.Id.ShiftTypeId,
            slot.Id.Date,
            slot.ActualTime);
    }
}
