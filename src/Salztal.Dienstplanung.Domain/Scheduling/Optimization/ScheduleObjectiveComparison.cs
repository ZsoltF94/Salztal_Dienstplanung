namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public enum ScheduleObjectiveComparisonOutcome
{
    Better,
    Equivalent,
    Worse,
}

public sealed record ScheduleObjectiveComparison(
    ScheduleObjectiveComparisonOutcome Outcome,
    ScheduleObjectiveStage? DecisiveStage)
{
    public static ScheduleObjectiveComparison Equivalent { get; } = new(
        ScheduleObjectiveComparisonOutcome.Equivalent,
        null);
}
