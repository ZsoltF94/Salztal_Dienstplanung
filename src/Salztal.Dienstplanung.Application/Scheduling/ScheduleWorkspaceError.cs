namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ScheduleWorkspaceErrorCode
{
    PeriodDoesNotFit,
    OverlappingPeriod,
    DraftNotFound,
    StoredDataInvalid,
    CatalogInvalid,
    Conflict,
}

public sealed record ScheduleWorkspaceError(
    ScheduleWorkspaceErrorCode Code,
    string Message,
    DateOnly? ConflictingPeriodMonday = null,
    DateOnly? ConflictingPeriodSunday = null);
