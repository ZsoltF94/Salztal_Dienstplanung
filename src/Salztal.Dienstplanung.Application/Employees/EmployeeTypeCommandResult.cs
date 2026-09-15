using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public enum EmployeeTypeCommandStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    DuplicateCode,
    CatalogInvalid,
    ConfirmationRequired,
    Protected,
    Referenced,
    Conflict,
}

public enum EmployeeTypeCommandErrorCode
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
    EligibilityTargetRequired,
    UnknownShiftType,
    UnknownShiftPattern,
    UnsupportedEligibilityMode,
    UnsupportedEligibilityActivation,
    NotFound,
    DuplicateCode,
    CatalogInvalid,
    ConfirmationRequired,
    Protected,
    AssignedEmployee,
    Referenced,
    Conflict,
}

public sealed record EmployeeTypeCommandError(
    EmployeeTypeCommandErrorCode Code,
    string Message);

public sealed class EmployeeTypeCommandResult
{
    private EmployeeTypeCommandResult(
        EmployeeTypeCommandStatus status,
        EmployeeTypeSnapshot? value,
        ReadOnlyCollection<EmployeeTypeCommandError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public EmployeeTypeCommandStatus Status { get; }

    public EmployeeTypeSnapshot? Value { get; }

    public IReadOnlyList<EmployeeTypeCommandError> Errors { get; }

    internal static EmployeeTypeCommandResult Success(EmployeeTypeSnapshot value)
    {
        return new EmployeeTypeCommandResult(
            EmployeeTypeCommandStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<EmployeeTypeCommandError>()));
    }

    internal static EmployeeTypeCommandResult Failure(
        EmployeeTypeCommandStatus status,
        IEnumerable<EmployeeTypeCommandError> errors)
    {
        return new EmployeeTypeCommandResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
