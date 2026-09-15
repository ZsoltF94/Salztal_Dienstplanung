using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Availabilities;

public enum AvailabilityPeriodQueryStatus
{
    Succeeded,
    ValidationFailed,
    CatalogInvalid,
    StoredDataInvalid,
}

public sealed class AvailabilityPeriodQueryResult
{
    private AvailabilityPeriodQueryResult(
        AvailabilityPeriodQueryStatus status,
        AvailabilityPeriodSnapshot? value,
        ReadOnlyCollection<AvailabilityPeriodQueryError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Status == AvailabilityPeriodQueryStatus.Succeeded;

    public AvailabilityPeriodQueryStatus Status { get; }

    public AvailabilityPeriodSnapshot? Value { get; }

    public ReadOnlyCollection<AvailabilityPeriodQueryError> Errors { get; }

    internal static AvailabilityPeriodQueryResult Success(
        AvailabilityPeriodSnapshot value)
    {
        return new AvailabilityPeriodQueryResult(
            AvailabilityPeriodQueryStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<AvailabilityPeriodQueryError>()));
    }

    internal static AvailabilityPeriodQueryResult Failure(
        AvailabilityPeriodQueryStatus status,
        IEnumerable<AvailabilityPeriodQueryError> errors)
    {
        if (status == AvailabilityPeriodQueryStatus.Succeeded)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentNullException.ThrowIfNull(errors);

        return new AvailabilityPeriodQueryResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
