namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record AssignmentLock
{
    public AssignmentLock(ScheduleAssignmentId assignmentId)
    {
        ArgumentNullException.ThrowIfNull(assignmentId);

        AssignmentId = assignmentId;
    }

    public ScheduleAssignmentId AssignmentId { get; }
}
