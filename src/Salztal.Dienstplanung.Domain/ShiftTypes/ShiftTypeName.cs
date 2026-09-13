using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed record ShiftTypeName
{
    private ShiftTypeName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    internal static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out ShiftTypeName? shiftTypeName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            shiftTypeName = null;
            return false;
        }

        shiftTypeName = new ShiftTypeName(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
