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
        await viewModel.ConfirmPendingActionCommand.ExecuteAsync(null);
        Assert.Equal(string.Empty, Assert.IsType<ScheduleCellViewModel>(
            viewModel.SelectedCell).EntryDisplay);
        Assert.Equal(2, dataAccess.ChangeCallCount);
    }

    [Fact]
    public async Task Typ1OffersStructuredOfficeOptionAndStoresB()
    {
        FakeScheduleDataAccess dataAccess = new();
        ScheduleOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectCell(
            viewModel,
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            PeriodMonday);
        ServiceManagementAssignmentOptionViewModel office = Assert.Single(
            viewModel.Typ1Editor.Options,
            option => option.Snapshot.Kind
                    == ServiceManagementAssignmentSelectionKind.OfficeTime
                && option.Snapshot.FirstSlot.Ordinal == 1
                && option.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");
        viewModel.Typ1Editor.SelectedOption = office;

        await viewModel.SetTyp1AssignmentCommand.ExecuteAsync(null);

        ScheduleCellViewModel selected = Assert.IsType<ScheduleCellViewModel>(
            viewModel.SelectedCell);
        Assert.Equal("B", selected.EntryDisplay);
        Assert.Contains("Bedarf bleibt offen", selected.EntryMeaning);
        Assert.Equal("Der Typ1-Dienst wurde gespeichert.", viewModel.SuccessMessage);
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
        RecordingUnexpectedErrorReporter? reporter = null)
    {
        return new ScheduleOverviewViewModel(
            new OpenOrCreateScheduleDraftCommand(dataAccess, dataAccess),
            new GetScheduleWorkspaceQuery(dataAccess),
            new SetServiceManagementAssignmentCommand(dataAccess, dataAccess),
            new RemoveServiceManagementAssignmentCommand(dataAccess, dataAccess),
            new ChangeScheduleDayEntryCommand(dataAccess, dataAccess),
            new RemoveScheduleDayEntryCommand(dataAccess, dataAccess),
            new PreparePlanningInputCommand(dataAccess, dataAccess),
            PeriodMonday,
            reporter ?? new RecordingUnexpectedErrorReporter());
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
        SelectCell(
            viewModel,
            FakeScheduleDataAccess.ServiceManagementEmployeeId,
            date);
        ServiceManagementAssignmentOptionViewModel option = Assert.Single(
            viewModel.Typ1Editor.Options,
            item => item.Snapshot.Kind
                    == ServiceManagementAssignmentSelectionKind.NormalDemand
                && item.Snapshot.FirstSlot.Ordinal == 1
                && item.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");
        viewModel.Typ1Editor.SelectedOption = option;
        await viewModel.SetTyp1AssignmentCommand.ExecuteAsync(null);
    }
}
