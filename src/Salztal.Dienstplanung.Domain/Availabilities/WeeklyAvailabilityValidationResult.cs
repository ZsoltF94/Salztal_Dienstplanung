using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed class WeeklyAvailabilityValidationResult
{
    private WeeklyAvailabilityValidationResult(
        WeeklyAvailability? value,
        ReadOnlyCollection<WeeklyAvailabilityValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public WeeklyAvailability? Value { get; }

    public IReadOnlyList<WeeklyAvailabilityValidationError> Errors { get; }

    internal static WeeklyAvailabilityValidationResult Success(WeeklyAvailability value)
    {
        return new WeeklyAvailabilityValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<WeeklyAvailabilityValidationError>()));
    }

    internal static WeeklyAvailabilityValidationResult Failure(
        IEnumerable<WeeklyAvailabilityValidationError> errors)
    {
        return new WeeklyAvailabilityValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
