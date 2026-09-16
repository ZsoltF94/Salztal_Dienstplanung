using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record GeneratedDayOffMarker
{
    public GeneratedDayOffMarker(EmployeeId employeeId, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        EmployeeId = employeeId;
        Date = date;
    }

    public EmployeeId EmployeeId { get; }

    public DateOnly Date { get; }
}
