using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleDescriptionKey
{
    private RuleDescriptionKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out RuleDescriptionKey? descriptionKey)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            descriptionKey = null;
            return false;
        }

        descriptionKey = new RuleDescriptionKey(value.Trim());
        return true;
    }

    public override string ToString()
    {
        return Value;
    }
}
