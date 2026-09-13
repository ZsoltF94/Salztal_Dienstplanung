using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed class ShiftTypeValidationResult
{
    private ShiftTypeValidationResult(
        ShiftType? value,
        ReadOnlyCollection<ShiftTypeValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public ShiftType? Value { get; }

    public IReadOnlyList<ShiftTypeValidationError> Errors { get; }

    internal static ShiftTypeValidationResult Success(ShiftType value)
    {
        return new ShiftTypeValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<ShiftTypeValidationError>()));
    }

    internal static ShiftTypeValidationResult Failure(
        IEnumerable<ShiftTypeValidationError> errors)
    {
        return new ShiftTypeValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
