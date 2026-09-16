using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record WeeklyMinutesMaximumParameters : RuleParameters
{
    internal WeeklyMinutesMaximumParameters(
        EmployeeTypePlanningRole role,
        int maximumMinutes)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumMinutes);

        Role = role;
        MaximumMinutes = maximumMinutes;
    }

    public EmployeeTypePlanningRole Role { get; }

    public int MaximumMinutes { get; }
}
