using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StandardStaffingDemandRevisionValidationResult
{
    private StandardStaffingDemandRevisionValidationResult(
        StandardStaffingDemandRevision? value,
        ReadOnlyCollection<StandardStaffingDemandRevisionValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public StandardStaffingDemandRevision? Value { get; }

    public IReadOnlyList<StandardStaffingDemandRevisionValidationError> Errors { get; }

    internal static StandardStaffingDemandRevisionValidationResult Success(
        StandardStaffingDemandRevision value)
    {
        return new StandardStaffingDemandRevisionValidationResult(
            value,
            Array.AsReadOnly(
                Array.Empty<StandardStaffingDemandRevisionValidationError>()));
    }

    internal static StandardStaffingDemandRevisionValidationResult Failure(
        IEnumerable<StandardStaffingDemandRevisionValidationError> errors)
    {
        return new StandardStaffingDemandRevisionValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
