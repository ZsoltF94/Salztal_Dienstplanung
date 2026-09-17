namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record AutomaticSchedulePlanningRequest
{
    public static TimeSpan ProductiveTimeLimit { get; } = TimeSpan.FromSeconds(120);

    public AutomaticSchedulePlanningRequest(
        PlanningInputSnapshot snapshot,
        TimeSpan timeLimit)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            timeLimit,
            TimeSpan.Zero);

        Snapshot = snapshot;
        TimeLimit = timeLimit;
    }

    public PlanningInputSnapshot Snapshot { get; }

    public TimeSpan TimeLimit { get; }
}
