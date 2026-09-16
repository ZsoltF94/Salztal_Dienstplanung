namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record RuleDefinitionLookupError(
    RuleDefinitionLookupCode Code,
    RuleId RuleId);
