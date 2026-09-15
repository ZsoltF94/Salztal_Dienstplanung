namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed record AvailabilityPeriodDaySnapshot(
    DateOnly Date,
    DayOfWeek DayOfWeek);
