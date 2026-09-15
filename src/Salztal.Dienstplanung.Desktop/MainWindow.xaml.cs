using System.Windows;
using Salztal.Dienstplanung.Desktop.Features.Availabilities;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
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
    private readonly AvailabilityOverviewViewModel _availabilities;

    public MainWindow(
        ServiceCatalogViewModel serviceCatalog,
        EmployeeOverviewViewModel employees,
        EmployeeTypeOverviewViewModel employeeTypes,
        StaffingDemandOverviewViewModel staffingDemands,
        StandardStaffingDemandEditorViewModel standardStaffingDemands,
        AvailabilityOverviewViewModel availabilities)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(employeeTypes);
        ArgumentNullException.ThrowIfNull(staffingDemands);
        ArgumentNullException.ThrowIfNull(standardStaffingDemands);
        ArgumentNullException.ThrowIfNull(availabilities);

        _serviceCatalog = serviceCatalog;
        _employees = employees;
        _employeeTypes = employeeTypes;
        _staffingDemands = staffingDemands;
        _standardStaffingDemands = standardStaffingDemands;
        _availabilities = availabilities;
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            serviceCatalog,
            employees,
            employeeTypes,
            staffingDemands,
            standardStaffingDemands,
            availabilities);
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
            _availabilities.LoadCommand.ExecuteAsync(null));
    }
}
