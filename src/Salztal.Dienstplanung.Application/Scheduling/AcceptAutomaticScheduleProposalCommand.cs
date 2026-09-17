using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class AcceptAutomaticScheduleProposalCommand
{
    private readonly IScheduleWorkspaceReader reader;
    private readonly IAcceptAutomaticScheduleProposalStore store;

    public AcceptAutomaticScheduleProposalCommand(
        IScheduleWorkspaceReader reader,
        IAcceptAutomaticScheduleProposalStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        this.reader = reader;
        this.store = store;
    }

    public async Task<AutomaticScheduleAcceptanceResult> ExecuteAsync(
        AcceptAutomaticScheduleProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        try
        {
            AutomaticScheduleProposal proposal = request.Proposal;
            SchedulePeriodValidationResult periodResult =
                SchedulePeriod.Create(request.PeriodMonday);
            if (!periodResult.IsSuccess
                || proposal.ExpectedDraftVersion <= 0
                || proposal.ExpectedDraftVersion > int.MaxValue)
            {
                return Failure(
                    AutomaticScheduleAcceptanceStatus.ValidationFailed,
                    "Die Angaben für die Übernahme sind ungültig. Bitte laden Sie den Zeitraum erneut.");
            }

            ScheduleWorkspaceReadData data = await reader.LoadAsync(
                periodResult.Value!,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            ScheduleDayCommandContextValidationResult contextResult =
                ScheduleDayCommandContext.Create(
                    proposal.DraftId,
                    proposal.ExpectedDraftVersion,
                    request.PeriodMonday,
                    data);
            if (contextResult.Failure is not null)
            {
                return MapContextFailure(contextResult.Failure);
            }

            if (data.PreparedSnapshot is null
                || data.PreparedSnapshot.Id != proposal.SnapshotId
                || data.PreparedSnapshot.DraftId != proposal.DraftId
                || data.PreparedSnapshot.DraftVersion != proposal.ExpectedDraftVersion)
            {
                return Conflict(
                    "Der Vorschlag gehört nicht mehr zur aktuellen Planungsvorbereitung. Bitte erzeugen Sie einen neuen Vorschlag.");
            }

            ScheduleDayCommandContext context = contextResult.Value!;
            ScheduleAssignment[]? replacements =
                AutomaticScheduleProposalMapper.TryCreateAssignments(
                    proposal,
                    context.Draft,
                    data.StaffingDemands.ServiceCatalog);
            GeneratedDayOffMarker[]? markers = TryCreateMarkers(proposal);
            if (replacements is null || markers is null)
            {
                return InvalidProposal();
            }

            ScheduleDraftValidationResult replacementResult =
                context.Draft.ReplaceAutomaticGeneration(replacements, markers);
            if (!replacementResult.IsSuccess)
            {
                return Failure(
                    AutomaticScheduleAcceptanceStatus.ProposalInvalid,
                    "Der automatische Vorschlag widerspricht dem aktuellen Entwurf und wurde nicht übernommen.",
                    replacementResult.Errors);
            }

            ScheduleDraft updatedDraft = replacementResult.Value!;
            AcceptAutomaticScheduleProposalStoreResult storeResult =
                await store.AcceptAsync(
                    new AcceptAutomaticScheduleProposalChange(
                        updatedDraft,
                        context.Draft.Version.Value,
                        new AutomaticScheduleRunRecord(
                            proposal.SnapshotId,
                            proposal.Metadata,
                            AutomaticScheduleObjectiveSnapshot.Create(
                                proposal.ObjectiveVector))),
                    cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (storeResult.Status
                    == AcceptAutomaticScheduleProposalStoreStatus.Conflict)
            {
                return Conflict(
                    "Der Entwurf oder die Planungsvorbereitung wurde zwischenzeitlich geändert. Der Vorschlag wurde nicht übernommen.");
            }

            if (storeResult.Draft is null
                || storeResult.Draft.Id != updatedDraft.Id
                || storeResult.Draft.Version != updatedDraft.Version)
            {
                return Failure(
                    AutomaticScheduleAcceptanceStatus.TechnicalFailure,
                    "Die Übernahme lieferte einen widersprüchlichen Speicherstand. Bitte laden Sie den Zeitraum erneut.");
            }

            return AutomaticScheduleAcceptanceResult.Success(storeResult.Draft);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failure(
                AutomaticScheduleAcceptanceStatus.TechnicalFailure,
                "Der automatische Vorschlag konnte wegen eines technischen Fehlers nicht übernommen werden. Der Entwurf blieb unverändert.");
        }
    }

    private static GeneratedDayOffMarker[]? TryCreateMarkers(
        AutomaticScheduleProposal proposal)
    {
        List<GeneratedDayOffMarker> markers = [];
        foreach (AutomaticScheduleDayOffProposal dayOff in proposal.GeneratedDayOffs)
        {
            if (!EmployeeId.TryCreate(dayOff.EmployeeId, out EmployeeId? employeeId))
            {
                return null;
            }

            markers.Add(new GeneratedDayOffMarker(employeeId, dayOff.Date));
        }

        return markers.ToArray();
    }

    private static AutomaticScheduleAcceptanceResult MapContextFailure(
        ScheduleDayChangeResult failure)
    {
        return failure.Status switch
        {
            ScheduleDayChangeStatus.NotFound => Failure(
                AutomaticScheduleAcceptanceStatus.DraftNotFound,
                "Der Entwurf ist nicht mehr vorhanden. Bitte laden Sie den Zeitraum erneut."),
            ScheduleDayChangeStatus.Conflict => Conflict(
                "Der Entwurf wurde zwischenzeitlich geändert. Bitte erzeugen Sie einen neuen Vorschlag."),
            ScheduleDayChangeStatus.ValidationFailed => Failure(
                AutomaticScheduleAcceptanceStatus.ValidationFailed,
                "Die Angaben für die Übernahme sind ungültig. Bitte laden Sie den Zeitraum erneut."),
            _ => Failure(
                AutomaticScheduleAcceptanceStatus.TechnicalFailure,
                "Die gespeicherten Planungsdaten sind widersprüchlich. Der Vorschlag wurde nicht übernommen."),
        };
    }

    private static AutomaticScheduleAcceptanceResult InvalidProposal() => Failure(
        AutomaticScheduleAcceptanceStatus.ProposalInvalid,
        "Der automatische Vorschlag ist unvollständig oder widersprüchlich und wurde nicht übernommen.");

    private static AutomaticScheduleAcceptanceResult Conflict(string message) =>
        Failure(AutomaticScheduleAcceptanceStatus.Conflict, message);

    private static AutomaticScheduleAcceptanceResult Cancelled() => Failure(
        AutomaticScheduleAcceptanceStatus.Cancelled,
        "Die Übernahme wurde abgebrochen. Der Entwurf blieb unverändert.");

    private static AutomaticScheduleAcceptanceResult Failure(
        AutomaticScheduleAcceptanceStatus status,
        string message,
        IEnumerable<ScheduleDraftValidationError>? validationErrors = null) =>
        AutomaticScheduleAcceptanceResult.Failure(
            status,
            message,
            validationErrors);
}
