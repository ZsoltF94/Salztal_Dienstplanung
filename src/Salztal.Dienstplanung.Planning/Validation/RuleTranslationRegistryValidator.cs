using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Rules;

namespace Salztal.Dienstplanung.Planning.Validation;

internal static class RuleTranslationRegistryValidator
{
    public static IEnumerable<PlanningInputValidationIssue> Validate(
        InitialRuleTranslationRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        RuleCatalog catalog = InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value
            ?? throw new InvalidOperationException("The initial rule catalog is unavailable.");
        Dictionary<string, RuleDefinition> known = catalog.Definitions.ToDictionary(
            definition => definition.Id.Value,
            StringComparer.Ordinal);

        foreach (IGrouping<string, RuleTranslationDescriptor> group in
                 registry.Descriptors.GroupBy(
                     descriptor => descriptor.RuleId,
                     StringComparer.Ordinal))
        {
            if (group.Count() > 1)
            {
                yield return new PlanningInputValidationIssue(
                    PlanningInputValidationCode.DuplicateRule,
                    group.Key);
            }

            if (!known.TryGetValue(group.Key, out RuleDefinition? definition))
            {
                yield return new PlanningInputValidationIssue(
                    PlanningInputValidationCode.UnknownRule,
                    group.Key);
                continue;
            }

            RuleTranslationKind expectedKind = RuleTranslationDescriptor.For(
                definition).Kind;
            if (group.Any(descriptor => descriptor.Kind != expectedKind))
            {
                yield return new PlanningInputValidationIssue(
                    PlanningInputValidationCode.RuleDefinitionMismatch,
                    group.Key,
                    Parameter: expectedKind.ToString());
            }
        }

        HashSet<string> translated = registry.Descriptors
            .Select(descriptor => descriptor.RuleId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (string missingRuleId in known.Keys
                     .Where(ruleId => !translated.Contains(ruleId))
                     .Order(StringComparer.Ordinal))
        {
            yield return new PlanningInputValidationIssue(
                PlanningInputValidationCode.RuleNotTranslated,
                missingRuleId);
        }
    }
}
