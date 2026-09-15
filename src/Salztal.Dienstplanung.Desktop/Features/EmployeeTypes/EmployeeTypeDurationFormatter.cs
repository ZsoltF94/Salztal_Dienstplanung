using System.Globalization;

namespace Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;

internal static class EmployeeTypeDurationFormatter
{
    public static string Format(int minutes)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{minutes / 60}:{minutes % 60:00}");
    }

    public static bool TryParse(string? value, out int minutes)
    {
        minutes = 0;
        string[] parts = value?.Trim().Split(':') ?? [];
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int hours)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int remainingMinutes)
            || hours < 0
            || remainingMinutes is < 0 or > 59)
        {
            return false;
        }

        try
        {
            minutes = checked((hours * 60) + remainingMinutes);
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}
