using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed class SaveAvailabilityEntryCommand
{
    private readonly IAvailabilityReader _reader;
    private readonly ISetAvailabilityEntryStore _store;

    public SaveAvailabilityEntryCommand(
        IAvailabilityReader reader,
        ISetAvailabilityEntryStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<AvailabilityEntryCommandResult> ExecuteAsync(
        SaveAvailabilityEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        AvailabilityEntryCommandResult? requestFailure = ValidateRequest(request);
        if (requestFailure is not null)
        {
            return requestFailure;
        }

        AvailabilityEntryValidationResult entryResult = AvailabilityEntry.Create(
            request.EmployeeId,
            request.Date,
            AvailabilityEntrySnapshotMapper.ToDomainKind(request.Kind));
        AvailabilityEntry replacement = entryResult.Value!;
        DateOnly weekMonday = GetMonday(request.Date);
        AvailabilityReadData data = await _reader.LoadAsync(
            weekMonday,
            weekMonday.AddDays(6),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        AvailabilityEntryCommandContextValidationResult contextResult =
            AvailabilityEntryCommandContext.Create(
                replacement.EmployeeId,
                replacement.Date,
                data);
        if (contextResult.Failure is not null)
        {
            return contextResult.Failure;
        }

        AvailabilityEntryCommandContext context = contextResult.Value!;
        if (!MatchesExpectedVersion(context.Current, request.ExpectedChangeVersion))
        {
            return AvailabilityEntryCommandErrors.Conflict();
        }

        if (context.Current is not null
            && context.Current.Entry.Kind != replacement.Kind
            && request.ReplacementConfirmation
                != AvailabilityEntryReplacementConfirmation.Confirmed)
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ConfirmationRequired,
                AvailabilityEntryCommandErrorCode.ConfirmationRequired,
                "Bitte bestätigen Sie das Ersetzen des vorhandenen Tageseintrags.");
        }

        WeeklyAvailabilityValidationResult weekResult = WeeklyAvailability.Calculate(
            context.Employee,
            context.EmployeeType,
            weekMonday,
            context.Entries.WithEntry(replacement));
        if (!weekResult.IsSuccess)
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.VacationAndSicknessNotAllowed,
                "Für diesen Mitarbeitertyp sind U und K nicht zulässig. Verwenden Sie bei Bedarf ein rotes X.");
        }

        AvailabilityEntryWriteStoreResult storeResult = await _store.SaveAsync(
            context.Current?.Entry,
            context.Current?.ChangeVersion,
            replacement,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return storeResult.Status switch
        {
            AvailabilityEntryWriteStoreStatus.Succeeded =>
                AvailabilityEntryCommandResult.Success(
                    AvailabilityEntrySnapshotMapper.Create(
                        replacement,
                        storeResult.ChangeVersion!.Value)),
            AvailabilityEntryWriteStoreStatus.Conflict =>
                AvailabilityEntryCommandErrors.Conflict(),
            _ => throw new InvalidOperationException(
                $"Unsupported availability write result: {storeResult.Status}"),
        };
    }

    private static AvailabilityEntryCommandResult? ValidateRequest(
        SaveAvailabilityEntryRequest request)
    {
        if (!EmployeeId.TryCreate(request.EmployeeId, out _))
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.EmployeeIdentifierRequired,
                "Die ausgewählte Person besitzt keine gültige Kennung.");
        }

        if (!Enum.IsDefined(request.Kind))
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.UnsupportedKind,
                "Das ausgewählte Tageskennzeichen wird nicht unterstützt.");
        }

        if (request.ExpectedChangeVersion is <= 0)
        {
            return AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.ExpectedChangeVersionMustBePositive,
                "Der erwartete Änderungsstand des Tagesfelds ist ungültig.");
        }

        DateOnly monday = GetMonday(request.Date);
        return monday.DayNumber > DateOnly.MaxValue.DayNumber - 6
            ? AvailabilityEntryCommandErrors.Failure(
                AvailabilityEntryCommandStatus.ValidationFailed,
                AvailabilityEntryCommandErrorCode.DateMustFitWeek,
                "Für das ausgewählte Datum kann keine vollständige Woche gebildet werden.")
            : null;
    }

    private static bool MatchesExpectedVersion(
        AvailabilityEntryReadItem? current,
        long? expectedChangeVersion)
    {
        return current?.ChangeVersion == expectedChangeVersion;
    }

    internal static DateOnly GetMonday(DateOnly date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
