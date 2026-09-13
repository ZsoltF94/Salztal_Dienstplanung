using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed class UpdateShiftTypeStandardTimeCommand
{
    private readonly IShiftTypeStandardTimeUpdateStore _store;

    public UpdateShiftTypeStandardTimeCommand(IShiftTypeStandardTimeUpdateStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public async Task<UpdateShiftTypeStandardTimeResult> ExecuteAsync(
        UpdateShiftTypeStandardTimeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        List<UpdateShiftTypeStandardTimeError> errors = [];
        if (!ShiftTypeId.TryCreate(request.ShiftTypeId, out ShiftTypeId? shiftTypeId))
        {
            errors.Add(new UpdateShiftTypeStandardTimeError(
                UpdateShiftTypeStandardTimeErrorCode.IdentifierRequired,
                "Der Diensttyp besitzt keine gültige Kennung."));
        }

        ShiftStandardTimeValidationResult timeValidation = ShiftStandardTime.Create(
            request.StandardStart,
            request.StandardEnd);
        errors.AddRange(timeValidation.Errors.Select(CreateValidationError));

        if (errors.Count > 0)
        {
            return UpdateShiftTypeStandardTimeResult.Failure(
                UpdateShiftTypeStandardTimeStatus.ValidationFailed,
                errors);
        }

        ShiftTypeStandardTimeUpdateData? updateData = await _store.FindAsync(
            shiftTypeId!,
            cancellationToken);
        if (updateData is null)
        {
            return UpdateShiftTypeStandardTimeResult.Failure(
                UpdateShiftTypeStandardTimeStatus.NotFound,
                [new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.NotFound,
                    "Der Diensttyp wurde nicht gefunden.")]);
        }

        ShiftType replacement = updateData.Current.WithStandardTime(timeValidation.Value).Value
            ?? throw new InvalidOperationException("A validated standard time was rejected.");

        UpdateShiftTypeStandardTimeError[] splitShiftErrors =
            ValidateSplitShift(updateData, replacement);
        if (splitShiftErrors.Length > 0)
        {
            return UpdateShiftTypeStandardTimeResult.Failure(
                UpdateShiftTypeStandardTimeStatus.ValidationFailed,
                splitShiftErrors);
        }

        CatalogEntryUpdateStoreResult storeResult = await _store.UpdateAsync(
            updateData,
            replacement,
            cancellationToken);

        return storeResult switch
        {
            CatalogEntryUpdateStoreResult.Updated =>
                UpdateShiftTypeStandardTimeResult.Success(CreateSnapshot(replacement)),
            CatalogEntryUpdateStoreResult.Conflict =>
                UpdateShiftTypeStandardTimeResult.Failure(
                UpdateShiftTypeStandardTimeStatus.Conflict,
                [new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.Conflict,
                    "Der Diensttyp wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.")]),
            _ => throw new InvalidOperationException(
                $"Unsupported shift-type update result: {storeResult}"),
        };
    }

    private static UpdateShiftTypeStandardTimeError[] ValidateSplitShift(
        ShiftTypeStandardTimeUpdateData updateData,
        ShiftType replacement)
    {
        ShiftType earlyShift = replacement.Id == updateData.EarlyShift.Id
            ? replacement
            : updateData.EarlyShift;
        ShiftType lateShift = replacement.Id == updateData.LateShift.Id
            ? replacement
            : updateData.LateShift;

        SplitShiftPatternValidationResult validation = SplitShiftPattern.Create(
            updateData.SplitShiftPattern.Id.Value,
            earlyShift,
            lateShift);

        return validation.Errors.Select(CreateSplitShiftValidationError).ToArray();
    }

    private static ShiftTypeSnapshot CreateSnapshot(ShiftType shiftType)
    {
        return new ShiftTypeSnapshot(
            shiftType.Id.Value,
            shiftType.Name.Value,
            shiftType.WorkLocationId.Value,
            shiftType.Display.Abbreviation,
            shiftType.Display.Kind == ShiftTypeDisplayKind.ActualTime,
            shiftType.StandardTime.Start,
            shiftType.StandardTime.End,
            shiftType.StandardTime.DurationMinutes);
    }

    private static UpdateShiftTypeStandardTimeError CreateValidationError(
        ShiftStandardTimeValidationError error)
    {
        return error.Code switch
        {
            ShiftStandardTimeValidationCode.StartMustUseWholeMinute =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.StartMustUseWholeMinute,
                    "Die Startzeit darf keine Sekunden enthalten."),
            ShiftStandardTimeValidationCode.EndMustUseWholeMinute =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.EndMustUseWholeMinute,
                    "Die Endzeit darf keine Sekunden enthalten."),
            ShiftStandardTimeValidationCode.StartMustUseThirtyMinuteIncrement =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.StartMustUseThirtyMinuteIncrement,
                    "Die Startzeit muss auf einer vollen oder halben Stunde liegen."),
            ShiftStandardTimeValidationCode.EndMustUseThirtyMinuteIncrement =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.EndMustUseThirtyMinuteIncrement,
                    "Die Endzeit muss auf einer vollen oder halben Stunde liegen."),
            ShiftStandardTimeValidationCode.EndMustBeAfterStart =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.EndMustBeAfterStart,
                    "Die Endzeit muss nach der Startzeit liegen."),
            _ => throw new InvalidOperationException(
                $"Unsupported standard-time validation code: {error.Code}"),
        };
    }

    private static UpdateShiftTypeStandardTimeError CreateSplitShiftValidationError(
        SplitShiftPatternValidationError error)
    {
        return error.Code switch
        {
            SplitShiftPatternValidationCode.SegmentsMustBeInChronologicalOrder =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode
                        .SplitShiftSegmentsMustBeInChronologicalOrder,
                    "Beim Doppeldienst muss der Frühdienst vor dem Spätdienst liegen."),
            SplitShiftPatternValidationCode.SegmentsMustNotOverlap =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.SplitShiftSegmentsMustNotOverlap,
                    "Frühdienst und Spätdienst dürfen sich im Doppeldienst nicht überschneiden."),
            SplitShiftPatternValidationCode.BreakRequired =>
                new UpdateShiftTypeStandardTimeError(
                    UpdateShiftTypeStandardTimeErrorCode.SplitShiftBreakRequired,
                    "Zwischen Frühdienst und Spätdienst muss eine Unterbrechung liegen."),
            _ => throw new InvalidOperationException(
                $"The stored split-shift definition is invalid: {error.Code}"),
        };
    }
}
