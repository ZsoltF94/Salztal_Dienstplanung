using System.Collections.ObjectModel;
using System.Globalization;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class StaffingDemandDayGroupViewModel
{
    public StaffingDemandDayGroupViewModel(
        DateOnly date,
        Guid workLocationId,
        string workLocationName,
        IEnumerable<StaffingDemandItemViewModel> demands,
        long requiredWorkMinutes,
        bool hasDateException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workLocationName);
        ArgumentNullException.ThrowIfNull(demands);

        Date = date;
        WorkLocationId = workLocationId;
        WorkLocationName = workLocationName;
        Demands = Array.AsReadOnly(demands.ToArray());
        RequiredWorkMinutes = requiredWorkMinutes;
        HasDateException = hasDateException;
    }

    public DateOnly Date { get; }

    public Guid WorkLocationId { get; }

    public string WorkLocationName { get; }

    public ReadOnlyCollection<StaffingDemandItemViewModel> Demands { get; }

    public long RequiredWorkMinutes { get; }

    public bool HasDateException { get; }

    public string DayName => StaffingDemandDisplayFormatter.GetDayName(Date.DayOfWeek);

    public string DateDisplay => Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    public string RequiredWorkDisplay => StaffingDemandDisplayFormatter.FormatMinutes(
        RequiredWorkMinutes);

    public bool HasDemands => Demands.Count > 0;

    public bool IsEmpty => !HasDemands;
}
