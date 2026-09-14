namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public enum StaffingDemandValidationCode
{
    IdentifierRequired,
    WorkLocationRequired,
    ShiftTypeRequired,
    RequiredEmployeeCountMustBePositive,
    ActualStartMustUseWholeMinute,
    ActualEndMustUseWholeMinute,
    ActualStartMustUseThirtyMinuteIncrement,
    ActualEndMustUseThirtyMinuteIncrement,
    ActualEndMustBeAfterStart,
}
