using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Planning.Validation;

internal sealed class PlanningInputValidationResult
{
    public PlanningInputValidationResult(
        IEnumerable<PlanningInputValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        Issues = Array.AsReadOnly(
            issues
                .OrderBy(issue => issue.Code)
                .ThenBy(issue => issue.RuleId, StringComparer.Ordinal)
                .ThenBy(issue => issue.TechnicalEntityId)
                .ThenBy(issue => issue.Date)
                .ThenBy(issue => issue.CatalogVersion)
                .ThenBy(issue => issue.Parameter, StringComparer.Ordinal)
                .ToArray());
    }

    public bool IsValid => Issues.Count == 0;

    public ReadOnlyCollection<PlanningInputValidationIssue> Issues { get; }
}
