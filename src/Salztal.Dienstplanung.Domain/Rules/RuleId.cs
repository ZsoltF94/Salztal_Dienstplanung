using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleId
{
    private RuleId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out RuleId? ruleId)
    {
        string normalizedValue = value?.Trim() ?? string.Empty;

        if (!IsValid(normalizedValue))
        {
            ruleId = null;
            return false;
        }

        ruleId = new RuleId(normalizedValue);
        return true;
    }

    public override string ToString()
    {
        return Value;
    }

    private static bool IsValid(string value)
    {
        if (value.Length == 0 || !IsUppercaseAsciiLetter(value[0]))
        {
            return false;
        }

        return value.All(character =>
            IsUppercaseAsciiLetter(character)
            || char.IsAsciiDigit(character)
            || character == '_');
    }

    private static bool IsUppercaseAsciiLetter(char value)
    {
        return value is >= 'A' and <= 'Z';
    }
}
