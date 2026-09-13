namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public enum ShiftStandardTimeValidationCode
{
    StartMustUseWholeMinute,
    EndMustUseWholeMinute,
    StartMustUseThirtyMinuteIncrement,
    EndMustUseThirtyMinuteIncrement,
    EndMustBeAfterStart,
}
