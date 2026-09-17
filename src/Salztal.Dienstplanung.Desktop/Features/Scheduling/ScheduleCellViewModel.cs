using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ScheduleCellViewModel : ObservableObject
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private bool _isAssignmentEditorOpen;
    private bool _isSelected;

    public ScheduleCellViewModel(
        Guid employeeId,
        string employeeName,
        DateOnly date,
        bool allowsVacationAndSickness,
        bool isServiceManagement,
        AvailabilityDayEntryKind? entryKind,
        long? changeVersion,
        bool isGeneratedDayOff,
        ScheduleAssignmentSnapshot? assignment,
        ScheduleAssignmentDisplay? assignmentDisplay,
        IEnumerable<ServiceManagementAssignmentOptionViewModel> assignmentOptions)
    {
        ArgumentNullException.ThrowIfNull(assignmentOptions);
        EmployeeId = employeeId;
        EmployeeName = employeeName;
        Date = date;
        AllowsVacationAndSickness = allowsVacationAndSickness;
        IsServiceManagement = isServiceManagement;
        EntryKind = entryKind;
        ChangeVersion = changeVersion;
        IsGeneratedDayOff = isGeneratedDayOff;
        Assignment = assignment;
        AssignmentDisplay = assignmentDisplay;
        AssignmentOptions = Array.AsReadOnly(assignmentOptions.ToArray());
    }

    public Guid EmployeeId { get; }

    public string EmployeeName { get; }

    public DateOnly Date { get; }

    public bool AllowsVacationAndSickness { get; }

    public bool IsServiceManagement { get; }

    public AvailabilityDayEntryKind? EntryKind { get; }

    public long? ChangeVersion { get; }

    public ScheduleAssignmentSnapshot? Assignment { get; }

    public ScheduleAssignmentDisplay? AssignmentDisplay { get; }

    public bool IsGeneratedDayOff { get; }

    public ReadOnlyCollection<ServiceManagementAssignmentOptionViewModel>
        AssignmentOptions
    { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsAssignmentEditorOpen
    {
        get => _isAssignmentEditorOpen;
        set => SetProperty(ref _isAssignmentEditorOpen, value);
    }

    public string EntryDisplay => EntryKind switch
    {
        AvailabilityDayEntryKind.Vacation => "U",
        AvailabilityDayEntryKind.Sickness => "K",
        AvailabilityDayEntryKind.FixedDayOff => "X",
        null when IsGeneratedDayOff => "X",
        null => AssignmentDisplay?.Text ?? string.Empty,
        _ => "?",
    };

    public bool IsFixedDayOff => EntryKind == AvailabilityDayEntryKind.FixedDayOff;

    public bool HasAssignment => Assignment is not null;

    public bool HasEntry => EntryKind is not null || IsGeneratedDayOff;

    public bool CanOpenAssignmentEditor =>
        IsServiceManagement
        && AssignmentOptions.Count > 0
        && !IsGeneratedDayOff
        && Assignment?.Origin is not ScheduleAssignmentOriginSnapshot.AutomaticGeneration
            and not ScheduleAssignmentOriginSnapshot.ManualEdit;

    public string DateDisplay => Date.ToString("dddd, dd.MM.yyyy", GermanCulture);

    public string AutomationName => string.IsNullOrEmpty(EntryDisplay)
        ? $"{EmployeeName}, {DateDisplay}, leer"
        : $"{EmployeeName}, {DateDisplay}, {EntryMeaning}";

    public string EntryMeaning => EntryKind switch
    {
        AvailabilityDayEntryKind.Vacation => "Urlaub",
        AvailabilityDayEntryKind.Sickness => "Krankheit",
        AvailabilityDayEntryKind.FixedDayOff => "fest vorgegebenes Frei",
        null when IsGeneratedDayOff => "automatisch erzeugtes schwarzes X",
        null when Assignment is not null =>
            AssignmentDisplay?.Meaning ?? "Einteilung ohne Anzeigetext",
        null => "kein Tageseintrag",
        _ => "unbekannter Tageseintrag",
    };
}
