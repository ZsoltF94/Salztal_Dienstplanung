using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public sealed class RuleViolationSet
{
    public RuleViolationSet(IEnumerable<RuleViolationCase> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);

        RuleViolationCase[] values = violations.ToArray();
        if (values.Any(violation => violation is null))
        {
            throw new ArgumentException(
                "Rule violations cannot contain null values.",
                nameof(violations));
        }

        RuleViolationCase[] ordered = values
            .OrderBy(violation => violation.RuleId.Value, StringComparer.Ordinal)
            .ThenBy(violation => violation.CaseKey, StringComparer.Ordinal)
            .ToArray();

        if (ordered
            .GroupBy(
                violation => (violation.RuleId, violation.CaseKey),
                RuleViolationIdentityComparer.Instance)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Rule violation identities must be unique.",
                nameof(violations));
        }

        Violations = Array.AsReadOnly(ordered);
        MagnitudeByRule = new ReadOnlyDictionary<RuleId, long>(
            ordered
                .GroupBy(violation => violation.RuleId)
                .ToDictionary(
                    group => group.Key,
                    group => checked(group.Sum(item => item.Magnitude.Value))));
    }

    public static RuleViolationSet Empty { get; } = new([]);

    public ReadOnlyCollection<RuleViolationCase> Violations { get; }

    public IReadOnlyDictionary<RuleId, long> MagnitudeByRule { get; }

    public int Count => Violations.Count;

    private sealed class RuleViolationIdentityComparer :
        IEqualityComparer<(RuleId RuleId, string CaseKey)>
    {
        internal static RuleViolationIdentityComparer Instance { get; } = new();

        public bool Equals(
            (RuleId RuleId, string CaseKey) left,
            (RuleId RuleId, string CaseKey) right)
        {
            return left.RuleId == right.RuleId
                && StringComparer.Ordinal.Equals(left.CaseKey, right.CaseKey);
        }

        public int GetHashCode((RuleId RuleId, string CaseKey) value)
        {
            return HashCode.Combine(
                value.RuleId,
                StringComparer.Ordinal.GetHashCode(value.CaseKey));
        }
    }
}
