using System.Globalization;
using Salztal.Dienstplanung.Application.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Features.Availabilities;

internal sealed class AvailabilityWeekSummaryViewModel
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    public AvailabilityWeekSummaryViewModel(
        int weekNumber,
        AvailabilityPeriodWeekSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        WeekNumber = weekNumber;
        WeekMonday = snapshot.WeekMonday;
        WeekSunday = snapshot.WeekSunday;
        UncutWorkTargetMinutes = snapshot.UncutWorkTargetMinutes;
        EffectiveWorkTargetMinutes = snapshot.EffectiveWorkTargetMinutes;
        IsFullyUnavailable = snapshot.IsFullyUnavailable;
    }

    public int WeekNumber { get; }

    public DateOnly WeekMonday { get; }

    public DateOnly WeekSunday { get; }

    public int UncutWorkTargetMinutes { get; }

    public int EffectiveWorkTargetMinutes { get; }

    public bool IsFullyUnavailable { get; }

    public string HeaderDisplay => $"Woche {WeekNumber}";

    public string DateRangeDisplay => string.Create(
        GermanCulture,
        $"{WeekMonday:dd.MM.}–{WeekSunday:dd.MM.}");

    public string UncutDisplay => $"Soll: {FormatMinutes(UncutWorkTargetMinutes)}";

    public string EffectiveDisplay => $"Wirksam: {FormatMinutes(EffectiveWorkTargetMinutes)}";

    private static string FormatMinutes(int minutes)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{minutes / 60}:{minutes % 60:00} Std.");
    }
}
