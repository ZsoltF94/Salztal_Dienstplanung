using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class DateStaffingDemandEditorViewModel : ObservableObject
{
    private static readonly ReadOnlyCollection<StaffingDemandTimeOption> AvailableTimes =
        Array.AsReadOnly(
            Enumerable.Range(0, 48)
                .Select(index => new StaffingDemandTimeOption(
                    TimeOnly.MinValue.AddMinutes(index * 30)))
                .ToArray());

    private readonly SaveStaffingDemandDateExceptionCommand _saveCommand;
    private readonly RemoveStaffingDemandDateExceptionCommand _removeCommand;
    private readonly Func<CancellationToken, Task> _refreshWeekAsync;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private IReadOnlyList<DateStaffingDemandEditItemSnapshot> _items = [];
    private DateStaffingDemandEditItemSnapshot? _selectedItem;
    private DateOnly _selectedDate;
    private Guid _selectedWorkLocationId;
    private string _selectedWorkLocationName = string.Empty;
    private StaffingDemandShiftTypeOption? _selectedShiftType;
    private StaffingDemandTimeOption _selectedStartTime = AvailableTimes[0];
    private StaffingDemandTimeOption _selectedEndTime = AvailableTimes[1];
    private string _requiredEmployeeCountInput = "1";
    private bool _isOpen;
    private bool _isSaving;
    private string? _errorMessage;
    private string? _successMessage;

    public DateStaffingDemandEditorViewModel(
        SaveStaffingDemandDateExceptionCommand saveCommand,
        RemoveStaffingDemandDateExceptionCommand removeCommand,
        Func<CancellationToken, Task> refreshWeekAsync,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(saveCommand);
        ArgumentNullException.ThrowIfNull(removeCommand);
        ArgumentNullException.ThrowIfNull(refreshWeekAsync);
        ArgumentNullException.ThrowIfNull(errorReporter);

        _saveCommand = saveCommand;
        _removeCommand = removeCommand;
        _refreshWeekAsync = refreshWeekAsync;
        _errorReporter = errorReporter;
        ShiftTypes = [];
        TimeOptions = AvailableTimes;
        OpenDayCommand = new RelayCommand<StaffingDemandDayGroupViewModel>(OpenDay);
        OpenDemandCommand = new RelayCommand<StaffingDemandItemViewModel>(OpenDemand);
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        NoDemandCommand = new AsyncRelayCommand(SetNoDemandAsync, CanSetNoDemand);
        ResetCommand = new AsyncRelayCommand(ResetAsync, CanReset);
        CloseCommand = new RelayCommand(Close, () => !IsSaving);
    }

    public ObservableCollection<StaffingDemandShiftTypeOption> ShiftTypes { get; }

    public ReadOnlyCollection<StaffingDemandTimeOption> TimeOptions { get; }

    public IRelayCommand<StaffingDemandDayGroupViewModel> OpenDayCommand { get; }

    public IRelayCommand<StaffingDemandItemViewModel> OpenDemandCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand NoDemandCommand { get; }

    public IAsyncRelayCommand ResetCommand { get; }

    public IRelayCommand CloseCommand { get; }

    public bool IsOpen
    {
        get => _isOpen;
        private set => SetProperty(ref _isOpen, value);
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

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
            {
                NotifyCommandsChanged();
                CloseCommand.NotifyCanExecuteChanged();
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

    public bool HasDateException => _selectedItem?.HasDateException == true;

    public bool IsNoDemandChange => _selectedItem?.IsNoDemandChange == true;

    public string SelectedDateDisplay => _selectedDate.ToString(
        "dddd, dd.MM.yyyy",
        CultureInfo.GetCultureInfo("de-DE"));

    public string SelectedWorkLocationName => _selectedWorkLocationName;

    public string CurrentStateDisplay => _selectedItem switch
    {
        { IsNoDemandChange: true } =>
            "Einmalige Änderung: Für diesen Dienst gilt an diesem Tag kein Bedarf.",
        { HasDateException: true } =>
            "Einmalige Änderung: Die angezeigten Werte gelten nur an diesem Tag.",
        { HasRegularStandard: true } =>
            "Regelmäßiger Standard: Eine Speicherung ändert nur diesen Tag.",
        not null =>
            "An diesem Tag fehlt dieser Bedarf. Sie können ihn einmalig ergänzen.",
        _ => "Bitte wählen Sie einen normalen Diensttyp.",
    };

    public string PrimaryActionDisplay => _selectedItem?.HasEffectiveDemand == true
        ? "Nur diesen Tag speichern"
        : "Nur an diesem Tag ergänzen";

    public void ApplySnapshot(
        IReadOnlyCollection<DateStaffingDemandEditItemSnapshot> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Guid? selectedShiftTypeId = SelectedShiftType?.Id;
        _items = items.ToArray();

        if (!IsOpen)
        {
            return;
        }

        DateStaffingDemandEditItemSnapshot? context = _items.FirstOrDefault(item =>
            item.Date == _selectedDate
            && item.WorkLocationId == _selectedWorkLocationId);
        if (context is null)
        {
            Close();
            return;
        }

        Open(context.Date, context.WorkLocationId, context.WorkLocationName, selectedShiftTypeId);
    }

    private void OpenDay(StaffingDemandDayGroupViewModel? day)
    {
        if (day is null)
        {
            return;
        }

        Open(day.Date, day.WorkLocationId, day.WorkLocationName, null);
    }

    private void OpenDemand(StaffingDemandItemViewModel? demand)
    {
        if (demand is null)
        {
            return;
        }

        Open(
            demand.Date,
            demand.WorkLocationId,
            demand.WorkLocationName,
            demand.ShiftTypeId);
    }

    private void Open(
        DateOnly date,
        Guid workLocationId,
        string workLocationName,
        Guid? preferredShiftTypeId)
    {
        _selectedDate = date;
        _selectedWorkLocationId = workLocationId;
        _selectedWorkLocationName = workLocationName;
        IsOpen = true;
        OnPropertyChanged(nameof(SelectedDateDisplay));
        OnPropertyChanged(nameof(SelectedWorkLocationName));

        ShiftTypes.Clear();
        foreach (StaffingDemandShiftTypeOption option in _items
                     .Where(item => item.Date == date && item.WorkLocationId == workLocationId)
                     .Select(item => new StaffingDemandShiftTypeOption(
                         item.ShiftTypeId,
                         item.WorkLocationId,
                         item.ShiftTypeName))
                     .OrderBy(option => option.Name, StringComparer.CurrentCulture))
        {
            ShiftTypes.Add(option);
        }

        _selectedShiftType = ShiftTypes.FirstOrDefault(option =>
                option.Id == preferredShiftTypeId)
            ?? ShiftTypes.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedShiftType));
        ApplySelectedItem();
    }

    private void Close()
    {
        IsOpen = false;
        ClearMessages();
    }

    private bool CanSave()
    {
        return !IsSaving && _selectedItem is not null;
    }

    private bool CanSetNoDemand()
    {
        return !IsSaving
            && _selectedItem is not null
            && (_selectedItem.HasRegularStandard
                || _selectedItem.HasEffectiveDemand
                || _selectedItem.HasDateException);
    }

    private bool CanReset()
    {
        return !IsSaving && _selectedItem?.HasDateException == true;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_selectedItem is null)
        {
            return;
        }

        int? requiredEmployeeCount = int.TryParse(
            RequiredEmployeeCountInput,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int parsedEmployeeCount)
            ? parsedEmployeeCount
            : null;
        SaveStaffingDemandDateExceptionRequest request = _selectedItem.HasRegularStandard
            ? SaveStaffingDemandDateExceptionRequest.CreateReplacement(
                Guid.NewGuid(),
                _selectedItem.Date,
                _selectedItem.WorkLocationId,
                _selectedItem.ShiftTypeId,
                SelectedStartTime.Value,
                SelectedEndTime.Value,
                requiredEmployeeCount,
                _selectedItem.ExpectedCurrentExceptionId)
            : SaveStaffingDemandDateExceptionRequest.CreateAddition(
                Guid.NewGuid(),
                _selectedItem.Date,
                _selectedItem.WorkLocationId,
                _selectedItem.ShiftTypeId,
                SelectedStartTime.Value,
                SelectedEndTime.Value,
                requiredEmployeeCount,
                _selectedItem.ExpectedCurrentExceptionId);

        await SaveChangeAsync(
            request,
            "Die einmalige Änderung wurde gespeichert.",
            cancellationToken);
    }

    private Task SetNoDemandAsync(CancellationToken cancellationToken)
    {
        if (_selectedItem is null)
        {
            return Task.CompletedTask;
        }

        if (!_selectedItem.HasRegularStandard)
        {
            return ResetAsync(cancellationToken);
        }

        SaveStaffingDemandDateExceptionRequest request =
            SaveStaffingDemandDateExceptionRequest.CreateRemoval(
            Guid.NewGuid(),
            _selectedItem.Date,
            _selectedItem.WorkLocationId,
            _selectedItem.ShiftTypeId,
            _selectedItem.ExpectedCurrentExceptionId);
        return SaveChangeAsync(
            request,
            "Für diesen Dienst gilt nur an diesem Tag kein Bedarf.",
            cancellationToken);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task SaveChangeAsync(
        SaveStaffingDemandDateExceptionRequest request,
        string successMessage,
        CancellationToken cancellationToken)
    {
        IsSaving = true;
        ClearMessages();

        try
        {
            StaffingDemandCommandResult result = await _saveCommand.ExecuteAsync(
                request,
                cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(error => error.Message));
                return;
            }

            await _refreshWeekAsync(cancellationToken);
            SuccessMessage = successMessage;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "SaveStaffingDemandDateChange");
            ErrorMessage = "Die einmalige Änderung konnte nicht gespeichert werden. Bitte versuchen Sie es erneut.";
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
    private async Task ResetAsync(CancellationToken cancellationToken)
    {
        if (_selectedItem?.ExpectedCurrentExceptionId is not Guid exceptionId)
        {
            return;
        }

        string successMessage = _selectedItem.HasRegularStandard
            ? "Die einmalige Änderung wurde zurückgesetzt. Es gilt wieder der regelmäßige Standard."
            : "Die einmalige Ergänzung wurde zurückgesetzt. Für diesen Tag besteht wieder kein Bedarf.";

        IsSaving = true;
        ClearMessages();

        try
        {
            StaffingDemandCommandResult result = await _removeCommand.ExecuteAsync(
                new RemoveStaffingDemandDateExceptionRequest(
                    _selectedItem.Date,
                    _selectedItem.WorkLocationId,
                    _selectedItem.ShiftTypeId,
                    exceptionId),
                cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(error => error.Message));
                return;
            }

            await _refreshWeekAsync(cancellationToken);
            SuccessMessage = successMessage;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "ResetStaffingDemandDateChange");
            ErrorMessage = "Die einmalige Änderung konnte nicht zurückgesetzt werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ApplySelectedItem()
    {
        _selectedItem = SelectedShiftType is null
            ? null
            : _items.SingleOrDefault(item =>
                item.Date == _selectedDate
                && item.WorkLocationId == _selectedWorkLocationId
                && item.ShiftTypeId == SelectedShiftType.Id);
        if (_selectedItem is null)
        {
            NotifySelectionChanged();
            return;
        }

        _selectedStartTime = FindTime(
            _selectedItem.ActualStart
            ?? _selectedItem.RegularActualStart
            ?? _selectedItem.ShiftTypeStandardStart);
        _selectedEndTime = FindTime(
            _selectedItem.ActualEnd
            ?? _selectedItem.RegularActualEnd
            ?? _selectedItem.ShiftTypeStandardEnd);
        _requiredEmployeeCountInput = (
            _selectedItem.RequiredEmployeeCount
            ?? _selectedItem.RegularRequiredEmployeeCount
            ?? 1).ToString(CultureInfo.InvariantCulture);
        OnPropertyChanged(nameof(SelectedStartTime));
        OnPropertyChanged(nameof(SelectedEndTime));
        OnPropertyChanged(nameof(RequiredEmployeeCountInput));
        ClearMessages();
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(HasDateException));
        OnPropertyChanged(nameof(IsNoDemandChange));
        OnPropertyChanged(nameof(CurrentStateDisplay));
        OnPropertyChanged(nameof(PrimaryActionDisplay));
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
        NoDemandCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
    }

    private static StaffingDemandTimeOption FindTime(TimeOnly time)
    {
        return AvailableTimes.First(option => option.Value == time);
    }
}
