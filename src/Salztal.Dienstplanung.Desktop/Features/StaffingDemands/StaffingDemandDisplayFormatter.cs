namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal static class StaffingDemandDisplayFormatter
{
    public static string FormatMinutes(long totalMinutes)
    {
        long hours = totalMinutes / 60;
        long minutes = totalMinutes % 60;

        if (minutes == 0)
        {
            return hours == 1 ? "1 Stunde" : $"{hours} Stunden";
        }

        return hours == 0
            ? $"{minutes} Minuten"
            : $"{hours} Std. {minutes} Min.";
    }

    public static string GetDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Montag",
            DayOfWeek.Tuesday => "Dienstag",
            DayOfWeek.Wednesday => "Mittwoch",
            DayOfWeek.Thursday => "Donnerstag",
            DayOfWeek.Friday => "Freitag",
            DayOfWeek.Saturday => "Samstag",
            DayOfWeek.Sunday => "Sonntag",
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek)),
        };
    }

    public static string GetColorName(string colorCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(colorCode);

        return colorCode.ToLowerInvariant() switch
        {
            "blue" => "Blau",
            "red" => "Rot",
            "yellow" => "Gelb",
            _ => colorCode,
        };
    }
}
