namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed record UpdateShiftTypeStandardTimeRequest(
    Guid ShiftTypeId,
    TimeOnly StandardStart,
    TimeOnly StandardEnd);
