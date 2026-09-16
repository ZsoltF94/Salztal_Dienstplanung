using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class PreparePlanningInputCommand
{
    private readonly IPlanningInputReader _reader;
    private readonly IPreparePlanningSnapshotStore _store;

    public PreparePlanningInputCommand(
        IPlanningInputReader reader,
        IPreparePlanningSnapshotStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<PreparePlanningInputResult> ExecuteAsync(
        PreparePlanningInputRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        PreparePlanningInputResult? requestFailure = ValidateRequest(request);
        if (requestFailure is not null)
        {
            return requestFailure;
        }

        SchedulePeriod period = SchedulePeriod.Create(request.PeriodMonday).Value!;
        PlanningInputReadData data = await _reader.LoadAsync(
            period,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        ScheduleDayCommandContextValidationResult contextResult =
            ScheduleDayCommandContext.Create(
                request.DraftId,
                request.ExpectedDraftVersion,
                request.PeriodMonday,
                data.Workspace);
        if (contextResult.Failure is not null)
        {
            return MapContextFailure(contextResult.Failure);
        }

        PlanningInputSnapshot? previous = data.PreparedSnapshot;
        if (previous is not null
            && (previous.DraftId != request.DraftId
                || previous.PeriodMonday != period.StartMonday
                || previous.PeriodSunday != period.EndSunday))
        {
            return Failure(
                PreparePlanningInputStatus.StoredDataInvalid,
                PreparePlanningInputErrorCode.StoredDataInvalid,
                "Die gespeicherte Planungsmomentaufnahme gehört nicht zum ausgewählten Entwurf.");
        }

        if (!MatchesExpectedPreparation(request, previous))
        {
            return Failure(
                PreparePlanningInputStatus.Conflict,
                PreparePlanningInputErrorCode.PreparationStateConflict,
                "Der Vorbereitungsstand wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
        }

        RuleCatalogSnapshot ruleCatalog = await GetRuleCatalogQuery.ExecuteAsync(
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        PlanningInputSnapshotBuildResult snapshotResult =
            PlanningInputSnapshotBuilder.Build(
                contextResult.Value!,
                data.HistoryDays,
                ruleCatalog,
                request.RunOptions);
        if (snapshotResult.Error is not null)
        {
            PreparePlanningInputStatus status = snapshotResult.Error.Code switch
            {
                PreparePlanningInputErrorCode.ServiceManagementNotReady =>
                    PreparePlanningInputStatus.NotReady,
                PreparePlanningInputErrorCode.InvalidServiceManagementAssignment =>
                    PreparePlanningInputStatus.ValidationFailed,
                PreparePlanningInputErrorCode.PeriodInvalid =>
                    PreparePlanningInputStatus.ValidationFailed,
                _ => PreparePlanningInputStatus.StoredDataInvalid,
            };
            return PreparePlanningInputResult.Failure(status, snapshotResult.Error);
        }

        PreparePlanningSnapshotStoreResult storeResult = await _store.SaveAsync(
            new PreparePlanningSnapshotChange(
                snapshotResult.Value!,
                contextResult.Value!.Draft.Version.Value,
                previous?.Id),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return storeResult.Status switch
        {
            PreparePlanningSnapshotStoreStatus.Succeeded =>
                PreparePlanningInputResult.Success(storeResult.Snapshot!),
            PreparePlanningSnapshotStoreStatus.Conflict =>
                Failure(
                    PreparePlanningInputStatus.Conflict,
                    PreparePlanningInputErrorCode.Conflict,
                    "Der Entwurf oder Vorbereitungsstand wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut."),
            _ => throw new InvalidOperationException(
                $"Unsupported prepare-planning store result: {storeResult.Status}"),
        };
    }

    private static PreparePlanningInputResult? ValidateRequest(
        PreparePlanningInputRequest request)
    {
        if (!ScheduleDraftId.TryCreate(request.DraftId, out _)
            || request.ExpectedDraftVersion <= 0
            || request.ExpectedDraftVersion > int.MaxValue
            || !Enum.IsDefined(request.Expectation)
            || request.RunOptions is null)
        {
            return Failure(
                PreparePlanningInputStatus.ValidationFailed,
                PreparePlanningInputErrorCode.RequestInvalid,
                "Die Angaben zur Planungsvorbereitung sind ungültig.");
        }

        if (!SchedulePeriod.Create(request.PeriodMonday).IsSuccess)
        {
            return Failure(
                PreparePlanningInputStatus.ValidationFailed,
                PreparePlanningInputErrorCode.PeriodInvalid,
                "Der Planungszeitraum ist ungültig.");
        }

        bool snapshotExpectationInvalid = request.Expectation switch
        {
            PlanningPreparationExpectation.NotPrepared =>
                request.ExpectedSnapshotId is not null,
            PlanningPreparationExpectation.Prepared =>
                request.ExpectedSnapshotId is null
                || request.ExpectedSnapshotId == Guid.Empty,
            _ => true,
        };
        return snapshotExpectationInvalid
            ? Failure(
                PreparePlanningInputStatus.ValidationFailed,
                PreparePlanningInputErrorCode.RequestInvalid,
                "Der erwartete Vorbereitungsstand ist ungültig.")
            : null;
    }

    private static bool MatchesExpectedPreparation(
        PreparePlanningInputRequest request,
        PlanningInputSnapshot? previous)
    {
        return request.Expectation switch
        {
            PlanningPreparationExpectation.NotPrepared => previous is null,
            PlanningPreparationExpectation.Prepared =>
                previous?.Id == request.ExpectedSnapshotId,
            _ => false,
        };
    }

    private static PreparePlanningInputResult MapContextFailure(
        ScheduleDayChangeResult failure)
    {
        ScheduleDayChangeError error = failure.Errors[0];
        return failure.Status switch
        {
            ScheduleDayChangeStatus.NotFound =>
                Failure(
                    PreparePlanningInputStatus.NotFound,
                    PreparePlanningInputErrorCode.DraftNotFound,
                    error.Message),
            ScheduleDayChangeStatus.CatalogInvalid =>
                Failure(
                    PreparePlanningInputStatus.CatalogInvalid,
                    PreparePlanningInputErrorCode.CatalogInvalid,
                    error.Message),
            ScheduleDayChangeStatus.StoredDataInvalid =>
                Failure(
                    PreparePlanningInputStatus.StoredDataInvalid,
                    PreparePlanningInputErrorCode.StoredDataInvalid,
                    error.Message),
            ScheduleDayChangeStatus.Conflict =>
                Failure(
                    PreparePlanningInputStatus.Conflict,
                    PreparePlanningInputErrorCode.Conflict,
                    error.Message),
            _ => Failure(
                PreparePlanningInputStatus.ValidationFailed,
                PreparePlanningInputErrorCode.RequestInvalid,
                error.Message),
        };
    }

    private static PreparePlanningInputResult Failure(
        PreparePlanningInputStatus status,
        PreparePlanningInputErrorCode code,
        string message)
    {
        return PreparePlanningInputResult.Failure(
            status,
            new PreparePlanningInputError(code, message));
    }
}
