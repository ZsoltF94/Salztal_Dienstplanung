using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Application.Tests.Rules;

public sealed class GetRuleCatalogQueryTests
{
    private static readonly Type[] ExpectedParameterTypes =
    [
        typeof(NoRuleParameters),
        typeof(PlanningRoleRuleParameters),
        typeof(ExplicitRunOptionRuleParameters),
        typeof(WeeklyManualAssignmentRuleParameters),
        typeof(EffectiveWeeklyTargetMaximumParameters),
        typeof(WeeklyMinutesMaximumParameters),
        typeof(MaximumConsecutiveWorkdaysParameters),
        typeof(VacationBoundaryWeekendParameters),
        typeof(ReliefShiftEmergencyParameters),
        typeof(EffectiveWeeklyTargetMinimumParameters),
        typeof(WeeklyConsecutiveDaysOffParameters),
        typeof(GuaranteedDayOffAdjacencyParameters),
        typeof(SplitShiftWeeklyMaximumParameters),
        typeof(PlanningPeriodFreeWeekendParameters),
        typeof(ShiftPatternMinimizationParameters),
        typeof(WeeklyMinutesMinimumParameters),
        typeof(RelativeWeeklyTargetParameters),
        typeof(WeeklyMinutesBelowNoticeParameters),
        typeof(WeeklyMinutesRangeNoticeParameters),
        typeof(CurrentPeriodFairDistributionParameters),
    ];

    [Fact]
    public async Task ExecuteAsyncWhenCatalogExistsReturnsCompleteImmutableSnapshot()
    {
        RuleCatalogSnapshot snapshot = await GetRuleCatalogQuery.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(2, snapshot.Version);
        Assert.Equal(30, snapshot.Definitions.Count);
        Assert.True(((ICollection<RuleDefinitionSnapshot>)snapshot.Definitions).IsReadOnly);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<RuleDefinitionSnapshot>)snapshot.Definitions).Add(
                snapshot.Definitions[0]));
        Assert.All(
            typeof(RuleCatalogSnapshot).GetProperties()
                .Concat(typeof(RuleDefinitionSnapshot).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public async Task ExecuteAsyncForVersionOneReturnsHistoricalCatalogUnchanged()
    {
        RuleCatalogSnapshot snapshot = await GetRuleCatalogQuery.ExecuteAsync(
            1,
            TestContext.Current.CancellationToken);

        Assert.Equal(1, snapshot.Version);
        Assert.Equal(28, snapshot.Definitions.Count);
        Assert.Contains(snapshot.Definitions, definition =>
            definition.Id == "AH_WEEKLY_TARGET"
            && definition.Parameters is WeeklyMinutesTargetParameters);
        Assert.DoesNotContain(snapshot.Definitions, definition =>
            definition.Id == "AH_WEEKLY_MINIMUM");
    }

    [Fact]
    public async Task ExecuteAsyncWhenCatalogExistsMapsEveryDomainValueExactly()
    {
        RuleCatalog domainCatalog = ReadDomainCatalog();
        RuleCatalogSnapshot snapshot = await GetRuleCatalogQuery.ExecuteAsync(
            TestContext.Current.CancellationToken);

        foreach ((RuleDefinition expected, RuleDefinitionSnapshot actual) in
                 domainCatalog.Definitions.Zip(snapshot.Definitions))
        {
            Assert.Equal(expected.Id.Value, actual.Id);
            Assert.Equal(expected.Family, actual.Family);
            Assert.Equal(expected.Scope, actual.Scope);
            Assert.Equal(expected.AutomaticEffect, actual.AutomaticEffect);
            Assert.Equal(expected.ManualEffect, actual.ManualEffect);
            Assert.Equal(expected.Priority, actual.Priority);
            Assert.Same(expected.Parameters, actual.Parameters);
            Assert.Equal(expected.DescriptionKey.Value, actual.DescriptionKey);
        }
    }

    [Fact]
    public async Task ExecuteAsyncWhenRepeatedPreservesDeterministicOrderAndValues()
    {
        RuleCatalogSnapshot first = await GetRuleCatalogQuery.ExecuteAsync(
            TestContext.Current.CancellationToken);
        RuleCatalogSnapshot second = await GetRuleCatalogQuery.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            first.Definitions.Select(definition => definition.Id),
            second.Definitions.Select(definition => definition.Id));
        Assert.Equal(
            first.Definitions.Select(definition => definition.Parameters),
            second.Definitions.Select(definition => definition.Parameters));
    }

    [Fact]
    public async Task ExecuteAsyncWhenCatalogExistsSupportsEveryTypedParameterFamily()
    {
        RuleCatalogSnapshot snapshot = await GetRuleCatalogQuery.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Type[] actualParameterTypes = snapshot.Definitions
            .Select(definition => definition.Parameters.GetType())
            .Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ExpectedParameterTypes.OrderBy(type => type.FullName, StringComparer.Ordinal),
            actualParameterTypes);
    }

    [Fact]
    public async Task ExecuteAsyncWhenCancelledReturnsNoSnapshot()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => GetRuleCatalogQuery.ExecuteAsync(cancellation.Token));
    }

    [Fact]
    public void RulesApplicationAreaWhenInspectedContainsNoStoreCommandOrAdapter()
    {
        Type[] ruleTypes = typeof(GetRuleCatalogQuery).Assembly
            .GetTypes()
            .Where(type => type.Namespace == typeof(GetRuleCatalogQuery).Namespace)
            .ToArray();

        Assert.DoesNotContain(
            ruleTypes,
            type => type.Name.Contains("Store", StringComparison.Ordinal)
                || type.Name.Contains("Command", StringComparison.Ordinal)
                || type.Name.Contains("Adapter", StringComparison.Ordinal));
    }

    private static RuleCatalog ReadDomainCatalog()
    {
        RuleCatalogReadResult result = InitialRuleCatalog.Read(InitialRuleCatalog.Version);
        return Assert.IsType<RuleCatalog>(result.Value);
    }
}
