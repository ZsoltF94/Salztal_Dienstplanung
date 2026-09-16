using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ScheduleOverviewViewModel : ObservableObject
{
    private readonly OpenOrCreateScheduleDraftCommand _openCommand;
    private readonly GetScheduleWorkspaceQuery _query;
    private readonly SetServiceManagementAssignmentCommand _setAssignmentCommand;
    private readonly RemoveServiceManagementAssignmentCommand _removeAssignmentCommand;
    private readonly ChangeScheduleDayEntryCommand _changeDayEntryCommand;
    private readonly RemoveScheduleDayEntryCommand _removeDayEntryCommand;
    private readonly PreparePlanningInputCommand _prepareCommand;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private DateTime _selectedPeriodDate;
    private ScheduleCellViewModel? _selectedCell;
    private PendingScheduleAction? _pendingAction;
    private ScheduleWorkspaceSnapshot? _snapshot;
    private bool _canPrepare;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;
    private string _readinessDisplay = string.Empty;

    public ScheduleOverviewViewModel(
        OpenOrCreateScheduleDraftCommand openCommand,
        GetScheduleWorkspaceQuery query,
        SetServiceManagementAssignmentCommand setAssignmentCommand,
        RemoveServiceManagementAssignmentCommand removeAssignmentCommand,
        ChangeScheduleDayEntryCommand changeDayEntryCommand,
        RemoveScheduleDayEntryCommand removeDayEntryCommand,
        PreparePlanningInputCommand prepareCommand,
        DateOnly initialPeriodMonday,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(openCommand);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(setAssignmentCommand);
        ArgumentNullException.ThrowIfNull(removeAssignmentCommand);
        ArgumentNullException.ThrowIfNull(changeDayEntryCommand);
        ArgumentNullException.ThrowIfNull(removeDayEntryCommand);
        ArgumentNullException.ThrowIfNull(prepareCommand);
        ArgumentNullException.ThrowIfNull(errorReporter);
        if (initialPeriodMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "The initial schedule period must start on a Monday.",
                nameof(initialPeriodMonday));
        }

        _openCommand = openCommand;
        _query = query;
        _setAssignmentCommand = setAssignmentCommand;
        _removeAssignmentCommand = removeAssignmentCommand;
        _changeDayEntryCommand = changeDayEntryCommand;
        _removeDayEntryCommand = removeDayEntryCommand;
        _prepareCommand = prepareCommand;
        _errorReporter = errorReporter;
        _selectedPeriodDate = initialPeriodMonday.ToDateTime(TimeOnly.MinValue);
        Days = [];
        Employees = [];
        Typ1Editor = new ServiceManagementAssignmentEditorViewModel();
        Preparation = new PlanningPreparationViewModel();
        Typ1Editor.SelectionChanged += NotifyCommandsChanged;
        Preparation.PropertyChanged += (_, _) => NotifyCommandsChanged();
        LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
        PreviousPeriodCommand = new AsyncRelayCommand(
            cancellationToken => ChangePeriodAsync(-21, cancellationToken),
            CanLoad);
        NextPeriodCommand = new AsyncRelayCommand(
            cancellationToken => ChangePeriodAsync(21, cancellationToken),
            CanLoad);
        SelectCellCommand = new RelayCommand<ScheduleCellViewModel>(SelectCell);
        SetVacationCommand = CreateDayEntryCommand(AvailabilityDayEntryKind.Vacation);
        SetSicknessCommand = CreateDayEntryCommand(AvailabilityDayEntryKind.Sickness);
        SetFixedDayOffCommand = CreateDayEntryCommand(AvailabilityDayEntryKind.FixedDayOff);
        SetTyp1AssignmentCommand = new AsyncRelayCommand(
            RequestOrSetAssignmentAsync,
            CanSetAssignment);
        RequestRemovalCommand = new RelayCommand(RequestRemoval, CanRemove);
        PreparePlanningCommand = new AsyncRelayCommand(
            PreparePlanningAsync,
            CanPreparePlanning);
        ConfirmPendingActionCommand = new AsyncRelayCommand(
            ConfirmPendingActionAsync,
            () => IsConfirmationOpen && !IsBusy);
        CancelPendingActionCommand = new RelayCommand(
            CancelPendingAction,
            () => IsConfirmationOpen && !IsBusy);
    }

    public ObservableCollection<ScheduleDayHeaderViewModel> Days { get; }

    public ObservableCollection<ScheduleEmployeeRowViewModel> Employees { get; }

    public ServiceManagementAssignmentEditorViewModel Typ1Editor { get; }

    public PlanningPreparationViewModel Preparation { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand PreviousPeriodCommand { get; }

    public IAsyncRelayCommand NextPeriodCommand { get; }

    public IRelayCommand<ScheduleCellViewModel> SelectCellCommand { get; }

    public IAsyncRelayCommand SetVacationCommand { get; }

    public IAsyncRelayCommand SetSicknessCommand { get; }

    public IAsyncRelayCommand SetFixedDayOffCommand { get; }

    public IAsyncRelayCommand SetTyp1AssignmentCommand { get; }

    public IRelayCommand RequestRemovalCommand { get; }

    public IAsyncRelayCommand PreparePlanningCommand { get; }

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

    public ScheduleCellViewModel? SelectedCell
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

            Typ1Editor.ApplyCell(value);
            OnPropertyChanged();
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

    public string SelectedCellDisplay => SelectedCell is null
        ? "Wähle ein Tagesfeld aus."
        : $"{SelectedCell.EmployeeName} – {SelectedCell.DateDisplay}";

    public string SelectedCellEntryDisplay => SelectedCell is null
        ? string.Empty
        : $"Aktueller Wert: {SelectedCell.EntryMeaning}";

    public string ReadinessDisplay
    {
        get => _readinessDisplay;
        private set => SetProperty(ref _readinessDisplay, value);
    }

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

    private AsyncRelayCommand CreateDayEntryCommand(AvailabilityDayEntryKind kind)
    {
        return new AsyncRelayCommand(
            cancellationToken => RequestOrSetDayEntryAsync(kind, cancellationToken),
            () => CanSetDayEntry(kind));
    }

    private bool CanLoad()
    {
        return !IsBusy && !IsConfirmationOpen;
    }

    private bool CanSetDayEntry(AvailabilityDayEntryKind kind)
    {
        return SelectedCell is not null
            && _snapshot is not null
            && !IsBusy
            && !IsConfirmationOpen
            && (kind == AvailabilityDayEntryKind.FixedDayOff
                || SelectedCell.AllowsVacationAndSickness);
    }

    private bool CanSetAssignment()
    {
        return SelectedCell?.IsServiceManagement == true
            && Typ1Editor.SelectedOption is not null
            && _snapshot is not null
            && !IsBusy
            && !IsConfirmationOpen;
    }

    private bool CanRemove()
    {
        return (SelectedCell is { HasEntry: true } or { HasAssignment: true })
            && !IsBusy
            && !IsConfirmationOpen;
    }

    private bool CanPreparePlanning()
    {
        return _snapshot is not null
            && _canPrepare
            && !IsBusy
            && !IsConfirmationOpen
            && (_snapshot.PreparationStatus != SchedulePreparationStatus.Prepared
                || Preparation.HasLocalRunOptionChange);
    }

    private async Task ChangePeriodAsync(int days, CancellationToken cancellationToken)
    {
        SelectedPeriodDate = SelectedPeriodDate.AddDays(days);
        await LoadAsync(cancellationToken);
    }

    private void SelectCell(ScheduleCellViewModel? cell)
    {
        if (cell is null || IsBusy || IsConfirmationOpen)
        {
            return;
        }

        ClearFeedback();
        SelectedCell = cell;
    }

    private async Task RequestOrSetDayEntryAsync(
        AvailabilityDayEntryKind kind,
        CancellationToken cancellationToken)
    {
        ScheduleCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No schedule cell is selected.");
        ClearFeedback();
        if (cell.EntryKind == kind)
        {
            SuccessMessage = "Der ausgewählte Tageswert ist bereits eingetragen.";
            return;
        }

        if (cell.HasEntry || cell.HasAssignment)
        {
            SetPendingAction(new PendingScheduleAction(
                PendingScheduleActionKind.SetDayEntry,
                kind,
                null,
                $"Soll bei {cell.EmployeeName} am {cell.Date:dd.MM.yyyy} "
                + $"„{cell.EntryMeaning}“ durch „{GetMeaning(kind)}“ ersetzt werden?"));
            return;
        }

        await ExecuteDayEntryAsync(
            cell,
            kind,
            ScheduleReplacementConfirmation.NotConfirmed,
            cancellationToken);
    }

    private async Task RequestOrSetAssignmentAsync(CancellationToken cancellationToken)
    {
        ScheduleCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No schedule cell is selected.");
        ServiceManagementAssignmentOptionViewModel option = Typ1Editor.SelectedOption
            ?? throw new InvalidOperationException("No service-management option is selected.");
        ClearFeedback();
        if (cell.HasEntry)
        {
            SetPendingAction(new PendingScheduleAction(
                PendingScheduleActionKind.SetAssignment,
                null,
                option,
                $"Soll bei {cell.EmployeeName} am {cell.Date:dd.MM.yyyy} "
                + $"„{cell.EntryMeaning}“ entfernt und „{option.Display}“ eingetragen werden?"));
            return;
        }

        await ExecuteAssignmentAsync(
            cell,
            option,
            ScheduleReplacementConfirmation.NotConfirmed,
            cancellationToken);
    }

    private void RequestRemoval()
    {
        ScheduleCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No schedule cell is selected.");
        PendingScheduleActionKind kind = cell.HasAssignment
            ? PendingScheduleActionKind.RemoveAssignment
            : PendingScheduleActionKind.RemoveDayEntry;
        ClearFeedback();
        SetPendingAction(new PendingScheduleAction(
            kind,
            null,
            null,
            $"Soll bei {cell.EmployeeName} am {cell.Date:dd.MM.yyyy} "
            + $"„{cell.EntryMeaning}“ wirklich entfernt werden?"));
    }

    private async Task ConfirmPendingActionAsync(CancellationToken cancellationToken)
    {
        ScheduleCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No schedule cell is selected.");
        PendingScheduleAction action = _pendingAction
            ?? throw new InvalidOperationException("No schedule action is pending.");
        SetPendingAction(null);
        switch (action.Kind)
        {
            case PendingScheduleActionKind.SetDayEntry:
                await ExecuteDayEntryAsync(
                    cell,
                    action.DayEntryKind!.Value,
                    ScheduleReplacementConfirmation.Confirmed,
                    cancellationToken);
                break;
            case PendingScheduleActionKind.SetAssignment:
                await ExecuteAssignmentAsync(
                    cell,
                    action.AssignmentOption!,
                    ScheduleReplacementConfirmation.Confirmed,
                    cancellationToken);
                break;
            case PendingScheduleActionKind.RemoveDayEntry:
                await ExecuteDayEntryRemovalAsync(cell, cancellationToken);
                break;
            case PendingScheduleActionKind.RemoveAssignment:
                await ExecuteAssignmentRemovalAsync(cell, cancellationToken);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported pending schedule action: {action.Kind}");
        }
    }

    private void CancelPendingAction()
    {
        SetPendingAction(null);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ExecuteDayEntryAsync(
        ScheduleCellViewModel cell,
        AvailabilityDayEntryKind kind,
        ScheduleReplacementConfirmation confirmation,
        CancellationToken cancellationToken)
    {
        await ExecuteChangeAsync(
            async () => await _changeDayEntryCommand.ExecuteAsync(
                new ChangeScheduleDayEntryRequest(
                    _snapshot!.DraftId,
                    _snapshot.Version,
                    _snapshot.PeriodMonday,
                    cell.EmployeeId,
                    cell.Date,
                    kind,
                    cell.ChangeVersion,
                    confirmation),
                cancellationToken),
            cell,
            "Der Tageseintrag wurde gespeichert.",
            "ChangeScheduleDayEntry",
            cancellationToken);
    }

    private async Task ExecuteDayEntryRemovalAsync(
        ScheduleCellViewModel cell,
        CancellationToken cancellationToken)
    {
        await ExecuteChangeAsync(
            async () => await _removeDayEntryCommand.ExecuteAsync(
                new RemoveScheduleDayEntryRequest(
                    _snapshot!.DraftId,
                    _snapshot.Version,
                    _snapshot.PeriodMonday,
                    cell.EmployeeId,
                    cell.Date,
                    cell.ChangeVersion
                        ?? throw new InvalidOperationException(
                            "The selected day entry has no change version."),
                    ScheduleReplacementConfirmation.Confirmed),
                cancellationToken),
            cell,
            "Der Tageseintrag wurde entfernt.",
            "RemoveScheduleDayEntry",
            cancellationToken);
    }

    private async Task ExecuteAssignmentAsync(
        ScheduleCellViewModel cell,
        ServiceManagementAssignmentOptionViewModel option,
        ScheduleReplacementConfirmation confirmation,
        CancellationToken cancellationToken)
    {
        ServiceManagementAssignmentOptionSnapshot value = option.Snapshot;
        await ExecuteChangeAsync(
            async () => await _setAssignmentCommand.ExecuteAsync(
                new SetServiceManagementAssignmentRequest(
                    _snapshot!.DraftId,
                    _snapshot.Version,
                    _snapshot.PeriodMonday,
                    cell.EmployeeId,
                    value.Kind,
                    CreateSelection(value.FirstSlot),
                    value.SecondSlot is null ? null : CreateSelection(value.SecondSlot),
                    cell.ChangeVersion,
                    confirmation),
                cancellationToken),
            cell,
            "Der Typ1-Dienst wurde gespeichert.",
            "SetServiceManagementAssignment",
            cancellationToken);
    }

    private async Task ExecuteAssignmentRemovalAsync(
        ScheduleCellViewModel cell,
        CancellationToken cancellationToken)
    {
        await ExecuteChangeAsync(
            async () => await _removeAssignmentCommand.ExecuteAsync(
                new RemoveServiceManagementAssignmentRequest(
                    _snapshot!.DraftId,
                    _snapshot.Version,
                    _snapshot.PeriodMonday,
                    cell.Assignment?.AssignmentId
                        ?? throw new InvalidOperationException(
                            "The selected cell has no assignment.")),
                cancellationToken),
            cell,
            "Der Typ1-Dienst wurde entfernt.",
            "RemoveServiceManagementAssignment",
            cancellationToken);
    }

    private async Task ExecuteChangeAsync(
        Func<Task<ScheduleDayChangeResult>> action,
        ScheduleCellViewModel cell,
        string successMessage,
        string operation,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            ScheduleDayChangeResult result = await action();
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Status != ScheduleDayChangeStatus.Succeeded)
            {
                ErrorMessage = CreateErrorMessage(result.Errors.Select(error => error.Message));
                return;
            }

            await LoadWorkspaceAsync(
                (cell.EmployeeId, cell.Date),
                successMessage,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, operation);
            ErrorMessage = "Die Änderung konnte nicht gespeichert werden. Bitte versuche es erneut.";
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
    private async Task PreparePlanningAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ClearFeedback();
        try
        {
            ScheduleWorkspaceSnapshot snapshot = _snapshot
                ?? throw new InvalidOperationException("No schedule workspace is loaded.");
            PreparePlanningInputResult result = await _prepareCommand.ExecuteAsync(
                new PreparePlanningInputRequest(
                    snapshot.DraftId,
                    snapshot.Version,
                    snapshot.PeriodMonday,
                    snapshot.PreparedSnapshotId is null
                        ? PlanningPreparationExpectation.NotPrepared
                        : PlanningPreparationExpectation.Prepared,
                    snapshot.PreparedSnapshotId,
                    Preparation.CreateRunOptions()),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Status != PreparePlanningInputStatus.Succeeded)
            {
                ErrorMessage = CreateErrorMessage(result.Errors.Select(error => error.Message));
                return;
            }

            await LoadWorkspaceAsync(
                SelectedCell is null ? null : (SelectedCell.EmployeeId, SelectedCell.Date),
                snapshot.PreparedSnapshotId is null
                    ? "Die Planung wurde vorbereitet."
                    : "Die Planungsvorbereitung wurde bewusst aktualisiert.",
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "PreparePlanningInput");
            ErrorMessage =
                "Die Planung konnte nicht vorbereitet werden. Bitte versuche es erneut.";
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
            OpenOrCreateScheduleDraftResult openResult = await _openCommand.ExecuteAsync(
                selectedDate,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (openResult.Status != OpenOrCreateScheduleDraftStatus.Succeeded)
            {
                ErrorMessage = CreateErrorMessage(openResult.Errors.Select(error => error.Message));
                return;
            }

            string? openMessage = openResult.Value!.Outcome == ScheduleDraftOpenOutcome.Created
                ? "Ein neuer Drei-Wochen-Entwurf wurde angelegt."
                : successMessage;
            await LoadWorkspaceAsync(selection, openMessage, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadScheduleWorkspace");
            ErrorMessage =
                "Der Drei-Wochen-Zeitraum konnte nicht geladen werden. Bitte versuche es erneut.";
        }
        finally
        {
            IsBusy = false;
            NotifyViewStateChanged();
        }
    }

    private async Task LoadWorkspaceAsync(
        (Guid EmployeeId, DateOnly Date)? selection,
        string? successMessage,
        CancellationToken cancellationToken)
    {
        ScheduleWorkspaceQueryResult result = await _query.ExecuteAsync(
            DateOnly.FromDateTime(SelectedPeriodDate),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (result.Status != ScheduleWorkspaceQueryStatus.Succeeded)
        {
            ErrorMessage = CreateErrorMessage(result.Errors.Select(error => error.Message));
            return;
        }

        ClearSnapshot();
        ApplySnapshot(
            result.Value
            ?? throw new InvalidOperationException(
                "A successful schedule query returned no workspace snapshot."),
            selection);
        SuccessMessage = successMessage;
    }

    private void ApplySnapshot(
        ScheduleWorkspaceSnapshot snapshot,
        (Guid EmployeeId, DateOnly Date)? selection)
    {
        if (snapshot.Availability.Days.Count != 21
            || snapshot.PeriodMonday != DateOnly.FromDateTime(SelectedPeriodDate))
        {
            throw new InvalidOperationException(
                "The schedule snapshot does not match the requested three-week period.");
        }

        _snapshot = snapshot;
        HashSet<Guid> serviceManagementIds = snapshot.ServiceManagementReadiness
            .Select(item => item.EmployeeId)
            .ToHashSet();
        Dictionary<(Guid EmployeeId, DateOnly Date), ScheduleAssignmentSnapshot>
            assignments = snapshot.Assignments.ToDictionary(item =>
                (item.EmployeeId, item.Date));
        Dictionary<(Guid EmployeeId, DateOnly Date),
            ServiceManagementAssignmentOptionViewModel[]> options = snapshot.AssignmentOptions
                .GroupBy(item => (item.EmployeeId, item.FirstSlot.Date))
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item =>
                        new ServiceManagementAssignmentOptionViewModel(item)).ToArray());

        foreach (AvailabilityPeriodDaySnapshot day in snapshot.Availability.Days)
        {
            Days.Add(new ScheduleDayHeaderViewModel(day.Date));
        }

        foreach (AvailabilityPeriodEmployeeSnapshot employee in snapshot.Availability.Employees)
        {
            Dictionary<DateOnly, AvailabilityPeriodEntrySnapshot> entries = employee.Entries
                .ToDictionary(entry => entry.Date);
            bool isServiceManagement = serviceManagementIds.Contains(employee.EmployeeId);
            ScheduleCellViewModel[] cells = snapshot.Availability.Days.Select(day =>
            {
                entries.TryGetValue(day.Date, out AvailabilityPeriodEntrySnapshot? entry);
                assignments.TryGetValue(
                    (employee.EmployeeId, day.Date),
                    out ScheduleAssignmentSnapshot? assignment);
                options.TryGetValue(
                    (employee.EmployeeId, day.Date),
                    out ServiceManagementAssignmentOptionViewModel[]? cellOptions);
                return new ScheduleCellViewModel(
                    employee.EmployeeId,
                    employee.DisplayName,
                    day.Date,
                    employee.AllowsVacationAndSickness,
                    isServiceManagement,
                    entry?.Kind,
                    entry?.ChangeVersion,
                    assignment,
                    cellOptions ?? []);
            }).ToArray();
            ScheduleWeekSummaryViewModel[] weeks = employee.Weeks
                .Select((week, index) => new ScheduleWeekSummaryViewModel(index + 1, week))
                .ToArray();
            Employees.Add(new ScheduleEmployeeRowViewModel(
                employee.EmployeeId,
                employee.DisplayName,
                employee.EmployeeTypeCode,
                employee.EmployeeTypeName,
                employee.AllowsVacationAndSickness,
                isServiceManagement,
                cells,
                weeks));
        }

        Preparation.Apply(snapshot);
        _canPrepare = snapshot.ServiceManagementReadiness.All(item => item.CanPrepare);
        ReadinessDisplay = CreateReadinessDisplay(snapshot.ServiceManagementReadiness);
        if (selection is not null)
        {
            SelectedCell = Employees
                .Where(employee => employee.EmployeeId == selection.Value.EmployeeId)
                .SelectMany(employee => employee.Cells)
                .SingleOrDefault(cell => cell.Date == selection.Value.Date);
        }

        NotifyViewStateChanged();
        NotifyCommandsChanged();
    }

    private void ClearSnapshot()
    {
        _snapshot = null;
        _canPrepare = false;
        SelectedCell = null;
        Days.Clear();
        Employees.Clear();
        ReadinessDisplay = string.Empty;
        NotifyViewStateChanged();
    }

    private void ClearFeedback()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private void SetPendingAction(PendingScheduleAction? action)
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
        SetTyp1AssignmentCommand.NotifyCanExecuteChanged();
        RequestRemovalCommand.NotifyCanExecuteChanged();
        PreparePlanningCommand.NotifyCanExecuteChanged();
        ConfirmPendingActionCommand.NotifyCanExecuteChanged();
        CancelPendingActionCommand.NotifyCanExecuteChanged();
    }

    private static ScheduleDemandSlotSelection CreateSelection(
        ScheduleDemandSlotSnapshot slot)
    {
        return new ScheduleDemandSlotSelection(
            slot.SourceId,
            slot.SourceKind,
            slot.Date,
            slot.WorkLocationId,
            slot.ShiftTypeId,
            slot.Ordinal,
            slot.ActualStart,
            slot.ActualEnd);
    }

    private static string CreateReadinessDisplay(
        IEnumerable<ServiceManagementReadinessSnapshot> readiness)
    {
        ServiceManagementReadinessSnapshot? item = readiness.SingleOrDefault();
        if (item is null)
        {
            return "Keine aktive Typ1-Person vorhanden.";
        }

        return string.Join(
            " · ",
            item.Weeks.Select((week, index) =>
                $"Woche {index + 1}: {week.Status switch
                {
                    ServiceManagementWeekReadinessStatusSnapshot.Ready => "Typ1 vorhanden",
                    ServiceManagementWeekReadinessStatusSnapshot.ExemptFullyUnavailable =>
                        "vollständig abwesend",
                    ServiceManagementWeekReadinessStatusSnapshot.MissingAssignment =>
                        "Typ1 fehlt",
                    _ => "unbekannt",
                }}"));
    }

    private static string CreateErrorMessage(IEnumerable<string> messages)
    {
        string message = string.Join(
            " ",
            messages.Where(value => !string.IsNullOrWhiteSpace(value)));
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

    private enum PendingScheduleActionKind
    {
        SetDayEntry,
        SetAssignment,
        RemoveDayEntry,
        RemoveAssignment,
    }

    private sealed record PendingScheduleAction(
        PendingScheduleActionKind Kind,
        AvailabilityDayEntryKind? DayEntryKind,
        ServiceManagementAssignmentOptionViewModel? AssignmentOption,
        string Message);
}
