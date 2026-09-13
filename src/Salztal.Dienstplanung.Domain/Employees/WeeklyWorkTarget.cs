using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record WeeklyWorkTarget
{
    public const int MaximumMinutes = 7 * 24 * 60;

    private WeeklyWorkTarget(int minutes)
    {
        Minutes = minutes;
    }

    public int Minutes { get; }

    internal static WeeklyWorkTargetValidationCode? TryCreate(
        int minutes,
        [NotNullWhen(true)] out WeeklyWorkTarget? weeklyWorkTarget)
    {
        if (minutes <= 0)
        {
            weeklyWorkTarget = null;
            return WeeklyWorkTargetValidationCode.MustBePositive;
        }

        if (minutes > MaximumMinutes)
        {
            weeklyWorkTarget = null;
            return WeeklyWorkTargetValidationCode.ExceedsWeek;
        }

        weeklyWorkTarget = new WeeklyWorkTarget(minutes);
        return null;
    }

    public override string ToString()
    {
        return Minutes.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
