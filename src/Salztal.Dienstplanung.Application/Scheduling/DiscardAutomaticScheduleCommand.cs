using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class DiscardAutomaticScheduleCommand
{
    private readonly IScheduleWorkspaceReader _reader;
    private readonly IDiscardAutomaticScheduleStore _store;

    public DiscardAutomaticScheduleCommand(
        IScheduleWorkspaceReader reader,
        IDiscardAutomaticScheduleStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<AutomaticScheduleDiscardResult> ExecuteAsync(
        DiscardAutomaticScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        try
        {
            SchedulePeriodValidationResult periodResult =
                SchedulePeriod.Create(request.PeriodMonday);
            if (!periodResult.IsSuccess
                || request.DraftId == Guid.Empty
                || request.ExpectedDraftVersion <= 0
                || request.ExpectedDraftVersion > int.MaxValue)
            {
                return Failure(
                    AutomaticScheduleDiscardStatus.ValidationFailed,
                    "Die Angaben zum Verwerfen sind ung\u00fcltig. Bitte laden Sie den Zeitraum erneut.");
            }

            ScheduleWorkspaceReadData data = await _reader.LoadAsync(
                periodResult.Value!,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            ScheduleDayCommandContextValidationResult contextResult =
                ScheduleDayCommandContext.Create(
                    request.DraftId,
                    request.ExpectedDraftVersion,
                    request.PeriodMonday,
                    data);
            if (contextResult.Failure is not null)
            {
                return MapContextFailure(contextResult.Failure);
            }

            if (data.AutomaticScheduleRun is null)
            {
                return Failure(
                    AutomaticScheduleDiscardStatus.NoAutomaticSchedule,
                    "F\u00fcr diesen Entwurf ist kein \u00fcbernommener automatischer Plan vorhanden.");
            }

            ScheduleDraftValidationResult discardResult =
                contextResult.Value!.Draft.DiscardAutomaticGeneration();
            if (!discardResult.IsSuccess)
            {
                return Failure(
                    AutomaticScheduleDiscardStatus.ValidationFailed,
                    "Der automatische Plan konnte wegen widerspr\u00fcchlicher Entwurfsdaten nicht verworfen werden.",
                    discardResult.Errors);
            }

            ScheduleDraft updatedDraft = discardResult.Value!;
            DiscardAutomaticScheduleStoreResult storeResult =
                await _store.DiscardAsync(
                    new DiscardAutomaticScheduleChange(
                        updatedDraft,
                        contextResult.Value.Draft.Version.Value,
                        data.PreparedSnapshot?.Id),
                    cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (storeResult.Status == DiscardAutomaticScheduleStoreStatus.Conflict)
            {
                return Conflict();
            }

            if (storeResult.Draft is null
                || storeResult.Draft.Id != updatedDraft.Id
                || storeResult.Draft.Version != updatedDraft.Version)
            {
                return Failure(
                    AutomaticScheduleDiscardStatus.TechnicalFailure,
                    "Das Verwerfen lieferte einen widerspr\u00fcchlichen Speicherstand. Bitte laden Sie den Zeitraum erneut.");
            }

            return AutomaticScheduleDiscardResult.Success(storeResult.Draft);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failure(
                AutomaticScheduleDiscardStatus.TechnicalFailure,
                "Der automatische Plan konnte wegen eines technischen Fehlers nicht verworfen werden. Der Entwurf blieb unver\u00e4ndert.");
        }
    }

    private static AutomaticScheduleDiscardResult MapContextFailure(
        ScheduleDayChangeResult failure) => failure.Status switch
        {
            ScheduleDayChangeStatus.NotFound => Failure(
                AutomaticScheduleDiscardStatus.DraftNotFound,
                "Der Entwurf ist nicht mehr vorhanden. Bitte laden Sie den Zeitraum erneut."),
            ScheduleDayChangeStatus.Conflict => Conflict(),
            ScheduleDayChangeStatus.ValidationFailed => Failure(
                AutomaticScheduleDiscardStatus.ValidationFailed,
                "Die Angaben zum Verwerfen sind ung\u00fcltig. Bitte laden Sie den Zeitraum erneut."),
            _ => Failure(
                AutomaticScheduleDiscardStatus.TechnicalFailure,
                "Die gespeicherten Planungsdaten sind widerspr\u00fcchlich. Der automatische Plan wurde nicht verworfen."),
        };

    private static AutomaticScheduleDiscardResult Conflict() => Failure(
        AutomaticScheduleDiscardStatus.Conflict,
        "Der Entwurf wurde zwischenzeitlich ge\u00e4ndert. Bitte laden Sie den Zeitraum erneut.");

    private static AutomaticScheduleDiscardResult Cancelled() => Failure(
        AutomaticScheduleDiscardStatus.Cancelled,
        "Das Verwerfen wurde abgebrochen. Der Entwurf blieb unver\u00e4ndert.");

    private static AutomaticScheduleDiscardResult Failure(
        AutomaticScheduleDiscardStatus status,
        string message,
        IEnumerable<ScheduleDraftValidationError>? validationErrors = null) =>
        AutomaticScheduleDiscardResult.Failure(status, message, validationErrors);
}
