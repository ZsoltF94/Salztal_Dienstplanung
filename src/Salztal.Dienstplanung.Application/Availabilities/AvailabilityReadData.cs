using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed class AvailabilityReadData
{
    public AvailabilityReadData(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeType> employeeTypes,
        IEnumerable<AvailabilityEntryReadItem> entries)
    {
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(employeeTypes);
        ArgumentNullException.ThrowIfNull(entries);

        Employees = Array.AsReadOnly(employees.ToArray());
        EmployeeTypes = Array.AsReadOnly(employeeTypes.ToArray());
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public ReadOnlyCollection<Employee> Employees { get; }

    public ReadOnlyCollection<EmployeeType> EmployeeTypes { get; }

    public ReadOnlyCollection<AvailabilityEntryReadItem> Entries { get; }
}
