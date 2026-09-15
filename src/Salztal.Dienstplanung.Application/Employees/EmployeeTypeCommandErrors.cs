using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

internal static class EmployeeTypeCommandErrors
{
    public static EmployeeTypeCommandError FromValidation(
        EmployeeTypeValidationError error)
    {
        return error.Code switch
        {
            EmployeeTypeValidationCode.IdentifierRequired => Error(
                EmployeeTypeCommandErrorCode.IdentifierRequired,
                "Der Mitarbeitertyp besitzt keine gültige Kennung."),
            EmployeeTypeValidationCode.CodeRequired => Error(
                EmployeeTypeCommandErrorCode.CodeRequired,
                "Bitte geben Sie einen Typcode ein."),
            EmployeeTypeValidationCode.NameRequired => Error(
                EmployeeTypeCommandErrorCode.NameRequired,
                "Bitte geben Sie einen Namen für den Mitarbeitertyp ein."),
            EmployeeTypeValidationCode.WeeklyWorkTargetMustBePositive => Error(
                EmployeeTypeCommandErrorCode.WeeklyWorkTargetMustBePositive,
                "Das Wochen-Soll muss größer als null Minuten sein."),
            EmployeeTypeValidationCode.WeeklyWorkTargetExceedsWeek => Error(
                EmployeeTypeCommandErrorCode.WeeklyWorkTargetExceedsWeek,
                "Das Wochen-Soll darf eine vollständige Woche nicht überschreiten."),
            EmployeeTypeValidationCode.AbsenceDayValueRequired => Error(
                EmployeeTypeCommandErrorCode.AbsenceDayValueRequired,
                "Bitte geben Sie einen Tageswert für Urlaub und Krankheit ein."),
            EmployeeTypeValidationCode.AbsenceDayValueMustNotBeSet => Error(
                EmployeeTypeCommandErrorCode.AbsenceDayValueMustNotBeSet,
                "Für diesen Mitarbeitertyp darf kein Tageswert für Urlaub und Krankheit gesetzt sein."),
            EmployeeTypeValidationCode.AbsenceDayValueMustBePositive => Error(
                EmployeeTypeCommandErrorCode.AbsenceDayValueMustBePositive,
                "Der Tageswert für Urlaub und Krankheit muss größer als null Minuten sein."),
            EmployeeTypeValidationCode.AbsenceDayValueExceedsDay => Error(
                EmployeeTypeCommandErrorCode.AbsenceDayValueExceedsDay,
                "Der Tageswert für Urlaub und Krankheit darf 24 Stunden nicht überschreiten."),
            EmployeeTypeValidationCode.ShiftEligibilityRequired => Error(
                EmployeeTypeCommandErrorCode.ShiftEligibilityRequired,
                "Die Einsatzfreigaben konnten nicht vollständig gelesen werden."),
            EmployeeTypeValidationCode.DuplicateShiftEligibility => Error(
                EmployeeTypeCommandErrorCode.DuplicateShiftEligibility,
                "Eine Einsatzfreigabe wurde mehrfach ausgewählt."),
            EmployeeTypeValidationCode.PlanningPolicyRequired => throw new InvalidOperationException(
                "The application always supplies an employee-type planning policy."),
            _ => throw new InvalidOperationException(
                $"Unsupported employee-type validation code: {error.Code}"),
        };
    }

    public static EmployeeTypeCommandResult NotFound()
    {
        return Failure(
            EmployeeTypeCommandStatus.NotFound,
            EmployeeTypeCommandErrorCode.NotFound,
            "Der Mitarbeitertyp wurde nicht gefunden.");
    }

    public static EmployeeTypeCommandResult DuplicateCode()
    {
        return Failure(
            EmployeeTypeCommandStatus.DuplicateCode,
            EmployeeTypeCommandErrorCode.DuplicateCode,
            "Der Typcode wird bereits verwendet. Bitte geben Sie einen eindeutigen Typcode ein.");
    }

    public static EmployeeTypeCommandResult CatalogInvalid()
    {
        return Failure(
            EmployeeTypeCommandStatus.CatalogInvalid,
            EmployeeTypeCommandErrorCode.CatalogInvalid,
            "Der Mitarbeitertyp enthält unbekannte Einsatzfreigaben. Bitte laden Sie die Daten neu.");
    }

    public static EmployeeTypeCommandResult ConfirmationRequired()
    {
        return Failure(
            EmployeeTypeCommandStatus.ConfirmationRequired,
            EmployeeTypeCommandErrorCode.ConfirmationRequired,
            "Bitte bestätigen Sie das endgültige Löschen des Mitarbeitertyps.");
    }

    public static EmployeeTypeCommandResult Protected()
    {
        return Failure(
            EmployeeTypeCommandStatus.Protected,
            EmployeeTypeCommandErrorCode.Protected,
            "Dieser Mitarbeitertyp besitzt eine geschützte Typ1- oder AH-Rolle und kann nicht gelöscht werden.");
    }

    public static EmployeeTypeCommandResult AssignedEmployee()
    {
        return Failure(
            EmployeeTypeCommandStatus.Referenced,
            EmployeeTypeCommandErrorCode.AssignedEmployee,
            "Der Mitarbeitertyp kann nicht gelöscht werden, weil er mindestens einer aktiven oder deaktivierten Person zugeordnet ist.");
    }

    public static EmployeeTypeCommandResult FromWriteStoreResult(
        EmployeeTypeWriteStoreResult storeResult,
        EmployeeTypeSnapshot successValue)
    {
        return storeResult switch
        {
            EmployeeTypeWriteStoreResult.Succeeded =>
                EmployeeTypeCommandResult.Success(successValue),
            EmployeeTypeWriteStoreResult.DuplicateCode => DuplicateCode(),
            EmployeeTypeWriteStoreResult.Conflict => Failure(
                EmployeeTypeCommandStatus.Conflict,
                EmployeeTypeCommandErrorCode.Conflict,
                "Der Mitarbeitertyp wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu."),
            _ => throw new InvalidOperationException(
                $"Unsupported employee-type store result: {storeResult}"),
        };
    }

    public static EmployeeTypeCommandResult FromDeleteStoreResult(
        EmployeeTypeDeleteStoreResult storeResult,
        EmployeeTypeSnapshot successValue)
    {
        return storeResult switch
        {
            EmployeeTypeDeleteStoreResult.Succeeded =>
                EmployeeTypeCommandResult.Success(successValue),
            EmployeeTypeDeleteStoreResult.Referenced => Failure(
                EmployeeTypeCommandStatus.Referenced,
                EmployeeTypeCommandErrorCode.Referenced,
                "Der Mitarbeitertyp kann nicht gelöscht werden, weil er bereits in anderen Fachdaten verwendet wird."),
            EmployeeTypeDeleteStoreResult.Conflict => Failure(
                EmployeeTypeCommandStatus.Conflict,
                EmployeeTypeCommandErrorCode.Conflict,
                "Der Mitarbeitertyp wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu."),
            _ => throw new InvalidOperationException(
                $"Unsupported employee-type deletion store result: {storeResult}"),
        };
    }

    private static EmployeeTypeCommandResult Failure(
        EmployeeTypeCommandStatus status,
        EmployeeTypeCommandErrorCode code,
        string message)
    {
        return EmployeeTypeCommandResult.Failure(status, [Error(code, message)]);
    }

    private static EmployeeTypeCommandError Error(
        EmployeeTypeCommandErrorCode code,
        string message)
    {
        return new EmployeeTypeCommandError(code, message);
    }
}
