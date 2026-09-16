namespace Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;

internal sealed class ScheduleDraftEntity
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public DateOnly StartMonday { get; set; }

    public DateOnly EndSunday { get; set; }

    public Guid? PreparedSnapshotId { get; set; }
}

internal sealed class SchedulePeriodDayEntity
{
    public DateOnly Date { get; set; }

    public Guid DraftId { get; set; }
}

internal sealed class ScheduleDemandSlotEntity
{
    public Guid DraftId { get; set; }

    public required string SlotKey { get; set; }

    public Guid SourceId { get; set; }

    public int SourceKind { get; set; }

    public DateOnly Date { get; set; }

    public Guid WorkLocationId { get; set; }

    public Guid ShiftTypeId { get; set; }

    public int Ordinal { get; set; }

    public TimeOnly ActualStart { get; set; }

    public TimeOnly ActualEnd { get; set; }
}

internal sealed class ScheduleAvailabilityEntryEntity
{
    public Guid DraftId { get; set; }

    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public int Kind { get; set; }
}

internal sealed class ScheduleAssignmentEntity
{
    public Guid Id { get; set; }

    public Guid DraftId { get; set; }

    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public int Kind { get; set; }

    public int Origin { get; set; }

    public Guid? PatternId { get; set; }

    public int WorkMinutes { get; set; }

    public bool IsProtectedFromAutomaticGeneration { get; set; }
}

internal sealed class ScheduleAssignmentSegmentEntity
{
    public Guid AssignmentId { get; set; }

    public int Sequence { get; set; }

    public Guid DraftId { get; set; }

    public required string AnchorSlotKey { get; set; }

    public DateOnly Date { get; set; }

    public Guid WorkLocationId { get; set; }

    public Guid ShiftTypeId { get; set; }

    public TimeOnly ActualStart { get; set; }

    public TimeOnly ActualEnd { get; set; }

    public int WorkMinutes { get; set; }
}

internal sealed class ScheduleDemandCoverageEntity
{
    public Guid AssignmentId { get; set; }

    public int Sequence { get; set; }

    public Guid DraftId { get; set; }

    public required string SlotKey { get; set; }

    public TimeOnly CoveredStart { get; set; }

    public TimeOnly CoveredEnd { get; set; }

    public int CoveredMinutes { get; set; }

    public int Kind { get; set; }
}

internal sealed class ScheduleGeneratedDayOffEntity
{
    public Guid DraftId { get; set; }

    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }
}

internal sealed class ScheduleAssignmentLockEntity
{
    public Guid DraftId { get; set; }

    public Guid AssignmentId { get; set; }
}
