using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed record AvailabilityEntrySetValidationError(
    AvailabilityEntrySetValidationCode Code,
    EmployeeId EmployeeId,
    DateOnly Date);
