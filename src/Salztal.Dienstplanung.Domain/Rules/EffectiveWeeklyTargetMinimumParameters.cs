using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record EffectiveWeeklyTargetMinimumParameters : RuleParameters
{
    internal EffectiveWeeklyTargetMinimumParameters(
        EmployeeTypePlanningRole role,
        int minimumMinutesBelowTarget)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumMinutesBelowTarget);

        Role = role;
        MinimumMinutesBelowTarget = minimumMinutesBelowTarget;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int MinimumMinutesBelowTarget { get; }
}
