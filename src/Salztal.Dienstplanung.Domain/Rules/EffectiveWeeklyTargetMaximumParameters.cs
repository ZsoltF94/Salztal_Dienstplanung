using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record EffectiveWeeklyTargetMaximumParameters : RuleParameters
{
    internal EffectiveWeeklyTargetMaximumParameters(
        EmployeeTypePlanningRole role,
        int maximumMinutesAboveTarget)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumMinutesAboveTarget);

        Role = role;
        MaximumMinutesAboveTarget = maximumMinutesAboveTarget;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int MaximumMinutesAboveTarget { get; }
}
