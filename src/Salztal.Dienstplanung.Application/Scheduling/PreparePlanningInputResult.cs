using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum PlanningPreparationExpectation
{
    NotPrepared,
    Prepared,
}

public sealed record PreparePlanningInputRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday,
    PlanningPreparationExpectation Expectation,
    Guid? ExpectedSnapshotId,
    PlanningRunOptions RunOptions);

public enum PreparePlanningInputStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    NotReady,
    CatalogInvalid,
    StoredDataInvalid,
    Conflict,
}

public enum PreparePlanningInputErrorCode
{
    RequestInvalid,
    DraftNotFound,
    PeriodInvalid,
    ServiceManagementNotReady,
    InvalidServiceManagementAssignment,
    PreparationStateConflict,
    CatalogInvalid,
    StoredDataInvalid,
    Conflict,
}

public enum InvalidServiceManagementAssignmentCode
{
    EmployeeMissing,
    EmployeeInactive,
    WrongPlanningRole,
    AvailabilityConflict,
    DemandSlotMissing,
    DemandSlotTimeChanged,
    EligibilityMissing,
    AssignmentStructureInvalid,
}

public sealed record InvalidServiceManagementAssignment(
    Guid AssignmentId,
    InvalidServiceManagementAssignmentCode Code);

public sealed class PreparePlanningInputError
{
    public PreparePlanningInputError(
        PreparePlanningInputErrorCode code,
        string message,
        Guid? employeeId = null,
        IEnumerable<DateOnly>? weekMondays = null,
        IEnumerable<InvalidServiceManagementAssignment>? invalidAssignments = null)
    {
        Code = code;
        Message = message;
        EmployeeId = employeeId;
        WeekMondays = Array.AsReadOnly((weekMondays ?? []).ToArray());
        InvalidAssignments = Array.AsReadOnly((invalidAssignments ?? []).ToArray());
    }

    public PreparePlanningInputErrorCode Code { get; }

    public string Message { get; }

    public Guid? EmployeeId { get; }

    public ReadOnlyCollection<DateOnly> WeekMondays { get; }

    public ReadOnlyCollection<InvalidServiceManagementAssignment> InvalidAssignments
    {
        get;
    }
}

public sealed class PreparePlanningInputResult
{
    private PreparePlanningInputResult(
        PreparePlanningInputStatus status,
        PlanningInputSnapshot? value,
        ReadOnlyCollection<PreparePlanningInputError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public PreparePlanningInputStatus Status { get; }

    public PlanningInputSnapshot? Value { get; }

    public IReadOnlyList<PreparePlanningInputError> Errors { get; }

    internal static PreparePlanningInputResult Success(PlanningInputSnapshot value)
    {
        return new PreparePlanningInputResult(
            PreparePlanningInputStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<PreparePlanningInputError>()));
    }

    internal static PreparePlanningInputResult Failure(
        PreparePlanningInputStatus status,
        PreparePlanningInputError error)
    {
        return new PreparePlanningInputResult(
            status,
            null,
            Array.AsReadOnly([error]));
    }
}
