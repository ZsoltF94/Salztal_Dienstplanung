using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class RuleEvaluationResultTests
{
    [Theory]
    [InlineData(RuleEvaluationStatus.Satisfied)]
    [InlineData(RuleEvaluationStatus.Violated)]
    [InlineData(RuleEvaluationStatus.NotApplicable)]
    [InlineData(RuleEvaluationStatus.NotFullyEvaluable)]
    public void CreateForEveryApprovedStatusReturnsStructuredResult(
        RuleEvaluationStatus status)
    {
        RuleEvaluationResult result = RuleEvaluationResult.Create(
            CreateRuleId(),
            status,
            NoRuleResultParameters.Instance);

        Assert.Equal(status, result.Status);
        Assert.Equal("RULE_A", result.RuleId.Value);
        Assert.Same(NoRuleResultParameters.Instance, result.Parameters);
    }

    [Fact]
    public void CreateWhenStatusIsUnknownRejectsResult()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RuleEvaluationResult.Create(
            CreateRuleId(),
            (RuleEvaluationStatus)99,
            NoRuleResultParameters.Instance));
    }

    [Fact]
    public void CreateWhenValuesMatchProducesImmutableValueEqualResults()
    {
        RuleEvaluationResult first = RuleEvaluationResult.Create(
            CreateRuleId(),
            RuleEvaluationStatus.NotFullyEvaluable,
            NoRuleResultParameters.Instance);
        RuleEvaluationResult second = RuleEvaluationResult.Create(
            CreateRuleId(),
            RuleEvaluationStatus.NotFullyEvaluable,
            NoRuleResultParameters.Instance);

        Assert.Equal(first, second);
        Assert.All(
            typeof(RuleEvaluationResult).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void ResultContractWhenInspectedContainsNoGermanMessageText()
    {
        Assert.DoesNotContain(
            typeof(RuleEvaluationResult).GetProperties(),
            property => property.PropertyType == typeof(string));
        Assert.True(typeof(RuleResultParameters).IsAbstract);
    }

    private static RuleId CreateRuleId()
    {
        Assert.True(RuleId.TryCreate("RULE_A", out RuleId? ruleId));
        return ruleId;
    }
}
