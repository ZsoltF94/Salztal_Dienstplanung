namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record MaximumConsecutiveWorkdaysParameters : RuleParameters
{
    internal MaximumConsecutiveWorkdaysParameters(
        int maximumDays,
        bool requiresPrecedingHistory,
        RuleDayMarkerKinds workSequenceBreakingMarkers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumDays);

        if (!RuleDayMarkerKindsValidation.IsNonEmptyDefinedCombination(
                workSequenceBreakingMarkers))
        {
            throw new ArgumentOutOfRangeException(nameof(workSequenceBreakingMarkers));
        }

        MaximumDays = maximumDays;
        RequiresPrecedingHistory = requiresPrecedingHistory;
        WorkSequenceBreakingMarkers = workSequenceBreakingMarkers;
    }

    public int MaximumDays { get; }

    public bool RequiresPrecedingHistory { get; }

    public RuleDayMarkerKinds WorkSequenceBreakingMarkers { get; }
}
