using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public enum StaffingDemandCommandStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    CatalogInvalid,
    StoredDataInvalid,
    Conflict,
}

public enum StaffingDemandCommandErrorCode
{
    IdentifierRequired,
    ExpectedIdentifierRequired,
    UnsupportedOperation,
    UnsupportedDayOfWeek,
    WorkLocationRequired,
    ShiftTypeRequired,
    EffectiveDateMustBeMonday,
    ActualStartRequired,
    ActualEndRequired,
    ActualStartMustUseWholeMinute,
    ActualEndMustUseWholeMinute,
    ActualStartMustUseThirtyMinuteIncrement,
    ActualEndMustUseThirtyMinuteIncrement,
    ActualEndMustBeAfterStart,
    RequiredEmployeeCountRequired,
    RequiredEmployeeCountMustBePositive,
    WorkLocationNotFound,
    ShiftTypeNotFound,
    ShiftTypeWorkLocationMismatch,
    StandardAlreadyExists,
    CatalogInvalid,
    StoredDataInvalid,
    NotFound,
    Conflict,
}

public sealed record StaffingDemandCommandError(
    StaffingDemandCommandErrorCode Code,
    string Message);

public sealed class StaffingDemandCommandResult
{
    private StaffingDemandCommandResult(
        StaffingDemandCommandStatus status,
        ReadOnlyCollection<StaffingDemandCommandError> errors)
    {
        Status = status;
        Errors = errors;
    }

    public bool IsSuccess => Status == StaffingDemandCommandStatus.Succeeded;

    public StaffingDemandCommandStatus Status { get; }

    public ReadOnlyCollection<StaffingDemandCommandError> Errors { get; }

    internal static StaffingDemandCommandResult Success()
    {
        return new StaffingDemandCommandResult(
            StaffingDemandCommandStatus.Succeeded,
            Array.AsReadOnly(Array.Empty<StaffingDemandCommandError>()));
    }

    internal static StaffingDemandCommandResult Failure(
        StaffingDemandCommandStatus status,
        IEnumerable<StaffingDemandCommandError> errors)
    {
        if (status == StaffingDemandCommandStatus.Succeeded)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentNullException.ThrowIfNull(errors);
        return new StaffingDemandCommandResult(
            status,
            Array.AsReadOnly(errors.ToArray()));
    }
}
