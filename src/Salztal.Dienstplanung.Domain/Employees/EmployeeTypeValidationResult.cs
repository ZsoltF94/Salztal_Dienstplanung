using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class EmployeeTypeValidationResult
{
    private EmployeeTypeValidationResult(
        EmployeeType? value,
        ReadOnlyCollection<EmployeeTypeValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public EmployeeType? Value { get; }

    public IReadOnlyList<EmployeeTypeValidationError> Errors { get; }

    internal static EmployeeTypeValidationResult Success(EmployeeType value)
    {
        return new EmployeeTypeValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<EmployeeTypeValidationError>()));
    }

    internal static EmployeeTypeValidationResult Failure(
        IEnumerable<EmployeeTypeValidationError> errors)
    {
        return new EmployeeTypeValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
