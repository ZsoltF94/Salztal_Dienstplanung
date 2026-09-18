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
        Assert.Contains("4 Std. 00 min", viewModel.Preview.OpenDemandDisplay);
        Assert.Contains("1 schwarzes X", viewModel.Preview.AssignmentDisplay);
        Assert.NotNull(viewModel.Preview.DemandReport);
        Assert.Equal(3, viewModel.Preview.DemandReport.Demands.Count);
        Assert.NotNull(viewModel.Preview.EmployeeReport);
        Assert.NotNull(viewModel.Preview.GenerationReport);
        Assert.True(viewModel.HasGenerationReport);
        Assert.NotNull(viewModel.GenerationReport);
        Assert.Equal(15, viewModel.GenerationReport.Phases.Count);
        AutomaticScheduleGenerationPhaseViewModel reliefPhase =
            viewModel.GenerationReport.Phases[6];
        Assert.Equal("Spr minimieren", reliefPhase.Name);
        Assert.Equal("2", reliefPhase.Metrics[0].InitialDisplay);
        Assert.Equal("0", reliefPhase.Metrics[0].AchievedDisplay);
        Assert.NotNull(viewModel.GenerationReport.TechnicalDetails);
        Assert.Contains(
            "Testsolver",
            viewModel.GenerationReport.TechnicalDetails.SolverDisplay);
        Assert.Contains("Optimalität", viewModel.GenerationReport.StatusDisplay);

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
        Assert.True(viewModel.HasGenerationReport);
        Assert.Equal(
            "Technischer Fehler",
            viewModel.GenerationReport!.StatusDisplay);
        Assert.Equal(
            "Zuletzt erreicht: Modellaufbau",
            viewModel.GenerationReport.LastReachedPhaseDisplay);
        Assert.Contains("TechnicalFailure", viewModel.ErrorCodeDisplay);
        Assert.Contains("test-4711", viewModel.ErrorCodeDisplay);
    }

    [Fact]
    public async Task GeneratedReportCanBeOpenedThroughPresenter()
    {
        RecordingAutomaticScheduleGenerationActions actions = new();
        RecordingAutomaticScheduleReportPresenter presenter = new();
        AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(
            actions,
            reportPresenter: presenter);
        actions.CompleteGeneration(AutomaticScheduleGenerationTestData.SuccessOutcome());

        await viewModel.StartCommand.ExecuteAsync(null);
        viewModel.OpenReportCommand.Execute(null);

        Assert.True(viewModel.OpenReportCommand.CanExecute(null));
        Assert.NotNull(presenter.Source);
        Assert.StartsWith("preview:", presenter.Source.Identity, StringComparison.Ordinal);
        Assert.Equal("Flüchtiger Generierungsvorschlag", presenter.Source.SourceDisplay);
        Assert.Equal(1, presenter.ShowCallCount);
    }

    [Fact]
    public void LaterDraftVersionInvalidatesAcceptedReportImmediately()
    {
        RecordingAutomaticScheduleGenerationActions actions = new();
        RecordingAutomaticScheduleReportPresenter presenter = new();
        AutomaticScheduleGenerationReport report =
            AutomaticScheduleGenerationTestData.SuccessOutcome().Report;
        AutomaticScheduleGenerationViewModel viewModel = CreateViewModel(
            actions,
            reportPresenter: presenter);

        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext() with
            {
                AcceptedAutomaticSchedule = new AcceptedAutomaticScheduleSnapshot(
                    0,
                    0,
                    AcceptedAutomaticScheduleReportStatus.Current,
                    report),
            },
            false,
            false);
        Assert.True(viewModel.HasGenerationReport);
        Assert.StartsWith("accepted:", presenter.Source!.Identity, StringComparison.Ordinal);

        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext() with
            {
                Version = 4,
                AcceptedAutomaticSchedule = new AcceptedAutomaticScheduleSnapshot(
                    0,
                    0,
                    AcceptedAutomaticScheduleReportStatus.ChangedAfterGeneration),
            },
            false,
            false);

        Assert.False(viewModel.HasGenerationReport);
        Assert.Null(presenter.Source);
        Assert.Contains("nach der automatischen Übernahme geändert", presenter.UnavailableMessage);
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
        Func<CancellationToken, Task>? reload = null,
        IAutomaticScheduleReportPresenter? reportPresenter = null)
    {
        AutomaticScheduleGenerationViewModel viewModel = CreateViewModel(
            actions,
            reload,
            reportPresenter);
        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext(),
            false,
            false);
        return viewModel;
    }

    private static AutomaticScheduleGenerationViewModel CreateViewModel(
        RecordingAutomaticScheduleGenerationActions actions,
        Func<CancellationToken, Task>? reload = null,
        IAutomaticScheduleReportPresenter? reportPresenter = null)
    {
        return new AutomaticScheduleGenerationViewModel(
            actions,
            reload ?? (_ => Task.CompletedTask),
            new RecordingUnexpectedErrorReporter(),
            reportPresenter);
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
