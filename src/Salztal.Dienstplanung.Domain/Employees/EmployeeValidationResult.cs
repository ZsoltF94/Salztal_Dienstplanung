using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class EmployeeValidationResult
{
    private EmployeeValidationResult(
        Employee? value,
        ReadOnlyCollection<EmployeeValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public Employee? Value { get; }

    public IReadOnlyList<EmployeeValidationError> Errors { get; }

    internal static EmployeeValidationResult Success(Employee value)
    {
        return new EmployeeValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<EmployeeValidationError>()));
    }

    internal static EmployeeValidationResult Failure(
        IEnumerable<EmployeeValidationError> errors)
    {
        return new EmployeeValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
