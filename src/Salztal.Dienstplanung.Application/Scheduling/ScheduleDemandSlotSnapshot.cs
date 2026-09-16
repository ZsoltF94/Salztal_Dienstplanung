namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ScheduleDemandSourceKindSnapshot
{
    Standard,
    DateException,
}

public sealed record ScheduleDemandSlotSnapshot(
    Guid SourceId,
    ScheduleDemandSourceKindSnapshot SourceKind,
    DateOnly Date,
    Guid WorkLocationId,
    string WorkLocationName,
    Guid ShiftTypeId,
    string ShiftTypeName,
    int Ordinal,
    TimeOnly ActualStart,
    TimeOnly ActualEnd,
    int DurationMinutes);
