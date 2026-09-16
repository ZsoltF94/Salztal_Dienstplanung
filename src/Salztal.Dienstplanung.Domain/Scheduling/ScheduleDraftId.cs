using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ScheduleDraftId
{
    private ScheduleDraftId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out ScheduleDraftId? draftId)
    {
        if (value == Guid.Empty)
        {
            draftId = null;
            return false;
        }

        draftId = new ScheduleDraftId(value);
        return true;
    }
}
