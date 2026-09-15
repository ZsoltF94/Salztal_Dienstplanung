using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed record AbsenceDayValue
{
    public const int MaximumMinutes = 24 * 60;

    private AbsenceDayValue(int minutes)
    {
        Minutes = minutes;
    }

    public int Minutes { get; }

    internal static AbsenceDayValueValidationCode? TryCreate(
        int minutes,
        [NotNullWhen(true)] out AbsenceDayValue? absenceDayValue)
    {
        if (minutes <= 0)
        {
            absenceDayValue = null;
            return AbsenceDayValueValidationCode.MustBePositive;
        }

        if (minutes > MaximumMinutes)
        {
            absenceDayValue = null;
            return AbsenceDayValueValidationCode.ExceedsDay;
        }

        absenceDayValue = new AbsenceDayValue(minutes);
        return null;
    }

    public override string ToString()
    {
        return Minutes.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
