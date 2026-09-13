using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.WorkLocations;

public sealed record WorkLocationId
{
    private WorkLocationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out WorkLocationId? workLocationId)
    {
        if (value == Guid.Empty)
        {
            workLocationId = null;
            return false;
        }

        workLocationId = new WorkLocationId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
