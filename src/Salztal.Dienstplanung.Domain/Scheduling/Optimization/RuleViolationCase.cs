using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public sealed record RuleViolationCase
{
    public RuleViolationCase(
        RuleDefinition rule,
        string caseKey,
        RuleViolationMagnitude magnitude)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentException.ThrowIfNullOrWhiteSpace(caseKey);

        RuleId = rule.Id;
        RuleFamily = rule.Family;
        Priority = rule.Priority;
        CaseKey = caseKey.Trim();
        Magnitude = magnitude;
    }

    public RuleId RuleId { get; }

    public RuleFamily RuleFamily { get; }

    public RulePriority? Priority { get; }

    public string CaseKey { get; }

    public RuleViolationMagnitude Magnitude { get; }
}
