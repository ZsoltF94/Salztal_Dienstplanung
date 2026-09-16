using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyMinutesTargetParameters : RuleParameters
{
    internal WeeklyMinutesTargetParameters(
        EmployeeTypePlanningRole role,
        int targetMinutes)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetMinutes);

        Role = role;
        TargetMinutes = targetMinutes;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int TargetMinutes { get; }
}
