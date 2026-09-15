using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed class RemoveAvailabilityEntryCommand
{
    private readonly IAvailabilityReader _reader;
    private readonly IRemoveAvailabilityEntryStore _store;

    public RemoveAvailabilityEntryCommand(
        IAvailabilityReader reader,
        IRemoveAvailabilityEntryStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<AvailabilityEntryCommandResult> ExecuteAsync(
        RemoveAvailabilityEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        AvailabilityEntryCommandResult? requestFailure = ValidateRequest(request);
        if (requestFailure is not null)
        {
            return requestFailure;
        }

        EmployeeId.TryCreate(request.EmployeeId, out EmployeeId? employeeId);
        DateOnly weekMonday = SaveAvailabilityEntryCommand.GetMonday(request.Date);
        AvailabilityReadData data = await _reader.LoadAsync(
            weekMonday,
            weekMonday.AddDays(6),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        AvailabilityEntryCommandContextValidationResult contextResult =
            AvailabilityEntryCommandContext.Create(employeeId!, request.Date, data);
        if (contextResult.Failure is not null)
        {
            return contextResult.Failure;
        }

        AvailabilityEntryReadItem? current = contextResult.Value!.Current;
        if (current is null)
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.NotFound,
                AvailabilityEntryCommandErrorCode.EntryNotFound,
                "Für die ausgewählte Person und das Datum ist kein Tageseintrag vorhanden.");
        }

        if (current.ChangeVersion != request.ExpectedChangeVersion)
        {
            return AvailabilityEntryCommandErrors.Conflict();
        }

        AvailabilityEntryRemoveStoreResult storeResult = await _store.RemoveAsync(
            current.Entry,
            current.ChangeVersion,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return storeResult switch
        {
            AvailabilityEntryRemoveStoreResult.Succeeded =>
                AvailabilityEntryCommandResult.Success(
                    AvailabilityEntrySnapshotMapper.Create(
                        current.Entry,
                        current.ChangeVersion)),
            AvailabilityEntryRemoveStoreResult.Conflict =>
                AvailabilityEntryCommandErrors.Conflict(),
            _ => throw new InvalidOperationException(
                $"Unsupported availability removal result: {storeResult}"),
        };
    }

    private static AvailabilityEntryCommandResult? ValidateRequest(
        RemoveAvailabilityEntryRequest request)
    {
        if (!EmployeeId.TryCreate(request.EmployeeId, out _))
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.EmployeeIdentifierRequired,
                "Die ausgewählte Person besitzt keine gültige Kennung.");
        }

        if (request.ExpectedChangeVersion <= 0)
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.ExpectedChangeVersionMustBePositive,
                "Der erwartete Änderungsstand des Tagesfelds ist ungültig.");
        }

        if (request.Confirmation != AvailabilityEntryRemovalConfirmation.Confirmed)
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ConfirmationRequired,
                AvailabilityEntryCommandErrorCode.ConfirmationRequired,
                "Bitte bestätigen Sie das Entfernen des vorhandenen Tageseintrags.");
        }

        DateOnly monday = SaveAvailabilityEntryCommand.GetMonday(request.Date);
        return monday.DayNumber > DateOnly.MaxValue.DayNumber - 6
            ? AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.DateMustFitWeek,
                "Für das ausgewählte Datum kann keine vollständige Woche gebildet werden.")
            : null;
    }
}
