using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record DemandSlot
{
    internal DemandSlot(DemandSlotId id, StaffingDemandTime actualTime)
    {
        Id = id;
        ActualTime = actualTime;
    }

    public DemandSlotId Id { get; }

    public StaffingDemandTime ActualTime { get; }

    public int DurationMinutes => ActualTime.DurationMinutes;
}
