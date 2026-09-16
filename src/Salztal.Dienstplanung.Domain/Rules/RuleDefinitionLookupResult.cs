namespace Salztal.Dienstplanung.Domain.Rules;

public sealed class RuleDefinitionLookupResult
{
    private RuleDefinitionLookupResult(
        RuleDefinition? value,
        RuleDefinitionLookupError? error)
    {
        Value = value;
        Error = error;
    }

    public bool IsSuccess => Value is not null;

    public RuleDefinition? Value { get; }

    public RuleDefinitionLookupError? Error { get; }

    internal static RuleDefinitionLookupResult Success(RuleDefinition value)
    {
        return new RuleDefinitionLookupResult(value, null);
    }

    internal static RuleDefinitionLookupResult Failure(RuleId ruleId)
    {
        return new RuleDefinitionLookupResult(
            null,
            new RuleDefinitionLookupError(
                RuleDefinitionLookupCode.UnknownRuleId,
                ruleId));
    }
}
