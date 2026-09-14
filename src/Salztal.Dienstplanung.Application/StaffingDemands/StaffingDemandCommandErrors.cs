using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

internal static class StaffingDemandCommandErrors
{
    public static StaffingDemandCommandError FromStandardValidation(
        StandardStaffingDemandRevisionValidationError error)
    {
        return error.Code switch
        {
            StandardStaffingDemandRevisionValidationCode.IdentifierRequired => new(
                StaffingDemandCommandErrorCode.IdentifierRequired,
                "Die Standardänderung besitzt keine gültige Kennung."),
            StandardStaffingDemandRevisionValidationCode.UnsupportedDayOfWeek => new(
                StaffingDemandCommandErrorCode.UnsupportedDayOfWeek,
                "Der ausgewählte Wochentag ist ungültig."),
            StandardStaffingDemandRevisionValidationCode.WorkLocationRequired => new(
                StaffingDemandCommandErrorCode.WorkLocationRequired,
                "Bitte wählen Sie einen Einsatzort aus."),
            StandardStaffingDemandRevisionValidationCode.ShiftTypeRequired => new(
                StaffingDemandCommandErrorCode.ShiftTypeRequired,
                "Bitte wählen Sie einen Diensttyp aus."),
            StandardStaffingDemandRevisionValidationCode.EffectiveDateMustBeMonday => new(
                StaffingDemandCommandErrorCode.EffectiveDateMustBeMonday,
                "Die Standardänderung muss ab einem Montag gelten."),
            StandardStaffingDemandRevisionValidationCode
                .CorrectionSequenceMustBePositive => new(
                    StaffingDemandCommandErrorCode.UnsupportedOperation,
                    "Die Korrekturfolge des Wochenstandards ist ungültig."),
            StandardStaffingDemandRevisionValidationCode
                .RequiredEmployeeCountMustBePositive => new(
                    StaffingDemandCommandErrorCode.RequiredEmployeeCountMustBePositive,
                    "Die benötigte Personenzahl muss größer als null sein."),
            StandardStaffingDemandRevisionValidationCode.ActualStartMustUseWholeMinute => new(
                StaffingDemandCommandErrorCode.ActualStartMustUseWholeMinute,
                "Die tatsächliche Startzeit darf keine Sekunden enthalten."),
            StandardStaffingDemandRevisionValidationCode.ActualEndMustUseWholeMinute => new(
                StaffingDemandCommandErrorCode.ActualEndMustUseWholeMinute,
                "Die tatsächliche Endzeit darf keine Sekunden enthalten."),
            StandardStaffingDemandRevisionValidationCode
                .ActualStartMustUseThirtyMinuteIncrement => new(
                    StaffingDemandCommandErrorCode.ActualStartMustUseThirtyMinuteIncrement,
                    "Die tatsächliche Startzeit muss auf einer vollen oder halben Stunde liegen."),
            StandardStaffingDemandRevisionValidationCode
                .ActualEndMustUseThirtyMinuteIncrement => new(
                    StaffingDemandCommandErrorCode.ActualEndMustUseThirtyMinuteIncrement,
                    "Die tatsächliche Endzeit muss auf einer vollen oder halben Stunde liegen."),
            StandardStaffingDemandRevisionValidationCode.ActualEndMustBeAfterStart => new(
                StaffingDemandCommandErrorCode.ActualEndMustBeAfterStart,
                "Die tatsächliche Endzeit muss nach der Startzeit liegen."),
            _ => throw new InvalidOperationException(
                $"Unsupported standard staffing-demand validation code: {error.Code}"),
        };
    }

    public static StaffingDemandCommandError FromDateExceptionValidation(
        StaffingDemandDateExceptionValidationError error)
    {
        return error.Code switch
        {
            StaffingDemandDateExceptionValidationCode.IdentifierRequired => new(
                StaffingDemandCommandErrorCode.IdentifierRequired,
                "Die Datumsausnahme besitzt keine gültige Kennung."),
            StaffingDemandDateExceptionValidationCode.WorkLocationRequired => new(
                StaffingDemandCommandErrorCode.WorkLocationRequired,
                "Bitte wählen Sie einen Einsatzort aus."),
            StaffingDemandDateExceptionValidationCode.ShiftTypeRequired => new(
                StaffingDemandCommandErrorCode.ShiftTypeRequired,
                "Bitte wählen Sie einen Diensttyp aus."),
            StaffingDemandDateExceptionValidationCode
                .RequiredEmployeeCountMustBePositive => new(
                    StaffingDemandCommandErrorCode.RequiredEmployeeCountMustBePositive,
                    "Die benötigte Personenzahl muss größer als null sein."),
            StaffingDemandDateExceptionValidationCode.ActualStartMustUseWholeMinute => new(
                StaffingDemandCommandErrorCode.ActualStartMustUseWholeMinute,
                "Die tatsächliche Startzeit darf keine Sekunden enthalten."),
            StaffingDemandDateExceptionValidationCode.ActualEndMustUseWholeMinute => new(
                StaffingDemandCommandErrorCode.ActualEndMustUseWholeMinute,
                "Die tatsächliche Endzeit darf keine Sekunden enthalten."),
            StaffingDemandDateExceptionValidationCode
                .ActualStartMustUseThirtyMinuteIncrement => new(
                    StaffingDemandCommandErrorCode.ActualStartMustUseThirtyMinuteIncrement,
                    "Die tatsächliche Startzeit muss auf einer vollen oder halben Stunde liegen."),
            StaffingDemandDateExceptionValidationCode
                .ActualEndMustUseThirtyMinuteIncrement => new(
                    StaffingDemandCommandErrorCode.ActualEndMustUseThirtyMinuteIncrement,
                    "Die tatsächliche Endzeit muss auf einer vollen oder halben Stunde liegen."),
            StaffingDemandDateExceptionValidationCode.ActualEndMustBeAfterStart => new(
                StaffingDemandCommandErrorCode.ActualEndMustBeAfterStart,
                "Die tatsächliche Endzeit muss nach der Startzeit liegen."),
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand date-exception validation code: {error.Code}"),
        };
    }

    public static StaffingDemandCommandResult FromStoreResult(
        StaffingDemandWriteStoreResult storeResult)
    {
        return storeResult switch
        {
            StaffingDemandWriteStoreResult.Succeeded =>
                StaffingDemandCommandResult.Success(),
            StaffingDemandWriteStoreResult.NotFound => NotFound(
                "Der erwartete Bedarfsstand wurde nicht gefunden. Bitte laden Sie die Daten neu."),
            StaffingDemandWriteStoreResult.Conflict => Conflict(),
            _ => throw new InvalidOperationException(
                $"Unsupported staffing-demand store result: {storeResult}"),
        };
    }

    public static StaffingDemandCommandResult NotFound(string message)
    {
        return StaffingDemandCommandResult.Failure(
            StaffingDemandCommandStatus.NotFound,
            [new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.NotFound,
                message)]);
    }

    public static StaffingDemandCommandResult Conflict()
    {
        return StaffingDemandCommandResult.Failure(
            StaffingDemandCommandStatus.Conflict,
            [new StaffingDemandCommandError(
                StaffingDemandCommandErrorCode.Conflict,
                "Der Bedarf wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.")]);
    }
}
