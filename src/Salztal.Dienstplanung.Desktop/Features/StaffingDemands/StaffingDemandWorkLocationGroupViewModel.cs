using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class StaffingDemandWorkLocationGroupViewModel
{
    public StaffingDemandWorkLocationGroupViewModel(
        Guid id,
        string name,
        string colorCode,
        IEnumerable<StaffingDemandDayGroupViewModel> days,
        long requiredWorkMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(colorCode);
        ArgumentNullException.ThrowIfNull(days);

        Id = id;
        Name = name;
        ColorCode = colorCode;
        ColorName = StaffingDemandDisplayFormatter.GetColorName(colorCode);
        Days = Array.AsReadOnly(days.ToArray());
        RequiredWorkMinutes = requiredWorkMinutes;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string ColorCode { get; }

    public string ColorName { get; }

    public ReadOnlyCollection<StaffingDemandDayGroupViewModel> Days { get; }

    public long RequiredWorkMinutes { get; }

    public string RequiredWorkDisplay => StaffingDemandDisplayFormatter.FormatMinutes(
        RequiredWorkMinutes);
}
