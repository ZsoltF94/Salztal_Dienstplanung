using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandWeekResolutionResult
{
    private StaffingDemandWeekResolutionResult(
        StaffingDemandWeek? value,
        ReadOnlyCollection<StaffingDemandWeekResolutionError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public StaffingDemandWeek? Value { get; }

    public IReadOnlyList<StaffingDemandWeekResolutionError> Errors { get; }

    internal static StaffingDemandWeekResolutionResult Success(StaffingDemandWeek value)
    {
        return new StaffingDemandWeekResolutionResult(
            value,
            Array.AsReadOnly(Array.Empty<StaffingDemandWeekResolutionError>()));
    }

    internal static StaffingDemandWeekResolutionResult Failure(
        IEnumerable<StaffingDemandWeekResolutionError> errors)
    {
        return new StaffingDemandWeekResolutionResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
