using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record DemandSlotRestoreValue(
    Guid SourceId,
    StaffingDemandSourceKind SourceKind,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    int Ordinal,
    TimeOnly ActualStart,
    TimeOnly ActualEnd);

public enum DemandSlotRestoreCode
{
    IdentifierInvalid,
    SourceKindInvalid,
    DateOutsidePeriod,
    WorkLocationUnknown,
    ShiftTypeUnknown,
    OrdinalInvalid,
    ActualTimeInvalid,
    DuplicateIdentity,
}

public sealed record DemandSlotRestoreError(
    DemandSlotRestoreCode Code,
    Guid SourceId,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    int Ordinal);

public sealed record DemandSlotSetRestoreResult(
    DemandSlotSet? Value,
    IReadOnlyList<DemandSlotRestoreError> Errors)
{
    public bool IsSuccess => Value is not null;
}
