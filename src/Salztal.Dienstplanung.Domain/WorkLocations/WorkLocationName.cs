using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.WorkLocations;

public sealed record WorkLocationName
{
    private WorkLocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    internal static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out WorkLocationName? workLocationName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            workLocationName = null;
            return false;
        }

        workLocationName = new WorkLocationName(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
