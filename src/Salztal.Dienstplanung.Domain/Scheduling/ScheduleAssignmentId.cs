using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ScheduleAssignmentId
{
    private ScheduleAssignmentId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out ScheduleAssignmentId? assignmentId)
    {
        if (value == Guid.Empty)
        {
            assignmentId = null;
            return false;
        }

        assignmentId = new ScheduleAssignmentId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
