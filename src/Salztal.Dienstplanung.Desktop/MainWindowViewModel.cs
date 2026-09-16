using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
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
        ScheduleOverviewViewModel schedule)
    {
        ArgumentNullException.ThrowIfNull(serviceCatalog);
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(employeeTypes);
        ArgumentNullException.ThrowIfNull(staffingDemands);
        ArgumentNullException.ThrowIfNull(standardStaffingDemands);
        ArgumentNullException.ThrowIfNull(schedule);

        ServiceCatalog = serviceCatalog;
        Employees = employees;
        EmployeeTypes = employeeTypes;
        StaffingDemands = staffingDemands;
        StandardStaffingDemands = standardStaffingDemands;
        Schedule = schedule;
    }

    public ServiceCatalogViewModel ServiceCatalog { get; }

    public EmployeeOverviewViewModel Employees { get; }

    public EmployeeTypeOverviewViewModel EmployeeTypes { get; }

    public StaffingDemandOverviewViewModel StaffingDemands { get; }

    public StandardStaffingDemandEditorViewModel StandardStaffingDemands { get; }

    public ScheduleOverviewViewModel Schedule { get; }
}
