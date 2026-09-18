using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class ScheduleOverviewViewModel : ObservableObject
{
    private static readonly TimeSpan SuccessMessageReadingTime = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan SuccessMessageFadeTime = TimeSpan.FromMilliseconds(350);
    private readonly OpenOrCreateScheduleDraftCommand _openCommand;
    private readonly GetScheduleWorkspaceQuery _query;
    private readonly SetServiceManagementAssignmentCommand _setAssignmentCommand;
    private readonly RemoveServiceManagementAssignmentCommand _removeAssignmentCommand;
    private readonly ChangeScheduleDayEntryCommand _changeDayEntryCommand;
    private readonly RemoveScheduleDayEntryCommand _removeDayEntryCommand;
    private readonly PreparePlanningInputCommand _prepareCommand;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private readonly IScheduleFeedbackDelay _feedbackDelay;
    private CancellationTokenSource? _feedbackCancellation;
    private long _feedbackSequence;
    private DateTime _selectedPeriodDate;
    private ScheduleCellViewModel? _selectedCell;
    private PendingScheduleAction? _pendingAction;
    private ScheduleWorkspaceSnapshot? _snapshot;
    private bool _canPrepare;
    private bool _isBusy;
    private bool _isSuccessMessageFading;
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
        IAutomaticScheduleGenerationActions generationActions,
        IAutomaticScheduleResetActions resetActions,
        DateOnly initialPeriodMonday,
        IUnexpectedErrorReporter errorReporter,
        IScheduleFeedbackDelay feedbackDelay,
        IAutomaticScheduleReportPresenter? reportPresenter = null)
    {
        ArgumentNullException.ThrowIfNull(openCommand);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(setAssignmentCommand);
        ArgumentNullException.ThrowIfNull(removeAssignmentCommand);
        ArgumentNullException.ThrowIfNull(changeDayEntryCommand);
        ArgumentNullException.ThrowIfNull(removeDayEntryCommand);
        ArgumentNullException.ThrowIfNull(prepareCommand);
        ArgumentNullException.ThrowIfNull(generationActions);
        ArgumentNullException.ThrowIfNull(resetActions);
        ArgumentNullException.ThrowIfNull(errorReporter);
        ArgumentNullException.ThrowIfNull(feedbackDelay);
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
        _feedbackDelay = feedbackDelay;
        _selectedPeriodDate = initialPeriodMonday.ToDateTime(TimeOnly.MinValue);
        Days = [];
        Employees = [];
        Preparation = new PlanningPreparationViewModel();
        Generation = new AutomaticScheduleGenerationViewModel(
            generationActions,
            ReloadAfterAutomaticAcceptanceAsync,
            errorReporter,
            reportPresenter);
        AutomaticReset = new AutomaticScheduleResetViewModel(
            resetActions,
            ReloadAfterAutomaticDiscardAsync,
            errorReporter);
        Preparation.PropertyChanged += (_, _) =>
        {
            Generation.UpdateParentState(Preparation.HasLocalRunOptionChange, IsBusy);
            NotifyCommandsChanged();
        };
        Generation.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(Generation.IsOperationActive)
                or nameof(Generation.HasPreview))
            {
                if (Generation.IsOperationActive || Generation.HasPreview)
                {
                    CloseAssignmentEditor();
                }

                OnPropertyChanged(nameof(CanEditSchedule));
                AutomaticReset.UpdateParentState(
                    IsBusy || Generation.IsOperationActive || Generation.HasPreview);
                NotifyCommandsChanged();
            }
        };
        AutomaticReset.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(AutomaticReset.IsInteractionActive))
            {
                if (AutomaticReset.IsInteractionActive)
                {
                    CloseAssignmentEditor();
                }

                OnPropertyChanged(nameof(CanEditSchedule));
                Generation.UpdateParentState(
                    Preparation.HasLocalRunOptionChange,
                    IsBusy || AutomaticReset.IsInteractionActive);
                NotifyCommandsChanged();
            }

            if (args.PropertyName is nameof(AutomaticReset.IsConfirmationOpen)
                or nameof(AutomaticReset.ConfirmationMessage))
            {
                NotifyConfirmationStateChanged();
            }
        };
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
        SetTyp1AssignmentCommand = new AsyncRelayCommand<ServiceManagementAssignmentOptionViewModel>(
            RequestOrSetAssignmentAsync,
            CanSetAssignment);
        RequestRemovalCommand = new RelayCommand(RequestRemoval, CanRemove);
        PreparePlanningCommand = new AsyncRelayCommand(
            PreparePlanningAsync,
            CanPreparePlanning);
        ConfirmPendingActionCommand = new AsyncRelayCommand(
            ConfirmPendingActionAsync,
            CanDecidePendingAction);
        CancelPendingActionCommand = new RelayCommand(
            CancelPendingAction,
            CanDecidePendingAction);
        ConfirmActiveConfirmationCommand = new AsyncRelayCommand(
            ConfirmActiveConfirmationAsync,
            CanConfirmActiveConfirmation);
        CancelActiveConfirmationCommand = new RelayCommand(
            CancelActiveConfirmation,
            CanCancelActiveConfirmation);
    }

    public ObservableCollection<ScheduleDayHeaderViewModel> Days { get; }

    public ObservableCollection<ScheduleEmployeeRowViewModel> Employees { get; }

    public PlanningPreparationViewModel Preparation { get; }

    public AutomaticScheduleGenerationViewModel Generation { get; }

    public AutomaticScheduleResetViewModel AutomaticReset { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand PreviousPeriodCommand { get; }

    public IAsyncRelayCommand NextPeriodCommand { get; }

    public IRelayCommand<ScheduleCellViewModel> SelectCellCommand { get; }

    public IAsyncRelayCommand SetVacationCommand { get; }

    public IAsyncRelayCommand SetSicknessCommand { get; }

    public IAsyncRelayCommand SetFixedDayOffCommand { get; }

    public IAsyncRelayCommand<ServiceManagementAssignmentOptionViewModel>
        SetTyp1AssignmentCommand
    { get; }

    public IRelayCommand RequestRemovalCommand { get; }

    public IAsyncRelayCommand PreparePlanningCommand { get; }

    public IAsyncRelayCommand ConfirmPendingActionCommand { get; }

    public IRelayCommand CancelPendingActionCommand { get; }

    public IAsyncRelayCommand ConfirmActiveConfirmationCommand { get; }

    public IRelayCommand CancelActiveConfirmationCommand { get; }

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
                _selectedCell.IsAssignmentEditorOpen = false;
                _selectedCell.IsSelected = false;
            }

            _selectedCell = value;
            if (_selectedCell is not null)
            {
                _selectedCell.IsSelected = true;
            }

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
                if (value && SelectedCell is not null)
                {
                    SelectedCell.IsAssignmentEditorOpen = false;
                }

                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(CanEditSchedule));
                Generation.UpdateParentState(
                    Preparation.HasLocalRunOptionChange,
                    value || AutomaticReset.IsInteractionActive);
                AutomaticReset.UpdateParentState(
                    value || Generation.IsOperationActive || Generation.HasPreview);
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
                NotifyFeedbackStateChanged();
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
                NotifyFeedbackStateChanged();
            }
        }
    }

    public bool IsSuccessMessageFading
    {
        get => _isSuccessMessageFading;
        private set => SetProperty(ref _isSuccessMessageFading, value);
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

    public bool IsConfirmationOpen =>
        _pendingAction is not null || AutomaticReset.IsConfirmationOpen;

    public string? ConfirmationMessage => _pendingAction?.Message
        ?? (AutomaticReset.IsConfirmationOpen
            ? AutomaticReset.ConfirmationMessage
            : null);

    public string ConfirmationTitle => AutomaticReset.IsConfirmationOpen
        ? "Automatischen Plan wirklich vollständig verwerfen?"
        : "Änderung am Tagesfeld bestätigen?";

    public string ConfirmationConfirmText => AutomaticReset.IsConfirmationOpen
        ? "Vollständig verwerfen"
        : "Bestätigen";

    public string ConfirmationConfirmAutomationName => AutomaticReset.IsConfirmationOpen
        ? "Vollständiges Verwerfen bestätigen"
        : "Änderung am Tagesfeld bestätigen";

    public string ConfirmationAutomationName => AutomaticReset.IsConfirmationOpen
        ? "Modale Bestätigung zum vollständigen Verwerfen des automatischen Plans"
        : "Modale Bestätigung für eine Änderung am Tagesfeld";

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool HasFeedback => HasError || HasSuccessMessage;

    public string? FeedbackMessage => ErrorMessage ?? SuccessMessage;

    public string FeedbackStatusDisplay => HasError ? "Fehler" : "Hinweis";

    public string FeedbackAutomationName => $"Dienstplanmeldung {FeedbackStatusDisplay}";

    public bool HasEmployees => Employees.Count > 0;

    public bool IsEmpty => !IsBusy && !HasError && !HasEmployees;

    public bool CanEditSchedule => !IsInteractionLocked;

    private bool IsInteractionLocked =>
        _pendingAction is not null
        || IsBusy
        || Generation.IsOperationActive
        || Generation.HasPreview
        || AutomaticReset.IsInteractionActive;

    private bool IsBackgroundOperationActive =>
        IsBusy
        || Generation.IsOperationActive
        || Generation.HasPreview
        || AutomaticReset.IsInteractionActive;

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
        return !IsInteractionLocked && !IsConfirmationOpen;
    }

    private bool CanSetDayEntry(AvailabilityDayEntryKind kind)
    {
        return SelectedCell is not null
            && _snapshot is not null
            && !IsInteractionLocked
            && !IsConfirmationOpen
            && !SelectedCell.IsGeneratedDayOff
            && SelectedCell.Assignment?.Origin
                != ScheduleAssignmentOriginSnapshot.AutomaticGeneration
            && (kind == AvailabilityDayEntryKind.FixedDayOff
                || SelectedCell.AllowsVacationAndSickness);
    }

    private bool CanSetAssignment(ServiceManagementAssignmentOptionViewModel? option)
    {
        return option is not null
            && SelectedCell?.CanOpenAssignmentEditor == true
            && SelectedCell.AssignmentOptions.Contains(option)
            && _snapshot is not null
            && !IsInteractionLocked
            && !IsConfirmationOpen;
    }

    private bool CanRemove()
    {
        return (SelectedCell is { EntryKind: not null }
                || SelectedCell is
                {
                    HasAssignment: true,
                    Assignment.Origin: not ScheduleAssignmentOriginSnapshot.AutomaticGeneration,
                })
            && !IsInteractionLocked
            && !IsConfirmationOpen;
    }

    private bool CanPreparePlanning()
    {
        return _snapshot is not null
            && _canPrepare
            && !IsInteractionLocked
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
        if (cell is null || IsInteractionLocked || IsConfirmationOpen)
        {
            return;
        }

        ClearFeedback();
        SelectedCell = cell;
        if (cell.CanOpenAssignmentEditor)
        {
            cell.IsAssignmentEditorOpen = true;
        }
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
            ShowSuccessMessage("Der ausgewählte Tageswert ist bereits eingetragen.");
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

    private async Task RequestOrSetAssignmentAsync(
        ServiceManagementAssignmentOptionViewModel? option,
        CancellationToken cancellationToken)
    {
        ScheduleCellViewModel cell = SelectedCell
            ?? throw new InvalidOperationException("No schedule cell is selected.");
        if (option is null)
        {
            throw new InvalidOperationException(
                "No service-management option is selected.");
        }

        cell.IsAssignmentEditorOpen = false;
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

    private bool CanDecidePendingAction() =>
        _pendingAction is not null && !IsBackgroundOperationActive;

    private bool CanConfirmActiveConfirmation()
    {
        if (_pendingAction is not null)
        {
            return ConfirmPendingActionCommand.CanExecute(null);
        }

        return AutomaticReset.IsConfirmationOpen
            && AutomaticReset.ConfirmCommand.CanExecute(null);
    }

    private async Task ConfirmActiveConfirmationAsync(CancellationToken cancellationToken)
    {
        if (_pendingAction is not null)
        {
            await ConfirmPendingActionAsync(cancellationToken);
            return;
        }

        if (AutomaticReset.IsConfirmationOpen)
        {
            await AutomaticReset.ConfirmCommand.ExecuteAsync(null);
            return;
        }

        throw new InvalidOperationException("No schedule confirmation is active.");
    }

    private bool CanCancelActiveConfirmation()
    {
        if (_pendingAction is not null)
        {
            return CancelPendingActionCommand.CanExecute(null);
        }

        return AutomaticReset.IsConfirmationOpen
            && AutomaticReset.CancelCommand.CanExecute(null);
    }

    private void CancelActiveConfirmation()
    {
        if (_pendingAction is not null)
        {
            CancelPendingAction();
            return;
        }

        if (AutomaticReset.IsConfirmationOpen)
        {
            AutomaticReset.CancelCommand.Execute(null);
            return;
        }

        throw new InvalidOperationException("No schedule confirmation is active.");
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
                ShowErrorMessage(CreateErrorMessage(
                    result.Errors.Select(error => error.Message)));
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
            ShowErrorMessage(
                "Die Änderung konnte nicht gespeichert werden. Bitte versuche es erneut.");
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
                ShowErrorMessage(CreateErrorMessage(
                    result.Errors.Select(error => error.Message)));
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
            ShowErrorMessage(
                "Die Planung konnte nicht vorbereitet werden. Bitte versuche es erneut.");
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
        ClearFeedback();
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
                ShowErrorMessage(CreateErrorMessage(
                    openResult.Errors.Select(error => error.Message)));
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
            ShowErrorMessage(
                "Der Drei-Wochen-Zeitraum konnte nicht geladen werden. Bitte versuche es erneut.");
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
            ShowErrorMessage(CreateErrorMessage(
                result.Errors.Select(error => error.Message)));
            return;
        }

        ClearSnapshot();
        ApplySnapshot(
            result.Value
            ?? throw new InvalidOperationException(
                "A successful schedule query returned no workspace snapshot."),
            selection);
        ShowSuccessMessage(successMessage);
    }

    private async Task ReloadAfterAutomaticAcceptanceAsync(
        CancellationToken cancellationToken)
    {
        (Guid EmployeeId, DateOnly Date)? selection = SelectedCell is null
            ? null
            : (SelectedCell.EmployeeId, SelectedCell.Date);
        await LoadWorkspaceAsync(
            selection,
            "Der automatische Vorschlag wurde vollständig übernommen.",
            cancellationToken);
    }

    private async Task ReloadAfterAutomaticDiscardAsync(
        CancellationToken cancellationToken)
    {
        (Guid EmployeeId, DateOnly Date)? selection = SelectedCell is null
            ? null
            : (SelectedCell.EmployeeId, SelectedCell.Date);
        await LoadWorkspaceAsync(
            selection,
            "Der automatische Plan wurde vollst\u00e4ndig verworfen.",
            cancellationToken);
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
        Dictionary<(Guid EmployeeId, DateOnly Date), ScheduleAssignmentSnapshot>
            assignments = snapshot.Assignments.ToDictionary(item =>
                (item.EmployeeId, item.Date));
        HashSet<(Guid EmployeeId, DateOnly Date)> generatedDayOffs = snapshot
            .GeneratedDayOffs
            .Select(item => (item.EmployeeId, item.Date))
            .ToHashSet();
        Dictionary<(Guid EmployeeId, DateOnly Date),
            ServiceManagementAssignmentOptionSnapshot[]> options = snapshot.AssignmentOptions
                .GroupBy(item => (item.EmployeeId, item.FirstSlot.Date))
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray());

        foreach (AvailabilityPeriodDaySnapshot day in snapshot.Availability.Days)
        {
            Days.Add(new ScheduleDayHeaderViewModel(day.Date));
        }

        AvailabilityPeriodEmployeeSnapshot[] orderedEmployees = snapshot
            .Availability
            .Employees
            .OrderBy(employee => GetEmployeeGroupOrder(employee.PlanningRole))
            .ToArray();
        Guid? firstAuxiliaryEmployeeId = orderedEmployees
            .FirstOrDefault(employee => employee.PlanningRole
                == EmployeeTypePlanningRoleKind.Auxiliary)
            ?.EmployeeId;
        foreach (AvailabilityPeriodEmployeeSnapshot employee in orderedEmployees)
        {
            Dictionary<DateOnly, AvailabilityPeriodEntrySnapshot> entries = employee.Entries
                .ToDictionary(entry => entry.Date);
            bool isServiceManagement = employee.PlanningRole
                == EmployeeTypePlanningRoleKind.ServiceManagement;
            ScheduleCellViewModel[] cells = snapshot.Availability.Days.Select(day =>
            {
                entries.TryGetValue(day.Date, out AvailabilityPeriodEntrySnapshot? entry);
                assignments.TryGetValue(
                    (employee.EmployeeId, day.Date),
                    out ScheduleAssignmentSnapshot? assignment);
                options.TryGetValue(
                    (employee.EmployeeId, day.Date),
                    out ServiceManagementAssignmentOptionSnapshot[]? cellOptions);
                ScheduleAssignmentDisplay? assignmentDisplay = assignment is null
                    ? null
                    : ScheduleAssignmentDisplayFormatter.Create(
                        assignment,
                        snapshot.DemandSlots);
                ServiceManagementAssignmentOptionSnapshot? currentOption = assignment is null
                    ? null
                    : cellOptions?.FirstOrDefault(option =>
                        MatchesAssignment(option, assignment));
                return new ScheduleCellViewModel(
                    employee.EmployeeId,
                    employee.DisplayName,
                    day.Date,
                    employee.AllowsVacationAndSickness,
                    isServiceManagement,
                    entry?.Kind,
                    entry?.ChangeVersion,
                    generatedDayOffs.Contains((employee.EmployeeId, day.Date)),
                    assignment,
                    assignmentDisplay,
                    (cellOptions ?? []).Select(option =>
                        new ServiceManagementAssignmentOptionViewModel(
                            option,
                            ReferenceEquals(option, currentOption),
                            SetTyp1AssignmentCommand)));
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
                employee.EmployeeId == firstAuxiliaryEmployeeId,
                cells,
                weeks));
        }

        Preparation.Apply(snapshot);
        _canPrepare = snapshot.ServiceManagementReadiness.All(item => item.CanPrepare);
        ReadinessDisplay = CreateReadinessDisplay(snapshot.ServiceManagementReadiness);
        AutomaticScheduleGenerationContext automaticContext = new(
                snapshot.DraftId,
                snapshot.Version,
                snapshot.PeriodMonday,
                snapshot.PreparedSnapshotId,
                snapshot.PreparationStatus,
                snapshot.ServiceManagementReadiness.All(item => item.CanPrepare),
                snapshot.AcceptedAutomaticSchedule);
        Generation.ApplyContext(
            automaticContext,
            Preparation.HasLocalRunOptionChange,
            IsBusy || AutomaticReset.IsInteractionActive);
        AutomaticReset.ApplyContext(
            automaticContext,
            IsBusy || Generation.IsOperationActive || Generation.HasPreview);
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

    private static int GetEmployeeGroupOrder(EmployeeTypePlanningRoleKind planningRole)
    {
        return planningRole switch
        {
            EmployeeTypePlanningRoleKind.ServiceManagement => 0,
            EmployeeTypePlanningRoleKind.Normal => 1,
            EmployeeTypePlanningRoleKind.Auxiliary => 2,
            _ => throw new InvalidOperationException(
                $"Unsupported employee-type planning role: {planningRole}"),
        };
    }

    private void ClearSnapshot()
    {
        _snapshot = null;
        _canPrepare = false;
        Generation.ClearWorkspace();
        AutomaticReset.ClearWorkspace();
        SelectedCell = null;
        Days.Clear();
        Employees.Clear();
        ReadinessDisplay = string.Empty;
        NotifyViewStateChanged();
    }

    private void ClearFeedback()
    {
        CancelFeedbackDismissal();
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private void ShowErrorMessage(string message)
    {
        CancelFeedbackDismissal();
        SuccessMessage = null;
        ErrorMessage = message;
    }

    private void ShowSuccessMessage(string? message)
    {
        CancelFeedbackDismissal();
        ErrorMessage = null;
        SuccessMessage = message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        CancellationTokenSource cancellation = new();
        _feedbackCancellation = cancellation;
        long sequence = _feedbackSequence;
        _ = DismissSuccessMessageAsync(sequence, cancellation);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The UI feedback boundary reports unexpected delay failures without crashing the dispatcher.")]
    private async Task DismissSuccessMessageAsync(
        long sequence,
        CancellationTokenSource cancellation)
    {
        try
        {
            await _feedbackDelay.DelayAsync(
                SuccessMessageReadingTime,
                cancellation.Token);
            if (sequence != _feedbackSequence || cancellation.IsCancellationRequested)
            {
                return;
            }

            IsSuccessMessageFading = true;
            await _feedbackDelay.DelayAsync(
                SuccessMessageFadeTime,
                cancellation.Token);
            if (sequence == _feedbackSequence && !cancellation.IsCancellationRequested)
            {
                SuccessMessage = null;
                IsSuccessMessageFading = false;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "DismissScheduleFeedback");
        }
        finally
        {
            if (ReferenceEquals(_feedbackCancellation, cancellation))
            {
                _feedbackCancellation = null;
                cancellation.Dispose();
            }
        }
    }

    private void CancelFeedbackDismissal()
    {
        _feedbackSequence++;
        CancellationTokenSource? cancellation = _feedbackCancellation;
        _feedbackCancellation = null;
        cancellation?.Cancel();
        cancellation?.Dispose();
        IsSuccessMessageFading = false;
    }

    private void NotifyFeedbackStateChanged()
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasSuccessMessage));
        OnPropertyChanged(nameof(HasFeedback));
        OnPropertyChanged(nameof(FeedbackMessage));
        OnPropertyChanged(nameof(FeedbackStatusDisplay));
        OnPropertyChanged(nameof(FeedbackAutomationName));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private void SetPendingAction(PendingScheduleAction? action)
    {
        if (action is not null)
        {
            CloseAssignmentEditor();
        }

        _pendingAction = action;
        OnPropertyChanged(nameof(CanEditSchedule));
        Generation.UpdateParentState(
            Preparation.HasLocalRunOptionChange,
            IsBusy || AutomaticReset.IsInteractionActive || action is not null);
        AutomaticReset.UpdateParentState(
            IsBusy
            || Generation.IsOperationActive
            || Generation.HasPreview
            || action is not null);
        NotifyConfirmationStateChanged();
        NotifyCommandsChanged();
    }

    private void NotifyConfirmationStateChanged()
    {
        OnPropertyChanged(nameof(IsConfirmationOpen));
        OnPropertyChanged(nameof(ConfirmationMessage));
        OnPropertyChanged(nameof(ConfirmationTitle));
        OnPropertyChanged(nameof(ConfirmationConfirmText));
        OnPropertyChanged(nameof(ConfirmationConfirmAutomationName));
        OnPropertyChanged(nameof(ConfirmationAutomationName));
        ConfirmActiveConfirmationCommand.NotifyCanExecuteChanged();
        CancelActiveConfirmationCommand.NotifyCanExecuteChanged();
    }

    private void CloseAssignmentEditor()
    {
        if (SelectedCell is not null)
        {
            SelectedCell.IsAssignmentEditorOpen = false;
        }
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
        ConfirmActiveConfirmationCommand.NotifyCanExecuteChanged();
        CancelActiveConfirmationCommand.NotifyCanExecuteChanged();
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

    private static bool MatchesAssignment(
        ServiceManagementAssignmentOptionSnapshot option,
        ScheduleAssignmentSnapshot assignment)
    {
        ScheduleAssignmentKindSnapshot expectedKind = option.Kind switch
        {
            ServiceManagementAssignmentSelectionKind.NormalDemand =>
                ScheduleAssignmentKindSnapshot.NormalDemand,
            ServiceManagementAssignmentSelectionKind.OfficeTime =>
                ScheduleAssignmentKindSnapshot.OfficeTime,
            ServiceManagementAssignmentSelectionKind.SplitShiftPattern =>
                ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            ServiceManagementAssignmentSelectionKind.ReliefShiftPattern =>
                ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
            _ => throw new InvalidOperationException(
                $"Unsupported service-management option: {option.Kind}"),
        };
        if (assignment.Origin != ScheduleAssignmentOriginSnapshot.ServiceManagement
            || assignment.Kind != expectedKind)
        {
            return false;
        }

        if (option.Kind == ServiceManagementAssignmentSelectionKind.OfficeTime)
        {
            return assignment.Segments.Count == 1
                && MatchesSegment(option.FirstSlot, assignment.Segments[0]);
        }

        ScheduleDemandSlotSnapshot[] slots = option.SecondSlot is null
            ? [option.FirstSlot]
            : [option.FirstSlot, option.SecondSlot];
        return assignment.Coverages.Count == slots.Length
            && slots.All(slot => assignment.Coverages.Any(coverage =>
                MatchesCoverage(slot, coverage)));
    }

    private static bool MatchesSegment(
        ScheduleDemandSlotSnapshot slot,
        ScheduleAssignmentSegmentSnapshot segment)
    {
        return segment.DemandSourceId == slot.SourceId
            && segment.Date == slot.Date
            && segment.WorkLocationId == slot.WorkLocationId
            && segment.ShiftTypeId == slot.ShiftTypeId
            && segment.ActualStart == slot.ActualStart
            && segment.ActualEnd == slot.ActualEnd;
    }

    private static bool MatchesCoverage(
        ScheduleDemandSlotSnapshot slot,
        ScheduleDemandCoverageSnapshot coverage)
    {
        return coverage.DemandSourceId == slot.SourceId
            && coverage.Date == slot.Date
            && coverage.WorkLocationId == slot.WorkLocationId
            && coverage.ShiftTypeId == slot.ShiftTypeId
            && coverage.Ordinal == slot.Ordinal;
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
