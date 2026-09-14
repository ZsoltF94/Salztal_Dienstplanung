using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class EmployeeTypeCatalogSnapshot
{
    internal EmployeeTypeCatalogSnapshot(IEnumerable<EmployeeTypeSnapshot> employeeTypes)
    {
        EmployeeTypes = Array.AsReadOnly(employeeTypes.ToArray());
    }

    public ReadOnlyCollection<EmployeeTypeSnapshot> EmployeeTypes { get; }
}
