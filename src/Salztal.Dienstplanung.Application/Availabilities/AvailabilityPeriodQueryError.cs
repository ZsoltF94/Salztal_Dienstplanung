namespace Salztal.Dienstplanung.Application.Availabilities;

public enum AvailabilityPeriodQueryErrorCode
{
    PeriodMustFitTwentyOneDays,
    DuplicateEmployeeIdentifier,
    DuplicateEmployeeTypeIdentifier,
    EmployeeTypeMissing,
    AvailabilityEmployeeMissing,
    DuplicateEmployeeAndDate,
    InvalidChangeVersion,
    VacationAndSicknessNotAllowed,
}

public sealed record AvailabilityPeriodQueryError(
    AvailabilityPeriodQueryErrorCode Code,
    string Message,
    Guid? EmployeeId = null,
    DateOnly? Date = null);
