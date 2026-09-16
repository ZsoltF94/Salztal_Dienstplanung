using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyMinutesBelowNoticeParameters : RuleParameters
{
    internal WeeklyMinutesBelowNoticeParameters(
        EmployeeTypePlanningRole role,
        int exclusiveUpperMinutes)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveUpperMinutes);

        Role = role;
        ExclusiveUpperMinutes = exclusiveUpperMinutes;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int ExclusiveUpperMinutes { get; }
}
