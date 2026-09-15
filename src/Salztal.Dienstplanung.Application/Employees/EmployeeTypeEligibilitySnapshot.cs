namespace Salztal.Dienstplanung.Application.Employees;

public sealed record EmployeeTypeEligibilitySnapshot(
    Guid TargetId,
    string TargetName,
    EmployeeTypeEligibilityTargetKind TargetKind,
    EmployeeTypeEligibilityMode Mode,
    EmployeeTypeEligibilityActivation Activation,
    string AvailabilityDisplay)
{
    public bool IsShiftPattern =>
        TargetKind == EmployeeTypeEligibilityTargetKind.ShiftPattern;
}
