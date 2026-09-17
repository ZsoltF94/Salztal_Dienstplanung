using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Planning.Rules.HardRules;

internal interface IAutomaticHardRuleTranslator
{
    public RuleId RuleId { get; }

    public void Apply(HardRulePlanningContext context);
}
