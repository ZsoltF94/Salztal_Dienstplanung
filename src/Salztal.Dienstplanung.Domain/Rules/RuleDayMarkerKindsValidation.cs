namespace Salztal.Dienstplanung.Domain.Rules;

internal static class RuleDayMarkerKindsValidation
{
    private const RuleDayMarkerKinds AllDefinedMarkers =
        RuleDayMarkerKinds.Vacation
        | RuleDayMarkerKinds.Sickness
        | RuleDayMarkerKinds.GuaranteedDayOff
        | RuleDayMarkerKinds.GeneratedDayOff;

    internal static bool IsNonEmptyDefinedCombination(RuleDayMarkerKinds markers)
    {
        return markers != RuleDayMarkerKinds.None
            && (markers & ~AllDefinedMarkers) == RuleDayMarkerKinds.None;
    }
}
