namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyConsecutiveDaysOffParameters : RuleParameters
{
    internal WeeklyConsecutiveDaysOffParameters(
        int minimumConsecutiveDays,
        RuleDayMarkerKinds qualifyingRegularDayOffMarkers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumConsecutiveDays);

        if (!RuleDayMarkerKindsValidation.IsNonEmptyDefinedCombination(
                qualifyingRegularDayOffMarkers))
        {
            throw new ArgumentOutOfRangeException(nameof(qualifyingRegularDayOffMarkers));
        }

        MinimumConsecutiveDays = minimumConsecutiveDays;
        QualifyingRegularDayOffMarkers = qualifyingRegularDayOffMarkers;
    }

    public int MinimumConsecutiveDays { get; }

    public RuleDayMarkerKinds QualifyingRegularDayOffMarkers { get; }
}
