using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Salztal.Dienstplanung.Desktop.Composition;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class App : System.Windows.Application
{
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
                "Die lokalen Anwendungsdaten konnten nicht geöffnet werden. "
                + "Bitte starten Sie die Anwendung erneut.",
                "Salztal Dienstplanung",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
