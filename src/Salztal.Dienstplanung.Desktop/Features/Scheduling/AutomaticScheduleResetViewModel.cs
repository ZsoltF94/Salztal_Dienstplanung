using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class AutomaticScheduleResetViewModel : ObservableObject
{
    private readonly IAutomaticScheduleResetActions _actions;
    private readonly Func<CancellationToken, Task> _reloadWorkspace;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private AutomaticScheduleGenerationContext? _context;
    private bool _isParentBusy;
    private bool _isConfirmationOpen;
    private bool _isDiscarding;
    private string? _statusMessage;
    private string? _errorCodeDisplay;

    public AutomaticScheduleResetViewModel(
        IAutomaticScheduleResetActions actions,
        Func<CancellationToken, Task> reloadWorkspace,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(reloadWorkspace);
        ArgumentNullException.ThrowIfNull(errorReporter);
        _actions = actions;
        _reloadWorkspace = reloadWorkspace;
        _errorReporter = errorReporter;
        RequestCommand = new RelayCommand(Request, CanRequest);
        ConfirmCommand = new AsyncRelayCommand(ConfirmAsync, CanConfirm);
        CancelCommand = new RelayCommand(Cancel, CanCancel);
    }

    public IRelayCommand RequestCommand { get; }

    public IAsyncRelayCommand ConfirmCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public bool IsAvailable => _context?.AcceptedAutomaticSchedule is not null;

    public bool IsVisible => IsAvailable || HasStatusMessage;

    public bool IsConfirmationOpen
    {
        get => _isConfirmationOpen;
        private set
        {
            if (SetProperty(ref _isConfirmationOpen, value))
            {
                OnPropertyChanged(nameof(IsInteractionActive));
                NotifyCommandsChanged();
            }
        }
    }

    public bool IsDiscarding
    {
        get => _isDiscarding;
        private set
        {
            if (SetProperty(ref _isDiscarding, value))
            {
                OnPropertyChanged(nameof(IsInteractionActive));
                NotifyCommandsChanged();
            }
        }
    }

    public bool IsInteractionActive => IsConfirmationOpen || IsDiscarding;

    public string ConfirmationMessage
    {
        get
        {
            AcceptedAutomaticScheduleSnapshot summary = _context?.AcceptedAutomaticSchedule
                ?? throw new InvalidOperationException(
                    "An accepted automatic schedule is required.");
            return $"Sollen {summary.AssignmentCount} automatisch erzeugte Einteilungen "
                + $"und {summary.GeneratedDayOffCount} schwarze X vollst\u00e4ndig verworfen werden? "
                + "Typ1-Dienste, U, K, rote X und manuelle Einteilungen bleiben erhalten. "
                + "Die Planungsvorbereitung wird entfernt und diese Aktion kann derzeit nicht r\u00fcckg\u00e4ngig gemacht werden.";
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
                OnPropertyChanged(nameof(IsVisible));
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
        AutomaticScheduleGenerationContext context,
        bool isParentBusy)
    {
        ArgumentNullException.ThrowIfNull(context);
        bool contextChanged = _context is not null
            && (_context.DraftId != context.DraftId
                || _context.PeriodMonday != context.PeriodMonday);
        _context = context;
        _isParentBusy = isParentBusy;
        if (contextChanged)
        {
            ClearFeedback();
            IsConfirmationOpen = false;
        }

        NotifyAvailabilityChanged();
    }

    public void UpdateParentState(bool isParentBusy)
    {
        _isParentBusy = isParentBusy;
        NotifyCommandsChanged();
    }

    public void ClearWorkspace()
    {
        _context = null;
        _isParentBusy = false;
        IsConfirmationOpen = false;
        ClearFeedback();
        NotifyAvailabilityChanged();
    }

    private bool CanRequest() => IsAvailable && !_isParentBusy && !IsInteractionActive;

    private void Request()
    {
        ClearFeedback();
        IsConfirmationOpen = true;
        OnPropertyChanged(nameof(ConfirmationMessage));
    }

    private bool CanConfirm() => IsConfirmationOpen && !IsDiscarding;

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ConfirmAsync(CancellationToken cancellationToken)
    {
        AutomaticScheduleGenerationContext context = _context
            ?? throw new InvalidOperationException("A schedule context is required.");
        IsConfirmationOpen = false;
        IsDiscarding = true;
        ClearFeedback();
        try
        {
            AutomaticScheduleDiscardOutcome result = await _actions.DiscardAsync(
                new DiscardAutomaticScheduleRequest(
                    context.DraftId,
                    context.Version,
                    context.PeriodMonday),
                cancellationToken);
            StatusMessage = result.Message;
            if (result.Status == AutomaticScheduleDiscardStatus.Succeeded)
            {
                await _reloadWorkspace(cancellationToken);
            }
            else
            {
                ErrorCodeDisplay = $"Verwerfcode: {result.Status}";
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "DiscardAutomaticSchedule");
            StatusMessage =
                "Der automatische Plan konnte wegen eines unerwarteten Fehlers nicht verworfen werden.";
            ErrorCodeDisplay = "Technischer Code: DesktopUnexpectedError";
        }
        finally
        {
            IsDiscarding = false;
        }
    }

    private bool CanCancel() => IsConfirmationOpen && !IsDiscarding;

    private void Cancel()
    {
        IsConfirmationOpen = false;
        StatusMessage = "Das Verwerfen wurde abgebrochen. Der Entwurf blieb unver\u00e4ndert.";
        ErrorCodeDisplay = null;
    }

    private void ClearFeedback()
    {
        StatusMessage = null;
        ErrorCodeDisplay = null;
    }

    private void NotifyAvailabilityChanged()
    {
        OnPropertyChanged(nameof(IsAvailable));
        OnPropertyChanged(nameof(IsVisible));
        if (IsAvailable)
        {
            OnPropertyChanged(nameof(ConfirmationMessage));
        }

        NotifyCommandsChanged();
    }

    private void NotifyCommandsChanged()
    {
        RequestCommand.NotifyCanExecuteChanged();
        ConfirmCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}
