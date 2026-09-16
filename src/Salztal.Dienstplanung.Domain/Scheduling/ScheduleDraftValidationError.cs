using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ScheduleDraftValidationError(
    ScheduleDraftValidationCode Code,
    ScheduleAssignmentId? AssignmentId = null,
    EmployeeId? EmployeeId = null,
    DateOnly? Date = null,
    DemandSlotId? DemandSlotId = null);
