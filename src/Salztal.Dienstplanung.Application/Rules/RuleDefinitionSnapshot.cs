using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Application.Rules;

public sealed class RuleDefinitionSnapshot
{
    public RuleDefinitionSnapshot(
        string id,
        RuleFamily family,
        RuleScope scope,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority,
        RuleParameters parameters,
        string descriptionKey)
    {
        Id = id;
        Family = family;
        Scope = scope;
        AutomaticEffect = automaticEffect;
        ManualEffect = manualEffect;
        Priority = priority;
        Parameters = parameters;
        DescriptionKey = descriptionKey;
    }

    public string Id { get; }

    public RuleFamily Family { get; }

    public RuleScope Scope { get; }

    public RuleAutomaticEffect AutomaticEffect { get; }

    public RuleManualEffect ManualEffect { get; }

    public RulePriority? Priority { get; }

    public RuleParameters Parameters { get; }

    public string DescriptionKey { get; }
}
