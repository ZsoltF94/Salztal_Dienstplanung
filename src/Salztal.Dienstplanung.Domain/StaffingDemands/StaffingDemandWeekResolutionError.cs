namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StaffingDemandWeekResolutionError(
    StaffingDemandWeekResolutionCode Code,
    StaffingDemandDateKey? Key = null);
