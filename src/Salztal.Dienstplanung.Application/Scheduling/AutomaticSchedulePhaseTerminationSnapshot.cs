namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticSchedulePhaseDetailAvailability
{
    Complete,
    TerminationDetailsNotRecorded,
}

public enum AutomaticSchedulePhaseTerminationReason
{
    TimeLimitWithFeasibleSelection,
    TimeLimitWithoutFeasibleSelection,
    CancellationRequested,
    TechnicalFailure,
}

public enum AutomaticScheduleOptimizationTargetKind
{
    HardRuleFeasibility,
    RegularCoveredMinutes,
    RegularTouchedDemandSlots,
    ReliefCoveredMinutes,
    ReliefTouchedDemandSlots,
    HighPriorityRules,
    ReliefShiftAssignments,
    SplitShiftAssignments,
    AuxiliaryMinimumFulfillment,
    RelativeWeeklyTargetDeviation,
    MediumPriorityRules,
    LowPriorityRules,
    StabilityRules,
    TechnicalTieBreak,
}

public sealed class AutomaticSchedulePhaseTerminationSnapshot
{
    public AutomaticSchedulePhaseTerminationSnapshot(
        AutomaticSchedulePhaseTerminationReason reason,
        AutomaticScheduleOptimizationTargetKind? activeTarget = null,
        TimeSpan? timeLimit = null,
        TimeSpan? budgetElapsed = null)
    {
        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason));
        }

        if (activeTarget is not null && !Enum.IsDefined(activeTarget.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(activeTarget));
        }

        if (timeLimit is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
                timeLimit.Value,
                TimeSpan.Zero);
        }

        if (budgetElapsed is not null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(
                budgetElapsed.Value,
                TimeSpan.Zero);
        }

        if (timeLimit.HasValue != budgetElapsed.HasValue)
        {
            throw new ArgumentException(
                "Time limit and elapsed budget must either both be present or both be absent.",
                nameof(timeLimit));
        }

        if (reason is AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection
                or AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection
            && (activeTarget is null || timeLimit is null))
        {
            throw new ArgumentException(
                "A time-limit termination requires an active target and budget values.",
                nameof(reason));
        }

        Reason = reason;
        ActiveTarget = activeTarget;
        TimeLimit = timeLimit;
        BudgetElapsed = budgetElapsed;
    }

    public AutomaticSchedulePhaseTerminationReason Reason { get; }

    public AutomaticScheduleOptimizationTargetKind? ActiveTarget { get; }

    public TimeSpan? TimeLimit { get; }

    public TimeSpan? BudgetElapsed { get; }
}
