using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class StandardStaffingDemandEditorViewModel : ObservableObject
{
    private static readonly ReadOnlyCollection<StaffingDemandDayOption> AvailableDays =
        Array.AsReadOnly(
        new StaffingDemandDayOption[]
        {
            new(DayOfWeek.Monday, "Montag"),
            new(DayOfWeek.Tuesday, "Dienstag"),
            new(DayOfWeek.Wednesday, "Mittwoch"),
            new(DayOfWeek.Thursday, "Donnerstag"),
            new(DayOfWeek.Friday, "Freitag"),
            new(DayOfWeek.Saturday, "Samstag"),
            new(DayOfWeek.Sunday, "Sonntag"),
        });

    private static readonly ReadOnlyCollection<StaffingDemandTimeOption> AvailableTimes =
        Array.AsReadOnly(
            Enumerable.Range(0, 48)
                .Select(index => new StaffingDemandTimeOption(
                    TimeOnly.MinValue.AddMinutes(index * 30)))
                .ToArray());

    private readonly GetStaffingDemandWeekQuery _query;
    private readonly ChangeStandardStaffingDemandCommand _changeCommand;
    private readonly Func<CancellationToken, Task> _refreshOverviewAsync;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private IReadOnlyList<StandardStaffingDemandEditItemSnapshot> _items = [];
    private Guid? _preferredWorkLocationId;
    private DateOnly _loadedWeekMonday;
    private StaffingDemandWorkLocationOption? _selectedWorkLocation;
    private StaffingDemandShiftTypeOption? _selectedShiftType;
    private StaffingDemandDayOption _selectedDay = AvailableDays[0];
    private StaffingDemandTimeOption _selectedStartTime = AvailableTimes[0];
    private StaffingDemandTimeOption _selectedEndTime = AvailableTimes[1];
    private StandardStaffingDemandEditItemSnapshot? _selectedItem;
    private DateTime _effectiveFromMonday;
    private string _requiredEmployeeCountInput = "1";
    private bool _hasEffectiveStandard;
    private bool _hasRevisionAtEffectiveMonday;
    private bool _isSaving;
    private string? _errorMessage;
    private string? _successMessage;

    public StandardStaffingDemandEditorViewModel(
        GetStaffingDemandWeekQuery query,
        ChangeStandardStaffingDemandCommand changeCommand,
        DateOnly initialEffectiveMonday,
        Func<CancellationToken, Task> refreshOverviewAsync,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(changeCommand);
        ArgumentNullException.ThrowIfNull(refreshOverviewAsync);
        ArgumentNullException.ThrowIfNull(errorReporter);

        if (initialEffectiveMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "The initial regular staffing-demand week must start on a Monday.",
                nameof(initialEffectiveMonday));
        }

        _query = query;
        _changeCommand = changeCommand;
        _refreshOverviewAsync = refreshOverviewAsync;
        _errorReporter = errorReporter;
        _effectiveFromMonday = initialEffectiveMonday.ToDateTime(TimeOnly.MinValue);
        WorkLocations = [];
        ShiftTypes = [];
        WeekDays = [];
        Days = AvailableDays;
        TimeOptions = AvailableTimes;
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        RemoveCommand = new AsyncRelayCommand(RemoveAsync, CanRemove);
        LoadEffectiveWeekCommand = new AsyncRelayCommand(
            LoadEffectiveWeekAsync,
            CanLoadEffectiveWeek);
        SelectWeekItemCommand = new RelayCommand<StandardStaffingDemandWeekItemViewModel>(
            SelectWeekItem);
    }

    public ObservableCollection<StaffingDemandWorkLocationOption> WorkLocations { get; }

    public ObservableCollection<StaffingDemandShiftTypeOption> ShiftTypes { get; }

    public ObservableCollection<StandardStaffingDemandWeekDayViewModel> WeekDays { get; }

    public ReadOnlyCollection<StaffingDemandDayOption> Days { get; }

    public ReadOnlyCollection<StaffingDemandTimeOption> TimeOptions { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand RemoveCommand { get; }

    public IAsyncRelayCommand LoadEffectiveWeekCommand { get; }

    public IRelayCommand<StandardStaffingDemandWeekItemViewModel> SelectWeekItemCommand { get; }

    public StaffingDemandWorkLocationOption? SelectedWorkLocation
    {
        get => _selectedWorkLocation;
        set
        {
            if (SetProperty(ref _selectedWorkLocation, value))
            {
                RefreshShiftTypes();
                RefreshWeekDays();
                ApplySelectedItem();
            }
        }
    }

    public StaffingDemandShiftTypeOption? SelectedShiftType
    {
        get => _selectedShiftType;
        set
        {
            if (SetProperty(ref _selectedShiftType, value))
            {
                ApplySelectedItem();
            }
        }
    }

    public StaffingDemandDayOption SelectedDay
    {
        get => _selectedDay;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (SetProperty(ref _selectedDay, value))
            {
                ApplySelectedItem();
            }
        }
    }

    public StaffingDemandTimeOption SelectedStartTime
    {
        get => _selectedStartTime;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (SetProperty(ref _selectedStartTime, value))
            {
                OnInputChanged();
            }
        }
    }

    public StaffingDemandTimeOption SelectedEndTime
    {
        get => _selectedEndTime;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (SetProperty(ref _selectedEndTime, value))
            {
                OnInputChanged();
            }
        }
    }

    public DateTime EffectiveFromMonday
    {
        get => _effectiveFromMonday;
        set
        {
            if (SetProperty(ref _effectiveFromMonday, value.Date))
            {
                OnInputChanged();
                OnPropertyChanged(nameof(IsLoadedEffectiveWeek));
            }
        }
    }

    public string RequiredEmployeeCountInput
    {
        get => _requiredEmployeeCountInput;
        set
        {
            if (SetProperty(ref _requiredEmployeeCountInput, value))
            {
                OnInputChanged();
            }
        }
    }

    public bool HasEffectiveStandard
    {
        get => _hasEffectiveStandard;
        private set
        {
            if (SetProperty(ref _hasEffectiveStandard, value))
            {
                OnPropertyChanged(nameof(EditStateDisplay));
                OnPropertyChanged(nameof(PrimaryActionDisplay));
                RemoveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasRevisionAtEffectiveMonday
    {
        get => _hasRevisionAtEffectiveMonday;
        private set
        {
            if (SetProperty(ref _hasRevisionAtEffectiveMonday, value))
            {
                OnPropertyChanged(nameof(EditStateDisplay));
                SaveCommand.NotifyCanExecuteChanged();
                RemoveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
            {
                NotifyCommandsChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value))
            {
                OnPropertyChanged(nameof(HasSuccessMessage));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool IsLoadedEffectiveWeek =>
        DateOnly.FromDateTime(EffectiveFromMonday) == _loadedWeekMonday;

    public string EditStateDisplay => HasRevisionAtEffectiveMonday
        ? "Für diesen Montag ist bereits eine Fassung gespeichert. Speichern legt eine neue Korrekturfassung an; ältere Fassungen bleiben erhalten."
        : HasEffectiveStandard
            ? "Für diese Auswahl gilt ein regelmäßiger Bedarf. Speichern legt ab dem Montag eine neue Fassung an."
            : "Für diese Auswahl fehlt ein regelmäßiger Bedarf. Speichern ergänzt ihn ab dem Montag.";

    public string PrimaryActionDisplay => HasEffectiveStandard
        ? "Regelmäßigen Bedarf speichern"
        : "Regelmäßigen Bedarf ergänzen";

    internal void SelectWorkLocation(Guid? workLocationId)
    {
        _preferredWorkLocationId = workLocationId;
        SelectedWorkLocation = WorkLocations.FirstOrDefault(option => option.Id == workLocationId);
    }

    public void ApplySnapshot(
        IReadOnlyCollection<StandardStaffingDemandEditItemSnapshot> items,
        DateOnly weekMonday)
    {
        ArgumentNullException.ThrowIfNull(items);

        Guid? selectedWorkLocationId = _preferredWorkLocationId ?? SelectedWorkLocation?.Id;
        Guid? selectedShiftTypeId = SelectedShiftType?.Id;
        DayOfWeek selectedDay = SelectedDay.Value;
        _items = items.ToArray();
        _loadedWeekMonday = weekMonday;
        _effectiveFromMonday = weekMonday.ToDateTime(TimeOnly.MinValue);
        OnPropertyChanged(nameof(EffectiveFromMonday));
        OnPropertyChanged(nameof(IsLoadedEffectiveWeek));

        WorkLocations.Clear();
        foreach (StaffingDemandWorkLocationOption option in _items
                     .GroupBy(item => item.WorkLocationId)
                     .Select(group => new StaffingDemandWorkLocationOption(
                         group.Key,
                         group.First().WorkLocationName))
                     .OrderBy(option => option.Name, StringComparer.CurrentCulture))
        {
            WorkLocations.Add(option);
        }

        _selectedWorkLocation = WorkLocations.FirstOrDefault(option =>
                option.Id == selectedWorkLocationId)
            ?? WorkLocations.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedWorkLocation));
        RefreshShiftTypes(selectedShiftTypeId);
        RefreshWeekDays();
        _selectedDay = Days.First(option => option.Value == selectedDay);
        OnPropertyChanged(nameof(SelectedDay));
        ApplySelectedItem();
    }

    private bool CanSave()
    {
        return !IsSaving
            && SelectedWorkLocation is not null
            && SelectedShiftType is not null;
    }

    private bool CanRemove()
    {
        return CanSave() && HasEffectiveStandard;
    }

    private bool CanLoadEffectiveWeek()
    {
        return !IsSaving;
    }

    private Task SaveAsync(CancellationToken cancellationToken)
    {
        return ChangeAsync(isRemoval: false, cancellationToken);
    }

    private Task RemoveAsync(CancellationToken cancellationToken)
    {
        return ChangeAsync(isRemoval: true, cancellationToken);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ChangeAsync(bool isRemoval, CancellationToken cancellationToken)
    {
        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            DateOnly effectiveFromMonday = DateOnly.FromDateTime(EffectiveFromMonday);
            if (effectiveFromMonday.DayOfWeek == DayOfWeek.Monday
                && effectiveFromMonday != _loadedWeekMonday)
            {
                ErrorMessage = "Bitte laden Sie zuerst die gewählte Wirksamkeitswoche. So bleibt der angezeigte Ausgangsstand eindeutig.";
                return;
            }

            int? requiredEmployeeCount = null;
            if (!isRemoval && int.TryParse(
                    RequiredEmployeeCountInput,
                    out int parsedEmployeeCount))
            {
                requiredEmployeeCount = parsedEmployeeCount;
            }

            ChangeStandardStaffingDemandRequest request = CreateRequest(
                isRemoval,
                effectiveFromMonday,
                requiredEmployeeCount);
            StaffingDemandCommandResult result = await _changeCommand.ExecuteAsync(
                request,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(error => error.Message));
                return;
            }

            if (!await LoadWeekAsync(effectiveFromMonday, cancellationToken))
            {
                return;
            }

            await _refreshOverviewAsync(cancellationToken);
            SuccessMessage = isRemoval
                ? "Ab dem gewählten Montag gilt für diese Auswahl kein regelmäßiger Bedarf."
                : "Der regelmäßige Bedarf wurde ab dem gewählten Montag gespeichert.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "ChangeStandardStaffingDemand");
            ErrorMessage = "Der regelmäßige Bedarf konnte nicht gespeichert werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task LoadEffectiveWeekAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        DateOnly monday = DateOnly.FromDateTime(EffectiveFromMonday);
        if (monday.DayOfWeek != DayOfWeek.Monday)
        {
            ErrorMessage = "Der regelmäßige Bedarf muss ab einem Montag gelten.";
            return;
        }

        try
        {
            await LoadWeekAsync(monday, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadStandardStaffingDemandEffectiveWeek");
            ErrorMessage = "Die Wirksamkeitswoche konnte nicht geladen werden. Bitte versuchen Sie es erneut.";
        }
    }

    private async Task<bool> LoadWeekAsync(
        DateOnly monday,
        CancellationToken cancellationToken)
    {
        StaffingDemandWeekQueryResult result = await _query.ExecuteAsync(
            monday,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (!result.IsSuccess)
        {
            ErrorMessage = string.Join(
                Environment.NewLine,
                result.Errors.Select(error => error.Message));
            return false;
        }

        ApplySnapshot(
            result.Value?.StandardEditItems
                ?? throw new InvalidOperationException(
                    "A successful staffing-demand query returned no week snapshot."),
            monday);
        return true;
    }

    private void RefreshShiftTypes(Guid? preferredShiftTypeId = null)
    {
        Guid? currentShiftTypeId = preferredShiftTypeId ?? SelectedShiftType?.Id;
        ShiftTypes.Clear();
        if (SelectedWorkLocation is not null)
        {
            foreach (StaffingDemandShiftTypeOption option in _items
                         .Where(item => item.WorkLocationId == SelectedWorkLocation.Id)
                         .GroupBy(item => item.ShiftTypeId)
                         .Select(group => new StaffingDemandShiftTypeOption(
                             group.Key,
                             group.First().WorkLocationId,
                             group.First().ShiftTypeName))
                         .OrderBy(option => option.Name, StringComparer.CurrentCulture))
            {
                ShiftTypes.Add(option);
            }
        }

        _selectedShiftType = ShiftTypes.FirstOrDefault(option =>
                option.Id == currentShiftTypeId)
            ?? ShiftTypes.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedShiftType));
    }

    private void RefreshWeekDays()
    {
        WeekDays.Clear();
        if (SelectedWorkLocation is null)
        {
            return;
        }

        foreach (StaffingDemandDayOption day in Days)
        {
            IEnumerable<StandardStaffingDemandWeekItemViewModel> items = _items
                .Where(item => item.WorkLocationId == SelectedWorkLocation.Id
                    && item.DayOfWeek == day.Value)
                .OrderBy(item => item.ShiftTypeName, StringComparer.CurrentCulture)
                .Select(item => new StandardStaffingDemandWeekItemViewModel(
                    item.DayOfWeek,
                    item.ShiftTypeId,
                    item.ShiftTypeName,
                    FormatDemand(item),
                    item.HasRevisionAtEffectiveMonday
                        ? "Korrekturfassung ab diesem Montag"
                        : null));
            WeekDays.Add(new StandardStaffingDemandWeekDayViewModel(
                day.DisplayName,
                items));
        }
    }

    private void SelectWeekItem(StandardStaffingDemandWeekItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        _selectedDay = Days.First(day => day.Value == item.DayOfWeek);
        OnPropertyChanged(nameof(SelectedDay));
        _selectedShiftType = ShiftTypes.First(shiftType => shiftType.Id == item.ShiftTypeId);
        OnPropertyChanged(nameof(SelectedShiftType));
        ApplySelectedItem();
    }

    private static string FormatDemand(StandardStaffingDemandEditItemSnapshot item)
    {
        if (!item.HasEffectiveStandard)
        {
            return "Kein regelmäßiger Bedarf";
        }

        string people = item.RequiredEmployeeCount == 1
            ? "1 Person"
            : $"{item.RequiredEmployeeCount} Personen";
        return $"{item.ActualStart:HH\\:mm}–{item.ActualEnd:HH\\:mm} Uhr · {people}";
    }

    private ChangeStandardStaffingDemandRequest CreateRequest(
        bool isRemoval,
        DateOnly effectiveFromMonday,
        int? requiredEmployeeCount)
    {
        if (isRemoval)
        {
            return ChangeStandardStaffingDemandRequest.CreateRemoval(
                Guid.NewGuid(),
                SelectedDay.Value,
                SelectedWorkLocation!.Id,
                SelectedShiftType!.Id,
                effectiveFromMonday,
                _selectedItem?.ExpectedCurrentRevisionId);
        }

        return HasEffectiveStandard
            ? ChangeStandardStaffingDemandRequest.CreateReplacement(
                Guid.NewGuid(),
                SelectedDay.Value,
                SelectedWorkLocation!.Id,
                SelectedShiftType!.Id,
                effectiveFromMonday,
                SelectedStartTime.Value,
                SelectedEndTime.Value,
                requiredEmployeeCount,
                _selectedItem?.ExpectedCurrentRevisionId)
            : ChangeStandardStaffingDemandRequest.CreateAddition(
                Guid.NewGuid(),
                SelectedDay.Value,
                SelectedWorkLocation!.Id,
                SelectedShiftType!.Id,
                effectiveFromMonday,
                SelectedStartTime.Value,
                SelectedEndTime.Value,
                requiredEmployeeCount,
                _selectedItem?.ExpectedCurrentRevisionId);
    }

    private void ApplySelectedItem()
    {
        _selectedItem = SelectedWorkLocation is null || SelectedShiftType is null
            ? null
            : _items.SingleOrDefault(item =>
                item.WorkLocationId == SelectedWorkLocation.Id
                && item.ShiftTypeId == SelectedShiftType.Id
                && item.DayOfWeek == SelectedDay.Value);

        if (_selectedItem is null)
        {
            HasEffectiveStandard = false;
            HasRevisionAtEffectiveMonday = false;
            NotifyCommandsChanged();
            return;
        }

        HasEffectiveStandard = _selectedItem.HasEffectiveStandard;
        HasRevisionAtEffectiveMonday = _selectedItem.HasRevisionAtEffectiveMonday;
        _selectedStartTime = FindTime(
            _selectedItem.ActualStart ?? _selectedItem.ShiftTypeStandardStart);
        _selectedEndTime = FindTime(
            _selectedItem.ActualEnd ?? _selectedItem.ShiftTypeStandardEnd);
        _requiredEmployeeCountInput =
            (_selectedItem.RequiredEmployeeCount ?? 1).ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        OnPropertyChanged(nameof(SelectedStartTime));
        OnPropertyChanged(nameof(SelectedEndTime));
        OnPropertyChanged(nameof(RequiredEmployeeCountInput));
        ClearMessages();
        NotifyCommandsChanged();
    }

    private void OnInputChanged()
    {
        ClearMessages();
        NotifyCommandsChanged();
    }

    private void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private void NotifyCommandsChanged()
    {
        SaveCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
        LoadEffectiveWeekCommand.NotifyCanExecuteChanged();
    }

    private static StaffingDemandTimeOption FindTime(TimeOnly time)
    {
        return AvailableTimes.First(option => option.Value == time);
    }
}
