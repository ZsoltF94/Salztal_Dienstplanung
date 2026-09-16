using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class ScheduleDraftValidationResult
{
    private ScheduleDraftValidationResult(
        ScheduleDraft? value,
        ReadOnlyCollection<ScheduleDraftValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public ScheduleDraft? Value { get; }

    public IReadOnlyList<ScheduleDraftValidationError> Errors { get; }

    internal static ScheduleDraftValidationResult Success(ScheduleDraft value)
    {
        return new ScheduleDraftValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<ScheduleDraftValidationError>()));
    }

    internal static ScheduleDraftValidationResult Failure(
        IEnumerable<ScheduleDraftValidationError> errors)
    {
        return new ScheduleDraftValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
