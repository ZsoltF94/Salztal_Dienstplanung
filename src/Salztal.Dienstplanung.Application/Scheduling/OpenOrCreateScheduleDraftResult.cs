using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum OpenOrCreateScheduleDraftStatus
{
    Succeeded,
    ValidationFailed,
    Overlap,
    StoredDataInvalid,
    CatalogInvalid,
    Conflict,
}

public enum ScheduleDraftOpenOutcome
{
    OpenedExisting,
    Created,
}

public sealed record OpenedScheduleDraftSnapshot(
    Guid DraftId,
    long Version,
    DateOnly PeriodMonday,
    DateOnly PeriodSunday,
    ScheduleDraftOpenOutcome Outcome);

public sealed class OpenOrCreateScheduleDraftResult
{
    private OpenOrCreateScheduleDraftResult(
        OpenOrCreateScheduleDraftStatus status,
        OpenedScheduleDraftSnapshot? value,
        ReadOnlyCollection<ScheduleWorkspaceError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public OpenOrCreateScheduleDraftStatus Status { get; }

    public OpenedScheduleDraftSnapshot? Value { get; }

    public IReadOnlyList<ScheduleWorkspaceError> Errors { get; }

    internal static OpenOrCreateScheduleDraftResult Success(
        OpenedScheduleDraftSnapshot value)
    {
        return new OpenOrCreateScheduleDraftResult(
            OpenOrCreateScheduleDraftStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<ScheduleWorkspaceError>()));
    }

    internal static OpenOrCreateScheduleDraftResult Failure(
        OpenOrCreateScheduleDraftStatus status,
        params ScheduleWorkspaceError[] errors)
    {
        return new OpenOrCreateScheduleDraftResult(
            status,
            null,
            Array.AsReadOnly(errors));
    }
}
