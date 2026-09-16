using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialStabilityRuleDefinitions
{
    private static readonly ReadOnlyCollection<RuleDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                InitialRuleDefinitionFactory.Create(
                    "CURRENT_PERIOD_FAIR_DISTRIBUTION",
                    RuleFamily.Stability,
                    RuleScope.PlanningPeriod,
                    RuleAutomaticEffect.StabilityTieBreaker,
                    RuleManualEffect.NotApplicable,
                    null,
                    new CurrentPeriodFairDistributionParameters(
                        3,
                        FairDistributionSubjects.UnfavorableShifts
                            | FairDistributionSubjects.SplitShifts
                            | FairDistributionSubjects.Weekends),
                    "rules.stability.current_period_fair_distribution"),
            });

    public static RuleDefinition CurrentPeriodFairDistribution => Definitions[0];

    public static IReadOnlyList<RuleDefinition> All => Definitions;
}
