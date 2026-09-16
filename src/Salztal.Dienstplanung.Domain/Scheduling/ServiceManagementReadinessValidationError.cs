using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ServiceManagementReadinessValidationError(
    ServiceManagementReadinessValidationCode Code,
    DateOnly WeekMonday,
    EmployeeId? EmployeeId = null);
