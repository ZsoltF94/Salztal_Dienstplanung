using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class AutomaticScheduleGenerationViewModel : ObservableObject
{
    private static readonly TimeSpan MaximumRuntime =
        AutomaticSchedulePlanningRequest.ProductiveTimeLimit;
    private readonly IAutomaticScheduleGenerationActions _actions;
    private readonly Func<CancellationToken, Task> _reloadWorkspace;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private readonly IAutomaticScheduleReportPresenter _reportPresenter;
    private readonly Stopwatch _stopwatch = new();
    private AutomaticScheduleGenerationContext? _context;
    private CancellationTokenSource? _runCancellation;
    private AutomaticSchedulePreviewViewModel? _preview;
    private AutomaticScheduleGenerationReportViewModel? _generationReport;
    private bool _isRunning;
    private bool _isAccepting;
    private bool _isParentBusy;
    private bool _hasLocalRunOptionChange;
    private string _elapsedDisplay = AutomaticScheduleGenerationPresentation
        .CreateElapsedDisplay(TimeSpan.Zero, MaximumRuntime);
    private string? _statusMessage;
    private string? _errorCodeDisplay;
    private string? _reportAvailabilityMessage;

    public AutomaticScheduleGenerationViewModel(
        IAutomaticScheduleGenerationActions actions,
        Func<CancellationToken, Task> reloadWorkspace,
        IUnexpectedErrorReporter errorReporter,
        IAutomaticScheduleReportPresenter? reportPresenter = null)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(reloadWorkspace);
        ArgumentNullException.ThrowIfNull(errorReporter);
        _actions = actions;
        _reloadWorkspace = reloadWorkspace;
        _errorReporter = errorReporter;
        _reportPresenter = reportPresenter
            ?? NullAutomaticScheduleReportPresenter.Instance;
        StartCommand = new AsyncRelayCommand(StartAsync, CanStart);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning);
        AcceptCommand = new AsyncRelayCommand(AcceptAsync, CanAccept);
        DiscardCommand = new RelayCommand(Discard, CanDiscard);
        OpenReportCommand = new RelayCommand(
            _reportPresenter.ShowCurrent,
            () => HasGenerationReport);
    }

    public IAsyncRelayCommand StartCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IAsyncRelayCommand AcceptCommand { get; }

    public IRelayCommand DiscardCommand { get; }

    public IRelayCommand OpenReportCommand { get; }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                NotifyStateChanged();
            }
        }
    }

    public bool IsAccepting
    {
        get => _isAccepting;
        private set
        {
            if (SetProperty(ref _isAccepting, value))
            {
                NotifyStateChanged();
            }
        }
    }

    public bool IsOperationActive => IsRunning || IsAccepting;

    public bool HasPreview => Preview is not null;

    public AutomaticSchedulePreviewViewModel? Preview
    {
        get => _preview;
        private set
        {
            if (SetProperty(ref _preview, value))
            {
                OnPropertyChanged(nameof(HasPreview));
                NotifyCommandsChanged();
            }
        }
    }

    public AutomaticScheduleGenerationReportViewModel? GenerationReport
    {
        get => _generationReport;
        private set
        {
            if (SetProperty(ref _generationReport, value))
            {
                OnPropertyChanged(nameof(HasGenerationReport));
                OpenReportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasGenerationReport => GenerationReport is not null;

    public string? ReportAvailabilityMessage
    {
        get => _reportAvailabilityMessage;
        private set
        {
            if (SetProperty(ref _reportAvailabilityMessage, value))
            {
                OnPropertyChanged(nameof(HasReportAvailabilityMessage));
            }
        }
    }

    public bool HasReportAvailabilityMessage =>
        !string.IsNullOrWhiteSpace(ReportAvailabilityMessage);

    public string ElapsedDisplay
    {
        get => _elapsedDisplay;
        private set => SetProperty(ref _elapsedDisplay, value);
    }

    public string StartAvailabilityDisplay =>
        AutomaticScheduleGenerationPresentation.CreateStartAvailability(
            _context,
            _hasLocalRunOptionChange,
            _isParentBusy,
            IsRunning,
            IsAccepting,
            HasPreview);

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public string? ErrorCodeDisplay
    {
        get => _errorCodeDisplay;
        private set
        {
            if (SetProperty(ref _errorCodeDisplay, value))
            {
                OnPropertyChanged(nameof(HasErrorCode));
            }
        }
    }

    public bool HasErrorCode => !string.IsNullOrWhiteSpace(ErrorCodeDisplay);

    public void ApplyContext(
        AutomaticScheduleGenerationContext next,
        bool hasLocalRunOptionChange,
        bool isParentBusy)
    {
        ArgumentNullException.ThrowIfNull(next);
        bool contextChanged = _context != next;
        if (_context is not null && contextChanged)
        {
            ClearPreview();
        }

        _context = next;
        if (contextChanged)
        {
            ApplyAcceptedReport(next.AcceptedAutomaticSchedule);
        }

        _hasLocalRunOptionChange = hasLocalRunOptionChange;
        _isParentBusy = isParentBusy;
        NotifyStateChanged();
    }

    public void ClearWorkspace()
    {
        _context = null;
        _hasLocalRunOptionChange = false;
        ClearPreview();
        ClearReport("Für den geschlossenen Planungszeitraum ist kein aktueller Bericht mehr verfügbar.");
        NotifyStateChanged();
    }

    public void UpdateParentState(bool hasLocalRunOptionChange, bool isParentBusy)
    {
        _hasLocalRunOptionChange = hasLocalRunOptionChange;
        _isParentBusy = isParentBusy;
        NotifyStateChanged();
    }

    private bool CanStart()
    {
        return _context is
        {
            PreparedSnapshotId: not null,
            PreparationStatus: SchedulePreparationStatus.Prepared,
            IsServiceManagementReady: true,
        }
            && !_hasLocalRunOptionChange
            && !_isParentBusy
            && !IsOperationActive
            && !HasPreview;
    }

    private async Task StartAsync(CancellationToken cancellationToken)
    {
        AutomaticScheduleGenerationContext context = _context
            ?? throw new InvalidOperationException("A generation context is required.");
        ClearFeedback();
        ClearReport("Ein neuer Generierungslauf wird ausgeführt. Der vorherige Bericht ist nicht mehr aktuell.");
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runCancellation = linked;
        _stopwatch.Restart();
        ElapsedDisplay = AutomaticScheduleGenerationPresentation.CreateElapsedDisplay(
            TimeSpan.Zero,
            MaximumRuntime);
        IsRunning = true;
        try
        {
            Task<AutomaticScheduleGenerationOutcome> generation = _actions.GenerateAsync(
                new GenerateAutomaticScheduleRequest(
                    context.DraftId,
                    context.Version,
                    context.PeriodMonday,
                    context.PreparedSnapshotId!.Value),
                linked.Token);
            await TrackElapsedAsync(generation);
            AutomaticScheduleGenerationOutcome result = await generation;
            ApplyGenerationResult(result);
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "GenerateAutomaticSchedule");
            StatusMessage =
                "Die automatische Planung konnte wegen eines unerwarteten Fehlers nicht abgeschlossen werden.";
            ErrorCodeDisplay = "Technischer Code: DesktopUnexpectedError";
            ClearReport(
                "Für den unerwartet abgebrochenen Desktop-Vorgang liegt kein strukturierter Generierungsbericht vor.");
        }
        finally
        {
            _stopwatch.Stop();
            ElapsedDisplay = AutomaticScheduleGenerationPresentation.CreateElapsedDisplay(
                _stopwatch.Elapsed,
                MaximumRuntime);
            _runCancellation = null;
            IsRunning = false;
        }
    }

    private async Task TrackElapsedAsync(Task generation)
    {
        while (!generation.IsCompleted)
        {
            await Task.WhenAny(generation, Task.Delay(TimeSpan.FromMilliseconds(250)));
            ElapsedDisplay = AutomaticScheduleGenerationPresentation.CreateElapsedDisplay(
                _stopwatch.Elapsed,
                MaximumRuntime);
        }
    }

    private void Cancel()
    {
        StatusMessage = "Abbruch wird durchgeführt …";
        _runCancellation?.Cancel();
    }

    private async Task AcceptAsync(CancellationToken cancellationToken)
    {
        AutomaticSchedulePreviewViewModel preview = Preview
            ?? throw new InvalidOperationException("A preview is required.");
        IsAccepting = true;
        ClearFeedback();
        try
        {
            AutomaticScheduleAcceptanceOutcome result = await _actions.AcceptAsync(
                new AcceptAutomaticScheduleProposalRequest(
                    _context!.PeriodMonday,
                    preview.Proposal),
                cancellationToken);
            StatusMessage = result.Message;
            if (result.Status == AutomaticScheduleAcceptanceStatus.Succeeded)
            {
                ClearPreview();
                await _reloadWorkspace(cancellationToken);
            }
            else
            {
                ErrorCodeDisplay = $"Übernahmecode: {result.Status}";
                ClearPreview();
                ClearReport(
                    "Der Bericht gehört zu einem nicht mehr aktuellen Vorschlag und wurde geschlossen.");
            }
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "AcceptAutomaticScheduleProposal");
            StatusMessage =
                "Der Vorschlag konnte wegen eines unerwarteten Fehlers nicht übernommen werden.";
            ErrorCodeDisplay = "Technischer Code: DesktopUnexpectedError";
            ClearPreview();
            ClearReport(
                "Der Bericht gehört zu einem nicht mehr aktuellen Vorschlag und wurde geschlossen.");
        }
        finally
        {
            IsAccepting = false;
        }
    }

    private bool CanAccept()
    {
        return HasPreview && !IsOperationActive && !_isParentBusy;
    }

    private void Discard()
    {
        ClearPreview();
        ClearReport(
            "Der Vorschlag und sein Bericht wurden verworfen. Der Entwurf blieb unverändert.");
        StatusMessage = "Der Vorschlag wurde verworfen. Der Entwurf blieb unverändert.";
        ErrorCodeDisplay = null;
    }

    private bool CanDiscard()
    {
        return HasPreview && !IsOperationActive;
    }

    private void ApplyGenerationResult(AutomaticScheduleGenerationOutcome result)
    {
        StatusMessage = result.Message;
        SetReport(
            result.Report,
            new AutomaticScheduleReportSource(
                CreateTransientReportIdentity(result.Report),
                result.Report.PlanningReport is null
                    ? "Letzter Generierungsversuch ohne Vorschlag"
                    : "Flüchtiger Generierungsvorschlag",
                result.Report));
        Preview = result.Preview is null
            ? null
            : new AutomaticSchedulePreviewViewModel(
                result.Preview,
                result.Report);
        ErrorCodeDisplay =
            AutomaticScheduleGenerationPresentation.CreateErrorCodeDisplay(result);
    }

    private void ClearPreview()
    {
        _actions.DiscardPreview();
        Preview = null;
    }

    private void ClearFeedback()
    {
        StatusMessage = null;
        ErrorCodeDisplay = null;
    }

    private void ApplyAcceptedReport(AcceptedAutomaticScheduleSnapshot? accepted)
    {
        if (accepted is null)
        {
            ClearReport("Für diesen Entwurf ist kein aktueller automatischer Bericht vorhanden.");
            return;
        }

        switch (accepted.ReportStatus)
        {
            case AcceptedAutomaticScheduleReportStatus.Current
                when accepted.Report is not null:
                SetReport(
                    accepted.Report,
                    new AutomaticScheduleReportSource(
                        CreateAcceptedReportIdentity(accepted.Report),
                        "Übernommener automatischer Lauf",
                        accepted.Report));
                break;
            case AcceptedAutomaticScheduleReportStatus.ChangedAfterGeneration:
                ClearReport(
                    "Der Entwurf wurde nach der automatischen Übernahme geändert. Der frühere Generierungsbericht wird nicht als aktuell angezeigt.");
                break;
            default:
                ClearReport(
                    "Für diesen übernommenen Lauf sind keine vollständigen Berichtsdetails verfügbar.");
                break;
        }
    }

    private void SetReport(
        AutomaticScheduleGenerationReport report,
        AutomaticScheduleReportSource source)
    {
        GenerationReport = new AutomaticScheduleGenerationReportViewModel(report);
        ReportAvailabilityMessage = null;
        _reportPresenter.SetSource(source, string.Empty);
    }

    private void ClearReport(string unavailableMessage)
    {
        GenerationReport = null;
        ReportAvailabilityMessage = unavailableMessage;
        _reportPresenter.SetSource(null, unavailableMessage);
    }

    private static string CreateTransientReportIdentity(
        AutomaticScheduleGenerationReport report)
    {
        AutomaticSchedulePlanningReport? planning = report.PlanningReport;
        return planning is null
            ? $"attempt:{Guid.NewGuid():D}"
            : $"preview:{planning.SnapshotId:D}:{planning.DraftId:D}:{planning.DraftVersion}";
    }

    private static string CreateAcceptedReportIdentity(
        AutomaticScheduleGenerationReport report)
    {
        AutomaticSchedulePlanningReport planning = report.PlanningReport
            ?? throw new ArgumentException(
                "An accepted report requires planning values.",
                nameof(report));
        return $"accepted:{planning.SnapshotId:D}:{planning.DraftId:D}:{planning.DraftVersion}";
    }

    private void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(IsOperationActive));
        OnPropertyChanged(nameof(StartAvailabilityDisplay));
        NotifyCommandsChanged();
    }

    private void NotifyCommandsChanged()
    {
        StartCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        AcceptCommand.NotifyCanExecuteChanged();
        DiscardCommand.NotifyCanExecuteChanged();
        OpenReportCommand.NotifyCanExecuteChanged();
    }

}
