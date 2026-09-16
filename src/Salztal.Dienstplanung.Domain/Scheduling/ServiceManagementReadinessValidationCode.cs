namespace Salztal.Dienstplanung.Domain.Scheduling;

public enum ServiceManagementReadinessValidationCode
{
    MissingWeek,
    DuplicateWeek,
    EmployeeMismatch,
    WeekOutsidePeriod,
}
