using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Desktop.Features.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Availabilities;

public sealed class AvailabilityOverviewViewModelTests
{
    private static readonly DateOnly PeriodMonday = new(2026, 12, 21);

    [Fact]
    public async Task LoadCreatesTwentyOneDayTableAndThreeWeeklyTargetsPerActiveEmployee()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        dataAccess.Add(
            FakeAvailabilityDataAccess.NormalEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.Vacation);
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(21, viewModel.Days.Count);
        Assert.Equal("Mo", viewModel.Days[0].DayDisplay);
        Assert.Equal("10.01.", viewModel.Days[^1].DateDisplay);
        Assert.Equal(2, viewModel.Employees.Count);
        AvailabilityEmployeeRowViewModel normal = Assert.Single(
            viewModel.Employees,
            employee => employee.EmployeeId == FakeAvailabilityDataAccess.NormalEmployeeId);
        Assert.Equal("Erika Muster", normal.DisplayName);
        Assert.Equal("Typ25 – Restaurant - 25 Stunden", normal.EmployeeTypeDisplay);
        Assert.Equal(21, normal.Cells.Count);
        Assert.Equal("U", normal.Cells[0].EntryDisplay);
        Assert.Equal(3, normal.Weeks.Count);
        Assert.Equal("Soll: 25:00 Std.", normal.Weeks[0].UncutDisplay);
        Assert.Equal("Wirksam: 20:00 Std.", normal.Weeks[0].EffectiveDisplay);
        Assert.True(viewModel.HasEmployees);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task DateSelectionAndNavigationAlwaysUseCompleteThreeWeekPeriods()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        viewModel.SelectedPeriodDate = new DateTime(2026, 12, 24);

        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.NextPeriodCommand.ExecuteAsync(null);
        await viewModel.PreviousPeriodCommand.ExecuteAsync(null);

        Assert.Equal(PeriodMonday.ToDateTime(TimeOnly.MinValue), viewModel.SelectedPeriodDate);
        Assert.Equal(
            [
                (PeriodMonday, PeriodMonday.AddDays(20)),
                (PeriodMonday.AddDays(21), PeriodMonday.AddDays(41)),
                (PeriodMonday, PeriodMonday.AddDays(20)),
            ],
            dataAccess.RequestedPeriods);
    }

    [Fact]
    public async Task EmptyCellCanBeSetAndWeeklyTargetRefreshesWithoutConfirmation()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        AvailabilityCellViewModel cell = SelectNormalCell(viewModel, PeriodMonday);

        await viewModel.SetVacationCommand.ExecuteAsync(null);

        Assert.Equal(1, dataAccess.SaveCallCount);
        Assert.False(viewModel.IsConfirmationOpen);
        Assert.Equal("Der Tageseintrag wurde gespeichert.", viewModel.SuccessMessage);
        Assert.Equal("U", Assert.IsType<AvailabilityCellViewModel>(viewModel.SelectedCell).EntryDisplay);
        AvailabilityEmployeeRowViewModel row = Assert.Single(
            viewModel.Employees,
            employee => employee.EmployeeId == FakeAvailabilityDataAccess.NormalEmployeeId);
        Assert.Equal("Wirksam: 20:00 Std.", row.Weeks[0].EffectiveDisplay);
    }

    [Fact]
    public async Task ReplacementRequiresConfirmationAndCancelKeepsExistingValue()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        dataAccess.Add(
            FakeAvailabilityDataAccess.NormalEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.Vacation,
            4);
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectNormalCell(viewModel, PeriodMonday);

        await viewModel.SetSicknessCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.Contains("Erika Muster", viewModel.ConfirmationMessage);
        Assert.Contains("Urlaub", viewModel.ConfirmationMessage);
        Assert.Contains("Krankheit", viewModel.ConfirmationMessage);
        Assert.Equal(0, dataAccess.SaveCallCount);

        viewModel.CancelPendingActionCommand.Execute(null);

        Assert.False(viewModel.IsConfirmationOpen);
        Assert.Equal("U", Assert.IsType<AvailabilityCellViewModel>(viewModel.SelectedCell).EntryDisplay);
        Assert.Equal(0, dataAccess.SaveCallCount);
    }

    [Fact]
    public async Task ConfirmedReplacementStoresNewValueAndKeepsCellSelected()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        dataAccess.Add(
            FakeAvailabilityDataAccess.NormalEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.Vacation,
            4);
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectNormalCell(viewModel, PeriodMonday);
        await viewModel.SetSicknessCommand.ExecuteAsync(null);

        await viewModel.ConfirmPendingActionCommand.ExecuteAsync(null);

        Assert.Equal(1, dataAccess.SaveCallCount);
        Assert.False(viewModel.IsConfirmationOpen);
        AvailabilityCellViewModel selected = Assert.IsType<AvailabilityCellViewModel>(
            viewModel.SelectedCell);
        Assert.True(selected.IsSelected);
        Assert.Equal("K", selected.EntryDisplay);
        Assert.Equal(5, selected.ChangeVersion);
    }

    [Fact]
    public async Task RemovalRequiresConfirmationAndConfirmedRemovalClearsCell()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        dataAccess.Add(
            FakeAvailabilityDataAccess.NormalEmployeeId,
            PeriodMonday,
            AvailabilityEntryKind.FixedDayOff,
            2);
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectNormalCell(viewModel, PeriodMonday);

        viewModel.RequestRemovalCommand.Execute(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.Contains("fest vorgegebenes Frei", viewModel.ConfirmationMessage);
        Assert.Equal(0, dataAccess.RemoveCallCount);

        await viewModel.ConfirmPendingActionCommand.ExecuteAsync(null);

        Assert.Equal(1, dataAccess.RemoveCallCount);
        Assert.Equal(string.Empty, Assert.IsType<AvailabilityCellViewModel>(
            viewModel.SelectedCell).EntryDisplay);
        Assert.Equal("Der Tageseintrag wurde entfernt.", viewModel.SuccessMessage);
    }

    [Fact]
    public async Task AuxiliaryEmployeeDisablesVacationAndSicknessButAllowsFixedDayOff()
    {
        FakeAvailabilityDataAccess dataAccess = new();
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        AvailabilityEmployeeRowViewModel auxiliary = Assert.Single(
            viewModel.Employees,
            employee => employee.EmployeeId == FakeAvailabilityDataAccess.AuxiliaryEmployeeId);
        viewModel.SelectCellCommand.Execute(auxiliary.Cells[0]);

        Assert.False(viewModel.SetVacationCommand.CanExecute(null));
        Assert.False(viewModel.SetSicknessCommand.CanExecute(null));
        Assert.True(viewModel.SetFixedDayOffCommand.CanExecute(null));

        await viewModel.SetFixedDayOffCommand.ExecuteAsync(null);

        Assert.Equal("X", Assert.IsType<AvailabilityCellViewModel>(
            viewModel.SelectedCell).EntryDisplay);
    }

    [Fact]
    public async Task ConcurrentSaveConflictKeepsCellEmptyAndShowsReloadMessage()
    {
        FakeAvailabilityDataAccess dataAccess = new()
        {
            RejectNextSave = true,
        };
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadAsync(TestContext.Current.CancellationToken);
        SelectNormalCell(viewModel, PeriodMonday);

        await viewModel.SetFixedDayOffCommand.ExecuteAsync(null);

        Assert.Equal(1, dataAccess.SaveCallCount);
        Assert.True(viewModel.HasError);
        Assert.Contains("zwischenzeitlich geändert", viewModel.ErrorMessage);
        Assert.Equal(string.Empty, Assert.IsType<AvailabilityCellViewModel>(
            viewModel.SelectedCell).EntryDisplay);
    }

    [Fact]
    public async Task UnexpectedLoadFailureIsReportedAndShownWithoutRows()
    {
        InvalidOperationException failure = new("synthetic load failure");
        FakeAvailabilityDataAccess dataAccess = new()
        {
            LoadException = failure,
        };
        RecordingUnexpectedErrorReporter reporter = new();
        AvailabilityOverviewViewModel viewModel = CreateViewModel(dataAccess, reporter);

        await viewModel.LoadAsync(TestContext.Current.CancellationToken);

        Assert.Same(failure, reporter.Exception);
        Assert.Equal("LoadAvailabilityPeriod", reporter.Operation);
        Assert.True(viewModel.HasError);
        Assert.Contains("konnte nicht geladen", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Employees);
    }

    private static AvailabilityOverviewViewModel CreateViewModel(
        FakeAvailabilityDataAccess dataAccess,
        RecordingUnexpectedErrorReporter? reporter = null)
    {
        return new AvailabilityOverviewViewModel(
            new GetAvailabilityPeriodQuery(dataAccess),
            new SaveAvailabilityEntryCommand(dataAccess, dataAccess),
            new RemoveAvailabilityEntryCommand(dataAccess, dataAccess),
            PeriodMonday,
            reporter ?? new RecordingUnexpectedErrorReporter());
    }

    private static AvailabilityCellViewModel SelectNormalCell(
        AvailabilityOverviewViewModel viewModel,
        DateOnly date)
    {
        AvailabilityEmployeeRowViewModel employee = Assert.Single(
            viewModel.Employees,
            item => item.EmployeeId == FakeAvailabilityDataAccess.NormalEmployeeId);
        AvailabilityCellViewModel cell = Assert.Single(
            employee.Cells,
            item => item.Date == date);
        viewModel.SelectCellCommand.Execute(cell);
        return cell;
    }
}
