using System.IO;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Infrastructure.Logging;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;
using Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;
using Salztal.Dienstplanung.Planning;

namespace Salztal.Dienstplanung.Desktop.Composition;

internal static class MainWindowComposition
{
    private const string ApplicationDataDirectoryName = "Salztal Dienstplanung";
    private const string DatabaseFileName = "dienstplanung.db";
    private const string TechnicalLogDirectoryName = "Logs";

    public static async Task<MainWindow> CreateMainWindowAsync(
        CancellationToken cancellationToken)
    {
        string localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        string databasePath = Path.Combine(
            localApplicationData,
            ApplicationDataDirectoryName,
            DatabaseFileName);
        IAutomaticScheduleTechnicalErrorStore technicalErrorStore =
            new JsonLinesAutomaticScheduleTechnicalErrorStore(Path.Combine(
                localApplicationData,
                ApplicationDataDirectoryName,
                TechnicalLogDirectoryName));
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
        EmployeeTypeOverviewViewModel employeeTypes = new(
            new GetEmployeeTypeCatalogQuery(dependencies.EmployeeReader),
            new CreateEmployeeTypeCommand(
                dependencies.EmployeeReader,
                dependencies.CreateEmployeeTypeStore),
            new UpdateEmployeeTypeCommand(
                dependencies.EmployeeReader,
                dependencies.UpdateEmployeeTypeStore),
            new DeleteEmployeeTypeCommand(
                dependencies.EmployeeReader,
                dependencies.DeleteEmployeeTypeStore),
            errorReporter);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly currentWeekMonday = GetWeekMonday(today);
        StaffingDemandOverviewViewModel staffingDemands = new(
            new GetStaffingDemandWeekQuery(dependencies.StaffingDemandReader),
            new SaveStaffingDemandDateExceptionCommand(
                dependencies.StaffingDemandReader,
                dependencies.StaffingDemandDateExceptionStore),
            new RemoveStaffingDemandDateExceptionCommand(
                dependencies.StaffingDemandReader,
                dependencies.RemoveStaffingDemandDateExceptionStore),
            currentWeekMonday,
            errorReporter);
        StandardStaffingDemandEditorViewModel standardStaffingDemands = new(
            new GetStaffingDemandWeekQuery(dependencies.StaffingDemandReader),
            new ChangeStandardStaffingDemandCommand(
                dependencies.StaffingDemandReader,
                dependencies.StandardStaffingDemandRevisionStore),
            currentWeekMonday,
            staffingDemands.LoadAsync,
            errorReporter);
        SchedulingDependencies scheduling = dependencies.Scheduling;
        ScheduleOverviewViewModel schedule = new(
            new OpenOrCreateScheduleDraftCommand(
                scheduling.WorkspaceReader,
                scheduling.OpenDraftStore),
            new GetScheduleWorkspaceQuery(scheduling.WorkspaceReader),
            new SetServiceManagementAssignmentCommand(
                scheduling.WorkspaceReader,
                scheduling.ChangeDayStore),
            new RemoveServiceManagementAssignmentCommand(
                scheduling.WorkspaceReader,
                scheduling.ChangeDayStore),
            new ChangeScheduleDayEntryCommand(
                scheduling.WorkspaceReader,
                scheduling.ChangeDayStore),
            new RemoveScheduleDayEntryCommand(
                scheduling.WorkspaceReader,
                scheduling.ChangeDayStore),
            new PreparePlanningInputCommand(
                scheduling.PlanningInputReader,
                scheduling.PrepareSnapshotStore),
            new AutomaticScheduleGenerationActions(
                new GenerateAutomaticScheduleCommand(
                    scheduling.PlanningInputReader,
                    AutomaticSchedulePlannerFactory.Create(),
                    technicalErrorStore),
                new AcceptAutomaticScheduleProposalCommand(
                    scheduling.WorkspaceReader,
                    scheduling.AcceptAutomaticScheduleProposalStore)),
            new AutomaticScheduleResetActions(
                new DiscardAutomaticScheduleCommand(
                    scheduling.WorkspaceReader,
                    scheduling.DiscardAutomaticScheduleStore)),
            currentWeekMonday,
            errorReporter,
            new ScheduleFeedbackDelay());
        serviceCatalog.SelectedWorkLocationChanged +=
            standardStaffingDemands.SelectWorkLocation;

        return new MainWindow(
            serviceCatalog,
            employees,
            employeeTypes,
            staffingDemands,
            standardStaffingDemands,
            schedule);
    }

    public static async Task<MainWindowDependencies> CreateInitializedAsync(
        string databasePath,
        CancellationToken cancellationToken)
    {
        SqliteServiceCatalogStore serviceCatalogStore = new(databasePath);
        await serviceCatalogStore.InitializeAsync(cancellationToken);
        SqliteEmployeeStore employeeStore = new(databasePath);
        SqliteStaffingDemandStore staffingDemandStore = new(databasePath);
        SqliteScheduleStore scheduleStore = new(databasePath);
        SqliteAutomaticScheduleDiscardStore automaticScheduleDiscardStore = new(
            databasePath);
        await staffingDemandStore.InitializeAsync(cancellationToken);
        await scheduleStore.InitializeAsync(cancellationToken);

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
            employeeStore,
            employeeStore,
            employeeStore,
            employeeStore,
            staffingDemandStore,
            staffingDemandStore,
            staffingDemandStore,
            staffingDemandStore,
            new SchedulingDependencies(
                scheduleStore,
                scheduleStore,
                scheduleStore,
                scheduleStore,
                scheduleStore,
                scheduleStore,
                automaticScheduleDiscardStore));
    }

    private static DateOnly GetWeekMonday(DateOnly date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
