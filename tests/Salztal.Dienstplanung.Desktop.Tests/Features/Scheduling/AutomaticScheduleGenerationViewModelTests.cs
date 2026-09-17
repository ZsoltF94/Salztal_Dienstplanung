using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class AutomaticScheduleGenerationViewModelTests
{
    [Fact]
    public void StartRequiresLoadedCurrentPreparedWorkspace()
    {
        AutomaticScheduleGenerationViewModel viewModel = CreateViewModel(
            new RecordingAutomaticScheduleGenerationActions());

        Assert.False(viewModel.StartCommand.CanExecute(null));
        Assert.Contains("Lade zuerst", viewModel.StartAvailabilityDisplay);

        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext() with
            {
                PreparedSnapshotId = null,
                PreparationStatus = SchedulePreparationStatus.NotPrepared,
            },
            false,
            false);
        Assert.False(viewModel.StartCommand.CanExecute(null));
        Assert.Contains("Bereite", viewModel.StartAvailabilityDisplay);

        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext(),
            true,
            false);
        Assert.False(viewModel.StartCommand.CanExecute(null));
        Assert.Contains("Aktualisiere", viewModel.StartAvailabilityDisplay);

        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext(),
            false,
            false);
        Assert.True(viewModel.StartCommand.CanExecute(null));
        Assert.Contains("aktuelle Vorbereitung", viewModel.StartAvailabilityDisplay);
    }

    [Fact]
    public async Task RunningGenerationShowsRuntimeAndCanBeCancelled()
    {
        RecordingAutomaticScheduleGenerationActions actions = new();
        AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(actions);

        Task operation = viewModel.StartCommand.ExecuteAsync(null);
        await WaitUntilAsync(() => actions.GenerateCallCount == 1);

        Assert.True(viewModel.IsRunning);
        Assert.True(viewModel.IsOperationActive);
        Assert.True(viewModel.CancelCommand.CanExecute(null));
        Assert.Contains("maximal 02:00", viewModel.ElapsedDisplay);
        viewModel.CancelCommand.Execute(null);
        await operation;

        Assert.False(viewModel.IsRunning);
        Assert.False(viewModel.HasPreview);
        Assert.Contains("abgebrochen", viewModel.StatusMessage);
    }

    [Fact]
    public async Task OptimalPreviewShowsSummaryAndAcceptanceReloadsWorkspace()
    {
        RecordingAutomaticScheduleGenerationActions actions = new();
        int reloadCount = 0;
        AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(
            actions,
            _ =>
            {
                reloadCount++;
                return Task.CompletedTask;
            });
        actions.CompleteGeneration(AutomaticScheduleGenerationTestData.SuccessOutcome());

        await viewModel.StartCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasPreview);
        Assert.Equal("Optimaler Vorschlag", viewModel.Preview!.StatusDisplay);
        Assert.Contains("1 vollständig, 1 teilweise", viewModel.Preview.OpenDemandDisplay);
        Assert.Contains("4 Std. 30 min", viewModel.Preview.OpenDemandDisplay);
        Assert.Contains("1 schwarzes X", viewModel.Preview.AssignmentDisplay);

        await viewModel.AcceptCommand.ExecuteAsync(null);

        Assert.Equal(1, actions.AcceptCallCount);
        Assert.Equal(1, reloadCount);
        Assert.False(viewModel.HasPreview);
        Assert.Equal(
            AutomaticScheduleGenerationTestData.PeriodMonday,
            actions.AcceptanceRequest!.PeriodMonday);
    }

    [Fact]
    public async Task FeasiblePreviewCanBeDiscardedWithoutReload()
    {
        RecordingAutomaticScheduleGenerationActions actions = new();
        int reloadCount = 0;
        AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(
            actions,
            _ =>
            {
                reloadCount++;
                return Task.CompletedTask;
            });
        actions.CompleteGeneration(AutomaticScheduleGenerationTestData.SuccessOutcome(
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal));

        await viewModel.StartCommand.ExecuteAsync(null);
        Assert.Contains("nicht nachgewiesen", viewModel.Preview!.StatusDisplay);

        viewModel.DiscardCommand.Execute(null);

        Assert.False(viewModel.HasPreview);
        Assert.Equal(0, reloadCount);
        Assert.Contains("verworfen", viewModel.StatusMessage);
    }

    [Fact]
    public async Task TechnicalFailureShowsStableCodeAndCorrelationIdentifier()
    {
        RecordingAutomaticScheduleGenerationActions actions = new();
        AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(actions);
        actions.CompleteGeneration(
            AutomaticScheduleGenerationTestData.TechnicalFailureOutcome());

        await viewModel.StartCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasPreview);
        Assert.True(viewModel.HasErrorCode);
        Assert.Contains("TechnicalFailure", viewModel.ErrorCodeDisplay);
        Assert.Contains("test-4711", viewModel.ErrorCodeDisplay);
    }

    [Fact]
    public async Task AcceptanceConflictClearsStalePreviewAndShowsCode()
    {
        RecordingAutomaticScheduleGenerationActions actions = new()
        {
            AcceptanceOutcome = new AutomaticScheduleAcceptanceOutcome(
                AutomaticScheduleAcceptanceStatus.Conflict,
                "Der Entwurf wurde zwischenzeitlich geändert."),
        };
        AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(actions);
        actions.CompleteGeneration(AutomaticScheduleGenerationTestData.SuccessOutcome());
        await viewModel.StartCommand.ExecuteAsync(null);

        await viewModel.AcceptCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasPreview);
        Assert.Contains("Conflict", viewModel.ErrorCodeDisplay);
        Assert.Contains("zwischenzeitlich geändert", viewModel.StatusMessage);
    }

    private static AutomaticScheduleGenerationViewModel CreateReadyViewModel(
        RecordingAutomaticScheduleGenerationActions actions,
        Func<CancellationToken, Task>? reload = null)
    {
        AutomaticScheduleGenerationViewModel viewModel = CreateViewModel(
            actions,
            reload);
        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext(),
            false,
            false);
        return viewModel;
    }

    private static AutomaticScheduleGenerationViewModel CreateViewModel(
        RecordingAutomaticScheduleGenerationActions actions,
        Func<CancellationToken, Task>? reload = null)
    {
        return new AutomaticScheduleGenerationViewModel(
            actions,
            reload ?? (_ => Task.CompletedTask),
            new RecordingUnexpectedErrorReporter());
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        for (int attempt = 0; attempt < 100 && !predicate(); attempt++)
        {
            await Task.Delay(10);
        }

        Assert.True(predicate());
    }
}
