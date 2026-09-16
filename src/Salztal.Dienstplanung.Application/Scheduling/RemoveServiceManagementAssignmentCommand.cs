using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class RemoveServiceManagementAssignmentCommand
{
    private readonly IScheduleWorkspaceReader _reader;
    private readonly IChangeScheduleDayStore _store;

    public RemoveServiceManagementAssignmentCommand(
        IScheduleWorkspaceReader reader,
        IChangeScheduleDayStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<ScheduleDayChangeResult> ExecuteAsync(
        RemoveServiceManagementAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!ScheduleAssignmentId.TryCreate(request.AssignmentId, out ScheduleAssignmentId? id))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AssignmentIdentifierRequired,
                "Die ausgewählte Einteilung besitzt keine gültige Kennung.");
        }

        SchedulePeriodValidationResult periodResult =
            SchedulePeriod.Create(request.PeriodMonday);
        if (!periodResult.IsSuccess)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.PeriodInvalid,
                "Der Planungszeitraum ist ungültig.");
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
            return contextResult.Failure;
        }

        ScheduleDayCommandContext context = contextResult.Value!;
        ScheduleAssignment? assignment = context.Draft.Assignments
            .SingleOrDefault(candidate => candidate.Id == id);
        if (assignment is null)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.NotFound,
                ScheduleDayChangeErrorCode.AssignmentNotFound,
                "Die ausgewählte Typ1-Einteilung ist nicht mehr vorhanden.");
        }

        if (assignment.Origin != AssignmentOrigin.ServiceManagement)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.ServiceManagementAssignmentRequired,
                "Nur eine geschützte Typ1-Einteilung kann mit dieser Aktion entfernt werden.");
        }

        ScheduleDraftValidationResult draftResult;
        try
        {
            draftResult = ScheduleDayCommandContext.CreateUpdatedDraft(
                context.Draft,
                context.Draft.AvailabilityEntries,
                context.Draft.Assignments.Where(candidate => candidate.Id != id));
        }
        catch (OverflowException)
        {
            return ScheduleDayChangeCommandSupport.StoredDataInvalid();
        }

        if (!draftResult.IsSuccess)
        {
            return ScheduleDayChangeCommandSupport.StoredDataInvalid();
        }

        return await ScheduleDayChangeCommandSupport.StoreAsync(
            _store,
            context.Draft,
            draftResult.Value!,
            ScheduleAvailabilityMutation.None(),
            null,
            assignment.EmployeeId.Value,
            assignment.Date,
            cancellationToken);
    }
}
