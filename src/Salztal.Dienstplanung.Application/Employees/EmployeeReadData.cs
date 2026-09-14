using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class EmployeeReadData
{
    public EmployeeReadData(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeType> employeeTypes,
        ServiceCatalogData serviceCatalog)
    {
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(employeeTypes);
        ArgumentNullException.ThrowIfNull(serviceCatalog);

        Employees = Array.AsReadOnly(employees.ToArray());
        EmployeeTypes = Array.AsReadOnly(employeeTypes.ToArray());
        ServiceCatalog = serviceCatalog;
    }

    public ReadOnlyCollection<Employee> Employees { get; }

    public ReadOnlyCollection<EmployeeType> EmployeeTypes { get; }

    public ServiceCatalogData ServiceCatalog { get; }
}
