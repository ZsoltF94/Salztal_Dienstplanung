using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class EmployeeTypeShiftEligibilityValidationResult
{
    private EmployeeTypeShiftEligibilityValidationResult(
        EmployeeTypeShiftEligibility? value,
        ReadOnlyCollection<EmployeeTypeShiftEligibilityValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public EmployeeTypeShiftEligibility? Value { get; }

    public IReadOnlyList<EmployeeTypeShiftEligibilityValidationError> Errors { get; }

    internal static EmployeeTypeShiftEligibilityValidationResult Success(
        EmployeeTypeShiftEligibility value)
    {
        return new EmployeeTypeShiftEligibilityValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<EmployeeTypeShiftEligibilityValidationError>()));
    }

    internal static EmployeeTypeShiftEligibilityValidationResult Failure(
        IEnumerable<EmployeeTypeShiftEligibilityValidationError> errors)
    {
        return new EmployeeTypeShiftEligibilityValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
