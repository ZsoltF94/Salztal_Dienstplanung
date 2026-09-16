using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleCatalogVersion
{
    private RuleCatalogVersion(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static bool TryCreate(
        int value,
        [NotNullWhen(true)] out RuleCatalogVersion? version)
    {
        if (value <= 0)
        {
            version = null;
            return false;
        }

        version = new RuleCatalogVersion(value);
        return true;
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
