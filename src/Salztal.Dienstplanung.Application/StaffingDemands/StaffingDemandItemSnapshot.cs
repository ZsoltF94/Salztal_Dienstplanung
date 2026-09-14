namespace Salztal.Dienstplanung.Application.StaffingDemands;

public enum StaffingDemandSourceSnapshotKind
{
    Standard,
    DateException,
}

public sealed record StaffingDemandItemSnapshot(
    Guid SourceId,
    StaffingDemandSourceSnapshotKind SourceKind,
    string SourceDisplay,
    DateOnly Date,
    Guid WorkLocationId,
    string WorkLocationName,
    string WorkLocationColorCode,
    Guid ShiftTypeId,
    string ShiftTypeName,
    string? ShiftTypeAbbreviation,
    bool ShiftTypeUsesActualTimeAsDisplay,
    TimeOnly ActualStart,
    TimeOnly ActualEnd,
    int RequiredEmployeeCount,
    int DurationMinutes,
    long RequiredWorkMinutes);
