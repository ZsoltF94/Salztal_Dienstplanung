using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Features.Availabilities;

internal sealed class AvailabilityCellViewModel : ObservableObject
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private bool _isSelected;

    public AvailabilityCellViewModel(
        Guid employeeId,
        string employeeName,
        DateOnly date,
        bool allowsVacationAndSickness,
        AvailabilityDayEntryKind? entryKind,
        long? changeVersion)
    {
        EmployeeId = employeeId;
        EmployeeName = employeeName;
        Date = date;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        EntryKind = entryKind;
        ChangeVersion = changeVersion;
    }

    public Guid EmployeeId { get; }

    public string EmployeeName { get; }

    public DateOnly Date { get; }

    public bool AllowsVacationAndSickness { get; }

    public AvailabilityDayEntryKind? EntryKind { get; }

    public long? ChangeVersion { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string EntryDisplay => EntryKind switch
    {
        AvailabilityDayEntryKind.Vacation => "U",
        AvailabilityDayEntryKind.Sickness => "K",
        AvailabilityDayEntryKind.FixedDayOff => "X",
        null => string.Empty,
        _ => "?",
    };

    public bool IsFixedDayOff => EntryKind == AvailabilityDayEntryKind.FixedDayOff;

    public string DateDisplay => Date.ToString("dddd, dd.MM.yyyy", GermanCulture);

    public string AutomationName => string.IsNullOrEmpty(EntryDisplay)
        ? $"{EmployeeName}, {DateDisplay}, leer"
        : $"{EmployeeName}, {DateDisplay}, {EntryMeaning}";

    public string EntryMeaning => EntryKind switch
    {
        AvailabilityDayEntryKind.Vacation => "Urlaub",
        AvailabilityDayEntryKind.Sickness => "Krankheit",
        AvailabilityDayEntryKind.FixedDayOff => "fest vorgegebenes Frei",
        null => "kein Tageseintrag",
        _ => "unbekannter Tageseintrag",
    };
}
