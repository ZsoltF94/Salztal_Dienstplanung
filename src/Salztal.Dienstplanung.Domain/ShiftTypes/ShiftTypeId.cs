using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed record ShiftTypeId
{
    private ShiftTypeId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out ShiftTypeId? shiftTypeId)
    {
        if (value == Guid.Empty)
        {
            shiftTypeId = null;
            return false;
        }

        shiftTypeId = new ShiftTypeId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
