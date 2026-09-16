using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyManualAssignmentRuleParameters : RuleParameters
{
    internal WeeklyManualAssignmentRuleParameters(
        EmployeeTypePlanningRole role,
        int minimumAssignments,
        bool skipWhenFullyAbsent)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumAssignments);

        Role = role;
        MinimumAssignments = minimumAssignments;
        SkipWhenFullyAbsent = skipWhenFullyAbsent;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int MinimumAssignments { get; }

    public bool SkipWhenFullyAbsent { get; }
}
