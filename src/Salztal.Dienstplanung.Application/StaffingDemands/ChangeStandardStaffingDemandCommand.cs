using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed class ChangeStandardStaffingDemandCommand
{
    private readonly IStaffingDemandReader _reader;
    private readonly IStandardStaffingDemandRevisionStore _store;

    public ChangeStandardStaffingDemandCommand(
        IStaffingDemandReader reader,
        IStandardStaffingDemandRevisionStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<StaffingDemandCommandResult> ExecuteAsync(
        ChangeStandardStaffingDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        List<StaffingDemandCommandError> errors = [];
        ValidateRequest(request, errors);
        ValidateExpectedIdentifier(request.ExpectedCurrentRevisionId, errors);
        if (errors.Count > 0)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                errors);
        }

        StandardStaffingDemandRevisionValidationResult revisionResult =
            CreateRevision(request, correctionSequence: 1);
        errors.AddRange(revisionResult.Errors.Select(
            StaffingDemandCommandErrors.FromStandardValidation));
        if (errors.Count > 0)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                errors);
        }

        StandardStaffingDemandRevision validatedRevision = revisionResult.Value!;
        StaffingDemandReadData data = await _reader.LoadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        StaffingDemandCommandContextValidationResult contextResult =
            StaffingDemandCommandContext.Create(
                data,
                validatedRevision.Key.WorkLocationId,
                validatedRevision.Key.ShiftTypeId);
        if (contextResult.Failure is not null)
        {
            return contextResult.Failure;
        }

        IReadOnlyList<StandardStaffingDemandRevision> revisions =
            contextResult.Value!.StandardRevisions.Revisions;
        StandardStaffingDemandRevision? current = revisions
            .Where(candidate => candidate.Key == validatedRevision.Key)
            .Where(candidate =>
                candidate.EffectiveFromMonday <= validatedRevision.EffectiveFromMonday)
            .OrderBy(candidate => candidate.EffectiveFromMonday)
            .ThenBy(candidate => candidate.CorrectionSequence)
            .LastOrDefault();
        StaffingDemandCommandResult? expectationFailure = ValidateCurrent(
            request.ExpectedCurrentRevisionId,
            current);
        if (expectationFailure is not null)
        {
            return expectationFailure;
        }

        StandardStaffingDemandRevision? sameMonday = revisions
            .Where(candidate => candidate.Key == validatedRevision.Key)
            .Where(candidate =>
                candidate.EffectiveFromMonday == validatedRevision.EffectiveFromMonday)
            .MaxBy(candidate => candidate.CorrectionSequence);
        if (sameMonday?.CorrectionSequence == int.MaxValue)
        {
            return StaffingDemandCommandResult.Failure(
                StaffingDemandCommandStatus.ValidationFailed,
                [new StaffingDemandCommandError(
                    StaffingDemandCommandErrorCode.UnsupportedOperation,
                    "Für diesen Montag können keine weiteren Korrekturfassungen gespeichert werden.")]);
        }

        int correctionSequence = (sameMonday?.CorrectionSequence ?? 0) + 1;
        StandardStaffingDemandRevision revision = correctionSequence == 1
            ? validatedRevision
            : CreateRevision(request, correctionSequence).Value!;

        StandardStaffingDemandRevisionSetValidationResult historyResult =
            StandardStaffingDemandRevisionSet.Create(
                revisions.Append(revision));
        if (!historyResult.IsSuccess)
        {
            return FromHistoryFailure(historyResult.Errors);
        }

        StaffingDemandWriteStoreResult storeResult = await _store.AppendAsync(
            current,
            revision,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return StaffingDemandCommandErrors.FromStoreResult(storeResult);
    }

    private static StandardStaffingDemandRevisionValidationResult CreateRevision(
        ChangeStandardStaffingDemandRequest request,
        int correctionSequence)
    {
        if (request.Kind == StandardStaffingDemandRevisionKind.Remove)
        {
            return StandardStaffingDemandRevision.CreateRemoval(
                request.RevisionId,
                request.DayOfWeek,
                request.WorkLocationId,
                request.ShiftTypeId,
                request.EffectiveFromMonday,
                correctionSequence);
        }

        return request.Kind switch
        {
            StandardStaffingDemandRevisionKind.Add =>
                StandardStaffingDemandRevision.CreateAddition(
                    request.RevisionId,
                    request.DayOfWeek,
                    request.WorkLocationId,
                    request.ShiftTypeId,
                    request.EffectiveFromMonday,
                    request.ActualStart.GetValueOrDefault(),
                    request.ActualEnd.GetValueOrDefault(),
                    request.RequiredEmployeeCount.GetValueOrDefault(),
                    correctionSequence),
            StandardStaffingDemandRevisionKind.Replace =>
                StandardStaffingDemandRevision.CreateReplacement(
                    request.RevisionId,
                    request.DayOfWeek,
                    request.WorkLocationId,
                    request.ShiftTypeId,
                    request.EffectiveFromMonday,
                    request.ActualStart.GetValueOrDefault(),
                    request.ActualEnd.GetValueOrDefault(),
                    request.RequiredEmployeeCount.GetValueOrDefault(),
                    correctionSequence),
            _ => throw new InvalidOperationException(
                $"Unsupported standard staffing-demand revision kind: {request.Kind}"),
        };
    }

    private static void ValidateRequest(
        ChangeStandardStaffingDemandRequest request,
        List<StaffingDemandCommandError> errors)
    {
        if (!Enum.IsDefined(request.Kind))
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.UnsupportedOperation,
                "Die ausgewählte Änderung des Wochenstandards wird nicht unterstützt."));
            return;
        }

        if (request.Kind == StandardStaffingDemandRevisionKind.Remove)
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

    private static void ValidateExpectedIdentifier(
        Guid? expectedIdentifier,
        List<StaffingDemandCommandError> errors)
    {
        if (expectedIdentifier == Guid.Empty)
        {
            errors.Add(new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.ExpectedIdentifierRequired,
                "Die Kennung des erwarteten aktuellen Revisionsstands ist ungültig."));
        }
    }

    private static StaffingDemandCommandResult? ValidateCurrent(
        Guid? expectedCurrentRevisionId,
        StandardStaffingDemandRevision? current)
    {
        if (expectedCurrentRevisionId is not null && current is null)
        {
            return StaffingDemandCommandErrors.NotFound(
                "Der erwartete vorherige Wochenstandard wurde nicht gefunden. Bitte laden Sie die Daten neu.");
        }

        if (expectedCurrentRevisionId is null && current is not null)
        {
            return StaffingDemandCommandErrors.Conflict();
        }

        return expectedCurrentRevisionId != current?.Id.Value
            ? StaffingDemandCommandErrors.Conflict()
            : null;
    }

    private static StaffingDemandCommandResult FromHistoryFailure(
        IEnumerable<StandardStaffingDemandRevisionSetValidationError> errors)
    {
        StandardStaffingDemandRevisionSetValidationError first = errors.First();
        return first.Code switch
        {
            StandardStaffingDemandRevisionSetValidationCode
                .DuplicateKeyEffectiveMondayAndCorrectionSequence =>
                StaffingDemandCommandErrors.Conflict(),
            StandardStaffingDemandRevisionSetValidationCode
                .NonContiguousCorrectionSequence => StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.StoredDataInvalid,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.StoredDataInvalid,
                        "Die gespeicherte Korrekturfolge des Wochenstandards ist widersprüchlich.")]),
            StandardStaffingDemandRevisionSetValidationCode
                .AdditionRequiresMissingStandard => StaffingDemandCommandResult.Failure(
                    StaffingDemandCommandStatus.ValidationFailed,
                    [new StaffingDemandCommandError(
                        StaffingDemandCommandErrorCode.StandardAlreadyExists,
                        "Für diesen Wochentag besteht bereits ein wirksamer Wochenstandard. Bitte wählen Sie Ersetzen.")]),
            StandardStaffingDemandRevisionSetValidationCode
                .ReplacementRequiresExistingStandard
                or StandardStaffingDemandRevisionSetValidationCode
                    .RemovalRequiresExistingStandard =>
                StaffingDemandCommandErrors.NotFound(
                    "Für diesen Wochentag besteht kein wirksamer Wochenstandard."),
            _ => throw new InvalidOperationException(
                $"Unsupported standard staffing-demand history code: {first.Code}"),
        };
    }
}
