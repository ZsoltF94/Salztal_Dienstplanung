using System.Globalization;

namespace Salztal.Dienstplanung.Desktop.Features.Availabilities;

internal sealed class AvailabilityDayHeaderViewModel
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    public AvailabilityDayHeaderViewModel(DateOnly date)
    {
        Date = date;
    }

    public DateOnly Date { get; }

    public string DayDisplay => Date.ToString("ddd", GermanCulture);

    public string DateDisplay => Date.ToString("dd.MM.", GermanCulture);

    public string AutomationName => Date.ToString("dddd, dd.MM.yyyy", GermanCulture);
}
