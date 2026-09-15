using Salztal.Dienstplanung.Desktop.Features.Availabilities;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

namespace Salztal.Dienstplanung.Desktop;

internal sealed class MainWindowViewModel
{
    public MainWindowViewModel(
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

        ServiceCatalog = serviceCatalog;
        Employees = employees;
        EmployeeTypes = employeeTypes;
        StaffingDemands = staffingDemands;
        StandardStaffingDemands = standardStaffingDemands;
        Availabilities = availabilities;
    }

    public ServiceCatalogViewModel ServiceCatalog { get; }

    public EmployeeOverviewViewModel Employees { get; }

    public EmployeeTypeOverviewViewModel EmployeeTypes { get; }

    public StaffingDemandOverviewViewModel StaffingDemands { get; }

    public StandardStaffingDemandEditorViewModel StandardStaffingDemands { get; }

    public AvailabilityOverviewViewModel Availabilities { get; }
}
