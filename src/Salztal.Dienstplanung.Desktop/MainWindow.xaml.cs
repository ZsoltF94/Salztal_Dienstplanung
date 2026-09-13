using System.Windows;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop;

internal sealed partial class MainWindow : Window
{
    private readonly ServiceCatalogViewModel _viewModel;

    public MainWindow(ServiceCatalogViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
