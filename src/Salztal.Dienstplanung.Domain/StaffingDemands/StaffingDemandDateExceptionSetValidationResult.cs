using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandDateExceptionSetValidationResult
{
    private StaffingDemandDateExceptionSetValidationResult(
        StaffingDemandDateExceptionSet? value,
        ReadOnlyCollection<StaffingDemandDateExceptionSetValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public StaffingDemandDateExceptionSet? Value { get; }

    public IReadOnlyList<StaffingDemandDateExceptionSetValidationError> Errors { get; }

    internal static StaffingDemandDateExceptionSetValidationResult Success(
        StaffingDemandDateExceptionSet value)
    {
        return new StaffingDemandDateExceptionSetValidationResult(
            value,
            Array.AsReadOnly(
                Array.Empty<StaffingDemandDateExceptionSetValidationError>()));
    }

    internal static StaffingDemandDateExceptionSetValidationResult Failure(
        IEnumerable<StaffingDemandDateExceptionSetValidationError> errors)
    {
        return new StaffingDemandDateExceptionSetValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
