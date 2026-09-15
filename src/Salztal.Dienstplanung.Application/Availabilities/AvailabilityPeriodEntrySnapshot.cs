namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed record AvailabilityPeriodEntrySnapshot(
    DateOnly Date,
    AvailabilityDayEntryKind Kind,
    long ChangeVersion);
