namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record NoRuleResultParameters : RuleResultParameters
{
    private NoRuleResultParameters()
    {
    }

    public static NoRuleResultParameters Instance { get; } = new();
}
