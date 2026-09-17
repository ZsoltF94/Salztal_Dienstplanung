using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.ModelBuilding;

namespace Salztal.Dienstplanung.Planning.Rules.HardRules;

internal static class AutomaticHardRuleModelBuilder
{
    public static HardRulePlanningContext Apply(
        PlanningInputSnapshot snapshot,
        StructuralPlanningModel structuralModel,
        ReliefShiftEmergencyGate reliefShiftEmergencyGate)
    {
        HardRulePlanningContext context = new(
            snapshot,
            structuralModel,
            reliefShiftEmergencyGate);
        IReadOnlyList<IAutomaticHardRuleTranslator> translators =
            AutomaticHardRuleTranslators.CreateAll();
        if (translators.Select(translator => translator.RuleId).Distinct().Count()
            != translators.Count)
        {
            throw new InvalidOperationException(
                "Automatic hard-rule translators must be unique.");
        }

        foreach (IAutomaticHardRuleTranslator translator in translators)
        {
            translator.Apply(context);
        }

        return context;
    }
}
