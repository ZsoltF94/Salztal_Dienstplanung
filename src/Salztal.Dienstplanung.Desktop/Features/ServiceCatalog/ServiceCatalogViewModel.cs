using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

internal sealed class ServiceCatalogViewModel : ObservableObject
{
    private readonly GetServiceCatalogQuery _query;
    private readonly UpdateWorkLocationCommand _updateCommand;
    private readonly UpdateShiftTypeStandardTimeCommand _updateShiftTypeCommand;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private WorkLocationEditorViewModel? _selectedWorkLocation;
    private SplitShiftPatternViewModel? _splitShiftPattern;
    private ReliefShiftPatternViewModel? _reliefShiftPattern;
    private bool _isLoading;
    private string? _loadErrorMessage;

    public ServiceCatalogViewModel(
        GetServiceCatalogQuery query,
        UpdateWorkLocationCommand updateCommand,
        UpdateShiftTypeStandardTimeCommand updateShiftTypeCommand,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(updateCommand);
        ArgumentNullException.ThrowIfNull(updateShiftTypeCommand);
        ArgumentNullException.ThrowIfNull(errorReporter);

        _query = query;
        _updateCommand = updateCommand;
        _updateShiftTypeCommand = updateShiftTypeCommand;
        _errorReporter = errorReporter;
        WorkLocations = [];
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<WorkLocationEditorViewModel> WorkLocations { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public SplitShiftPatternViewModel? SplitShiftPattern
    {
        get => _splitShiftPattern;
        private set => SetProperty(ref _splitShiftPattern, value);
    }

    public ReliefShiftPatternViewModel? ReliefShiftPattern
    {
        get => _reliefShiftPattern;
        private set => SetProperty(ref _reliefShiftPattern, value);
    }

    public WorkLocationEditorViewModel? SelectedWorkLocation
    {
        get => _selectedWorkLocation;
        set => SetProperty(ref _selectedWorkLocation, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                NotifyCollectionStateChanged();
            }
        }
    }

    public string? LoadErrorMessage
    {
        get => _loadErrorMessage;
        private set
        {
            if (SetProperty(ref _loadErrorMessage, value))
            {
                NotifyCollectionStateChanged();
            }
        }
    }

    public bool HasLoadError => !string.IsNullOrWhiteSpace(LoadErrorMessage);

    public bool HasWorkLocations => WorkLocations.Count > 0;

    public bool IsEmpty => !IsLoading && !HasLoadError && !HasWorkLocations;

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        LoadErrorMessage = null;
        SelectedWorkLocation = null;
        SplitShiftPattern = null;
        ReliefShiftPattern = null;
        WorkLocations.Clear();
        NotifyCollectionStateChanged();

        try
        {
            ServiceCatalogSnapshot snapshot = await _query.ExecuteAsync(cancellationToken);

            foreach (WorkLocationSnapshot workLocation in snapshot.WorkLocations)
            {
                WorkLocations.Add(new WorkLocationEditorViewModel(
                    workLocation,
                    _updateCommand,
                    RefreshPatternsAsync,
                    _errorReporter));
            }

            if (WorkLocations.Count > 0)
            {
                AddShiftTypes(snapshot);
                ApplyPatternPresentations(snapshot);
            }

            SelectedWorkLocation = WorkLocations.FirstOrDefault();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadServiceCatalog");
            LoadErrorMessage = "Die Einsatzorte konnten nicht geladen werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsLoading = false;
            NotifyCollectionStateChanged();
        }
    }

    private void NotifyCollectionStateChanged()
    {
        OnPropertyChanged(nameof(HasLoadError));
        OnPropertyChanged(nameof(HasWorkLocations));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private void AddShiftTypes(ServiceCatalogSnapshot snapshot)
    {
        Dictionary<Guid, WorkLocationEditorViewModel> workLocationsById =
            WorkLocations.ToDictionary(workLocation => workLocation.Id);

        foreach (ShiftTypeSnapshot shiftType in snapshot.ShiftTypes)
        {
            if (!workLocationsById.TryGetValue(
                    shiftType.WorkLocationId,
                    out WorkLocationEditorViewModel? workLocation))
            {
                throw new InvalidOperationException(
                    $"Shift type '{shiftType.Id}' references an unknown work location.");
            }

            workLocation.ShiftTypes.Add(new ShiftTypeEditorViewModel(
                shiftType,
                _updateShiftTypeCommand,
                RefreshPatternsAsync,
                _errorReporter));
        }
    }

    private async Task RefreshPatternsAsync(CancellationToken cancellationToken)
    {
        ServiceCatalogSnapshot snapshot = await _query.ExecuteAsync(cancellationToken);
        ApplyPatternPresentations(snapshot);
    }

    private void ApplyPatternPresentations(ServiceCatalogSnapshot snapshot)
    {
        IReadOnlyDictionary<Guid, WorkLocationSnapshot> workLocationsById =
            snapshot.WorkLocations.ToDictionary(workLocation => workLocation.Id);
        IReadOnlyDictionary<Guid, ShiftTypeSnapshot> shiftTypesById =
            snapshot.ShiftTypes.ToDictionary(shiftType => shiftType.Id);

        SplitShiftPatternSnapshot splitShift = snapshot.SplitShiftPattern;
        SplitShiftPattern = new SplitShiftPatternViewModel(
            splitShift.DisplayCode,
            FindWorkLocation(workLocationsById, splitShift.WorkLocationId).Name,
            FindShiftType(shiftTypesById, splitShift.FirstShiftTypeId).Name,
            FindShiftType(shiftTypesById, splitShift.SecondShiftTypeId).Name,
            FormatDuration(splitShift.StandardBreakMinutes),
            FormatDuration(splitShift.StandardWorkMinutes));

        ReliefShiftPatternSnapshot reliefShift = snapshot.ReliefShiftPattern;
        ReliefShiftPattern = new ReliefShiftPatternViewModel(
            reliefShift.DisplayCode,
            reliefShift.DisplayColorCode,
            GetColorName(reliefShift.DisplayColorCode),
            GetDayName(reliefShift.AllowedDay),
            FindShiftType(shiftTypesById, reliefShift.FirstShiftTypeId).Name,
            FindWorkLocation(workLocationsById, reliefShift.FirstWorkLocationId).Name,
            FindShiftType(shiftTypesById, reliefShift.SecondShiftTypeId).Name,
            FindWorkLocation(workLocationsById, reliefShift.SecondWorkLocationId).Name,
            reliefShift.SwitchesAtEndOfFirstActualDemand
                ? "Wechsel nach dem tatsächlichen Ende des ersten Bedarfs"
                : "Wechselregel ist nicht festgelegt",
            reliefShift.HasInterruption
                ? "mit Unterbrechung"
                : "ohne Unterbrechung");
    }

    private static WorkLocationSnapshot FindWorkLocation(
        IReadOnlyDictionary<Guid, WorkLocationSnapshot> workLocations,
        Guid id)
    {
        return workLocations.TryGetValue(id, out WorkLocationSnapshot? workLocation)
            ? workLocation
            : throw new InvalidOperationException(
                $"Shift pattern references unknown work location '{id}'.");
    }

    private static ShiftTypeSnapshot FindShiftType(
        IReadOnlyDictionary<Guid, ShiftTypeSnapshot> shiftTypes,
        Guid id)
    {
        return shiftTypes.TryGetValue(id, out ShiftTypeSnapshot? shiftType)
            ? shiftType
            : throw new InvalidOperationException(
                $"Shift pattern references unknown shift type '{id}'.");
    }

    private static string FormatDuration(int totalMinutes)
    {
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        return minutes == 0
            ? $"{hours} Stunden"
            : $"{hours} Stunden {minutes} Minuten";
    }

    private static string GetColorName(string colorCode)
    {
        return colorCode.ToLowerInvariant() switch
        {
            "blue" => "Blau",
            "red" => "Rot",
            "yellow" => "Gelb",
            _ => colorCode,
        };
    }

    private static string GetDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "Montag",
            DayOfWeek.Tuesday => "Dienstag",
            DayOfWeek.Wednesday => "Mittwoch",
            DayOfWeek.Thursday => "Donnerstag",
            DayOfWeek.Friday => "Freitag",
            DayOfWeek.Saturday => "Samstag",
            DayOfWeek.Sunday => "Sonntag",
            _ => throw new ArgumentOutOfRangeException(nameof(day), day, null),
        };
    }
}
