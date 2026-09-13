using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.WorkLocations;

public sealed record WorkLocationColor
{
    private WorkLocationColor(string code)
    {
        Code = code;
    }

    public string Code { get; }

    internal static bool TryCreate(
        string? code,
        [NotNullWhen(true)] out WorkLocationColor? workLocationColor)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            workLocationColor = null;
            return false;
        }

        workLocationColor = new WorkLocationColor(code.Trim());
        return true;
    }

    public override string ToString()
    {
        return Code;
    }
}
