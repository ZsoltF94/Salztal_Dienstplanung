using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Employees;

namespace Salztal.Dienstplanung.Desktop.Features.Employees;

internal sealed class EmployeeTypeOptionViewModel
{
    public EmployeeTypeOptionViewModel(EmployeeTypeSnapshot employeeType)
    {
        ArgumentNullException.ThrowIfNull(employeeType);

        Id = employeeType.Id;
        Code = employeeType.Code;
        Name = employeeType.Name;
        WeeklyWorkTargetDisplay = employeeType.WeeklyWorkTargetDisplay;
        SelectionDisplay = $"{Code} – {Name} ({WeeklyWorkTargetDisplay})";
        ShiftEligibilities = Array.AsReadOnly(
            employeeType.ShiftEligibilities
                .Select(eligibility => new EmployeeEligibilityItemViewModel(
                    eligibility.TargetName,
                    eligibility.AvailabilityDisplay))
                .ToArray());
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public string WeeklyWorkTargetDisplay { get; }

    public string SelectionDisplay { get; }

    public ReadOnlyCollection<EmployeeEligibilityItemViewModel> ShiftEligibilities { get; }
}
