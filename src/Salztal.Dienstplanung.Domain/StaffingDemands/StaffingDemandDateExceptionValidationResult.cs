using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandDateExceptionValidationResult
{
    private StaffingDemandDateExceptionValidationResult(
        StaffingDemandDateException? value,
        ReadOnlyCollection<StaffingDemandDateExceptionValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public StaffingDemandDateException? Value { get; }

    public IReadOnlyList<StaffingDemandDateExceptionValidationError> Errors { get; }

    internal static StaffingDemandDateExceptionValidationResult Success(
        StaffingDemandDateException value)
    {
        return new StaffingDemandDateExceptionValidationResult(
            value,
            Array.AsReadOnly(
                Array.Empty<StaffingDemandDateExceptionValidationError>()));
    }

    internal static StaffingDemandDateExceptionValidationResult Failure(
        IEnumerable<StaffingDemandDateExceptionValidationError> errors)
    {
        return new StaffingDemandDateExceptionValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
