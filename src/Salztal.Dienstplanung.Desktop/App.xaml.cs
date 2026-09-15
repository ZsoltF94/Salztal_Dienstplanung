using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Threading;
using Salztal.Dienstplanung.Desktop.Composition;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class App : System.Windows.Application
{
    private bool _isHandlingFatalUserInterfaceException;

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The application startup boundary must show a controlled German error before shutdown.")]
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            MainWindow window = await MainWindowComposition.CreateMainWindowAsync(
                CancellationToken.None);
            MainWindow = window;
            window.Show();
        }
        catch (Exception exception)
        {
            Trace.TraceError(
                "Application startup failed. ExceptionType={0}",
                exception.GetType().FullName);
            MessageBox.Show(
                "Die Anwendung konnte nicht gestartet werden. "
                + "Bitte versuchen Sie es erneut.",
                "Salztal Dienstplanung",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);
        eventArgs.Handled = true;

        if (_isHandlingFatalUserInterfaceException)
        {
            Shutdown(1);
            return;
        }

        _isHandlingFatalUserInterfaceException = true;
        Trace.TraceError(
            "Unhandled user interface failure. ExceptionType={0}",
            eventArgs.Exception.GetType().FullName);
        MessageBox.Show(
            "Ein unerwarteter technischer Fehler ist aufgetreten. "
            + "Die Anwendung wird beendet. Nicht gespeicherte Eingaben können verloren gehen.",
            "Salztal Dienstplanung",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Shutdown(1);
    }
}
