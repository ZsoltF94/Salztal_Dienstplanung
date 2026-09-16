namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record GuaranteedDayOffAdjacencyParameters : RuleParameters
{
    internal GuaranteedDayOffAdjacencyParameters(
        int minimumBlockLength,
        RuleDayMarkerKinds anchorMarker,
        RuleDayMarkerKinds adjacentQualifyingMarkers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumBlockLength);

        if (!Enum.IsDefined(anchorMarker)
            || anchorMarker != RuleDayMarkerKinds.GuaranteedDayOff)
        {
            throw new ArgumentOutOfRangeException(nameof(anchorMarker));
        }

        if (!RuleDayMarkerKindsValidation.IsNonEmptyDefinedCombination(
                adjacentQualifyingMarkers))
        {
            throw new ArgumentOutOfRangeException(nameof(adjacentQualifyingMarkers));
        }

        MinimumBlockLength = minimumBlockLength;
        AnchorMarker = anchorMarker;
        AdjacentQualifyingMarkers = adjacentQualifyingMarkers;
    }

    public int MinimumBlockLength { get; }

    public RuleDayMarkerKinds AnchorMarker { get; }

    public RuleDayMarkerKinds AdjacentQualifyingMarkers { get; }
}
