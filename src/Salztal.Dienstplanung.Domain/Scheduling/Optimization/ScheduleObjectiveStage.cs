namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public enum ScheduleObjectiveStage
{
    UncoveredEmployeeMinutes,
    FullyUncoveredDemandSlots,
    HighPriorityRules,
    ReliefShiftAssignments,
    SplitShiftAssignments,
    AuxiliaryWeeklyMinimum,
    RelativeWeeklyTarget,
    MediumPriorityRules,
    LowPriorityRules,
    Stability,
    TechnicalTieBreaker,
}
