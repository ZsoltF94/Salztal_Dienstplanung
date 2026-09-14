namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public enum StandardStaffingDemandRevisionValidationCode
{
    IdentifierRequired,
    UnsupportedDayOfWeek,
    WorkLocationRequired,
    ShiftTypeRequired,
    EffectiveDateMustBeMonday,
    CorrectionSequenceMustBePositive,
    RequiredEmployeeCountMustBePositive,
    ActualStartMustUseWholeMinute,
    ActualEndMustUseWholeMinute,
    ActualStartMustUseThirtyMinuteIncrement,
    ActualEndMustUseThirtyMinuteIncrement,
    ActualEndMustBeAfterStart,
}
