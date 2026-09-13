namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed record SplitShiftPatternSnapshot(
    Guid Id,
    string DisplayCode,
    Guid FirstShiftTypeId,
    Guid SecondShiftTypeId,
    Guid WorkLocationId,
    int StandardBreakMinutes,
    int StandardWorkMinutes);
