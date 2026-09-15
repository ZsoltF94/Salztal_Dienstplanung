using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Desktop.Features.Employees;

namespace Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;

internal sealed class EmployeeTypeOverviewItemViewModel
{
    public EmployeeTypeOverviewItemViewModel(EmployeeTypeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Snapshot = snapshot;
        Id = snapshot.Id;
        Code = snapshot.Code;
        Name = snapshot.Name;
        WeeklyWorkTargetDisplay = snapshot.WeeklyWorkTargetDisplay;
        PlanningRole = snapshot.PlanningRole;
        PlanningRoleDisplay = snapshot.PlanningRole switch
        {
            EmployeeTypePlanningRoleKind.Normal => "Normaler Mitarbeitertyp",
            EmployeeTypePlanningRoleKind.ServiceManagement => "Serviceleitung (geschützt)",
            EmployeeTypePlanningRoleKind.Auxiliary => "Aushilfe (geschützt)",
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot)),
        };
        AbsenceDisplay = snapshot.AllowsVacationAndSickness
            ? $"U/K zulässig – Tageswert {EmployeeTypeDurationFormatter.Format(snapshot.AbsenceDayValueMinutes!.Value)} Stunden"
            : "U/K nicht zulässig";
        ShiftEligibilities = Array.AsReadOnly(
            snapshot.ShiftEligibilities
                .Select(eligibility => new EmployeeEligibilityItemViewModel(
                    eligibility.TargetName,
                    eligibility.AvailabilityDisplay))
                .ToArray());
    }

    public EmployeeTypeSnapshot Snapshot { get; }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public string WeeklyWorkTargetDisplay { get; }

    public EmployeeTypePlanningRoleKind PlanningRole { get; }

    public string PlanningRoleDisplay { get; }

    public string AbsenceDisplay { get; }

    public ReadOnlyCollection<EmployeeEligibilityItemViewModel> ShiftEligibilities { get; }

    public bool IsNormal => PlanningRole == EmployeeTypePlanningRoleKind.Normal;
}
