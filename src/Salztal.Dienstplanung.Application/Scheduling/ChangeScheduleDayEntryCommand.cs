using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class ChangeScheduleDayEntryCommand
{
    private readonly IScheduleWorkspaceReader _reader;
    private readonly IChangeScheduleDayStore _store;

    public ChangeScheduleDayEntryCommand(
        IScheduleWorkspaceReader reader,
        IChangeScheduleDayStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<ScheduleDayChangeResult> ExecuteAsync(
        ChangeScheduleDayEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.IsDefined(request.Kind))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.UnsupportedAvailabilityKind,
                "Das ausgewählte Tageskennzeichen wird nicht unterstützt.");
        }

        if (!Enum.IsDefined(request.ReplacementConfirmation))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AssignmentInvalid,
                "Der Bestätigungswert ist ungültig.");
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
        if (!context.Draft.Period.Contains(request.Date))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.PeriodInvalid,
                "Das ausgewählte Datum liegt außerhalb des Planungszeitraums.");
        }

        ScheduleDayChangeResult? employeeFailure = context.ValidateActiveEmployee(
            request.EmployeeId,
            out Employee? employee,
            out EmployeeType? employeeType);
        if (employeeFailure is not null)
        {
            return employeeFailure;
        }

        AvailabilityEntryReadItem? currentEntry = context.FindCurrentEntry(
            employee!.Id,
            request.Date);
        if (!ScheduleDayCommandContext.MatchesExpectedAvailabilityVersion(
                currentEntry,
                request.ExpectedDayEntryChangeVersion))
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.Conflict,
                ScheduleDayChangeErrorCode.AvailabilityVersionConflict,
                "Das Tageskennzeichen wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
        }

        AvailabilityEntryKind domainKind =
            AvailabilityEntrySnapshotMapper.ToDomainKind(request.Kind);
        if (currentEntry?.Entry.Kind == domainKind)
        {
            return ScheduleDayChangeResult.Success(
                new ScheduleDayChangeSnapshot(
                    context.Draft.Id.Value,
                    context.Draft.Version.Value,
                    null,
                    employee.Id.Value,
                    request.Date));
        }

        ScheduleAssignment? currentAssignment = context.Draft.Assignments
            .SingleOrDefault(assignment =>
                assignment.EmployeeId == employee.Id
                && assignment.Date == request.Date);
        if (currentAssignment is not null
            && currentAssignment.Origin != AssignmentOrigin.ServiceManagement)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.ServiceManagementAssignmentRequired,
                "Der vorhandene Dienst ist keine geschützte Typ1-Einteilung und kann nicht durch ein Tageskennzeichen ersetzt werden.");
        }

        if ((currentEntry is not null || currentAssignment is not null)
            && request.ReplacementConfirmation
                != ScheduleReplacementConfirmation.Confirmed)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ConfirmationRequired,
                ScheduleDayChangeErrorCode.ReplacementConfirmationRequired,
                "Bitte bestätigen Sie das Ersetzen des vorhandenen Tageszustands.");
        }

        AvailabilityEntryValidationResult entryResult = AvailabilityEntry.Create(
            employee.Id.Value,
            request.Date,
            domainKind);
        AvailabilityEntry replacement = entryResult.Value!;
        AvailabilityEntrySet updatedEntries = context.Workspace.AvailabilityEntries
            .WithEntry(replacement);
        DateOnly weekMonday = request.Date.AddDays(
            -(((int)request.Date.DayOfWeek + 6) % 7));
        WeeklyAvailabilityValidationResult weekResult = WeeklyAvailability.Calculate(
            employee,
            employeeType!,
            weekMonday,
            updatedEntries);
        if (!weekResult.IsSuccess)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ValidationFailed,
                ScheduleDayChangeErrorCode.AvailabilityEntryInvalid,
                "Für diesen Mitarbeitertyp sind U und K nicht zulässig. Verwenden Sie bei Bedarf ein rotes X.");
        }

        ScheduleDraftValidationResult draftResult;
        try
        {
            draftResult = ScheduleDayCommandContext.CreateUpdatedDraft(
                context.Draft,
                context.Draft.AvailabilityEntries.WithEntry(replacement),
                context.Draft.Assignments.Where(candidate =>
                    candidate != currentAssignment));
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
            ScheduleAvailabilityMutation.Upsert(currentEntry, replacement),
            null,
            employee.Id.Value,
            request.Date,
            cancellationToken);
    }
}
