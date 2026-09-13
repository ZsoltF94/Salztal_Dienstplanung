using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public sealed class ReliefShiftPatternValidationResult
{
    private ReliefShiftPatternValidationResult(
        ReliefShiftPattern? value,
        ReadOnlyCollection<ReliefShiftPatternValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public ReliefShiftPattern? Value { get; }

    public IReadOnlyList<ReliefShiftPatternValidationError> Errors { get; }

    internal static ReliefShiftPatternValidationResult Success(ReliefShiftPattern value)
    {
        return new ReliefShiftPatternValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<ReliefShiftPatternValidationError>()));
    }

    internal static ReliefShiftPatternValidationResult Failure(
        IEnumerable<ReliefShiftPatternValidationError> errors)
    {
        return new ReliefShiftPatternValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
