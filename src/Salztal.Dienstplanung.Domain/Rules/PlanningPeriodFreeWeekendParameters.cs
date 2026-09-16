namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record PlanningPeriodFreeWeekendParameters : RuleParameters
{
    internal PlanningPeriodFreeWeekendParameters(
        int periodWeeks,
        int minimumFreeWeekends,
        DayOfWeek weekendFirstDay,
        DayOfWeek weekendSecondDay,
        RuleDayMarkerKinds qualifyingRegularDayOffMarkers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(periodWeeks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumFreeWeekends);

        if (!Enum.IsDefined(weekendFirstDay))
        {
            throw new ArgumentOutOfRangeException(nameof(weekendFirstDay));
        }

        if (!Enum.IsDefined(weekendSecondDay))
        {
            throw new ArgumentOutOfRangeException(nameof(weekendSecondDay));
        }

        if (!RuleDayMarkerKindsValidation.IsNonEmptyDefinedCombination(
                qualifyingRegularDayOffMarkers))
        {
            throw new ArgumentOutOfRangeException(nameof(qualifyingRegularDayOffMarkers));
        }

        PeriodWeeks = periodWeeks;
        MinimumFreeWeekends = minimumFreeWeekends;
        WeekendFirstDay = weekendFirstDay;
        WeekendSecondDay = weekendSecondDay;
        QualifyingRegularDayOffMarkers = qualifyingRegularDayOffMarkers;
    }

    public int PeriodWeeks { get; }

    public int MinimumFreeWeekends { get; }

    public DayOfWeek WeekendFirstDay { get; }

    public DayOfWeek WeekendSecondDay { get; }

    public RuleDayMarkerKinds QualifyingRegularDayOffMarkers { get; }
}
