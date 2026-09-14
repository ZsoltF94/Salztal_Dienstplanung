namespace Salztal.Dienstplanung.Application.Employees;

public sealed record EmployeeTypeEligibilitySnapshot(
    Guid TargetId,
    string TargetName,
    bool IsShiftPattern,
    string AvailabilityDisplay);
