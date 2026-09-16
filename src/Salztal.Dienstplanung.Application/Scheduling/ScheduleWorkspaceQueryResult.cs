using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ScheduleWorkspaceQueryStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    StoredDataInvalid,
    CatalogInvalid,
}

public sealed class ScheduleWorkspaceQueryResult
{
    private ScheduleWorkspaceQueryResult(
        ScheduleWorkspaceQueryStatus status,
        ScheduleWorkspaceSnapshot? value,
        ReadOnlyCollection<ScheduleWorkspaceError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public ScheduleWorkspaceQueryStatus Status { get; }

    public ScheduleWorkspaceSnapshot? Value { get; }

    public IReadOnlyList<ScheduleWorkspaceError> Errors { get; }

    internal static ScheduleWorkspaceQueryResult Success(
        ScheduleWorkspaceSnapshot value)
    {
        return new ScheduleWorkspaceQueryResult(
            ScheduleWorkspaceQueryStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<ScheduleWorkspaceError>()));
    }

    internal static ScheduleWorkspaceQueryResult Failure(
        ScheduleWorkspaceQueryStatus status,
        params ScheduleWorkspaceError[] errors)
    {
        return new ScheduleWorkspaceQueryResult(
            status,
            null,
            Array.AsReadOnly(errors));
    }
}
