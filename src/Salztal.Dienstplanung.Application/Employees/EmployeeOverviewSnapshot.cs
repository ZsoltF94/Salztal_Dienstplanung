using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class EmployeeOverviewSnapshot
{
    internal EmployeeOverviewSnapshot(IEnumerable<EmployeeOverviewItemSnapshot> employees)
    {
        Employees = Array.AsReadOnly(employees.ToArray());
    }

    public ReadOnlyCollection<EmployeeOverviewItemSnapshot> Employees { get; }
}
