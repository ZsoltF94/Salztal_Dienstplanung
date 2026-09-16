namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record SplitShiftWeeklyMaximumParameters : RuleParameters
{
    internal SplitShiftWeeklyMaximumParameters(int maximumAssignments)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumAssignments);
        MaximumAssignments = maximumAssignments;
    }

    public int MaximumAssignments { get; }
}
