using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

public sealed class ScheduleRuleEvaluationSet
{
    public ScheduleRuleEvaluationSet(
        RuleCatalog ruleCatalog,
        IEnumerable<RuleEvaluationResult> results)
    {
        ArgumentNullException.ThrowIfNull(ruleCatalog);
        ArgumentNullException.ThrowIfNull(results);

        RuleEvaluationResult[] resultValues = results.ToArray();
        if (resultValues.Any(result => result is null))
        {
            throw new ArgumentException(
                "Rule evaluation results cannot contain null values.",
                nameof(results));
        }

        Dictionary<RuleId, RuleEvaluationResult> resultsById = resultValues
            .ToDictionary(result => result.RuleId);
        RuleId[] expectedIds = ruleCatalog.Definitions
            .Select(definition => definition.Id)
            .ToArray();
        if (resultsById.Count != resultValues.Length
            || !expectedIds.ToHashSet().SetEquals(resultsById.Keys))
        {
            throw new ArgumentException(
                "Rule evaluation results must contain every catalog rule exactly once.",
                nameof(results));
        }

        StructureResults = SelectFamily(
            ruleCatalog,
            resultsById,
            RuleFamily.Structure);
        AutomaticHardResults = SelectFamily(
            ruleCatalog,
            resultsById,
            RuleFamily.AutomaticHard);
        SoftResults = SelectFamily(ruleCatalog, resultsById, RuleFamily.Soft);
        NoticeResults = SelectFamily(ruleCatalog, resultsById, RuleFamily.Notice);
        StabilityResults = SelectFamily(
            ruleCatalog,
            resultsById,
            RuleFamily.Stability);
    }

    public ReadOnlyCollection<RuleEvaluationResult> StructureResults { get; }

    public ReadOnlyCollection<RuleEvaluationResult> AutomaticHardResults { get; }

    public ReadOnlyCollection<RuleEvaluationResult> SoftResults { get; }

    public ReadOnlyCollection<RuleEvaluationResult> NoticeResults { get; }

    public ReadOnlyCollection<RuleEvaluationResult> StabilityResults { get; }

    private static ReadOnlyCollection<RuleEvaluationResult> SelectFamily(
        RuleCatalog catalog,
        Dictionary<RuleId, RuleEvaluationResult> resultsById,
        RuleFamily family)
    {
        return Array.AsReadOnly(
            catalog.Definitions
                .Where(definition => definition.Family == family)
                .Select(definition => resultsById[definition.Id])
                .ToArray());
    }
}
