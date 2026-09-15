namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed record AvailabilityPeriodWeekSnapshot(
    DateOnly WeekMonday,
    DateOnly WeekSunday,
    int UncutWorkTargetMinutes,
    int EffectiveWorkTargetMinutes,
    bool IsFullyUnavailable);
