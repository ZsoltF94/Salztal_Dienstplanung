using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record DiscardAutomaticScheduleRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday);

public enum AutomaticScheduleDiscardStatus
{
    Succeeded,
    Cancelled,
    DraftNotFound,
    NoAutomaticSchedule,
    ValidationFailed,
    Conflict,
    TechnicalFailure,
}

public sealed class AutomaticScheduleDiscardResult
{
    private AutomaticScheduleDiscardResult(
        AutomaticScheduleDiscardStatus status,
        string message,
        ScheduleDraft? draft,
        IEnumerable<ScheduleDraftValidationError>? validationErrors)
    {
        Status = status;
        Message = message;
        Draft = draft;
        ValidationErrors = Array.AsReadOnly((validationErrors ?? []).ToArray());
    }

    public AutomaticScheduleDiscardStatus Status { get; }

    public string Message { get; }

    public ScheduleDraft? Draft { get; }

    public ReadOnlyCollection<ScheduleDraftValidationError> ValidationErrors { get; }

    public static AutomaticScheduleDiscardResult Success(ScheduleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new AutomaticScheduleDiscardResult(
            AutomaticScheduleDiscardStatus.Succeeded,
            "Der automatische Plan wurde vollst\u00e4ndig verworfen. Bitte bereiten Sie die Planung vor einer neuen Generierung erneut vor.",
            draft,
            null);
    }

    public static AutomaticScheduleDiscardResult Failure(
        AutomaticScheduleDiscardStatus status,
        string message,
        IEnumerable<ScheduleDraftValidationError>? validationErrors = null) =>
        new(status, message, null, validationErrors);
}
