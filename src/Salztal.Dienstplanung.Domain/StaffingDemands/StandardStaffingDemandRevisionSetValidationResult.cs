using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StandardStaffingDemandRevisionSetValidationResult
{
    private StandardStaffingDemandRevisionSetValidationResult(
        StandardStaffingDemandRevisionSet? value,
        ReadOnlyCollection<StandardStaffingDemandRevisionSetValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public StandardStaffingDemandRevisionSet? Value { get; }

    public IReadOnlyList<StandardStaffingDemandRevisionSetValidationError> Errors { get; }

    internal static StandardStaffingDemandRevisionSetValidationResult Success(
        StandardStaffingDemandRevisionSet value)
    {
        return new StandardStaffingDemandRevisionSetValidationResult(
            value,
            Array.AsReadOnly(
                Array.Empty<StandardStaffingDemandRevisionSetValidationError>()));
    }

    internal static StandardStaffingDemandRevisionSetValidationResult Failure(
        IEnumerable<StandardStaffingDemandRevisionSetValidationError> errors)
    {
        return new StandardStaffingDemandRevisionSetValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
