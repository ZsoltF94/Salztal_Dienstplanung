using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.Availabilities;

internal sealed class AvailabilityOverviewViewModel : ObservableObject
{
    private readonly GetAvailabilityPeriodQuery _query;
    private readonly SaveAvailabilityEntryCommand _saveCommand;
    private readonly RemoveAvailabilityEntryCommand _removeCommand;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private DateTime _selectedPeriodDate;
    private AvailabilityCellViewModel? _selectedCell;
    private PendingAvailabilityAction? _pendingAction;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    public AvailabilityOverviewViewModel(
        GetAvailabilityPeriodQuery query,
        SaveAvailabilityEntryCommand saveCommand,
        RemoveAvailabilityEntryCommand removeCommand,
        DateOnly initialPeriodMonday,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(saveCommand);
        ArgumentNullException.ThrowIfNull(removeCommand);
        ArgumentNullException.ThrowIfNull(errorReporter);
        if (initialPeriodMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "The initial availability period must start on a Monday.",
                nameof(initialPeriodMonday));
        }

        _query = query;
        _saveCommand = saveCommand;
        _removeCommand = removeCommand;
        _errorReporter = errorReporter;
        _selectedPeriodDate = initialPeriodMonday.ToDateTime(TimeOnly.MinValue);
        Days = [];
        Employees = [];
        LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
        PreviousPeriodCommand = new AsyncRelayCommand(
            cancellationToken => ChangePeriodAsync(-21, cancellationToken),
            CanLoad);
        NextPeriodCommand = new AsyncRelayCommand(
            cancellationToken => ChangePeriodAsync(21, cancellationToken),
            CanLoad);
        SelectCellCommand = new RelayCommand<AvailabilityCellViewModel>(SelectCell);
        SetVacationCommand = new AsyncRelayCommand(
            cancellationToken => RequestOrSetAsync(
                AvailabilityDayEntryKind.Vacation,
                cancellationToken),
            () => CanSet(AvailabilityDayEntryKind.Vacation));
        SetSicknessCommand = new AsyncRelayCommand(
            cancellationToken => RequestOrSetAsync(
                AvailabilityDayEntryKind.Sickness,
                cancellationToken),
            () => CanSet(AvailabilityDayEntryKind.Sickness));
        SetFixedDayOffCommand = new AsyncRelayCommand(
            cancellationToken => RequestOrSetAsync(
                AvailabilityDayEntryKind.FixedDayOff,
                cancellationToken),
            () => CanSet(AvailabilityDayEntryKind.FixedDayOff));
        RequestRemovalCommand = new RelayCommand(RequestRemoval, CanRemove);
        ConfirmPendingActionCommand = new AsyncRelayCommand(
            ConfirmPendingActionAsync,
            () => IsConfirmationOpen && !IsBusy);
        CancelPendingActionCommand = new RelayCommand(
            CancelPendingAction,
            () => IsConfirmationOpen && !IsBusy);
    }

    public ObservableCollection<AvailabilityDayHeaderViewModel> Days { get; }

    public ObservableCollection<AvailabilityEmployeeRowViewModel> Employees { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand PreviousPeriodCommand { get; }

    public IAsyncRelayCommand NextPeriodCommand { get; }

    public IRelayCommand<AvailabilityCellViewModel> SelectCellCommand { get; }

    public IAsyncRelayCommand SetVacationCommand { get; }

    public IAsyncRelayCommand SetSicknessCommand { get; }

    public IAsyncRelayCommand SetFixedDayOffCommand { get; }

    public IRelayCommand RequestRemovalCommand { get; }

    public IAsyncRelayCommand ConfirmPendingActionCommand { get; }

    public IRelayCommand CancelPendingActionCommand { get; }

    public DateTime SelectedPeriodDate
    {
        get => _selectedPeriodDate;
        set
        {
            DateOnly monday = GetMonday(DateOnly.FromDateTime(value));
            if (SetProperty(ref _selectedPeriodDate, monday.ToDateTime(TimeOnly.MinValue)))
            {
                OnPropertyChanged(nameof(SelectedPeriodRangeDisplay));
            }
        }
    }

    public AvailabilityCellViewModel? SelectedCell
    {
        get => _selectedCell;
        private set
        {
            if (ReferenceEquals(_selectedCell, value))
            {
                return;
            }

            if (_selectedCell is not null)
            {
                _selectedCell.IsSelected = false;
            }

            _selectedCell = value;
            if (_selectedCell is not null)
            {
                _selectedCell.IsSelected = true;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SelectedCellDisplay));
            OnPropertyChanged(nameof(SelectedCellEntryDisplay));
            NotifyCommandsChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsLoading));
                NotifyViewStateChanged();
                NotifyCommandsChanged();
            }
        }
    }

    public bool IsLoading => IsBusy && Employees.Count == 0;

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

    public string SelectedPeriodRangeDisplay
    {
        get
        {
            DateOnly monday = DateOnly.FromDateTime(SelectedPeriodDate);
            return string.Create(
                CultureInfo.GetCultureInfo("de-DE"),
                $"{monday:dd.MM.yyyy} bis {monday.AddDays(20):dd.MM.yyyy}");
        }
    }

    public bool HasSelection => SelectedCell is not null;

    public string SelectedCellDisplay => SelectedCell is null
        ? "Wähle ein Tagesfeld aus."
        : $"{SelectedCell.EmployeeName} – {SelectedCell.DateDisplay}";

    public string SelectedCellEntryDisplay => SelectedCell is null
        ? string.Empty
        : $"Aktueller Wert: {SelectedCell.EntryMeaning}";

    public bool IsConfirmationOpen => _pendingAction is not null;

    public string? ConfirmationMessage => _pendingAction?.Message;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool HasEmployees => Employees.Count > 0;

    public bool IsEmpty => !IsBusy && !HasError && !HasEmployees;

    internal async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadPeriodAsync(null, null, cancellationToken);
    }

    private bool CanLoad()
    {
        return !IsBusy && !IsConfirmationOpen;
    }

    private bool CanSet(AvailabilityDayEntryKind kind)
    {
        return SelectedCell is not null
            && !IsBusy
            && !IsConfirmationOpen
            && (kind == AvailabilityDayEntryKind.FixedDayOff
                || SelectedCell.AllowsVacationAndSickness);
    }

    private bool CanRemove()
    {
        return SelectedCell?.EntryKind is not null && !IsBusy && !IsConfirmationOpen;
    }

    private async Task ChangePeriodAsync(int days, CancellationToken cancellationToken)
    {
        SelectedPeriodDate = SelectedPeriodDate.AddDays(days);
        await LoadAsync(cancellationToken);
    }

    private void SelectCell(AvailabilityCellViewModel? cell)
    {
        if (cell is null || IsBusy || IsConfirmationOpen)
        {
            return;
        }

        ClearFeedback();
        SelectedCell = cell;
    }

    private async Task RequestOrSetAsync(
        AvailabilityDayEntryKind kind,
        CancellationToken cancellationToken)
    {
        AvailabilityCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No availability cell is selected.");
        ClearFeedback();

        if (cell.EntryKind == kind)
        {
            SuccessMessage = "Der ausgewählte Tageswert ist bereits eingetragen.";
            return;
        }

        if (cell.EntryKind is not null)
        {
            SetPendingAction(new PendingAvailabilityAction(
                kind,
                false,
                $"Soll bei {cell.EmployeeName} am {cell.Date:dd.MM.yyyy} "
                + $"„{cell.EntryMeaning}“ durch „{GetMeaning(kind)}“ ersetzt werden?"));
            return;
        }

        await ExecuteSaveAsync(
            cell,
            kind,
            AvailabilityEntryReplacementConfirmation.NotRequired,
            cancellationToken);
    }

    private void RequestRemoval()
    {
        AvailabilityCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No availability cell is selected.");
        if (cell.EntryKind is null)
        {
            return;
        }

        ClearFeedback();
        SetPendingAction(new PendingAvailabilityAction(
            null,
            true,
            $"Soll bei {cell.EmployeeName} am {cell.Date:dd.MM.yyyy} "
            + $"„{cell.EntryMeaning}“ wirklich entfernt werden?"));
    }

    private async Task ConfirmPendingActionAsync(CancellationToken cancellationToken)
    {
        AvailabilityCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No availability cell is selected.");
        PendingAvailabilityAction action = _pendingAction
            ?? throw new InvalidOperationException("No availability action is pending.");
        SetPendingAction(null);

        if (action.IsRemoval)
        {
            await ExecuteRemovalAsync(cell, cancellationToken);
            return;
        }

        await ExecuteSaveAsync(
            cell,
            action.Kind!.Value,
            AvailabilityEntryReplacementConfirmation.Confirmed,
            cancellationToken);
    }

    private void CancelPendingAction()
    {
        SetPendingAction(null);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ExecuteSaveAsync(
        AvailabilityCellViewModel cell,
        AvailabilityDayEntryKind kind,
        AvailabilityEntryReplacementConfirmation confirmation,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            AvailabilityEntryCommandResult result = await _saveCommand.ExecuteAsync(
                new SaveAvailabilityEntryRequest(
                    cell.EmployeeId,
                    cell.Date,
                    kind,
                    cell.ChangeVersion,
                    confirmation),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Status != AvailabilityEntryCommandStatus.Succeeded)
            {
                ApplyCommandErrors(result.Errors);
                return;
            }

            await LoadPeriodAsync(
                (cell.EmployeeId, cell.Date),
                "Der Tageseintrag wurde gespeichert.",
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "SaveAvailabilityEntry");
            ErrorMessage =
                "Der Tageseintrag konnte nicht gespeichert werden. Bitte versuche es erneut.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ExecuteRemovalAsync(
        AvailabilityCellViewModel cell,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            AvailabilityEntryCommandResult result = await _removeCommand.ExecuteAsync(
                new RemoveAvailabilityEntryRequest(
                    cell.EmployeeId,
                    cell.Date,
                    cell.ChangeVersion!.Value,
                    AvailabilityEntryRemovalConfirmation.Confirmed),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Status != AvailabilityEntryCommandStatus.Succeeded)
            {
                ApplyCommandErrors(result.Errors);
                return;
            }

            await LoadPeriodAsync(
                (cell.EmployeeId, cell.Date),
                "Der Tageseintrag wurde entfernt.",
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "RemoveAvailabilityEntry");
            ErrorMessage =
                "Der Tageseintrag konnte nicht entfernt werden. Bitte versuche es erneut.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task LoadPeriodAsync(
        (Guid EmployeeId, DateOnly Date)? selection,
        string? successMessage,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        SetPendingAction(null);
        ClearSnapshot();

        try
        {
            DateOnly selectedDate = DateOnly.FromDateTime(SelectedPeriodDate);
            AvailabilityPeriodQueryResult result = await _query.ExecuteAsync(
                selectedDate,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!result.IsSuccess)
            {
                ErrorMessage = CreateErrorMessage(result.Errors.Select(error => error.Message));
                return;
            }

            ApplySnapshot(
                result.Value
                ?? throw new InvalidOperationException(
                    "A successful availability query returned no period snapshot."),
                selection);
            SuccessMessage = successMessage;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadAvailabilityPeriod");
            ErrorMessage =
                "Der Drei-Wochen-Zeitraum konnte nicht geladen werden. Bitte versuche es erneut.";
        }
        finally
        {
            IsBusy = false;
            NotifyViewStateChanged();
        }
    }

    private void ApplySnapshot(
        AvailabilityPeriodSnapshot snapshot,
        (Guid EmployeeId, DateOnly Date)? selection)
    {
        if (snapshot.Days.Count != 21
            || snapshot.PeriodMonday != DateOnly.FromDateTime(SelectedPeriodDate))
        {
            throw new InvalidOperationException(
                "The availability snapshot does not match the requested three-week period.");
        }

        foreach (AvailabilityPeriodDaySnapshot day in snapshot.Days)
        {
            Days.Add(new AvailabilityDayHeaderViewModel(day.Date));
        }

        foreach (AvailabilityPeriodEmployeeSnapshot employee in snapshot.Employees)
        {
            Dictionary<DateOnly, AvailabilityPeriodEntrySnapshot> entries = employee.Entries
                .ToDictionary(entry => entry.Date);
            AvailabilityCellViewModel[] cells = snapshot.Days
                .Select(day =>
                {
                    entries.TryGetValue(day.Date, out AvailabilityPeriodEntrySnapshot? entry);
                    return new AvailabilityCellViewModel(
                        employee.EmployeeId,
                        employee.DisplayName,
                        day.Date,
                        employee.AllowsVacationAndSickness,
                        entry?.Kind,
                        entry?.ChangeVersion);
                })
                .ToArray();
            AvailabilityWeekSummaryViewModel[] weeks = employee.Weeks
                .Select((week, index) => new AvailabilityWeekSummaryViewModel(index + 1, week))
                .ToArray();
            Employees.Add(new AvailabilityEmployeeRowViewModel(
                employee.EmployeeId,
                employee.DisplayName,
                employee.EmployeeTypeCode,
                employee.EmployeeTypeName,
                employee.AllowsVacationAndSickness,
                cells,
                weeks));
        }

        if (selection is not null)
        {
            SelectedCell = Employees
                .Where(employee => employee.EmployeeId == selection.Value.EmployeeId)
                .SelectMany(employee => employee.Cells)
                .SingleOrDefault(cell => cell.Date == selection.Value.Date);
        }

        NotifyViewStateChanged();
    }

    private void ClearSnapshot()
    {
        SelectedCell = null;
        Days.Clear();
        Employees.Clear();
        NotifyViewStateChanged();
    }

    private void ApplyCommandErrors(IEnumerable<AvailabilityEntryCommandError> errors)
    {
        ErrorMessage = CreateErrorMessage(errors.Select(error => error.Message));
    }

    private void ClearFeedback()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private void SetPendingAction(PendingAvailabilityAction? action)
    {
        _pendingAction = action;
        OnPropertyChanged(nameof(IsConfirmationOpen));
        OnPropertyChanged(nameof(ConfirmationMessage));
        NotifyCommandsChanged();
    }

    private void NotifyViewStateChanged()
    {
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(HasEmployees));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private void NotifyCommandsChanged()
    {
        LoadCommand.NotifyCanExecuteChanged();
        PreviousPeriodCommand.NotifyCanExecuteChanged();
        NextPeriodCommand.NotifyCanExecuteChanged();
        SetVacationCommand.NotifyCanExecuteChanged();
        SetSicknessCommand.NotifyCanExecuteChanged();
        SetFixedDayOffCommand.NotifyCanExecuteChanged();
        RequestRemovalCommand.NotifyCanExecuteChanged();
        ConfirmPendingActionCommand.NotifyCanExecuteChanged();
        CancelPendingActionCommand.NotifyCanExecuteChanged();
    }

    private static string CreateErrorMessage(IEnumerable<string> messages)
    {
        string message = string.Join(" ", messages.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(message)
            ? "Der Vorgang konnte nicht abgeschlossen werden."
            : message;
    }

    private static string GetMeaning(AvailabilityDayEntryKind kind)
    {
        return kind switch
        {
            AvailabilityDayEntryKind.Vacation => "Urlaub",
            AvailabilityDayEntryKind.Sickness => "Krankheit",
            AvailabilityDayEntryKind.FixedDayOff => "fest vorgegebenes Frei",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }

    private sealed record PendingAvailabilityAction(
        AvailabilityDayEntryKind? Kind,
        bool IsRemoval,
        string Message);
}
