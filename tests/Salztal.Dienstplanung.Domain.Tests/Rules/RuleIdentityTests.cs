using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class RuleIdentityTests
{
    [Theory]
    [InlineData("STRUCTURE_KNOWN_REFERENCES")]
    [InlineData("AH_WEEKLY_LOW_NOTICE")]
    [InlineData("RULE_2")]
    public void RuleIdWhenValueUsesStableMachineFormatCanBeCreated(string value)
    {
        bool wasCreated = RuleId.TryCreate(value, out RuleId? ruleId);

        Assert.True(wasCreated);
        Assert.Equal(value, ruleId!.Value);
    }

    [Fact]
    public void RuleIdWhenValueHasSurroundingWhitespaceReturnsNormalizedValue()
    {
        bool wasCreated = RuleId.TryCreate("  STRUCTURE_KNOWN_REFERENCES  ", out RuleId? ruleId);

        Assert.True(wasCreated);
        Assert.Equal("STRUCTURE_KNOWN_REFERENCES", ruleId!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("structure_known_references")]
    [InlineData("2_RULE")]
    [InlineData("RULE-ID")]
    [InlineData("RULE ID")]
    public void RuleIdWhenValueIsNotAStableMachineIdentifierCannotBeCreated(string? value)
    {
        bool wasCreated = RuleId.TryCreate(value, out RuleId? ruleId);

        Assert.False(wasCreated);
        Assert.Null(ruleId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void RuleCatalogVersionWhenValueIsPositiveCanBeCreated(int value)
    {
        bool wasCreated = RuleCatalogVersion.TryCreate(
            value,
            out RuleCatalogVersion? version);

        Assert.True(wasCreated);
        Assert.Equal(value, version!.Value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void RuleCatalogVersionWhenValueIsNotPositiveCannotBeCreated(int value)
    {
        bool wasCreated = RuleCatalogVersion.TryCreate(
            value,
            out RuleCatalogVersion? version);

        Assert.False(wasCreated);
        Assert.Null(version);
    }

    [Fact]
    public void RuleIdentifiersAndVersionsWhenValuesMatchUseValueEquality()
    {
        Assert.True(RuleId.TryCreate("RULE_A", out RuleId? firstId));
        Assert.True(RuleId.TryCreate("RULE_A", out RuleId? secondId));
        Assert.True(RuleCatalogVersion.TryCreate(1, out RuleCatalogVersion? firstVersion));
        Assert.True(RuleCatalogVersion.TryCreate(1, out RuleCatalogVersion? secondVersion));

        Assert.Equal(firstId, secondId);
        Assert.Equal(firstVersion, secondVersion);
    }
}
