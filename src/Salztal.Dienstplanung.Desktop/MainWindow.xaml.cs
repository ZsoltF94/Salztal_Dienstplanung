using System.Windows;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class MainWindow : Window
{
    // WPF StartupUri activation requires a public parameterless constructor.
    public MainWindow()
    {
        InitializeComponent();
    }
}
