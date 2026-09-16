using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ScheduleDayChangeStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    InactiveEmployee,
    WrongPlanningRole,
    ConfirmationRequired,
    CatalogInvalid,
    StoredDataInvalid,
    Conflict,
}

public enum ScheduleDayChangeErrorCode
{
    DraftIdentifierRequired,
    AssignmentIdentifierRequired,
    EmployeeIdentifierRequired,
    ExpectedDraftVersionInvalid,
    PeriodInvalid,
    UnsupportedAssignmentKind,
    UnsupportedAvailabilityKind,
    DraftNotFound,
    EmployeeNotFound,
    EmployeeInactive,
    ServiceManagementRoleRequired,
    DemandSlotRequired,
    DemandSlotNotFound,
    DemandSlotTimeChanged,
    SlotsMustShareDate,
    ShiftNotEligible,
    PatternNotEligible,
    OfficeTimeNotAllowedForSlot,
    AssignmentInvalid,
    AssignmentNotFound,
    ServiceManagementAssignmentRequired,
    AvailabilityVersionConflict,
    ReplacementConfirmationRequired,
    AvailabilityEntryInvalid,
    CatalogInvalid,
    StoredDataInvalid,
    Conflict,
}

public sealed record ScheduleDayChangeError(
    ScheduleDayChangeErrorCode Code,
    string Message);

public sealed record ScheduleDayChangeSnapshot(
    Guid DraftId,
    long Version,
    Guid? AssignmentId,
    Guid EmployeeId,
    DateOnly Date);

public sealed class ScheduleDayChangeResult
{
    private ScheduleDayChangeResult(
        ScheduleDayChangeStatus status,
        ScheduleDayChangeSnapshot? value,
        ReadOnlyCollection<ScheduleDayChangeError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public ScheduleDayChangeStatus Status { get; }

    public ScheduleDayChangeSnapshot? Value { get; }

    public IReadOnlyList<ScheduleDayChangeError> Errors { get; }

    internal static ScheduleDayChangeResult Success(
        ScheduleDayChangeSnapshot value)
    {
        return new ScheduleDayChangeResult(
            ScheduleDayChangeStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<ScheduleDayChangeError>()));
    }

    internal static ScheduleDayChangeResult Failure(
        ScheduleDayChangeStatus status,
        ScheduleDayChangeErrorCode code,
        string message)
    {
        return new ScheduleDayChangeResult(
            status,
            null,
            Array.AsReadOnly([new ScheduleDayChangeError(code, message)]));
    }
}
