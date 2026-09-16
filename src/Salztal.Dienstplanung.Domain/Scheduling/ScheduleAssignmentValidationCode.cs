namespace Salztal.Dienstplanung.Domain.Scheduling;

public enum ScheduleAssignmentValidationCode
{
    IdentifierRequired,
    EmployeeRequired,
    DemandSlotRequired,
    PatternRequired,
    OriginInvalid,
    SlotsMustShareDate,
    SlotsMustBeDistinct,
    SlotDoesNotMatchPattern,
    SplitShiftBreakRequired,
    ReliefShiftWrongDay,
    ReliefSwitchMustBeInsideSecondDemand,
    OfficeTimeRequiresRestaurantEarlyOrLate,
}
