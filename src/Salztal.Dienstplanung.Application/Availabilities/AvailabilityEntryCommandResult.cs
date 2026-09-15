using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Availabilities;

public enum AvailabilityEntryCommandStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    InactiveEmployee,
    CatalogInvalid,
    StoredDataInvalid,
    ConfirmationRequired,
    Conflict,
}

public enum AvailabilityEntryCommandErrorCode
{
    EmployeeIdentifierRequired,
    UnsupportedKind,
    ExpectedChangeVersionMustBePositive,
    DateMustFitWeek,
    EmployeeNotFound,
    EmployeeInactive,
    CatalogInvalid,
    StoredDataInvalid,
    VacationAndSicknessNotAllowed,
    ConfirmationRequired,
    EntryNotFound,
    Conflict,
}

public sealed record AvailabilityEntryCommandError(
    AvailabilityEntryCommandErrorCode Code,
    string Message);

public sealed class AvailabilityEntryCommandResult
{
    private AvailabilityEntryCommandResult(
        AvailabilityEntryCommandStatus status,
        AvailabilityPeriodEntrySnapshot? value,
        ReadOnlyCollection<AvailabilityEntryCommandError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public AvailabilityEntryCommandStatus Status { get; }

    public AvailabilityPeriodEntrySnapshot? Value { get; }

    public IReadOnlyList<AvailabilityEntryCommandError> Errors { get; }

    internal static AvailabilityEntryCommandResult Success(
        AvailabilityPeriodEntrySnapshot value)
    {
        return new AvailabilityEntryCommandResult(
            AvailabilityEntryCommandStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<AvailabilityEntryCommandError>()));
    }

    internal static AvailabilityEntryCommandResult Failure(
        AvailabilityEntryCommandStatus status,
        IEnumerable<AvailabilityEntryCommandError> errors)
    {
        return new AvailabilityEntryCommandResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
