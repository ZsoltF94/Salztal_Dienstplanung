namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed record ReliefShiftPatternSnapshot(
    Guid Id,
    string DisplayCode,
    string DisplayColorCode,
    DayOfWeek AllowedDay,
    Guid FirstShiftTypeId,
    Guid FirstWorkLocationId,
    Guid SecondShiftTypeId,
    Guid SecondWorkLocationId,
    bool SwitchesAtEndOfFirstActualDemand,
    bool HasInterruption);
