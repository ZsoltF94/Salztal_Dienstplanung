using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed class SaveStaffingDemandDateExceptionCommand
{
    private readonly IStaffingDemandReader _reader;
    private readonly IStaffingDemandDateExceptionStore _store;

    public SaveStaffingDemandDateExceptionCommand(
        IStaffingDemandReader reader,
        IStaffingDemandDateExceptionStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<StaffingDemandCommandResult> ExecuteAsync(
        SaveStaffingDemandDateExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        List<StaffingDemandCommandError> errors = [];
        ValidateRequest(request, errors);
        if (request.ExpectedCurrentExceptionId == Guid.Empty)
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.ExpectedIdentifierRequired,
                "Die erwartete Ausnahmekennung ist ungültig."));
        }

        if (errors.Count > 0)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                errors);
        }

        StaffingDemandDateExceptionValidationResult exceptionResult =
            CreateDateException(request);
        errors.AddRange(exceptionResult.Errors.Select(
            StaffingDemandCommandErrors.FromDateExceptionValidation));
        if (errors.Count > 0)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                errors);
        }

        StaffingDemandDateException dateException = exceptionResult.Value!;
        StaffingDemandReadData data = await _reader.LoadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        StaffingDemandCommandContextValidationResult contextResult =
            StaffingDemandCommandContext.Create(
                data,
                dateException.Key.WorkLocationId,
                dateException.Key.ShiftTypeId);
        if (contextResult.Failure is not null)
        {
            return contextResult.Failure;
        }

        StaffingDemandDateException? current = data.DateExceptions
            .SingleOrDefault(candidate => candidate.Key == dateException.Key);
        StaffingDemandCommandResult? currentFailure = ValidateCurrent(
            request.ExpectedCurrentExceptionId,
            current);
        if (currentFailure is not null)
        {
            return currentFailure;
        }

        StaffingDemandDateExceptionSetValidationResult proposedExceptions =
            StaffingDemandDateExceptionSet.Create(
                data.DateExceptions
                    .Where(candidate => candidate.Key != dateException.Key)
                    .Append(dateException));
        StaffingDemandCommandResult? applicabilityFailure = ValidateApplicability(
            dateException.Key.Date,
            contextResult.Value!.StandardRevisions,
            proposedExceptions.Value!);
        if (applicabilityFailure is not null)
        {
            return applicabilityFailure;
        }

        StaffingDemandWriteStoreResult storeResult = await _store.SaveAsync(
            current,
            dateException,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return StaffingDemandCommandErrors.FromStoreResult(storeResult);
    }

    private static StaffingDemandDateExceptionValidationResult CreateDateException(
        SaveStaffingDemandDateExceptionRequest request)
    {
        if (request.Kind == StaffingDemandDateExceptionKind.Remove)
        {
            return StaffingDemandDateException.CreateRemoval(
                request.ExceptionId,
                request.Date,
                request.WorkLocationId,
                request.ShiftTypeId);
        }

        return request.Kind switch
        {
            StaffingDemandDateExceptionKind.Add =>
                StaffingDemandDateException.CreateAddition(
                    request.ExceptionId,
                    request.Date,
                    request.WorkLocationId,
                    request.ShiftTypeId,
                    request.ActualStart.GetValueOrDefault(),
                    request.ActualEnd.GetValueOrDefault(),
                    request.RequiredEmployeeCount.GetValueOrDefault()),
            StaffingDemandDateExceptionKind.Replace =>
                StaffingDemandDateException.CreateReplacement(
                    request.ExceptionId,
                    request.Date,
                    request.WorkLocationId,
                    request.ShiftTypeId,
                    request.ActualStart.GetValueOrDefault(),
                    request.ActualEnd.GetValueOrDefault(),
                    request.RequiredEmployeeCount.GetValueOrDefault()),
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand date-exception kind: {request.Kind}"),
        };
    }

    private static void ValidateRequest(
        SaveStaffingDemandDateExceptionRequest request,
        List<StaffingDemandCommandError> errors)
    {
        if (!Enum.IsDefined(request.Kind))
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.UnsupportedOperation,
                "Die ausgewählte Art der Datumsausnahme wird nicht unterstützt."));
            return;
        }

        if (request.Kind == StaffingDemandDateExceptionKind.Remove)
        {
            return;
        }

        if (request.ActualStart is null)
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.ActualStartRequired,
                "Bitte geben Sie die tatsächliche Startzeit an."));
        }

        if (request.ActualEnd is null)
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.ActualEndRequired,
                "Bitte geben Sie die tatsächliche Endzeit an."));
        }

        if (request.RequiredEmployeeCount is null)
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.RequiredEmployeeCountRequired,
                "Bitte geben Sie die benötigte Personenzahl an."));
        }
    }

    private static StaffingDemandCommandResult? ValidateCurrent(
        Guid? expectedCurrentExceptionId,
        StaffingDemandDateException? current)
    {
        if (expectedCurrentExceptionId is not null && current is null)
        {
            return StaffingDemandCommandErrors.NotFound(
                "Die erwartete Datumsausnahme wurde nicht gefunden. Bitte laden Sie die Daten neu.");
        }

        if (expectedCurrentExceptionId is null && current is not null)
        {
            return StaffingDemandCommandErrors.Conflict();
        }

        return expectedCurrentExceptionId != current?.Id.Value
            ? StaffingDemandCommandErrors.Conflict()
            : null;
    }

    private static StaffingDemandCommandResult? ValidateApplicability(
        DateOnly date,
        StandardStaffingDemandRevisionSet standards,
        StaffingDemandDateExceptionSet exceptions)
    {
        int mondayOffset = ((int)date.DayOfWeek + 6) % 7;
        StaffingDemandWeekResolutionResult result = StaffingDemandWeek.Resolve(
            date.AddDays(-mondayOffset),
            standards,
            exceptions);
        if (result.IsSuccess)
        {
            return null;
        }

        StaffingDemandWeekResolutionError first = result.Errors[0];
        return first.Code switch
        {
            StaffingDemandWeekResolutionCode.AdditionRequiresMissingStandard =>
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.ValidationFailed,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.StandardAlreadyExists,
                        "Für dieses Datum besteht bereits ein Wochenstandard. Bitte wählen Sie Ersetzen.")]),
            StaffingDemandWeekResolutionCode.ReplacementRequiresExistingStandard
                or StaffingDemandWeekResolutionCode.RemovalRequiresExistingStandard =>
                StaffingDemandCommandErrors.NotFound(
                    "Für dieses Datum besteht kein wirksamer Wochenstandard."),
            StaffingDemandWeekResolutionCode.WeekMustFitSevenDays =>
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.ValidationFailed,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.EffectiveDateMustBeMonday,
                        "Für dieses Datum kann keine vollständige Planungswoche gebildet werden.")]),
            StaffingDemandWeekResolutionCode.RequiredWorkMinutesOverflow =>
                StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.ValidationFailed,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.StoredDataInvalid,
                        "Die Bedarfsminuten überschreiten den zulässigen Zahlenbereich.")]),
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand applicability code: {first.Code}"),
        };
    }
}
