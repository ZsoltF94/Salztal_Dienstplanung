using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record AcceptAutomaticScheduleProposalRequest(
    DateOnly PeriodMonday,
    AutomaticScheduleProposal Proposal);

public enum AutomaticScheduleAcceptanceStatus
{
    Succeeded,
    ValidationFailed,
    DraftNotFound,
    Conflict,
    ProposalInvalid,
    Cancelled,
    TechnicalFailure,
}

public sealed class AutomaticScheduleAcceptanceResult
{
    private AutomaticScheduleAcceptanceResult(
        AutomaticScheduleAcceptanceStatus status,
        string message,
        ScheduleDraft? draft,
        ReadOnlyCollection<ScheduleDraftValidationError> validationErrors)
    {
        Status = status;
        Message = message;
        Draft = draft;
        ValidationErrors = validationErrors;
    }

    public AutomaticScheduleAcceptanceStatus Status { get; }

    public string Message { get; }

    public ScheduleDraft? Draft { get; }

    public ReadOnlyCollection<ScheduleDraftValidationError> ValidationErrors { get; }

    public static AutomaticScheduleAcceptanceResult Success(ScheduleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new AutomaticScheduleAcceptanceResult(
            AutomaticScheduleAcceptanceStatus.Succeeded,
            "Der automatische Vorschlag wurde vollständig in den Entwurf übernommen.",
            draft,
            Array.AsReadOnly(Array.Empty<ScheduleDraftValidationError>()));
    }

    public static AutomaticScheduleAcceptanceResult Failure(
        AutomaticScheduleAcceptanceStatus status,
        string message,
        IEnumerable<ScheduleDraftValidationError>? validationErrors = null)
    {
        if (status == AutomaticScheduleAcceptanceStatus.Succeeded)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        return new AutomaticScheduleAcceptanceResult(
            status,
            message,
            null,
            Array.AsReadOnly((validationErrors ?? []).ToArray()));
    }
}
