using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class RuleDefinitionTests
{
    [Theory]
    [InlineData(
        RuleFamily.Structure,
        RuleAutomaticEffect.BlockInvalidStructure,
        RuleManualEffect.Block,
        null)]
    [InlineData(
        RuleFamily.AutomaticHard,
        RuleAutomaticEffect.HardConstraint,
        RuleManualEffect.Block,
        null)]
    [InlineData(
        RuleFamily.AutomaticHard,
        RuleAutomaticEffect.HardConstraint,
        RuleManualEffect.WarnAndRequireConfirmation,
        null)]
    [InlineData(
        RuleFamily.AutomaticHard,
        RuleAutomaticEffect.HardConstraint,
        RuleManualEffect.NotApplicable,
        null)]
    [InlineData(
        RuleFamily.Soft,
        RuleAutomaticEffect.OptimizationObjective,
        RuleManualEffect.WarnAndRequireConfirmation,
        RulePriority.High)]
    [InlineData(
        RuleFamily.Soft,
        RuleAutomaticEffect.OptimizationObjective,
        RuleManualEffect.WarnAndRequireConfirmation,
        RulePriority.Medium)]
    [InlineData(
        RuleFamily.Soft,
        RuleAutomaticEffect.OptimizationObjective,
        RuleManualEffect.WarnAndRequireConfirmation,
        RulePriority.Low)]
    [InlineData(
        RuleFamily.Notice,
        RuleAutomaticEffect.ReportNotice,
        RuleManualEffect.ReportNotice,
        null)]
    [InlineData(
        RuleFamily.Stability,
        RuleAutomaticEffect.StabilityTieBreaker,
        RuleManualEffect.NotApplicable,
        null)]
    public void CreateWhenEffectAndPriorityCombinationIsAllowedReturnsDefinition(
        RuleFamily family,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority)
    {
        RuleDefinitionValidationResult result = CreateDefinition(
            family,
            automaticEffect,
            manualEffect,
            priority);

        RuleDefinition definition = Assert.IsType<RuleDefinition>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(family, definition.Family);
        Assert.Equal(automaticEffect, definition.AutomaticEffect);
        Assert.Equal(manualEffect, definition.ManualEffect);
        Assert.Equal(priority, definition.Priority);
    }

    [Fact]
    public void CreateAcrossAllKnownCombinationsAcceptsExactlyTheApprovedMatrix()
    {
        RulePriority?[] priorities = [null, .. Enum.GetValues<RulePriority>().Cast<RulePriority?>()];

        foreach (RuleFamily family in Enum.GetValues<RuleFamily>())
        {
            foreach (RuleAutomaticEffect automaticEffect in Enum.GetValues<RuleAutomaticEffect>())
            {
                foreach (RuleManualEffect manualEffect in Enum.GetValues<RuleManualEffect>())
                {
                    foreach (RulePriority? priority in priorities)
                    {
                        RuleDefinitionValidationResult result = CreateDefinition(
                            family,
                            automaticEffect,
                            manualEffect,
                            priority);

                        Assert.Equal(
                            IsApprovedCombination(
                                family,
                                automaticEffect,
                                manualEffect,
                                priority),
                            result.IsSuccess);
                    }
                }
            }
        }
    }

    [Fact]
    public void CreateWhenValuesAreUnknownReturnsAllUnknownValueErrors()
    {
        RuleDefinitionValidationResult result = RuleDefinition.Create(
            "RULE_A",
            (RuleFamily)99,
            (RuleScope)99,
            (RuleAutomaticEffect)99,
            (RuleManualEffect)99,
            (RulePriority)99,
            NoRuleParameters.Instance,
            "rule.a");

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(RuleDefinitionValidationCode.FamilyUnknown, error.Code),
            error => Assert.Equal(RuleDefinitionValidationCode.ScopeUnknown, error.Code),
            error => Assert.Equal(
                RuleDefinitionValidationCode.AutomaticEffectUnknown,
                error.Code),
            error => Assert.Equal(
                RuleDefinitionValidationCode.ManualEffectUnknown,
                error.Code),
            error => Assert.Equal(RuleDefinitionValidationCode.PriorityUnknown, error.Code));
    }

    [Fact]
    public void CreateWhenRequiredValuesAreMissingReturnsAllValidationErrors()
    {
        RuleDefinitionValidationResult result = RuleDefinition.Create(
            null,
            RuleFamily.Soft,
            RuleScope.EmployeeWeek,
            RuleAutomaticEffect.OptimizationObjective,
            RuleManualEffect.WarnAndRequireConfirmation,
            null,
            null,
            " ");

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(RuleDefinitionValidationCode.IdentifierInvalid, error.Code),
            error => Assert.Equal(
                RuleDefinitionValidationCode.DescriptionKeyRequired,
                error.Code),
            error => Assert.Equal(RuleDefinitionValidationCode.ParametersRequired, error.Code),
            error => Assert.Equal(RuleDefinitionValidationCode.PriorityRequired, error.Code));
    }

    [Fact]
    public void CreateWhenValuesMatchProducesImmutableValueEqualDefinitions()
    {
        RuleDefinition first = Assert.IsType<RuleDefinition>(
            CreateDefinition(
                RuleFamily.Soft,
                RuleAutomaticEffect.OptimizationObjective,
                RuleManualEffect.WarnAndRequireConfirmation,
                RulePriority.High).Value);
        RuleDefinition second = Assert.IsType<RuleDefinition>(
            CreateDefinition(
                RuleFamily.Soft,
                RuleAutomaticEffect.OptimizationObjective,
                RuleManualEffect.WarnAndRequireConfirmation,
                RulePriority.High).Value);

        Assert.Equal(first, second);
        Assert.All(
            typeof(RuleDefinition).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void RuleParameterContractWhenInspectedHasNoUntypedDictionaryOrObjectValue()
    {
        Assert.True(typeof(RuleParameters).IsAbstract);
        Assert.All(
            typeof(RuleDefinition).GetProperties(),
            property => Assert.NotEqual(typeof(object), property.PropertyType));
        Assert.DoesNotContain(
            typeof(RuleDefinition).GetProperties(),
            property => property.PropertyType.Name.Contains(
                "Dictionary",
                StringComparison.Ordinal));
    }

    private static RuleDefinitionValidationResult CreateDefinition(
        RuleFamily family,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority)
    {
        return RuleDefinition.Create(
            "RULE_A",
            family,
            RuleScope.EmployeeWeek,
            automaticEffect,
            manualEffect,
            priority,
            NoRuleParameters.Instance,
            "rule.a");
    }

    private static bool IsApprovedCombination(
        RuleFamily family,
        RuleAutomaticEffect automaticEffect,
        RuleManualEffect manualEffect,
        RulePriority? priority)
    {
        return family switch
        {
            RuleFamily.Structure =>
                automaticEffect == RuleAutomaticEffect.BlockInvalidStructure
                && manualEffect == RuleManualEffect.Block
                && priority is null,
            RuleFamily.AutomaticHard =>
                automaticEffect == RuleAutomaticEffect.HardConstraint
                && manualEffect is RuleManualEffect.Block
                    or RuleManualEffect.WarnAndRequireConfirmation
                    or RuleManualEffect.NotApplicable
                && priority is null,
            RuleFamily.Soft =>
                automaticEffect == RuleAutomaticEffect.OptimizationObjective
                && manualEffect == RuleManualEffect.WarnAndRequireConfirmation
                && priority is not null,
            RuleFamily.Notice =>
                automaticEffect == RuleAutomaticEffect.ReportNotice
                && manualEffect == RuleManualEffect.ReportNotice
                && priority is null,
            RuleFamily.Stability =>
                automaticEffect == RuleAutomaticEffect.StabilityTieBreaker
                && manualEffect == RuleManualEffect.NotApplicable
                && priority is null,
            _ => false,
        };
    }
}
