using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Planning.Rules;

internal sealed record RuleTranslationDescriptor(
    string RuleId,
    RuleTranslationKind Kind)
{
    public static RuleTranslationDescriptor For(RuleDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return new RuleTranslationDescriptor(
            definition.Id.Value,
            definition.AutomaticEffect switch
            {
                RuleAutomaticEffect.BlockInvalidStructure =>
                    RuleTranslationKind.StructureConstraint,
                RuleAutomaticEffect.HardConstraint =>
                    RuleTranslationKind.HardConstraint,
                RuleAutomaticEffect.OptimizationObjective =>
                    RuleTranslationKind.OptimizationObjective,
                RuleAutomaticEffect.ReportNotice =>
                    RuleTranslationKind.NoticeEvaluation,
                RuleAutomaticEffect.StabilityTieBreaker =>
                    RuleTranslationKind.StabilityTieBreaker,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(definition),
                    definition.AutomaticEffect,
                    "The rule automatic effect is unknown."),
            });
    }
}
