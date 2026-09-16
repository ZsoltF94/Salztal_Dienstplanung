using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ServiceManagementAssignmentEditorViewModel : ObservableObject
{
    private ScheduleCellViewModel? _cell;
    private ServiceManagementAssignmentOptionViewModel? _selectedOption;

    public event Action? SelectionChanged;

    public ScheduleCellViewModel? Cell => _cell;

    public ReadOnlyCollection<ServiceManagementAssignmentOptionViewModel> Options =>
        _cell?.AssignmentOptions
        ?? Array.AsReadOnly(Array.Empty<ServiceManagementAssignmentOptionViewModel>());

    public ServiceManagementAssignmentOptionViewModel? SelectedOption
    {
        get => _selectedOption;
        set
        {
            if (SetProperty(ref _selectedOption, value))
            {
                SelectionChanged?.Invoke();
            }
        }
    }

    public bool IsVisible => _cell?.IsServiceManagement == true;

    public bool HasOptions => Options.Count > 0;

    public bool HasAssignment => _cell?.HasAssignment == true;

    public string CurrentAssignmentDisplay => _cell?.HasAssignment == true
        ? $"Aktuell: {_cell.EntryMeaning}"
        : "Aktuell ist kein Typ1-Dienst eingetragen.";

    public void ApplyCell(ScheduleCellViewModel? cell)
    {
        _cell = cell;
        _selectedOption = cell?.AssignmentOptions.FirstOrDefault();
        OnPropertyChanged(nameof(Cell));
        OnPropertyChanged(nameof(Options));
        OnPropertyChanged(nameof(SelectedOption));
        OnPropertyChanged(nameof(IsVisible));
        OnPropertyChanged(nameof(HasOptions));
        OnPropertyChanged(nameof(HasAssignment));
        OnPropertyChanged(nameof(CurrentAssignmentDisplay));
        SelectionChanged?.Invoke();
    }
}
