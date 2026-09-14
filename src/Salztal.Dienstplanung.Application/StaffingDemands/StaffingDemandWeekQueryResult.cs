using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public enum StaffingDemandWeekQueryStatus
{
    Succeeded,
    ValidationFailed,
    CatalogInvalid,
    StoredDataInvalid,
}

public sealed class StaffingDemandWeekQueryResult
{
    private StaffingDemandWeekQueryResult(
        StaffingDemandWeekQueryStatus status,
        StaffingDemandWeekSnapshot? value,
        ReadOnlyCollection<StaffingDemandWeekQueryError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Status == StaffingDemandWeekQueryStatus.Succeeded;

    public StaffingDemandWeekQueryStatus Status { get; }

    public StaffingDemandWeekSnapshot? Value { get; }

    public ReadOnlyCollection<StaffingDemandWeekQueryError> Errors { get; }

    internal static StaffingDemandWeekQueryResult Success(
        StaffingDemandWeekSnapshot value)
    {
        return new StaffingDemandWeekQueryResult(
            StaffingDemandWeekQueryStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<StaffingDemandWeekQueryError>()));
    }

    internal static StaffingDemandWeekQueryResult Failure(
        StaffingDemandWeekQueryStatus status,
        IEnumerable<StaffingDemandWeekQueryError> errors)
    {
        if (status == StaffingDemandWeekQueryStatus.Succeeded)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentNullException.ThrowIfNull(errors);

        return new StaffingDemandWeekQueryResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
