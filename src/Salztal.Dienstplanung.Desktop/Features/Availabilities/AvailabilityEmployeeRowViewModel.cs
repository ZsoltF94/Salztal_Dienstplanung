using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Desktop.Features.Availabilities;

internal sealed class AvailabilityEmployeeRowViewModel
{
    public AvailabilityEmployeeRowViewModel(
        Guid employeeId,
        string displayName,
        string employeeTypeCode,
        string employeeTypeName,
        bool allowsVacationAndSickness,
        IEnumerable<AvailabilityCellViewModel> cells,
        IEnumerable<AvailabilityWeekSummaryViewModel> weeks)
    {
        EmployeeId = employeeId;
        DisplayName = displayName;
        EmployeeTypeCode = employeeTypeCode;
        EmployeeTypeName = employeeTypeName;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        Cells = Array.AsReadOnly(cells.ToArray());
        Weeks = Array.AsReadOnly(weeks.ToArray());
    }

    public Guid EmployeeId { get; }

    public string DisplayName { get; }

    public string EmployeeTypeCode { get; }

    public string EmployeeTypeName { get; }

    public bool AllowsVacationAndSickness { get; }

    public string EmployeeTypeDisplay => $"{EmployeeTypeCode} – {EmployeeTypeName}";

    public ReadOnlyCollection<AvailabilityCellViewModel> Cells { get; }

    public ReadOnlyCollection<AvailabilityWeekSummaryViewModel> Weeks { get; }
}
