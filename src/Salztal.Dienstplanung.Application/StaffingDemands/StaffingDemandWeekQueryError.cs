namespace Salztal.Dienstplanung.Application.StaffingDemands;

public enum StaffingDemandWeekQueryErrorCode
{
    WeekStartMustBeMonday,
    WeekMustFitSevenDays,
    DuplicateWorkLocation,
    DuplicateShiftType,
    ShiftTypeWorkLocationNotFound,
    WorkLocationNotFound,
    ShiftTypeNotFound,
    ShiftTypeWorkLocationMismatch,
    DuplicateStandardRevision,
    InvalidStandardRevisionSequence,
    DuplicateDateException,
    InvalidDateExceptionApplication,
    RequiredWorkMinutesOverflow,
}

public sealed record StaffingDemandWeekQueryError(
    StaffingDemandWeekQueryErrorCode Code,
    string Message);
