using System.Windows;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class AutomaticScheduleReportWindowCoordinator
    : IAutomaticScheduleReportPresenter
{
    private Window? _owner;
    private AutomaticScheduleReportWindow? _window;
    private AutomaticScheduleReportSource? _source;
    private string _unavailableMessage =
        "Für diesen Planungsstand ist kein aktueller Bericht verfügbar.";

    public bool IsOpen => _window is not null;

    internal AutomaticScheduleReportWindow? Window => _window;

    internal AutomaticScheduleReportWindowViewModel? CurrentViewModel =>
        _window?.DataContext as AutomaticScheduleReportWindowViewModel;

    public void AttachOwner(Window owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (_owner is not null && !ReferenceEquals(_owner, owner))
        {
            throw new InvalidOperationException(
                "The report window coordinator already has an owner.");
        }

        _owner = owner;
    }

    public void SetSource(
        AutomaticScheduleReportSource? source,
        string unavailableMessage)
    {
        ArgumentNullException.ThrowIfNull(unavailableMessage);
        _source = source;
        if (!string.IsNullOrWhiteSpace(unavailableMessage))
        {
            _unavailableMessage = unavailableMessage.Trim();
        }

        if (_window is not null)
        {
            _window.DataContext = CreateViewModel();
        }
    }

    public void ShowCurrent()
    {
        if (_source is null)
        {
            return;
        }

        if (_window is null)
        {
            _window = new AutomaticScheduleReportWindow
            {
                DataContext = CreateViewModel(),
                Owner = _owner,
            };
            _window.Closed += OnWindowClosed;
            _window.Show();
        }
        else
        {
            _window.DataContext = CreateViewModel();
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.WindowState = WindowState.Normal;
            }

            _window.Show();
        }

        _window.Activate();
        _window.Focus();
    }

    public void Close()
    {
        AutomaticScheduleReportWindow? window = _window;
        if (window is null)
        {
            return;
        }

        window.Closed -= OnWindowClosed;
        _window = null;
        window.Close();
    }

    private AutomaticScheduleReportWindowViewModel CreateViewModel() =>
        _source is null
            ? AutomaticScheduleReportWindowViewModel.Unavailable(_unavailableMessage)
            : AutomaticScheduleReportWindowViewModel.Current(_source);

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (_window is not null)
        {
            _window.Closed -= OnWindowClosed;
            _window = null;
        }
    }
}
