using System.Diagnostics;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class GenerateAutomaticScheduleCommand
{
    private static readonly SemaphoreSlim ActiveGeneration = new(1, 1);
    private readonly object previewLock = new();
    private readonly IPlanningInputReader reader;
    private readonly IAutomaticSchedulePlanner planner;
    private readonly IAutomaticScheduleTechnicalErrorStore technicalErrorStore;
    private AutomaticSchedulePreview? currentPreview;

    public GenerateAutomaticScheduleCommand(
        IPlanningInputReader reader,
        IAutomaticSchedulePlanner planner)
        : this(reader, planner, DiscardingTechnicalErrorStore.Instance)
    {
    }

    public GenerateAutomaticScheduleCommand(
        IPlanningInputReader reader,
        IAutomaticSchedulePlanner planner,
        IAutomaticScheduleTechnicalErrorStore technicalErrorStore)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(planner);
        ArgumentNullException.ThrowIfNull(technicalErrorStore);
        this.reader = reader;
        this.planner = planner;
        this.technicalErrorStore = technicalErrorStore;
    }

    public AutomaticSchedulePreview? CurrentPreview
    {
        get
        {
            lock (previewLock)
            {
                return currentPreview;
            }
        }
    }

    public async Task<AutomaticScheduleGenerationResult> ExecuteAsync(
        GenerateAutomaticScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        if (!await ActiveGeneration.WaitAsync(0, CancellationToken.None))
        {
            return Failure(
                AutomaticScheduleGenerationStatus.ConcurrentRun,
                "Eine automatische Planung läuft bereits. Bitte warten Sie auf deren Abschluss oder brechen Sie sie ab.",
                [new AutomaticScheduleError(AutomaticScheduleErrorCode.ConcurrentRun)]);
        }

        AutomaticScheduleTechnicalStage technicalStage =
            AutomaticScheduleTechnicalStage.ApplicationValidation;
        try
        {
            SetPreview(null);
            AutomaticScheduleGenerationResult? requestFailure = ValidateRequest(request);
            if (requestFailure is not null)
            {
                return requestFailure;
            }

            SchedulePeriod period = SchedulePeriod.Create(request.PeriodMonday).Value!;
            technicalStage = AutomaticScheduleTechnicalStage.InputLoading;
            PlanningInputReadData data = await reader.LoadAsync(period, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            technicalStage = AutomaticScheduleTechnicalStage.CurrentSnapshotBuilding;
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

            PlanningInputSnapshot? prepared = data.PreparedSnapshot;
            AutomaticScheduleGenerationResult? preparationFailure =
                ValidatePreparation(request, period, prepared);
            if (preparationFailure is not null)
            {
                return preparationFailure;
            }

            PlanningInputSnapshotBuildResult currentResult =
                PlanningInputSnapshotBuilder.Build(
                    contextResult.Value!,
                    data.HistoryDays,
                    await GetRuleCatalogQuery.ExecuteAsync(cancellationToken),
                    prepared!.RunOptions);
            cancellationToken.ThrowIfCancellationRequested();
            if (currentResult.Error is not null)
            {
                return MapCurrentInputFailure(currentResult.Error);
            }

            if (!PlanningInputComparison.Compare(prepared, currentResult.Value!).IsCurrent)
            {
                return Failure(
                    AutomaticScheduleGenerationStatus.PreparationOutdated,
                    "Die Planungsvorbereitung ist nicht mehr aktuell. Bitte aktualisieren Sie sie und starten Sie danach erneut.");
            }

            technicalStage = AutomaticScheduleTechnicalStage.PlanningBoundary;
            AutomaticSchedulePlanningResult planningResult = await planner.PlanAsync(
                new AutomaticSchedulePlanningRequest(
                    prepared,
                    AutomaticSchedulePlanningRequest.ProductiveTimeLimit),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            RecordTechnicalErrors(planningResult.Errors);
            technicalStage = AutomaticScheduleTechnicalStage.ResultMapping;
            return MapPlanningResult(prepared, planningResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            SetPreview(null);
            return Cancelled();
        }
        catch (Exception exception)
        {
            SetPreview(null);
            string correlationId = Guid.NewGuid().ToString("N");
            AutomaticScheduleError error = new(
                AutomaticScheduleErrorCode.TechnicalFailure,
                CorrelationId: correlationId,
                Parameter: exception.GetType().FullName,
                TechnicalDetails:
                    AutomaticScheduleTechnicalFailureDetails.FromException(
                        technicalStage,
                        exception));
            RecordTechnicalError(error);
            return Failure(
                AutomaticScheduleGenerationStatus.TechnicalFailure,
                $"Die automatische Planung konnte wegen eines technischen Fehlers nicht abgeschlossen werden. Fehlerkennung: {correlationId}.",
                [error]);
        }
        finally
        {
            ActiveGeneration.Release();
        }
    }

    public bool DiscardPreview()
    {
        lock (previewLock)
        {
            bool discarded = currentPreview is not null;
            currentPreview = null;
            return discarded;
        }
    }

    private static AutomaticScheduleGenerationResult? ValidateRequest(
        GenerateAutomaticScheduleRequest request)
    {
        if (!ScheduleDraftId.TryCreate(request.DraftId, out _)
            || request.ExpectedDraftVersion <= 0
            || request.ExpectedDraftVersion > int.MaxValue
            || request.ExpectedSnapshotId == Guid.Empty
            || !SchedulePeriod.Create(request.PeriodMonday).IsSuccess)
        {
            return Failure(
                AutomaticScheduleGenerationStatus.ValidationFailed,
                "Die Angaben für die automatische Planung sind ungültig. Bitte laden Sie den Zeitraum erneut.");
        }

        return null;
    }

    private static AutomaticScheduleGenerationResult? ValidatePreparation(
        GenerateAutomaticScheduleRequest request,
        SchedulePeriod period,
        PlanningInputSnapshot? prepared)
    {
        if (prepared is null)
        {
            return Failure(
                AutomaticScheduleGenerationStatus.PreparationMissing,
                "Der Zeitraum ist noch nicht für die automatische Planung vorbereitet. Bitte bereiten Sie ihn zuerst vor.");
        }

        if (prepared.Id != request.ExpectedSnapshotId)
        {
            return Failure(
                AutomaticScheduleGenerationStatus.Conflict,
                "Die Planungsvorbereitung wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
        }

        if (prepared.DraftId != request.DraftId
            || prepared.DraftVersion != request.ExpectedDraftVersion
            || prepared.PeriodMonday != period.StartMonday
            || prepared.PeriodSunday != period.EndSunday)
        {
            return Failure(
                AutomaticScheduleGenerationStatus.PreparationOutdated,
                "Die Planungsvorbereitung passt nicht mehr zum aktuellen Entwurf. Bitte aktualisieren Sie sie und starten Sie danach erneut.");
        }

        return null;
    }

    private AutomaticScheduleGenerationResult MapPlanningResult(
        PlanningInputSnapshot prepared,
        AutomaticSchedulePlanningResult planningResult)
    {
        if (planningResult.Preview is not null)
        {
            AutomaticScheduleProposal proposal = planningResult.Preview.Proposal;
            if (proposal.SnapshotId != prepared.Id
                || proposal.DraftId != prepared.DraftId
                || proposal.ExpectedDraftVersion != prepared.DraftVersion
                || proposal.Metadata.ResultStatus != planningResult.Status)
            {
                return Failure(
                    AutomaticScheduleGenerationStatus.TechnicalFailure,
                    "Das Planungsergebnis ist technisch widersprüchlich und wurde nicht als Vorschau übernommen.",
                    [new AutomaticScheduleError(
                        AutomaticScheduleErrorCode.ResultValidationFailed)]);
            }

            SetPreview(planningResult.Preview);
            return AutomaticScheduleGenerationResult.Success(
                planningResult,
                planningResult.Status == AutomaticSchedulePlanningStatus.Optimal
                    ? "Der automatische Vorschlag wurde erfolgreich erzeugt. Bitte prüfen Sie ihn vor der späteren Übernahme."
                    : "Ein zulässiger Vorschlag wurde erzeugt, seine Optimalität konnte innerhalb der Zeitgrenze aber nicht nachgewiesen werden. Bitte prüfen Sie ihn besonders sorgfältig.");
        }

        return planningResult.Status switch
        {
            AutomaticSchedulePlanningStatus.Cancelled => Cancelled(),
            AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult => Failure(
                AutomaticScheduleGenerationStatus.TimedOutWithoutFeasibleResult,
                "Innerhalb der Zeitgrenze wurde kein zulässiger Vorschlag gefunden. Der Entwurf blieb unverändert."),
            AutomaticSchedulePlanningStatus.BlockedByInput => Failure(
                AutomaticScheduleGenerationStatus.BlockedByInput,
                "Die vorbereiteten Eingaben blockieren die automatische Planung. Bitte prüfen und aktualisieren Sie die Vorbereitung.",
                planningResult.Errors),
            AutomaticSchedulePlanningStatus.UnsupportedRule => Failure(
                AutomaticScheduleGenerationStatus.UnsupportedRule,
                "Mindestens eine vorbereitete Regel wird technisch noch nicht unterstützt. Bitte aktualisieren Sie die Vorbereitung oder melden Sie den Fehler.",
                planningResult.Errors),
            AutomaticSchedulePlanningStatus.TechnicalFailure
                when planningResult.Errors.Any(error =>
                    error.Code == AutomaticScheduleErrorCode.ConcurrentRun) => Failure(
                        AutomaticScheduleGenerationStatus.ConcurrentRun,
                        "Eine automatische Planung läuft bereits. Bitte warten Sie auf deren Abschluss oder brechen Sie sie ab.",
                        planningResult.Errors),
            AutomaticSchedulePlanningStatus.TechnicalFailure => Failure(
                AutomaticScheduleGenerationStatus.TechnicalFailure,
                TechnicalFailureMessage(planningResult.Errors),
                planningResult.Errors),
            _ => Failure(
                AutomaticScheduleGenerationStatus.TechnicalFailure,
                "Das Planungsergebnis besitzt einen unerwarteten Status und wurde verworfen.",
                [new AutomaticScheduleError(
                    AutomaticScheduleErrorCode.UnexpectedSolverStatus)]),
        };
    }

    private static AutomaticScheduleGenerationResult MapContextFailure(
        ScheduleDayChangeResult failure)
    {
        return failure.Status switch
        {
            ScheduleDayChangeStatus.NotFound => Failure(
                AutomaticScheduleGenerationStatus.DraftNotFound,
                "Der ausgewählte Entwurf ist nicht mehr vorhanden. Bitte laden Sie den Zeitraum erneut."),
            ScheduleDayChangeStatus.Conflict => Failure(
                AutomaticScheduleGenerationStatus.Conflict,
                "Der Entwurf wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut und aktualisieren Sie die Vorbereitung."),
            ScheduleDayChangeStatus.CatalogInvalid
                or ScheduleDayChangeStatus.StoredDataInvalid => Failure(
                    AutomaticScheduleGenerationStatus.TechnicalFailure,
                    "Die gespeicherten Planungsdaten sind widersprüchlich. Bitte melden Sie den Fehler; der Entwurf blieb unverändert."),
            _ => Failure(
                AutomaticScheduleGenerationStatus.ValidationFailed,
                "Die Angaben für die automatische Planung sind ungültig. Bitte laden Sie den Zeitraum erneut."),
        };
    }

    private static AutomaticScheduleGenerationResult MapCurrentInputFailure(
        PreparePlanningInputError error)
    {
        return error.Code switch
        {
            PreparePlanningInputErrorCode.ServiceManagementNotReady => Failure(
                AutomaticScheduleGenerationStatus.BlockedByInput,
                "Die Typ1-Voraussetzung ist nicht mehr erfüllt. Bitte vervollständigen Sie die Typ1-Planung und aktualisieren Sie danach die Vorbereitung."),
            PreparePlanningInputErrorCode.InvalidServiceManagementAssignment => Failure(
                AutomaticScheduleGenerationStatus.BlockedByInput,
                "Mindestens eine geschützte Typ1-Einteilung passt nicht mehr zu den aktuellen Eingaben. Bitte korrigieren und aktualisieren Sie die Vorbereitung."),
            _ => Failure(
                AutomaticScheduleGenerationStatus.TechnicalFailure,
                "Die aktuellen Planungsdaten konnten nicht sicher geprüft werden. Bitte melden Sie den Fehler; der Entwurf blieb unverändert."),
        };
    }

    private static AutomaticScheduleGenerationResult Cancelled() => Failure(
        AutomaticScheduleGenerationStatus.Cancelled,
        "Die automatische Planung wurde abgebrochen. Der Entwurf blieb unverändert.");

    private static AutomaticScheduleGenerationResult Failure(
        AutomaticScheduleGenerationStatus status,
        string message,
        IEnumerable<AutomaticScheduleError>? planningErrors = null) =>
        AutomaticScheduleGenerationResult.Failure(
            status,
            message,
            planningErrors ?? []);

    private static string TechnicalFailureMessage(
        IEnumerable<AutomaticScheduleError> errors)
    {
        string? correlationId = errors
            .Select(error => error.CorrelationId)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        return correlationId is null
            ? "Die automatische Planung konnte wegen eines technischen Fehlers nicht abgeschlossen werden. Bitte versuchen Sie es nicht automatisch erneut, sondern melden Sie den Fehler."
            : $"Die automatische Planung konnte wegen eines technischen Fehlers nicht abgeschlossen werden. Fehlerkennung: {correlationId}.";
    }

    private void SetPreview(AutomaticSchedulePreview? preview)
    {
        lock (previewLock)
        {
            currentPreview = preview;
        }
    }

    private void RecordTechnicalErrors(IEnumerable<AutomaticScheduleError> errors)
    {
        foreach (AutomaticScheduleError error in errors.Where(error =>
            !string.IsNullOrWhiteSpace(error.CorrelationId)
            && error.TechnicalDetails is not null))
        {
            RecordTechnicalError(error);
        }
    }

    private void RecordTechnicalError(AutomaticScheduleError error)
    {
        try
        {
            _ = technicalErrorStore.TryWrite("GenerateAutomaticSchedule", error);
        }
        catch (Exception exception)
        {
            Trace.TraceError(
                "Automatic schedule diagnostic store failed. ExceptionType={0}",
                exception.GetType().FullName);
        }
    }

    private sealed class DiscardingTechnicalErrorStore
        : IAutomaticScheduleTechnicalErrorStore
    {
        internal static readonly DiscardingTechnicalErrorStore Instance = new();

        public bool TryWrite(
            string operation,
            AutomaticScheduleError technicalError) => false;
    }
}
