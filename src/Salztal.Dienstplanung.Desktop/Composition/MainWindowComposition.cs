using System.IO;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Desktop.Composition;

internal static class MainWindowComposition
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
        MainWindowDependencies dependencies = await CreateInitializedAsync(
            databasePath,
            cancellationToken);
        IUnexpectedErrorReporter errorReporter = new TraceUnexpectedErrorReporter();
        ServiceCatalogViewModel serviceCatalog = new(
            new GetServiceCatalogQuery(dependencies.ServiceCatalogReader),
            new UpdateWorkLocationCommand(dependencies.WorkLocationUpdateStore),
            new UpdateShiftTypeStandardTimeCommand(
                dependencies.ShiftTypeStandardTimeUpdateStore),
            errorReporter);
        EmployeeOverviewViewModel employees = new(
            new GetEmployeeOverviewQuery(dependencies.EmployeeReader),
            new GetEmployeeTypeCatalogQuery(dependencies.EmployeeReader),
            new CreateEmployeeCommand(
                dependencies.EmployeeReader,
                dependencies.CreateEmployeeStore),
            new UpdateEmployeeNameCommand(
                dependencies.EmployeeReader,
                dependencies.UpdateEmployeeNameStore),
            new ChangeEmployeeTypeCommand(
                dependencies.EmployeeReader,
                dependencies.ChangeEmployeeTypeStore),
            new DeactivateEmployeeCommand(
                dependencies.EmployeeReader,
                dependencies.DeactivateEmployeeStore),
            new ReactivateEmployeeCommand(
                dependencies.EmployeeReader,
                dependencies.ReactivateEmployeeStore),
            new DeleteEmployeeCommand(
                dependencies.EmployeeReader,
                dependencies.DeleteEmployeeStore),
            errorReporter);

        return new MainWindow(serviceCatalog, employees);
    }

    public static async Task<MainWindowDependencies> CreateInitializedAsync(
        string databasePath,
        CancellationToken cancellationToken)
    {
        SqliteServiceCatalogStore serviceCatalogStore = new(databasePath);
        await serviceCatalogStore.InitializeAsync(cancellationToken);
        SqliteEmployeeStore employeeStore = new(databasePath);

        return new MainWindowDependencies(
            serviceCatalogStore,
            serviceCatalogStore,
            serviceCatalogStore,
            employeeStore,
            employeeStore,
            employeeStore,
            employeeStore,
            employeeStore,
            employeeStore,
            employeeStore);
    }
}
