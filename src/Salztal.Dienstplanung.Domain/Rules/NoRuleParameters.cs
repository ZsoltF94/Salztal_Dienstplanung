namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record NoRuleParameters : RuleParameters
{
    private NoRuleParameters()
    {
    }

    public static NoRuleParameters Instance { get; } = new();
}
