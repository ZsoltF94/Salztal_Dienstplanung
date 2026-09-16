namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record VacationBoundaryWeekendParameters : RuleParameters
{
    internal VacationBoundaryWeekendParameters(
        VacationBoundaryKind boundary,
        DayOfWeek boundaryDay,
        DayOfWeek weekendFirstDay,
        DayOfWeek weekendSecondDay)
    {
        if (!Enum.IsDefined(boundary))
        {
            throw new ArgumentOutOfRangeException(nameof(boundary));
        }

        if (!Enum.IsDefined(boundaryDay))
        {
            throw new ArgumentOutOfRangeException(nameof(boundaryDay));
        }

        if (!Enum.IsDefined(weekendFirstDay))
        {
            throw new ArgumentOutOfRangeException(nameof(weekendFirstDay));
        }

        if (!Enum.IsDefined(weekendSecondDay))
        {
            throw new ArgumentOutOfRangeException(nameof(weekendSecondDay));
        }

        Boundary = boundary;
        BoundaryDay = boundaryDay;
        WeekendFirstDay = weekendFirstDay;
        WeekendSecondDay = weekendSecondDay;
    }

    public VacationBoundaryKind Boundary { get; }

    public DayOfWeek BoundaryDay { get; }

    public DayOfWeek WeekendFirstDay { get; }

    public DayOfWeek WeekendSecondDay { get; }
}
