using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed class AvailabilityEntryValidationResult
{
    private AvailabilityEntryValidationResult(
        AvailabilityEntry? value,
        ReadOnlyCollection<AvailabilityEntryValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public AvailabilityEntry? Value { get; }

    public IReadOnlyList<AvailabilityEntryValidationError> Errors { get; }

    internal static AvailabilityEntryValidationResult Success(AvailabilityEntry value)
    {
        return new AvailabilityEntryValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<AvailabilityEntryValidationError>()));
    }

    internal static AvailabilityEntryValidationResult Failure(
        IEnumerable<AvailabilityEntryValidationError> errors)
    {
        return new AvailabilityEntryValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
