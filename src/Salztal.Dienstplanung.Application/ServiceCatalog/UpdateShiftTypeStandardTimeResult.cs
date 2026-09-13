using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public enum UpdateShiftTypeStandardTimeStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    Conflict,
}

public enum UpdateShiftTypeStandardTimeErrorCode
{
    IdentifierRequired,
    StartMustUseWholeMinute,
    EndMustUseWholeMinute,
    StartMustUseThirtyMinuteIncrement,
    EndMustUseThirtyMinuteIncrement,
    EndMustBeAfterStart,
    SplitShiftSegmentsMustBeInChronologicalOrder,
    SplitShiftSegmentsMustNotOverlap,
    SplitShiftBreakRequired,
    NotFound,
    Conflict,
}

public sealed record UpdateShiftTypeStandardTimeError(
    UpdateShiftTypeStandardTimeErrorCode Code,
    string Message);

public sealed class UpdateShiftTypeStandardTimeResult
{
    private UpdateShiftTypeStandardTimeResult(
        UpdateShiftTypeStandardTimeStatus status,
        ShiftTypeSnapshot? value,
        ReadOnlyCollection<UpdateShiftTypeStandardTimeError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public UpdateShiftTypeStandardTimeStatus Status { get; }

    public ShiftTypeSnapshot? Value { get; }

    public IReadOnlyList<UpdateShiftTypeStandardTimeError> Errors { get; }

    internal static UpdateShiftTypeStandardTimeResult Success(ShiftTypeSnapshot value)
    {
        return new UpdateShiftTypeStandardTimeResult(
            UpdateShiftTypeStandardTimeStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<UpdateShiftTypeStandardTimeError>()));
    }

    internal static UpdateShiftTypeStandardTimeResult Failure(
        UpdateShiftTypeStandardTimeStatus status,
        IEnumerable<UpdateShiftTypeStandardTimeError> errors)
    {
        return new UpdateShiftTypeStandardTimeResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
