using System.IO;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop.Composition;

internal static class ServiceCatalogComposition
{
    private const string ApplicationDataDirectoryName = "Salztal Dienstplanung";
    private const string DatabaseFileName = "dienstplanung.db";

    public static async Task<MainWindow> CreateMainWindowAsync(
        CancellationToken cancellationToken)
    {
        string localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        string databasePath = Path.Combine(
            localApplicationData,
            ApplicationDataDirectoryName,
            DatabaseFileName);
        ServiceCatalogDependencies dependencies = await CreateInitializedAsync(
            databasePath,
            cancellationToken);
        IUnexpectedErrorReporter errorReporter = new TraceUnexpectedErrorReporter();
        ServiceCatalogViewModel viewModel = new(
            new GetServiceCatalogQuery(dependencies.Reader),
            new UpdateWorkLocationCommand(dependencies.WorkLocationUpdateStore),
            new UpdateShiftTypeStandardTimeCommand(
                dependencies.ShiftTypeStandardTimeUpdateStore),
            errorReporter);

        return new MainWindow(viewModel);
    }

    public static async Task<ServiceCatalogDependencies> CreateInitializedAsync(
        string databasePath,
        CancellationToken cancellationToken)
    {
        SqliteServiceCatalogStore store = new(databasePath);
        await store.InitializeAsync(cancellationToken);

        return new ServiceCatalogDependencies(store, store, store);
    }
}
