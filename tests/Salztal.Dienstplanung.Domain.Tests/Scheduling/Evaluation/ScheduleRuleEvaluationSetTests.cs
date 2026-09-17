using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling.Evaluation;

public sealed class ScheduleRuleEvaluationSetTests
{
    [Fact]
    public void CompleteCatalogResultsAreSeparatedByRuleFamily()
    {
        RuleCatalog catalog = ReadCatalog();
        RuleEvaluationResult[] results = catalog.Definitions
            .Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.Satisfied,
                NoRuleResultParameters.Instance))
            .Reverse()
            .ToArray();

        ScheduleRuleEvaluationSet set = new(catalog, results);

        Assert.Equal(6, set.StructureResults.Count);
        Assert.Equal(11, set.AutomaticHardResults.Count);
        Assert.Equal(10, set.SoftResults.Count);
        Assert.Equal(2, set.NoticeResults.Count);
        Assert.Single(set.StabilityResults);
        Assert.Equal(
            InitialStructureRuleDefinitions.All.Select(definition => definition.Id),
            set.StructureResults.Select(result => result.RuleId));
    }

    [Fact]
    public void MissingOrDuplicateRuleResultIsRejected()
    {
        RuleCatalog catalog = ReadCatalog();
        RuleEvaluationResult first = RuleEvaluationResult.Create(
            catalog.Definitions[0].Id,
            RuleEvaluationStatus.Satisfied,
            NoRuleResultParameters.Instance);

        Assert.Throws<ArgumentException>(() => new ScheduleRuleEvaluationSet(
            catalog,
            [first, first]));
    }

    private static RuleCatalog ReadCatalog()
    {
        return Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
    }
}
