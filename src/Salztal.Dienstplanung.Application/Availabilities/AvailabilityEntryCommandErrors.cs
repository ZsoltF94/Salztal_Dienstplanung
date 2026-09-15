namespace Salztal.Dienstplanung.Application.Availabilities;

internal static class AvailabilityEntryCommandErrors
{
    public static AvailabilityEntryCommandResult Failure(
        AvailabilityEntryCommandStatus status,
        AvailabilityEntryCommandErrorCode code,
        string message)
    {
        return AvailabilityEntryCommandResult.Failure(
            status,
            [new AvailabilityEntryCommandError(code, message)]);
    }

    public static AvailabilityEntryCommandResult CatalogInvalid(
        string? message = null)
    {
        return Failure(
            AvailabilityEntryCommandStatus.CatalogInvalid,
            AvailabilityEntryCommandErrorCode.CatalogInvalid,
            message ?? "Die Mitarbeiter- und Typdaten sind widersprüchlich. Bitte laden Sie die Daten neu.");
    }

    public static AvailabilityEntryCommandResult StoredDataInvalid(
        string? message = null)
    {
        return Failure(
            AvailabilityEntryCommandStatus.StoredDataInvalid,
            AvailabilityEntryCommandErrorCode.StoredDataInvalid,
            message ?? "Die gespeicherten Tageseinträge sind widersprüchlich. Bitte laden Sie die Daten neu.");
    }

    public static AvailabilityEntryCommandResult Conflict()
    {
        return Failure(
            AvailabilityEntryCommandStatus.Conflict,
            AvailabilityEntryCommandErrorCode.Conflict,
            "Das Tagesfeld wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.");
    }

    public static AvailabilityEntryCommandResult EmployeeNotFound()
    {
        return Failure(
            AvailabilityEntryCommandStatus.NotFound,
            AvailabilityEntryCommandErrorCode.EmployeeNotFound,
            "Die ausgewählte Person wurde nicht gefunden.");
    }

    public static AvailabilityEntryCommandResult EmployeeInactive()
    {
        return Failure(
            AvailabilityEntryCommandStatus.InactiveEmployee,
            AvailabilityEntryCommandErrorCode.EmployeeInactive,
            "Für eine deaktivierte Person kann kein Tageseintrag geändert werden.");
    }
}
