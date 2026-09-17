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
    private readonly Stopwatch _stopwatch = new();
    private AutomaticScheduleGenerationContext? _context;
    private CancellationTokenSource? _runCancellation;
    private AutomaticSchedulePreviewViewModel? _preview;
    private bool _isRunning;
    private bool _isAccepting;
    private bool _isParentBusy;
    private bool _hasLocalRunOptionChange;
    private string _elapsedDisplay = AutomaticScheduleGenerationPresentation
        .CreateElapsedDisplay(TimeSpan.Zero, MaximumRuntime);
    private string? _statusMessage;
    private string? _errorCodeDisplay;

    public AutomaticScheduleGenerationViewModel(
        IAutomaticScheduleGenerationActions actions,
        Func<CancellationToken, Task> reloadWorkspace,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(reloadWorkspace);
        ArgumentNullException.ThrowIfNull(errorReporter);
        _actions = actions;
        _reloadWorkspace = reloadWorkspace;
        _errorReporter = errorReporter;
        StartCommand = new AsyncRelayCommand(StartAsync, CanStart);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning);
        AcceptCommand = new AsyncRelayCommand(AcceptAsync, CanAccept);
        DiscardCommand = new RelayCommand(Discard, CanDiscard);
    }

    public IAsyncRelayCommand StartCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IAsyncRelayCommand AcceptCommand { get; }

    public IRelayCommand DiscardCommand { get; }

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
        if (_context is not null && _context != next)
        {
            ClearPreview();
        }

        _context = next;
        _hasLocalRunOptionChange = hasLocalRunOptionChange;
        _isParentBusy = isParentBusy;
        NotifyStateChanged();
    }

    public void ClearWorkspace()
    {
        _context = null;
        _hasLocalRunOptionChange = false;
        ClearPreview();
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
            }
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "AcceptAutomaticScheduleProposal");
            StatusMessage =
                "Der Vorschlag konnte wegen eines unerwarteten Fehlers nicht übernommen werden.";
            ErrorCodeDisplay = "Technischer Code: DesktopUnexpectedError";
            ClearPreview();
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
        Preview = result.Preview is null
            ? null
            : new AutomaticSchedulePreviewViewModel(result.Preview);
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
    }

}
