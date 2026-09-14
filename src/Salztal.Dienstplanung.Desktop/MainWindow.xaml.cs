using System.Windows;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class MainWindow : Window
{
    private readonly ServiceCatalogViewModel _serviceCatalog;
    private readonly EmployeeOverviewViewModel _employees;
    private readonly StaffingDemandOverviewViewModel _staffingDemands;
    private readonly StandardStaffingDemandEditorViewModel _standardStaffingDemands;

    public MainWindow(
        ServiceCatalogViewModel serviceCatalog,
        EmployeeOverviewViewModel employees,
        StaffingDemandOverviewViewModel staffingDemands,
        StandardStaffingDemandEditorViewModel standardStaffingDemands)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(staffingDemands);
        ArgumentNullException.ThrowIfNull(standardStaffingDemands);

        _serviceCatalog = serviceCatalog;
        _employees = employees;
        _staffingDemands = staffingDemands;
        _standardStaffingDemands = standardStaffingDemands;
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            serviceCatalog,
            employees,
            staffingDemands,
            standardStaffingDemands);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await Task.WhenAll(
            _serviceCatalog.LoadCommand.ExecuteAsync(null),
            _employees.LoadCommand.ExecuteAsync(null),
            _staffingDemands.LoadCommand.ExecuteAsync(null),
            _standardStaffingDemands.LoadEffectiveWeekCommand.ExecuteAsync(null));
    }
}
