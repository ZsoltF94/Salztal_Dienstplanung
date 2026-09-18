using System.Windows;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class MainWindow : Window
{
    private readonly ServiceCatalogViewModel _serviceCatalog;
    private readonly EmployeeOverviewViewModel _employees;
    private readonly EmployeeTypeOverviewViewModel _employeeTypes;
    private readonly StaffingDemandOverviewViewModel _staffingDemands;
    private readonly StandardStaffingDemandEditorViewModel _standardStaffingDemands;
    private readonly ScheduleOverviewViewModel _schedule;
    private readonly AutomaticScheduleReportWindowCoordinator _reportCoordinator;

    public MainWindow(
        ServiceCatalogViewModel serviceCatalog,
        EmployeeOverviewViewModel employees,
        EmployeeTypeOverviewViewModel employeeTypes,
        StaffingDemandOverviewViewModel staffingDemands,
        StandardStaffingDemandEditorViewModel standardStaffingDemands,
        ScheduleOverviewViewModel schedule,
        AutomaticScheduleReportWindowCoordinator reportCoordinator)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(employeeTypes);
        ArgumentNullException.ThrowIfNull(staffingDemands);
        ArgumentNullException.ThrowIfNull(standardStaffingDemands);
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(reportCoordinator);

        _serviceCatalog = serviceCatalog;
        _employees = employees;
        _employeeTypes = employeeTypes;
        _staffingDemands = staffingDemands;
        _standardStaffingDemands = standardStaffingDemands;
        _schedule = schedule;
        _reportCoordinator = reportCoordinator;
        InitializeComponent();
        _reportCoordinator.AttachOwner(this);
        Closed += OnClosed;
        DataContext = new MainWindowViewModel(
            serviceCatalog,
            employees,
            employeeTypes,
            staffingDemands,
            standardStaffingDemands,
            schedule);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await Task.WhenAll(
            _serviceCatalog.LoadCommand.ExecuteAsync(null),
            _employees.LoadCommand.ExecuteAsync(null),
            _employeeTypes.LoadCommand.ExecuteAsync(null),
            _staffingDemands.LoadCommand.ExecuteAsync(null),
            _standardStaffingDemands.LoadEffectiveWeekCommand.ExecuteAsync(null),
            _schedule.LoadCommand.ExecuteAsync(null));
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Closed -= OnClosed;
        _reportCoordinator.Close();
    }
}
