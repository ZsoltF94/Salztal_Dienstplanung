namespace Salztal.Dienstplanung.Domain.StaffingDemands;

internal enum StaffingDemandTimeValidationCode
{
    StartMustUseWholeMinute,
    EndMustUseWholeMinute,
    StartMustUseThirtyMinuteIncrement,
    EndMustUseThirtyMinuteIncrement,
    EndMustBeAfterStart,
}
