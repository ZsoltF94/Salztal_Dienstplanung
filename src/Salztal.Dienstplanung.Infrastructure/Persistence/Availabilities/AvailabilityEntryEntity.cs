using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;

internal sealed class AvailabilityEntryEntity
{
    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public int Kind { get; set; }

    public long ChangeVersion { get; set; }

    public EmployeeEntity Employee { get; set; } = null!;
}
