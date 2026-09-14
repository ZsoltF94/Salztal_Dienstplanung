using System.Windows;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class MainWindow : Window
{
    private readonly ServiceCatalogViewModel _serviceCatalog;
    private readonly EmployeeOverviewViewModel _employees;

    public MainWindow(
        ServiceCatalogViewModel serviceCatalog,
        EmployeeOverviewViewModel employees)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);

        _serviceCatalog = serviceCatalog;
        _employees = employees;
        InitializeComponent();
        DataContext = new MainWindowViewModel(serviceCatalog, employees);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await Task.WhenAll(
            _serviceCatalog.LoadCommand.ExecuteAsync(null),
            _employees.LoadCommand.ExecuteAsync(null));
    }
}
