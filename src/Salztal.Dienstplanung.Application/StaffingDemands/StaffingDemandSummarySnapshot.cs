namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed record StaffingDemandDayWorkLocationSummarySnapshot(
    DateOnly Date,
    Guid WorkLocationId,
    string WorkLocationName,
    string WorkLocationColorCode,
    long RequiredWorkMinutes);

public sealed record StaffingDemandWorkLocationWeekSummarySnapshot(
    Guid WorkLocationId,
    string WorkLocationName,
    string WorkLocationColorCode,
    long RequiredWorkMinutes);
