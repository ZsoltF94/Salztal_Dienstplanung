using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ScheduleCellViewModel : ObservableObject
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private bool _isSelected;

    public ScheduleCellViewModel(
        Guid employeeId,
        string employeeName,
        DateOnly date,
        bool allowsVacationAndSickness,
        bool isServiceManagement,
        AvailabilityDayEntryKind? entryKind,
        long? changeVersion,
        ScheduleAssignmentSnapshot? assignment,
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
        Assignment = assignment;
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

    public ReadOnlyCollection<ServiceManagementAssignmentOptionViewModel>
        AssignmentOptions
    { get; }

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
        null => CreateAssignmentDisplay(Assignment),
        _ => "?",
    };

    public bool IsFixedDayOff => EntryKind == AvailabilityDayEntryKind.FixedDayOff;

    public bool HasAssignment => Assignment is not null;

    public bool HasEntry => EntryKind is not null;

    public string DateDisplay => Date.ToString("dddd, dd.MM.yyyy", GermanCulture);

    public string AutomationName => string.IsNullOrEmpty(EntryDisplay)
        ? $"{EmployeeName}, {DateDisplay}, leer"
        : $"{EmployeeName}, {DateDisplay}, {EntryMeaning}";

    public string EntryMeaning => EntryKind switch
    {
        AvailabilityDayEntryKind.Vacation => "Urlaub",
        AvailabilityDayEntryKind.Sickness => "Krankheit",
        AvailabilityDayEntryKind.FixedDayOff => "fest vorgegebenes Frei",
        null when Assignment is not null => CreateAssignmentMeaning(Assignment),
        null => "kein Tageseintrag",
        _ => "unbekannter Tageseintrag",
    };

    private static string CreateAssignmentDisplay(ScheduleAssignmentSnapshot? assignment)
    {
        return assignment?.Kind switch
        {
            ScheduleAssignmentKindSnapshot.NormalDemand => "Typ1",
            ScheduleAssignmentKindSnapshot.OfficeTime => "B",
            ScheduleAssignmentKindSnapshot.SplitShiftPattern => "D",
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern => "Spr",
            null => string.Empty,
            _ => "?",
        };
    }

    private static string CreateAssignmentMeaning(ScheduleAssignmentSnapshot assignment)
    {
        return assignment.Kind switch
        {
            ScheduleAssignmentKindSnapshot.NormalDemand => "vorgetragener Typ1-Dienst",
            ScheduleAssignmentKindSnapshot.OfficeTime =>
                "vorgetragener Typ1-Dienst mit Bürokennzeichnung B; Bedarf bleibt offen",
            ScheduleAssignmentKindSnapshot.SplitShiftPattern =>
                "vorgetragenes Typ1-Dienstmuster D",
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern =>
                "vorgetragenes Typ1-Dienstmuster Spr",
            _ => "vorgetragene Einteilung",
        };
    }
}
