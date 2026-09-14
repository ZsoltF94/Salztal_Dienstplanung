using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

internal static class EmployeeCommandErrors
{
    public static EmployeeCommandError FromValidation(EmployeeValidationError error)
    {
        return error.Code switch
        {
            EmployeeValidationCode.IdentifierRequired => new EmployeeCommandError(
                EmployeeCommandErrorCode.IdentifierRequired,
                "Der Mitarbeiter besitzt keine gültige Kennung."),
            EmployeeValidationCode.FirstNameRequired => new EmployeeCommandError(
                EmployeeCommandErrorCode.FirstNameRequired,
                "Bitte geben Sie einen Vornamen ein."),
            EmployeeValidationCode.LastNameRequired => new EmployeeCommandError(
                EmployeeCommandErrorCode.LastNameRequired,
                "Bitte geben Sie einen Nachnamen ein."),
            EmployeeValidationCode.EmployeeTypeRequired => new EmployeeCommandError(
                EmployeeCommandErrorCode.EmployeeTypeRequired,
                "Bitte wählen Sie einen Mitarbeitertyp aus."),
            _ => throw new InvalidOperationException(
                $"Unsupported employee validation code: {error.Code}"),
        };
    }

    public static EmployeeCommandResult EmployeeNotFound()
    {
        return EmployeeCommandResult.Failure(
            EmployeeCommandStatus.NotFound,
            [new EmployeeCommandError(
                EmployeeCommandErrorCode.NotFound,
                "Der Mitarbeiter wurde nicht gefunden.")]);
    }

    public static EmployeeCommandResult EmployeeTypeNotFound()
    {
        return EmployeeCommandResult.Failure(
            EmployeeCommandStatus.EmployeeTypeNotFound,
            [new EmployeeCommandError(
                EmployeeCommandErrorCode.EmployeeTypeNotFound,
                "Der ausgewählte Mitarbeitertyp wurde nicht gefunden.")]);
    }

    public static EmployeeCommandResult EmployeeTypeCatalogInvalid()
    {
        return EmployeeCommandResult.Failure(
            EmployeeCommandStatus.EmployeeTypeCatalogInvalid,
            [new EmployeeCommandError(
                EmployeeCommandErrorCode.EmployeeTypeCatalogInvalid,
                "Der ausgewählte Mitarbeitertyp enthält ungültige Einsatzfreigaben. Bitte laden Sie die Daten neu.")]);
    }

    public static EmployeeCommandResult EmployeeAlreadyActive()
    {
        return EmployeeCommandResult.Failure(
            EmployeeCommandStatus.AlreadyActive,
            [new EmployeeCommandError(
                EmployeeCommandErrorCode.AlreadyActive,
                "Der Mitarbeiter ist bereits aktiv.")]);
    }

    public static EmployeeCommandResult EmployeeMustBeInactive()
    {
        return EmployeeCommandResult.Failure(
            EmployeeCommandStatus.MustBeInactive,
            [new EmployeeCommandError(
                EmployeeCommandErrorCode.MustBeInactive,
                "Der Mitarbeiter muss vor dem endgültigen Löschen deaktiviert werden.")]);
    }

    public static EmployeeCommandResult FromStoreResult(
        EmployeeWriteStoreResult storeResult,
        EmployeeDetailsSnapshot successValue)
    {
        return storeResult switch
        {
            EmployeeWriteStoreResult.Succeeded => EmployeeCommandResult.Success(successValue),
            EmployeeWriteStoreResult.ActiveType1Conflict => EmployeeCommandResult.Failure(
                EmployeeCommandStatus.ActiveType1Conflict,
                [new EmployeeCommandError(
                    EmployeeCommandErrorCode.ActiveType1Conflict,
                    "Es gibt bereits eine aktive Person vom Typ1. Bitte ändern oder deaktivieren Sie zuerst diese Zuordnung.")]),
            EmployeeWriteStoreResult.Conflict => EmployeeCommandResult.Failure(
                EmployeeCommandStatus.Conflict,
                [new EmployeeCommandError(
                    EmployeeCommandErrorCode.Conflict,
                    "Der Mitarbeiter wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.")]),
            _ => throw new InvalidOperationException(
                $"Unsupported employee store result: {storeResult}"),
        };
    }

    public static EmployeeCommandResult FromDeleteStoreResult(
        EmployeeDeleteStoreResult storeResult,
        EmployeeDetailsSnapshot deletedEmployee)
    {
        return storeResult switch
        {
            EmployeeDeleteStoreResult.Succeeded =>
                EmployeeCommandResult.Success(deletedEmployee),
            EmployeeDeleteStoreResult.Referenced => EmployeeCommandResult.Failure(
                EmployeeCommandStatus.Referenced,
                [new EmployeeCommandError(
                    EmployeeCommandErrorCode.Referenced,
                    "Der Mitarbeiter kann nicht endgültig gelöscht werden, weil er bereits in anderen Fachdaten verwendet wird. Lassen Sie ihn deaktiviert.")]),
            EmployeeDeleteStoreResult.Conflict => EmployeeCommandResult.Failure(
                EmployeeCommandStatus.Conflict,
                [new EmployeeCommandError(
                    EmployeeCommandErrorCode.Conflict,
                    "Der Mitarbeiter wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.")]),
            _ => throw new InvalidOperationException(
                $"Unsupported employee deletion store result: {storeResult}"),
        };
    }
}
