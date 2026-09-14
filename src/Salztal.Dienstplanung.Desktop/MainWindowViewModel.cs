using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop;

internal sealed class MainWindowViewModel
{
    public MainWindowViewModel(
        ServiceCatalogViewModel serviceCatalog,
        EmployeeOverviewViewModel employees)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);

        ServiceCatalog = serviceCatalog;
        Employees = employees;
    }

    public ServiceCatalogViewModel ServiceCatalog { get; }

    public EmployeeOverviewViewModel Employees { get; }
}
