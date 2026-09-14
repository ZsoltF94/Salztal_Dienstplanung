using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Employees;

public enum EmployeeCommandStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    EmployeeTypeNotFound,
    EmployeeTypeCatalogInvalid,
    ActiveType1Conflict,
    AlreadyActive,
    MustBeInactive,
    Referenced,
    Conflict,
}

public enum EmployeeCommandErrorCode
{
    IdentifierRequired,
    FirstNameRequired,
    LastNameRequired,
    EmployeeTypeRequired,
    NotFound,
    EmployeeTypeNotFound,
    EmployeeTypeCatalogInvalid,
    ActiveType1Conflict,
    AlreadyActive,
    MustBeInactive,
    Referenced,
    Conflict,
}

public sealed record EmployeeCommandError(
    EmployeeCommandErrorCode Code,
    string Message);

public sealed class EmployeeCommandResult
{
    private EmployeeCommandResult(
        EmployeeCommandStatus status,
        EmployeeDetailsSnapshot? value,
        ReadOnlyCollection<EmployeeCommandError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public EmployeeCommandStatus Status { get; }

    public EmployeeDetailsSnapshot? Value { get; }

    public IReadOnlyList<EmployeeCommandError> Errors { get; }

    internal static EmployeeCommandResult Success(EmployeeDetailsSnapshot value)
    {
        return new EmployeeCommandResult(
            EmployeeCommandStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<EmployeeCommandError>()));
    }

    internal static EmployeeCommandResult Failure(
        EmployeeCommandStatus status,
        IEnumerable<EmployeeCommandError> errors)
    {
        return new EmployeeCommandResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
