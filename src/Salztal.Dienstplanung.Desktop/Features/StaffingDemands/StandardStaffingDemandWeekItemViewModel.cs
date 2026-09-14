namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed record StandardStaffingDemandWeekItemViewModel(
    DayOfWeek DayOfWeek,
    Guid ShiftTypeId,
    string ShiftTypeName,
    string DemandDisplay,
    string? RevisionDisplay);
