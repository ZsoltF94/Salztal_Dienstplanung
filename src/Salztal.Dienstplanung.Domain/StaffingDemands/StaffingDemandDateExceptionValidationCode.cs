namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public enum StaffingDemandDateExceptionValidationCode
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
