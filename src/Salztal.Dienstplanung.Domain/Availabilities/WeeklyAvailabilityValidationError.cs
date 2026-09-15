namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed record WeeklyAvailabilityValidationError(
    WeeklyAvailabilityValidationCode Code,
    DateOnly? Date = null);
