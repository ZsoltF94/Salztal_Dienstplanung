namespace Salztal.Dienstplanung.Domain.Rules;

internal static class InitialRuleDefinitionFactory
{
    internal static RuleDefinition Create(
        string id,
        RuleFamily family,
        RuleScope scope,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority,
        RuleParameters parameters,
        string descriptionKey)
    {
        RuleDefinitionValidationResult result = RuleDefinition.Create(
            id,
            family,
            scope,
            automaticEffect,
            manualEffect,
            priority,
            parameters,
            descriptionKey);

        return result.Value
            ?? throw new InvalidOperationException($"Initial rule {id} is invalid.");
    }
}
