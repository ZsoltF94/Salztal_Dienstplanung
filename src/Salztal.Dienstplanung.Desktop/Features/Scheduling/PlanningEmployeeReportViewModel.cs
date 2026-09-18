using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class PlanningEmployeeReportViewModel : ObservableObject
{
    private readonly AutomaticSchedulePlanningReport _report;
    private PlanningEmployeeReportWeekViewModel _selectedWeek;
    private PlanningEmployeeGroupFilterViewModel _selectedGroup;
    private ReadOnlyCollection<PlanningEmployeeReportRowViewModel> _employees;

    public PlanningEmployeeReportViewModel(AutomaticSchedulePlanningReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        _report = report;
        ServiceColumns = Array.AsReadOnly(report.ServiceColumns
            .Select(value => new PlanningEmployeeServiceColumnViewModel(value))
            .ToArray());
        Weeks = Array.AsReadOnly(report.EmployeeWeekSummaries
            .Select((summary, index) => new PlanningEmployeeReportWeekViewModel(
                index + 1,
                summary,
                report.ServiceColumns))
            .ToArray());
        Groups = Array.AsReadOnly(new[]
        {
            new PlanningEmployeeGroupFilterViewModel(null, "Alle Personengruppen"),
            new PlanningEmployeeGroupFilterViewModel(
                EmployeeTypePlanningRoleKind.ServiceManagement,
                "Typ1"),
            new PlanningEmployeeGroupFilterViewModel(
                EmployeeTypePlanningRoleKind.Normal,
                "Regulär"),
            new PlanningEmployeeGroupFilterViewModel(
                EmployeeTypePlanningRoleKind.Auxiliary,
                "AH"),
        });
        _selectedWeek = Weeks[0];
        _selectedGroup = Groups[0];
        _employees = CreateEmployees();
    }

    public ReadOnlyCollection<PlanningEmployeeServiceColumnViewModel> ServiceColumns
    {
        get;
    }

    public ReadOnlyCollection<PlanningEmployeeReportWeekViewModel> Weeks { get; }

    public ReadOnlyCollection<PlanningEmployeeGroupFilterViewModel> Groups { get; }

    public PlanningEmployeeReportWeekViewModel SelectedWeek
    {
        get => _selectedWeek;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (!Weeks.Contains(value))
            {
                throw new ArgumentException(
                    "The selected week must belong to this report.",
                    nameof(value));
            }

            if (SetProperty(ref _selectedWeek, value))
            {
                Employees = CreateEmployees();
                OnPropertyChanged(nameof(SelectedWeekSummary));
            }
        }
    }

    public PlanningEmployeeGroupFilterViewModel SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (!Groups.Contains(value))
            {
                throw new ArgumentException(
                    "The selected group must belong to this report.",
                    nameof(value));
            }

            if (SetProperty(ref _selectedGroup, value))
            {
                Employees = CreateEmployees();
            }
        }
    }

    public ReadOnlyCollection<PlanningEmployeeReportRowViewModel> Employees
    {
        get => _employees;
        private set => SetProperty(ref _employees, value);
    }

    public PlanningEmployeeWeekSummaryViewModel SelectedWeekSummary =>
        SelectedWeek.Summary;

    private ReadOnlyCollection<PlanningEmployeeReportRowViewModel> CreateEmployees() =>
        Array.AsReadOnly(_report.EmployeeWeeks
            .Where(value => value.WeekMonday == SelectedWeek.Summary.WeekMonday)
            .Where(value => SelectedGroup.Role is null
                || value.PlanningRole == SelectedGroup.Role)
            .Select(value => new PlanningEmployeeReportRowViewModel(
                value,
                _report.ServiceColumns))
            .ToArray());
}

internal sealed record PlanningEmployeeGroupFilterViewModel(
    EmployeeTypePlanningRoleKind? Role,
    string Display);
