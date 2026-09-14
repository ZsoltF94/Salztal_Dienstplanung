namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed record StaffingDemandShiftTypeOption(
    Guid Id,
    Guid WorkLocationId,
    string Name);
