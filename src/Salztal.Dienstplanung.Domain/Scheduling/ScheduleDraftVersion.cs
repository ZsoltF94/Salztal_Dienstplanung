using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ScheduleDraftVersion
{
    private ScheduleDraftVersion(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static bool TryCreate(
        int value,
        [NotNullWhen(true)] out ScheduleDraftVersion? version)
    {
        if (value <= 0)
        {
            version = null;
            return false;
        }

        version = new ScheduleDraftVersion(value);
        return true;
    }
}
