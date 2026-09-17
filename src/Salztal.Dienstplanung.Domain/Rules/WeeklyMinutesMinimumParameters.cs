using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyMinutesMinimumParameters : RuleParameters
{
    internal WeeklyMinutesMinimumParameters(
        EmployeeTypePlanningRole role,
        int minimumMinutes,
        bool requiresEligibleDemand)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumMinutes);
        Role = role;
        MinimumMinutes = minimumMinutes;
        RequiresEligibleDemand = requiresEligibleDemand;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int MinimumMinutes { get; }

    public bool RequiresEligibleDemand { get; }
}
