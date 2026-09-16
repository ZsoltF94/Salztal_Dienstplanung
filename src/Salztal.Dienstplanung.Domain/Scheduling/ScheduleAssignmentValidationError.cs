namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ScheduleAssignmentValidationError(
    ScheduleAssignmentValidationCode Code,
    DemandSlotId? SlotId = null);
