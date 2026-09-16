using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class SchedulePeriodValidationResult
{
    private SchedulePeriodValidationResult(
        SchedulePeriod? value,
        ReadOnlyCollection<SchedulePeriodValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public SchedulePeriod? Value { get; }

    public IReadOnlyList<SchedulePeriodValidationError> Errors { get; }

    internal static SchedulePeriodValidationResult Success(SchedulePeriod value)
    {
        return new SchedulePeriodValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<SchedulePeriodValidationError>()));
    }

    internal static SchedulePeriodValidationResult Failure(
        IEnumerable<SchedulePeriodValidationError> errors)
    {
        return new SchedulePeriodValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
