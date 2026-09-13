using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed class ShiftStandardTimeValidationResult
{
    private ShiftStandardTimeValidationResult(
        ShiftStandardTime? value,
        ReadOnlyCollection<ShiftStandardTimeValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public ShiftStandardTime? Value { get; }

    public IReadOnlyList<ShiftStandardTimeValidationError> Errors { get; }

    internal static ShiftStandardTimeValidationResult Success(ShiftStandardTime value)
    {
        return new ShiftStandardTimeValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<ShiftStandardTimeValidationError>()));
    }

    internal static ShiftStandardTimeValidationResult Failure(
        IEnumerable<ShiftStandardTimeValidationError> errors)
    {
        return new ShiftStandardTimeValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
