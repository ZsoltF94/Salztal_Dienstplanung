namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed record ShiftTypeSnapshot(
    Guid Id,
    string Name,
    Guid WorkLocationId,
    string? Abbreviation,
    bool UsesActualTimeAsDisplay,
    TimeOnly StandardStart,
    TimeOnly StandardEnd,
    int StandardDurationMinutes);
