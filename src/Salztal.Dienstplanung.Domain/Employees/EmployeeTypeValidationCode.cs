namespace Salztal.Dienstplanung.Domain.Employees;

public enum EmployeeTypeValidationCode
{
    IdentifierRequired,
    CodeRequired,
    NameRequired,
    WeeklyWorkTargetMustBePositive,
    WeeklyWorkTargetExceedsWeek,
    AbsenceDayValueRequired,
    AbsenceDayValueMustNotBeSet,
    AbsenceDayValueMustBePositive,
    AbsenceDayValueExceedsDay,
    ShiftEligibilityRequired,
    DuplicateShiftEligibility,
    PlanningPolicyRequired,
}
