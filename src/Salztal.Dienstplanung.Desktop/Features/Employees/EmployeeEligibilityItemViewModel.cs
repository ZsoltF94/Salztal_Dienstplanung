namespace Salztal.Dienstplanung.Desktop.Features.Employees;

internal sealed record EmployeeEligibilityItemViewModel(
    string TargetName,
    string AvailabilityDisplay)
{
    public string SummaryDisplay => $"{TargetName}: {AvailabilityDisplay}";
}
