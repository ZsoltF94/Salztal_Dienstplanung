using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Rules;
using Salztal.Dienstplanung.Planning.Validation;

namespace Salztal.Dienstplanung.Planning.Tests.Rules;

public sealed class InitialRuleTranslationRegistryTests
{
    [Fact]
    public void DefaultRegistryContainsEveryInitialRuleExactlyOnce()
    {
        RuleCatalog catalog = InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value!;
        InitialRuleTranslationRegistry registry = new();

        Assert.Equal(30, registry.Descriptors.Count);
        Assert.Equal(
            catalog.Definitions.Select(definition => definition.Id.Value).Order(),
            registry.Descriptors.Select(descriptor => descriptor.RuleId).Order());
        Assert.Empty(RuleTranslationRegistryValidator.Validate(registry));
    }

    [Fact]
    public void ValidatorReportsMissingDuplicateUnknownAndWrongClassification()
    {
        InitialRuleTranslationRegistry valid = new();
        RuleTranslationDescriptor[] descriptors = valid.Descriptors
            .Skip(1)
            .Append(valid.Descriptors[1])
            .Append(new RuleTranslationDescriptor(
                "UNKNOWN_RULE",
                RuleTranslationKind.HardConstraint))
            .Append(valid.Descriptors[2] with
            {
                Kind = RuleTranslationKind.NoticeEvaluation,
            })
            .ToArray();

        PlanningInputValidationIssue[] issues = RuleTranslationRegistryValidator
            .Validate(new InitialRuleTranslationRegistry(descriptors))
            .ToArray();

        Assert.Contains(issues, issue =>
            issue.Code == PlanningInputValidationCode.RuleNotTranslated
            && issue.RuleId == valid.Descriptors[0].RuleId);
        Assert.Contains(issues, issue =>
            issue.Code == PlanningInputValidationCode.DuplicateRule
            && issue.RuleId == valid.Descriptors[1].RuleId);
        Assert.Contains(issues, issue =>
            issue.Code == PlanningInputValidationCode.UnknownRule
            && issue.RuleId == "UNKNOWN_RULE");
        Assert.Contains(issues, issue =>
            issue.Code == PlanningInputValidationCode.RuleDefinitionMismatch
            && issue.RuleId == valid.Descriptors[2].RuleId);
    }
}
