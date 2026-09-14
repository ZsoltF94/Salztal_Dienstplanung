using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Employees;

namespace Salztal.Dienstplanung.Desktop.Features.Employees;

internal sealed class EmployeeOverviewItemViewModel
{
    public EmployeeOverviewItemViewModel(
        EmployeeOverviewItemSnapshot employee,
        EmployeeTypeSnapshot employeeType)
        : this(
            (employee ?? throw new ArgumentNullException(nameof(employee))).Id,
            employee.FirstName,
            employee.LastName,
            employee.DisplayName,
            employee.IsActive,
            employeeType ?? throw new ArgumentNullException(nameof(employeeType)))
    {
    }

    public EmployeeOverviewItemViewModel(EmployeeDetailsSnapshot employee)
        : this(
            (employee ?? throw new ArgumentNullException(nameof(employee))).Id,
            employee.FirstName,
            employee.LastName,
            employee.DisplayName,
            employee.IsActive,
            employee.EmployeeType)
    {
    }

    private EmployeeOverviewItemViewModel(
        Guid id,
        string firstName,
        string lastName,
        string displayName,
        bool isActive,
        EmployeeTypeSnapshot employeeType)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        IsActive = isActive;
        StatusDisplay = isActive ? "Aktiv" : "Deaktiviert";
        EmployeeTypeCode = employeeType.Code;
        EmployeeTypeName = employeeType.Name;
        WeeklyWorkTargetDisplay = employeeType.WeeklyWorkTargetDisplay;
        ShiftEligibilities = Array.AsReadOnly(
            employeeType.ShiftEligibilities
                .Select(eligibility => new EmployeeEligibilityItemViewModel(
                    eligibility.TargetName,
                    eligibility.AvailabilityDisplay))
                .ToArray());
        EligibilitySummary = string.Join(
            "; ",
            ShiftEligibilities.Select(
                eligibility => $"{eligibility.TargetName}: {eligibility.AvailabilityDisplay}"));
    }

    public Guid Id { get; }

    public string FirstName { get; }

    public string LastName { get; }

    public string DisplayName { get; }

    public bool IsActive { get; }

    public bool IsInactive => !IsActive;

    public string StatusDisplay { get; }

    public string EmployeeTypeCode { get; }

    public string EmployeeTypeName { get; }

    public string WeeklyWorkTargetDisplay { get; }

    public ReadOnlyCollection<EmployeeEligibilityItemViewModel> ShiftEligibilities { get; }

    public string EligibilitySummary { get; }
}
