namespace Salztal.Dienstplanung.Domain.Employees;

public enum EmployeeTypeValidationCode
{
    IdentifierRequired,
    CodeRequired,
    NameRequired,
    WeeklyWorkTargetMustBePositive,
    WeeklyWorkTargetExceedsWeek,
}
