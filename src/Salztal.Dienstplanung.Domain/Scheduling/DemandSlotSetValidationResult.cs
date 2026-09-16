using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class DemandSlotSetValidationResult
{
    private DemandSlotSetValidationResult(
        DemandSlotSet? value,
        ReadOnlyCollection<DemandSlotSetValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public DemandSlotSet? Value { get; }

    public IReadOnlyList<DemandSlotSetValidationError> Errors { get; }

    internal static DemandSlotSetValidationResult Success(DemandSlotSet value)
    {
        return new DemandSlotSetValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<DemandSlotSetValidationError>()));
    }

    internal static DemandSlotSetValidationResult Failure(
        IEnumerable<DemandSlotSetValidationError> errors)
    {
        return new DemandSlotSetValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
