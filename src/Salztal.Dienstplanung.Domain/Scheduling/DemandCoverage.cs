using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record DemandCoverage
{
    internal DemandCoverage(
        DemandSlotId slotId,
        StaffingDemandTime coveredTime,
        DemandCoverageKind kind)
    {
        SlotId = slotId;
        CoveredTime = coveredTime;
        Kind = kind;
    }

    public DemandSlotId SlotId { get; }

    public StaffingDemandTime CoveredTime { get; }

    public DemandCoverageKind Kind { get; }

    public int CoveredMinutes => CoveredTime.DurationMinutes;

    internal static DemandCoverage Full(DemandSlot slot)
    {
        return new DemandCoverage(
            slot.Id,
            slot.ActualTime,
            DemandCoverageKind.Full);
    }
}
