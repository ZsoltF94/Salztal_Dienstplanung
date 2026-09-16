using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record DemandSlotSetValidationError(
    DemandSlotSetValidationCode Code,
    StaffingDemandId SourceId,
    DateOnly Date,
    WorkLocationId WorkLocationId,
    ShiftTypeId ShiftTypeId,
    DemandSlotId? SlotId = null);
