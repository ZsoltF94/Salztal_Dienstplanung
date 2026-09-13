using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public sealed class SplitShiftPatternValidationResult
{
    private SplitShiftPatternValidationResult(
        SplitShiftPattern? value,
        ReadOnlyCollection<SplitShiftPatternValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public SplitShiftPattern? Value { get; }

    public IReadOnlyList<SplitShiftPatternValidationError> Errors { get; }

    internal static SplitShiftPatternValidationResult Success(SplitShiftPattern value)
    {
        return new SplitShiftPatternValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<SplitShiftPatternValidationError>()));
    }

    internal static SplitShiftPatternValidationResult Failure(
        IEnumerable<SplitShiftPatternValidationError> errors)
    {
        return new SplitShiftPatternValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
