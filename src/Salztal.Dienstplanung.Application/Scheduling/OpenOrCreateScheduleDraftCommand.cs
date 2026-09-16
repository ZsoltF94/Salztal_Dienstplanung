using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class OpenOrCreateScheduleDraftCommand
{
    private readonly IScheduleWorkspaceReader _reader;
    private readonly IOpenScheduleDraftStore _store;

    public OpenOrCreateScheduleDraftCommand(
        IScheduleWorkspaceReader reader,
        IOpenScheduleDraftStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<OpenOrCreateScheduleDraftResult> ExecuteAsync(
        DateOnly selectedDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SchedulePeriodValidationResult periodResult =
            SchedulePeriodSelection.Create(selectedDate);
        if (!periodResult.IsSuccess)
        {
            return OpenOrCreateScheduleDraftResult.Failure(
                OpenOrCreateScheduleDraftStatus.ValidationFailed,
                new ScheduleWorkspaceError(
                    ScheduleWorkspaceErrorCode.PeriodDoesNotFit,
                    "Für das ausgewählte Datum kann kein vollständiger Drei-Wochen-Zeitraum gebildet werden."));
        }

        SchedulePeriod period = periodResult.Value!;
        ScheduleWorkspaceReadData data = await _reader.LoadAsync(
            period,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        ScheduleDraftHeader[] exactHeaders = data.DraftHeaders
            .Where(header => header.Period == period)
            .ToArray();
        if (exactHeaders.Length > 1)
        {
            return StoredDataInvalid(
                "Für denselben Planungszeitraum sind mehrere Entwürfe gespeichert.");
        }

        if (exactHeaders.Length == 1)
        {
            ScheduleDraftHeader header = exactHeaders[0];
            if (data.ExactDraft is null
                || data.ExactDraft.Id != header.DraftId
                || data.ExactDraft.Version != header.Version
                || data.ExactDraft.Period != period)
            {
                return StoredDataInvalid(
                    "Der gespeicherte Entwurf passt nicht zu seinem Zeitraumseintrag.");
            }

            ScheduleWorkspaceContextResult existingContextResult =
                ScheduleWorkspaceContextBuilder.Build(period, data);
            if (!existingContextResult.IsSuccess)
            {
                return OpenOrCreateScheduleDraftResult.Failure(
                    existingContextResult.Status,
                    existingContextResult.Errors.ToArray());
            }

            return OpenOrCreateScheduleDraftResult.Success(
                CreateSnapshot(
                    data.ExactDraft,
                    ScheduleDraftOpenOutcome.OpenedExisting));
        }

        ScheduleDraftHeader? overlappingHeader = data.DraftHeaders
            .FirstOrDefault(header => header.Period.Overlaps(period));
        if (overlappingHeader is not null)
        {
            return Overlap(overlappingHeader.Period);
        }

        if (data.ExactDraft is not null)
        {
            return StoredDataInvalid(
                "Ein geladener Entwurf besitzt keinen passenden Zeitraumseintrag.");
        }

        ScheduleWorkspaceContextResult contextResult =
            ScheduleWorkspaceContextBuilder.Build(period, data);
        if (!contextResult.IsSuccess)
        {
            return OpenOrCreateScheduleDraftResult.Failure(
                contextResult.Status,
                contextResult.Errors.ToArray());
        }

        ScheduleWorkspaceContext context = contextResult.Value!;
        ScheduleDraftValidationResult draftResult = ScheduleDraft.Create(
            Guid.NewGuid(),
            1,
            period,
            context.DemandSlots,
            context.AvailabilityEntries,
            Array.Empty<ScheduleAssignment>(),
            Array.Empty<GeneratedDayOffMarker>(),
            Array.Empty<AssignmentLock>());
        if (!draftResult.IsSuccess)
        {
            return StoredDataInvalid(
                "Aus den aktuellen Eingaben kann kein widerspruchsfreier Entwurf angelegt werden.");
        }

        OpenScheduleDraftStoreResult storeResult = await _store.CreateAsync(
            draftResult.Value!,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return storeResult.Status switch
        {
            OpenScheduleDraftStoreStatus.Succeeded =>
                OpenOrCreateScheduleDraftResult.Success(
                    CreateSnapshot(
                        storeResult.Draft!,
                        ScheduleDraftOpenOutcome.Created)),
            OpenScheduleDraftStoreStatus.Overlap =>
                Overlap(storeResult.OverlappingPeriod!),
            OpenScheduleDraftStoreStatus.Conflict =>
                OpenOrCreateScheduleDraftResult.Failure(
                    OpenOrCreateScheduleDraftStatus.Conflict,
                    new ScheduleWorkspaceError(
                        ScheduleWorkspaceErrorCode.Conflict,
                        "Der Planungsstand wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.")),
            _ => throw new InvalidOperationException(
                $"Unsupported schedule draft store result: {storeResult.Status}"),
        };
    }

    private static OpenedScheduleDraftSnapshot CreateSnapshot(
        ScheduleDraft draft,
        ScheduleDraftOpenOutcome outcome)
    {
        return new OpenedScheduleDraftSnapshot(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            draft.Period.EndSunday,
            outcome);
    }

    private static OpenOrCreateScheduleDraftResult Overlap(SchedulePeriod period)
    {
        return OpenOrCreateScheduleDraftResult.Failure(
            OpenOrCreateScheduleDraftStatus.Overlap,
            new ScheduleWorkspaceError(
                ScheduleWorkspaceErrorCode.OverlappingPeriod,
                $"Der Zeitraum überschneidet sich mit dem gespeicherten Plan vom {period.StartMonday:dd.MM.yyyy} bis {period.EndSunday:dd.MM.yyyy}.",
                period.StartMonday,
                period.EndSunday));
    }

    private static OpenOrCreateScheduleDraftResult StoredDataInvalid(string message)
    {
        return OpenOrCreateScheduleDraftResult.Failure(
            OpenOrCreateScheduleDraftStatus.StoredDataInvalid,
            new ScheduleWorkspaceError(
                ScheduleWorkspaceErrorCode.StoredDataInvalid,
                message));
    }
}
