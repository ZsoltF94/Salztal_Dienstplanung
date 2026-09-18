using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ScheduleEmployeeRowViewModel
{
    public ScheduleEmployeeRowViewModel(
        Guid employeeId,
        string displayName,
        string employeeTypeCode,
        string employeeTypeName,
        bool allowsVacationAndSickness,
        bool isServiceManagement,
        bool showsAuxiliaryBoundary,
        IEnumerable<ScheduleCellViewModel> cells,
        IEnumerable<ScheduleWeekSummaryViewModel> weeks)
    {
        EmployeeId = employeeId;
        DisplayName = displayName;
        EmployeeTypeCode = employeeTypeCode;
        EmployeeTypeName = employeeTypeName;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        IsServiceManagement = isServiceManagement;
        ShowsAuxiliaryBoundary = showsAuxiliaryBoundary;
        Cells = Array.AsReadOnly(cells.ToArray());
        Weeks = Array.AsReadOnly(weeks.ToArray());
    }

    public Guid EmployeeId { get; }

    public string DisplayName { get; }

    public string EmployeeTypeCode { get; }

    public string EmployeeTypeName { get; }

    public bool AllowsVacationAndSickness { get; }

    public bool IsServiceManagement { get; }

    public bool ShowsAuxiliaryBoundary { get; }

    public string EmployeeTypeDisplay => $"{EmployeeTypeCode} – {EmployeeTypeName}";

    public ReadOnlyCollection<ScheduleCellViewModel> Cells { get; }

    public ReadOnlyCollection<ScheduleWeekSummaryViewModel> Weeks { get; }
}
