using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandValidationResult
{
    private StaffingDemandValidationResult(
        StaffingDemand? value,
        ReadOnlyCollection<StaffingDemandValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public StaffingDemand? Value { get; }

    public IReadOnlyList<StaffingDemandValidationError> Errors { get; }

    internal static StaffingDemandValidationResult Success(StaffingDemand value)
    {
        return new StaffingDemandValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<StaffingDemandValidationError>()));
    }

    internal static StaffingDemandValidationResult Failure(
        IEnumerable<StaffingDemandValidationError> errors)
    {
        return new StaffingDemandValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
