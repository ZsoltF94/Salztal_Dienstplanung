using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed class AvailabilityEntrySetValidationResult
{
    private AvailabilityEntrySetValidationResult(
        AvailabilityEntrySet? value,
        ReadOnlyCollection<AvailabilityEntrySetValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public AvailabilityEntrySet? Value { get; }

    public IReadOnlyList<AvailabilityEntrySetValidationError> Errors { get; }

    internal static AvailabilityEntrySetValidationResult Success(AvailabilityEntrySet value)
    {
        return new AvailabilityEntrySetValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<AvailabilityEntrySetValidationError>()));
    }

    internal static AvailabilityEntrySetValidationResult Failure(
        IEnumerable<AvailabilityEntrySetValidationError> errors)
    {
        return new AvailabilityEntrySetValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
