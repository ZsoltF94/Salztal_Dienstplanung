using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class RemoveScheduleDayEntryCommand
{
    private readonly IScheduleWorkspaceReader _reader;
    private readonly IChangeScheduleDayStore _store;

    public RemoveScheduleDayEntryCommand(
        IScheduleWorkspaceReader reader,
        IChangeScheduleDayStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<ScheduleDayChangeResult> ExecuteAsync(
        RemoveScheduleDayEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.ExpectedDayEntryChangeVersion <= 0
            || request.RemovalConfirmation != ScheduleReplacementConfirmation.Confirmed)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.ConfirmationRequired,
                ScheduleDayChangeErrorCode.ReplacementConfirmationRequired,
                "Bitte bestätigen Sie das Entfernen des Tageskennzeichens.");
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
            out _);
        if (employeeFailure is not null)
        {
            return employeeFailure;
        }

        AvailabilityEntryReadItem? currentEntry = context.FindCurrentEntry(
            employee!.Id,
            request.Date);
        if (currentEntry is null)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.NotFound,
                ScheduleDayChangeErrorCode.AvailabilityEntryInvalid,
                "Das Tageskennzeichen ist nicht mehr vorhanden.");
        }

        if (currentEntry.ChangeVersion != request.ExpectedDayEntryChangeVersion)
        {
            return ScheduleDayChangeResult.Failure(
                ScheduleDayChangeStatus.Conflict,
                ScheduleDayChangeErrorCode.AvailabilityVersionConflict,
                "Das Tageskennzeichen wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
        }

        AvailabilityEntrySet availabilityEntries = AvailabilityEntrySet.Create(
            context.Draft.AvailabilityEntries.Entries.Where(entry =>
                entry.EmployeeId != employee.Id || entry.Date != request.Date)).Value
            ?? throw new InvalidOperationException(
                "Validated availability entries could not be reconstructed.");
        ScheduleDraftValidationResult draftResult;
        try
        {
            draftResult = ScheduleDayCommandContext.CreateUpdatedDraft(
                context.Draft,
                availabilityEntries,
                context.Draft.Assignments);
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
            ScheduleAvailabilityMutation.Remove(currentEntry),
            null,
            employee.Id.Value,
            request.Date,
            cancellationToken);
    }
}
