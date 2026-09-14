using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class StandardStaffingDemandWeekDayViewModel
{
    public StandardStaffingDemandWeekDayViewModel(
        string dayName,
        IEnumerable<StandardStaffingDemandWeekItemViewModel> items)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dayName);
        ArgumentNullException.ThrowIfNull(items);

        DayName = dayName;
        Items = Array.AsReadOnly(items.ToArray());
    }

    public string DayName { get; }

    public ReadOnlyCollection<StandardStaffingDemandWeekItemViewModel> Items { get; }
}
