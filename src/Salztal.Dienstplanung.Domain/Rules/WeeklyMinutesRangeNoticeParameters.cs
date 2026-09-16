using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyMinutesRangeNoticeParameters : RuleParameters
{
    internal WeeklyMinutesRangeNoticeParameters(
        EmployeeTypePlanningRole role,
        int exclusiveLowerMinutes,
        int inclusiveUpperMinutes)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(exclusiveLowerMinutes);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            inclusiveUpperMinutes,
            exclusiveLowerMinutes);

        Role = role;
        ExclusiveLowerMinutes = exclusiveLowerMinutes;
        InclusiveUpperMinutes = inclusiveUpperMinutes;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int ExclusiveLowerMinutes { get; }

    public int InclusiveUpperMinutes { get; }
}
