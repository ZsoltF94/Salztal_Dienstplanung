using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public sealed record ShiftPatternId
{
    private ShiftPatternId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static bool TryCreate(
        Guid value,
        [NotNullWhen(true)] out ShiftPatternId? shiftPatternId)
    {
        if (value == Guid.Empty)
        {
            shiftPatternId = null;
            return false;
        }

        shiftPatternId = new ShiftPatternId(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
