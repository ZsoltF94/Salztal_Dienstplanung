namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record SchedulePeriod
{
    public const int DayCount = 21;

    private static readonly DateOnly LatestStartMonday =
        DateOnly.MaxValue.AddDays(-(DayCount - 1));

    private SchedulePeriod(DateOnly startMonday)
    {
        StartMonday = startMonday;
        EndSunday = startMonday.AddDays(DayCount - 1);
    }

    public DateOnly StartMonday { get; }

    public DateOnly EndSunday { get; }

    public static SchedulePeriodValidationResult Create(DateOnly startMonday)
    {
        List<SchedulePeriodValidationError> errors = [];

        if (startMonday.DayOfWeek != DayOfWeek.Monday)
        {
            errors.Add(new SchedulePeriodValidationError(
                SchedulePeriodValidationCode.StartMustBeMonday));
        }

        if (startMonday > LatestStartMonday)
        {
            errors.Add(new SchedulePeriodValidationError(
                SchedulePeriodValidationCode.PeriodMustFitTwentyOneDays));
        }

        return errors.Count == 0
            ? SchedulePeriodValidationResult.Success(new SchedulePeriod(startMonday))
            : SchedulePeriodValidationResult.Failure(errors);
    }

    public bool Contains(DateOnly date)
    {
        return date >= StartMonday && date <= EndSunday;
    }

    public bool Overlaps(SchedulePeriod other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return StartMonday <= other.EndSunday && EndSunday >= other.StartMonday;
    }
}
