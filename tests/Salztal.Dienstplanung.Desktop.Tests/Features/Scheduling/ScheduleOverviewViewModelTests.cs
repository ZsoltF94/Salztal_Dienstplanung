using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class ScheduleOverviewViewModelTests
{
    internal static readonly DateOnly PeriodMonday = new(2026, 12, 21);

    [Fact]
    public async Task LoadCreatesDraftTwentyOneDaysAndModularRows()
    {
        FakeScheduleDataAccess dataAccess = new();
        dataAccess.Add(
            FakeScheduleDataAccess.NormalEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.Vacation);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(21, viewModel.Days.Count);
        Assert.Equal("Mo", viewModel.Days[0].DayDisplay);
        Assert.Equal("10.01.", viewModel.Days[^1].DateDisplay);
        Assert.Equal(3, viewModel.Employees.Count);
        ScheduleEmployeeRowViewModel normal = Assert.Single(
            viewModel.Employees,
            employee => employee.EmployeeId == FakeScheduleDataAccess.NormalEmployeeId);
        Assert.Equal("Erika Muster", normal.DisplayName);
        Assert.Equal("Typ25 – Restaurant - 25 Stunden", normal.EmployeeTypeDisplay);
        Assert.Equal("U", normal.Cells[0].EntryDisplay);
        Assert.Equal(3, normal.Weeks.Count);
        Assert.Equal("Wirksam: 20:00 Std.", normal.Weeks[0].EffectiveDisplay);
        Assert.Equal("Noch nicht vorbereitet", viewModel.Preparation.StatusDisplay);
        Assert.Contains("Typ1 fehlt", viewModel.ReadinessDisplay);
        Assert.Equal("Ein neuer Drei-Wochen-Entwurf wurde angelegt.", viewModel.SuccessMessage);
    }

    [Fact]
    public async Task LoadPlacesServiceManagementEmployeeBeforeAlphabeticallyEarlierEmployees()
    {
        ScheduleOverviewViewModel viewModel = CreateViewModel(new FakeScheduleDataAccess());

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            viewModel.Employees[0].EmployeeId);
        Assert.True(viewModel.Employees[0].IsServiceManagement);
        Assert.Contains(
            viewModel.Employees,
            employee => employee.EmployeeId == FakeScheduleDataAccess.AuxiliaryEmployeeId);
        Assert.Contains(
            viewModel.Employees,
            employee => employee.EmployeeId == FakeScheduleDataAccess.NormalEmployeeId);
    }

    [Fact]
    public async Task LoadWithoutServiceManagementEmployeeKeepsExistingNameOrder()
    {
        FakeScheduleDataAccess dataAccess = new(
            additionalNormalEmployees: 2,
            includeServiceManagement: false,
            includeAuxiliary: false);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            ["Erika Muster", "Test Person 01", "Test Person 02"],
            viewModel.Employees.Select(employee => employee.DisplayName));
        Assert.DoesNotContain(viewModel.Employees, employee => employee.IsServiceManagement);
    }

    [Fact]
    public async Task LoadGroupsServiceManagementNormalAndAuxiliaryEmployees()
    {
        FakeScheduleDataAccess dataAccess = new(
            additionalNormalEmployees: 2,
            additionalAuxiliaryEmployees: 2);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "Sarah Leitung",
                "Erika Muster",
                "Test Person 01",
                "Test Person 02",
                "Alex Beispiel",
                "Weitere Person AH 01",
                "Weitere Person AH 02",
            ],
            viewModel.Employees.Select(employee => employee.DisplayName));
        ScheduleEmployeeRowViewModel boundaryRow = Assert.Single(
            viewModel.Employees,
            employee => employee.ShowsAuxiliaryBoundary);
        Assert.Equal(FakeScheduleDataAccess.AuxiliaryEmployeeId, boundaryRow.EmployeeId);
    }

    [Fact]
    public async Task LoadWithoutNormalEmployeesPlacesBoundaryBetweenTyp1AndAuxiliary()
    {
        FakeScheduleDataAccess dataAccess = new(includeNormal: false);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                FakeScheduleDataAccess.ServiceManagementEmployeeId,
                FakeScheduleDataAccess.AuxiliaryEmployeeId,
            ],
            viewModel.Employees.Select(employee => employee.EmployeeId));
        Assert.True(viewModel.Employees[1].ShowsAuxiliaryBoundary);
    }

    [Fact]
    public async Task LoadWithoutServiceManagementStartsWithNormalBeforeAuxiliary()
    {
        FakeScheduleDataAccess dataAccess = new(includeServiceManagement: false);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                FakeScheduleDataAccess.NormalEmployeeId,
                FakeScheduleDataAccess.AuxiliaryEmployeeId,
            ],
            viewModel.Employees.Select(employee => employee.EmployeeId));
        Assert.True(viewModel.Employees[1].ShowsAuxiliaryBoundary);
    }

    [Fact]
    public async Task LoadWithoutAuxiliaryEmployeesDoesNotMarkBoundary()
    {
        FakeScheduleDataAccess dataAccess = new(includeAuxiliary: false);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                FakeScheduleDataAccess.ServiceManagementEmployeeId,
                FakeScheduleDataAccess.NormalEmployeeId,
            ],
            viewModel.Employees.Select(employee => employee.EmployeeId));
        Assert.DoesNotContain(
            viewModel.Employees,
            employee => employee.ShowsAuxiliaryBoundary);
    }

    [Fact]
    public async Task DateSelectionAndNavigationUseCompleteThreeWeekPeriods()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        viewModel.SelectedPeriodDate = new DateTime(2026, 12, 24);

        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.NextPeriodCommand.ExecuteAsync(null);
        await viewModel.PreviousPeriodCommand.ExecuteAsync(null);

        Assert.Equal(PeriodMonday.ToDateTime(TimeOnly.MinValue), viewModel.SelectedPeriodDate);
        Assert.Contains(dataAccess.RequestedPeriods, period =>
            period.StartMonday == PeriodMonday && period.EndSunday == PeriodMonday.AddDays(20));
        Assert.Contains(dataAccess.RequestedPeriods, period =>
            period.StartMonday == PeriodMonday.AddDays(21)
            && period.EndSunday == PeriodMonday.AddDays(41));
    }

    [Fact]
    public async Task SuccessFeedbackWaitsFadesAndExpiresWithoutBlockingLoad()
    {
        ControlledScheduleFeedbackDelay delay = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(
            new FakeScheduleDataAccess(),
            feedbackDelay: delay);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.True(viewModel.HasFeedback);
        Assert.Equal("Hinweis", viewModel.FeedbackStatusDisplay);
        Assert.Equal("Dienstplanmeldung Hinweis", viewModel.FeedbackAutomationName);
        Assert.False(viewModel.IsSuccessMessageFading);
        Assert.Equal([TimeSpan.FromSeconds(4)], delay.RequestedDurations);

        delay.Complete(0);
        await WaitUntilAsync(() => viewModel.IsSuccessMessageFading);

        Assert.True(viewModel.HasSuccessMessage);
        Assert.Equal(
            [TimeSpan.FromSeconds(4), TimeSpan.FromMilliseconds(350)],
            delay.RequestedDurations);

        delay.Complete(1);
        await WaitUntilAsync(() => !viewModel.HasFeedback);

        Assert.Null(viewModel.SuccessMessage);
        Assert.False(viewModel.IsSuccessMessageFading);
    }

    [Fact]
    public async Task OlderFeedbackDelayCannotHideNewerSuccessMessage()
    {
        ControlledScheduleFeedbackDelay delay = new()
        {
            IgnoreCancellation = true,
        };
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(
            dataAccess,
            feedbackDelay: delay);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectCell(viewModel, FakeScheduleDataAccess.NormalEmployeeId, PeriodMonday);

        await viewModel.SetVacationCommand.ExecuteAsync(null);

        Assert.Equal("Der Tageseintrag wurde gespeichert.", viewModel.SuccessMessage);
        Assert.Equal(2, delay.PendingCount);

        delay.Complete(0);
        await Task.Yield();
        await Task.Yield();

        Assert.Equal("Der Tageseintrag wurde gespeichert.", viewModel.SuccessMessage);
        Assert.False(viewModel.IsSuccessMessageFading);

        delay.Complete(1);
        await WaitUntilAsync(() => viewModel.IsSuccessMessageFading);
        delay.Complete(2);
        await WaitUntilAsync(() => !viewModel.HasFeedback);
    }

    [Fact]
    public async Task ErrorFeedbackReplacesSuccessAndNeverStartsAutomaticDismissal()
    {
        ControlledScheduleFeedbackDelay delay = new();
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(
            dataAccess,
            feedbackDelay: delay);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        dataAccess.RejectNextChange = true;
        SelectCell(viewModel, FakeScheduleDataAccess.NormalEmployeeId, PeriodMonday);

        await viewModel.SetFixedDayOffCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasSuccessMessage);
        Assert.True(viewModel.HasFeedback);
        Assert.Equal("Fehler", viewModel.FeedbackStatusDisplay);
        Assert.Equal("Dienstplanmeldung Fehler", viewModel.FeedbackAutomationName);
        Assert.Contains("zwischenzeitlich geändert", viewModel.FeedbackMessage);
        Assert.Equal(0, delay.PendingCount);
    }

    [Fact]
    public async Task EmptyCellCanBeSetAndRemovedThroughCoordinatedCommands()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectCell(viewModel, FakeScheduleDataAccess.NormalEmployeeId, PeriodMonday);

        await viewModel.SetVacationCommand.ExecuteAsync(null);

        Assert.Equal("U", Assert.IsType<ScheduleCellViewModel>(
            viewModel.SelectedCell).EntryDisplay);
        viewModel.RequestRemovalCommand.Execute(null);
        Assert.True(viewModel.IsConfirmationOpen);
        await viewModel.ConfirmActiveConfirmationCommand.ExecuteAsync(null);
        Assert.Equal(string.Empty, Assert.IsType<ScheduleCellViewModel>(
            viewModel.SelectedCell).EntryDisplay);
        Assert.Equal(2, dataAccess.ChangeCallCount);
    }

    [Fact]
    public async Task UnifiedConfirmationCancelsCellRemovalAndBlocksBackgroundCommands()
    {
        FakeScheduleDataAccess dataAccess = new();
        dataAccess.Add(
            FakeScheduleDataAccess.NormalEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.Vacation);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectCell(viewModel, FakeScheduleDataAccess.NormalEmployeeId, PeriodMonday);

        viewModel.RequestRemovalCommand.Execute(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.Equal("Änderung am Tagesfeld bestätigen?", viewModel.ConfirmationTitle);
        Assert.Equal("Bestätigen", viewModel.ConfirmationConfirmText);
        Assert.False(viewModel.LoadCommand.CanExecute(null));
        Assert.False(viewModel.SetFixedDayOffCommand.CanExecute(null));
        Assert.False(viewModel.AutomaticReset.RequestCommand.CanExecute(null));
        Assert.True(viewModel.CancelActiveConfirmationCommand.CanExecute(null));
        Assert.Equal("U", viewModel.SelectedCell?.EntryDisplay);
        Assert.Equal(0, dataAccess.ChangeCallCount);

        viewModel.CancelActiveConfirmationCommand.Execute(null);

        Assert.False(viewModel.IsConfirmationOpen);
        Assert.Equal("U", viewModel.SelectedCell?.EntryDisplay);
        Assert.Equal(0, dataAccess.ChangeCallCount);
    }

    [Fact]
    public async Task UnifiedConfirmationRoutesAutomaticResetWithoutChangingWarningText()
    {
        RecordingAutomaticScheduleResetActions resetActions = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(
            new FakeScheduleDataAccess(),
            resetActions: resetActions);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        viewModel.AutomaticReset.ApplyContext(
            AutomaticScheduleResetTestData.AcceptedContext(12, 4),
            false);

        viewModel.AutomaticReset.RequestCommand.Execute(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.Equal(
            "Automatischen Plan wirklich vollständig verwerfen?",
            viewModel.ConfirmationTitle);
        Assert.Equal("Vollständig verwerfen", viewModel.ConfirmationConfirmText);
        Assert.Contains("12 automatisch erzeugte Einteilungen", viewModel.ConfirmationMessage);
        Assert.Contains("4 schwarze X", viewModel.ConfirmationMessage);
        Assert.Contains("Typ1-Dienste, U, K, rote X", viewModel.ConfirmationMessage);
        Assert.False(viewModel.LoadCommand.CanExecute(null));

        await viewModel.ConfirmActiveConfirmationCommand.ExecuteAsync(null);

        Assert.Equal(1, resetActions.CallCount);
        Assert.False(viewModel.IsConfirmationOpen);
    }

    [Fact]
    public async Task UnifiedConfirmationCancelsTyp1RemovalWithoutWriting()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        await SetTyp1Async(viewModel, PeriodMonday);
        int changesBeforeRemoval = dataAccess.ChangeCallCount;

        viewModel.RequestRemovalCommand.Execute(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.True(viewModel.SelectedCell?.HasAssignment);
        Assert.Equal(changesBeforeRemoval, dataAccess.ChangeCallCount);

        viewModel.CancelActiveConfirmationCommand.Execute(null);

        Assert.False(viewModel.IsConfirmationOpen);
        Assert.True(viewModel.SelectedCell?.HasAssignment);
        Assert.Equal(changesBeforeRemoval, dataAccess.ChangeCallCount);
    }

    [Fact]
    public async Task Typ1CellOffersStructuredOfficeOptionStoresAndMarksB()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        ScheduleCellViewModel cell = SelectCell(
            viewModel,
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            PeriodMonday);
        Assert.True(cell.IsAssignmentEditorOpen);
        ServiceManagementAssignmentOptionViewModel office = Assert.Single(
            cell.AssignmentOptions,
            option => option.Snapshot.Kind
                    == ServiceManagementAssignmentSelectionKind.OfficeTime
                && option.Snapshot.FirstSlot.Ordinal == 1
                && option.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");
        await viewModel.SetTyp1AssignmentCommand.ExecuteAsync(office);

        ScheduleCellViewModel selected = Assert.IsType<ScheduleCellViewModel>(
            viewModel.SelectedCell);
        Assert.Equal("B", selected.EntryDisplay);
        Assert.Contains("Bedarf bleibt offen", selected.EntryMeaning);
        Assert.False(selected.IsAssignmentEditorOpen);
        Assert.Same(
            Assert.Single(selected.AssignmentOptions, option => option.IsCurrent),
            selected.AssignmentOptions.First(option => option.Snapshot.Kind
                == ServiceManagementAssignmentSelectionKind.OfficeTime));
        Assert.Equal("Der Typ1-Dienst wurde gespeichert.", viewModel.SuccessMessage);
    }

    [Fact]
    public async Task Typ1SelectionReplacingDayEntryRequiresVisibleConfirmation()
    {
        FakeScheduleDataAccess dataAccess = new();
        dataAccess.Add(
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.Vacation);
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        ScheduleCellViewModel cell = SelectCell(
            viewModel,
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            PeriodMonday);
        ServiceManagementAssignmentOptionViewModel option = cell.AssignmentOptions[0];

        await viewModel.SetTyp1AssignmentCommand.ExecuteAsync(option);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.Contains("Urlaub", viewModel.ConfirmationMessage);
        Assert.Equal("U", viewModel.SelectedCell?.EntryDisplay);
        Assert.Equal(0, dataAccess.ChangeCallCount);
    }

    [Fact]
    public async Task Typ1StoreConflictKeepsCellEmptyAndShowsReloadMessage()
    {
        FakeScheduleDataAccess dataAccess = new()
        {
            RejectNextChange = true,
        };
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        ScheduleCellViewModel cell = SelectCell(
            viewModel,
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            PeriodMonday);

        await viewModel.SetTyp1AssignmentCommand.ExecuteAsync(cell.AssignmentOptions[0]);

        Assert.True(viewModel.HasError);
        Assert.Contains("zwischenzeitlich geändert", viewModel.ErrorMessage);
        Assert.Equal(string.Empty, viewModel.SelectedCell?.EntryDisplay);
        Assert.False(cell.IsAssignmentEditorOpen);
    }

    [Fact]
    public async Task NonTyp1CellNeverOpensAssignmentEditor()
    {
        ScheduleOverviewViewModel viewModel = CreateViewModel(new FakeScheduleDataAccess());
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        ScheduleCellViewModel cell = SelectCell(
            viewModel,
            FakeScheduleDataAccess.NormalEmployeeId,
            PeriodMonday);

        Assert.False(cell.CanOpenAssignmentEditor);
        Assert.False(cell.IsAssignmentEditorOpen);
        Assert.Empty(cell.AssignmentOptions);
    }

    [Fact]
    public async Task SwitchingTyp1ToDayEntryRequiresVisibleConfirmation()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        await SetTyp1Async(viewModel, PeriodMonday);

        await viewModel.SetFixedDayOffCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.Contains("Typ1", viewModel.ConfirmationMessage);
        viewModel.CancelPendingActionCommand.Execute(null);
        Assert.True(viewModel.SelectedCell?.HasAssignment);
        Assert.False(viewModel.SelectedCell?.HasEntry);
    }

    [Fact]
    public async Task PrepareShowsMissingHistoryAndOutdatedCategoriesAfterChange()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        await SetTyp1Async(viewModel, PeriodMonday);
        await SetTyp1Async(viewModel, PeriodMonday.AddDays(7));
        await SetTyp1Async(viewModel, PeriodMonday.AddDays(14));

        Assert.True(viewModel.PreparePlanningCommand.CanExecute(null));
        await viewModel.PreparePlanningCommand.ExecuteAsync(null);

        Assert.Equal("Vorbereitet und aktuell", viewModel.Preparation.StatusDisplay);
        Assert.Contains("fehlt", viewModel.Preparation.HistoryDisplay);
        SelectCell(viewModel, FakeScheduleDataAccess.NormalEmployeeId, PeriodMonday);
        await viewModel.SetFixedDayOffCommand.ExecuteAsync(null);
        Assert.Contains("veraltet", viewModel.Preparation.StatusDisplay);
        Assert.Contains("Tageskennzeichen", viewModel.Preparation.ChangedCategoriesDisplay);
    }

    [Fact]
    public async Task ChangedRunOptionRequiresAndPerformsConsciousRefresh()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        await SetTyp1Async(viewModel, PeriodMonday);
        await SetTyp1Async(viewModel, PeriodMonday.AddDays(7));
        await SetTyp1Async(viewModel, PeriodMonday.AddDays(14));
        await viewModel.PreparePlanningCommand.ExecuteAsync(null);

        viewModel.Preparation.EnableAuxiliaryReliefShift = true;

        Assert.True(viewModel.Preparation.HasLocalRunOptionChange);
        Assert.True(viewModel.PreparePlanningCommand.CanExecute(null));
        await viewModel.PreparePlanningCommand.ExecuteAsync(null);
        Assert.Equal("Vorbereitet und aktuell", viewModel.Preparation.StatusDisplay);
        Assert.True(dataAccess.PreparedSnapshot?.RunOptions.EnableAuxiliaryReliefShift);
        Assert.Equal(2, dataAccess.PrepareCallCount);
    }

    [Fact]
    public async Task AuxiliaryEmployeeDisablesVacationAndSicknessButAllowsFixedDayOff()
    {
        ScheduleOverviewViewModel viewModel = CreateViewModel(new FakeScheduleDataAccess());
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectCell(viewModel, FakeScheduleDataAccess.AuxiliaryEmployeeId, PeriodMonday);

        Assert.False(viewModel.SetVacationCommand.CanExecute(null));
        Assert.False(viewModel.SetSicknessCommand.CanExecute(null));
        Assert.True(viewModel.SetFixedDayOffCommand.CanExecute(null));
    }

    [Fact]
    public async Task StoreConflictKeepsCellEmptyAndShowsReloadMessage()
    {
        FakeScheduleDataAccess dataAccess = new()
        {
            RejectNextChange = true,
        };
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectCell(viewModel, FakeScheduleDataAccess.NormalEmployeeId, PeriodMonday);

        await viewModel.SetFixedDayOffCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Contains("zwischenzeitlich geändert", viewModel.ErrorMessage);
        Assert.Equal(string.Empty, viewModel.SelectedCell?.EntryDisplay);
    }

    [Fact]
    public async Task UnexpectedLoadFailureIsReportedAndShownWithoutRows()
    {
        InvalidOperationException failure = new("synthetic load failure");
        FakeScheduleDataAccess dataAccess = new()
        {
            LoadException = failure,
        };
        RecordingUnexpectedErrorReporter reporter = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess, reporter);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Same(failure, reporter.Exception);
        Assert.Equal("LoadScheduleWorkspace", reporter.Operation);
        Assert.True(viewModel.HasError);
        Assert.Contains("konnte nicht geladen", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Employees);
    }

    internal static ScheduleOverviewViewModel CreateViewModel(
        FakeScheduleDataAccess dataAccess,
        RecordingUnexpectedErrorReporter? reporter = null,
        IScheduleFeedbackDelay? feedbackDelay = null,
        IAutomaticScheduleResetActions? resetActions = null)
    {
        return new ScheduleOverviewViewModel(
            new OpenOrCreateScheduleDraftCommand(dataAccess, dataAccess),
            new GetScheduleWorkspaceQuery(dataAccess),
            new SetServiceManagementAssignmentCommand(dataAccess, dataAccess),
            new RemoveServiceManagementAssignmentCommand(dataAccess, dataAccess),
            new ChangeScheduleDayEntryCommand(dataAccess, dataAccess),
            new RemoveScheduleDayEntryCommand(dataAccess, dataAccess),
            new PreparePlanningInputCommand(dataAccess, dataAccess),
            new UnusedAutomaticScheduleGenerationActions(),
            resetActions ?? new UnusedAutomaticScheduleResetActions(),
            PeriodMonday,
            reporter ?? new RecordingUnexpectedErrorReporter(),
            feedbackDelay ?? new ControlledScheduleFeedbackDelay());
    }

    internal static ScheduleCellViewModel SelectCell(
        ScheduleOverviewViewModel viewModel,
        Guid employeeId,
        DateOnly date)
    {
        ScheduleEmployeeRowViewModel employee = Assert.Single(
            viewModel.Employees,
            item => item.EmployeeId == employeeId);
        ScheduleCellViewModel cell = Assert.Single(
            employee.Cells,
            item => item.Date == date);
        viewModel.SelectCellCommand.Execute(cell);
        return cell;
    }

    private static async Task SetTyp1Async(
        ScheduleOverviewViewModel viewModel,
        DateOnly date)
    {
        ScheduleCellViewModel cell = SelectCell(
            viewModel,
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            date);
        ServiceManagementAssignmentOptionViewModel option = Assert.Single(
            cell.AssignmentOptions,
            item => item.Snapshot.Kind
                    == ServiceManagementAssignmentSelectionKind.NormalDemand
                && item.Snapshot.FirstSlot.Ordinal == 1
                && item.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");
        await viewModel.SetTyp1AssignmentCommand.ExecuteAsync(option);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (int attempt = 0; attempt < 100 && !condition(); attempt++)
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
        }

        Assert.True(condition());
    }
}
