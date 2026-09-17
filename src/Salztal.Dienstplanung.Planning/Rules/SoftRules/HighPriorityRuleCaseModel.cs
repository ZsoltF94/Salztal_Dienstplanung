using Google.OrTools.Sat;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Planning.Rules.SoftRules;

internal sealed record HighPriorityRuleCaseModel(
    RuleDefinition Rule,
    string CaseKey,
    BoolVar IsViolated,
    IntVar Magnitude);

internal sealed class HighPriorityRuleModel(
    IEnumerable<HighPriorityRuleCaseModel> cases)
{
    public IReadOnlyList<HighPriorityRuleCaseModel> Cases { get; } = cases.ToArray();

    public LinearExpr ViolationCount => LinearExpr.Sum(Cases.Select(item => item.IsViolated));

    public LinearExpr Magnitude(RuleDefinition rule) => LinearExpr.Sum(
        Cases.Where(item => item.Rule.Id == rule.Id).Select(item => item.Magnitude));
}
