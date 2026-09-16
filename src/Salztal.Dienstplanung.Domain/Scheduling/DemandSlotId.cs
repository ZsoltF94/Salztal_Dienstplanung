using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record DemandSlotId
{
    internal DemandSlotId(
        StaffingDemandId sourceId,
        StaffingDemandSourceKind sourceKind,
        DateOnly date,
        WorkLocationId workLocationId,
        ShiftTypeId shiftTypeId,
        int ordinal)
    {
        SourceId = sourceId;
        SourceKind = sourceKind;
        Date = date;
        WorkLocationId = workLocationId;
        ShiftTypeId = shiftTypeId;
        Ordinal = ordinal;
    }

    public StaffingDemandId SourceId { get; }

    public StaffingDemandSourceKind SourceKind { get; }

    public DateOnly Date { get; }

    public WorkLocationId WorkLocationId { get; }

    public ShiftTypeId ShiftTypeId { get; }

    public int Ordinal { get; }

    internal static DemandSlotId CreateValidated(
        EffectiveStaffingDemand demand,
        int ordinal)
    {
        return new DemandSlotId(
            demand.SourceId,
            demand.SourceKind,
            demand.Date,
            demand.WorkLocationId,
            demand.ShiftTypeId,
            ordinal);
    }
}
