using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

namespace Salztal.Dienstplanung.Desktop;

internal sealed class MainWindowViewModel
{
    public MainWindowViewModel(
        ServiceCatalogViewModel serviceCatalog,
        EmployeeOverviewViewModel employees,
        StaffingDemandOverviewViewModel staffingDemands,
        StandardStaffingDemandEditorViewModel standardStaffingDemands)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(staffingDemands);
        ArgumentNullException.ThrowIfNull(standardStaffingDemands);

        ServiceCatalog = serviceCatalog;
        Employees = employees;
        StaffingDemands = staffingDemands;
        StandardStaffingDemands = standardStaffingDemands;
    }

    public ServiceCatalogViewModel ServiceCatalog { get; }

    public EmployeeOverviewViewModel Employees { get; }

    public StaffingDemandOverviewViewModel StaffingDemands { get; }

    public StandardStaffingDemandEditorViewModel StandardStaffingDemands { get; }
}
