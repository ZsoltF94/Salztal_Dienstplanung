using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class AutomaticScheduleResetViewModelTests
{
    [Fact]
    public void RequestRequiresAcceptedAutomaticScheduleAndShowsExactWarning()
    {
        AutomaticScheduleResetViewModel viewModel = CreateViewModel(
            new RecordingAutomaticScheduleResetActions());
        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext(),
            false);
        Assert.False(viewModel.RequestCommand.CanExecute(null));

        viewModel.ApplyContext(
            AutomaticScheduleResetTestData.AcceptedContext(12, 4),
            false);
        viewModel.RequestCommand.Execute(null);

        Assert.True(viewModel.IsConfirmationOpen);
        Assert.True(viewModel.IsInteractionActive);
        Assert.Contains("12 automatisch erzeugte Einteilungen", viewModel.ConfirmationMessage);
        Assert.Contains("4 schwarze X", viewModel.ConfirmationMessage);
        Assert.Contains("Typ1-Dienste, U, K, rote X", viewModel.ConfirmationMessage);
        Assert.Contains("nicht r\u00fcckg\u00e4ngig", viewModel.ConfirmationMessage);
    }

    [Fact]
    public void CancellationDoesNotWriteAndKeepsVisibleFeedback()
    {
        RecordingAutomaticScheduleResetActions actions = new();
        AutomaticScheduleResetViewModel viewModel = CreateAcceptedViewModel(actions);
        viewModel.RequestCommand.Execute(null);

        viewModel.CancelCommand.Execute(null);

        Assert.Equal(0, actions.CallCount);
        Assert.False(viewModel.IsConfirmationOpen);
        Assert.False(viewModel.IsInteractionActive);
        Assert.Contains("abgebrochen", viewModel.StatusMessage);
    }

    [Fact]
    public async Task ConfirmationDiscardsAndReloadsWorkspaceExactlyOnce()
    {
        RecordingAutomaticScheduleResetActions actions = new();
        int reloadCount = 0;
        AutomaticScheduleResetViewModel viewModel = CreateAcceptedViewModel(
            actions,
            _ =>
            {
                reloadCount++;
                return Task.CompletedTask;
            });
        viewModel.RequestCommand.Execute(null);

        await viewModel.ConfirmCommand.ExecuteAsync(null);

        Assert.Equal(1, actions.CallCount);
        Assert.Equal(1, reloadCount);
        Assert.Equal(
            AutomaticScheduleGenerationTestData.DraftId,
            actions.Request?.DraftId);
        Assert.Equal(3, actions.Request?.ExpectedDraftVersion);
        Assert.False(viewModel.IsInteractionActive);
        Assert.Contains("vollst\u00e4ndig verworfen", viewModel.StatusMessage);
        Assert.False(viewModel.HasErrorCode);
    }

    [Fact]
    public async Task ConflictDoesNotReloadAndShowsStableCode()
    {
        RecordingAutomaticScheduleResetActions actions = new()
        {
            Outcome = new AutomaticScheduleDiscardOutcome(
                AutomaticScheduleDiscardStatus.Conflict,
                "Der Entwurf wurde zwischenzeitlich ge\u00e4ndert."),
        };
        int reloadCount = 0;
        AutomaticScheduleResetViewModel viewModel = CreateAcceptedViewModel(
            actions,
            _ =>
            {
                reloadCount++;
                return Task.CompletedTask;
            });
        viewModel.RequestCommand.Execute(null);

        await viewModel.ConfirmCommand.ExecuteAsync(null);

        Assert.Equal(0, reloadCount);
        Assert.Contains("Conflict", viewModel.ErrorCodeDisplay);
        Assert.Contains("zwischenzeitlich", viewModel.StatusMessage);
    }

    [Fact]
    public async Task UnexpectedFailureIsReportedAndDoesNotReload()
    {
        InvalidOperationException failure = new("synthetic failure");
        RecordingAutomaticScheduleResetActions actions = new()
        {
            Exception = failure,
        };
        RecordingUnexpectedErrorReporter reporter = new();
        AutomaticScheduleResetViewModel viewModel = new(
            actions,
            _ => Task.CompletedTask,
            reporter);
        viewModel.ApplyContext(AutomaticScheduleResetTestData.AcceptedContext(), false);
        viewModel.RequestCommand.Execute(null);

        await viewModel.ConfirmCommand.ExecuteAsync(null);

        Assert.Same(failure, reporter.Exception);
        Assert.Equal("DiscardAutomaticSchedule", reporter.Operation);
        Assert.Contains("DesktopUnexpectedError", viewModel.ErrorCodeDisplay);
    }

    private static AutomaticScheduleResetViewModel CreateAcceptedViewModel(
        RecordingAutomaticScheduleResetActions actions,
        Func<CancellationToken, Task>? reload = null)
    {
        AutomaticScheduleResetViewModel viewModel = CreateViewModel(actions, reload);
        viewModel.ApplyContext(AutomaticScheduleResetTestData.AcceptedContext(), false);
        return viewModel;
    }

    private static AutomaticScheduleResetViewModel CreateViewModel(
        RecordingAutomaticScheduleResetActions actions,
        Func<CancellationToken, Task>? reload = null) =>
        new(
            actions,
            reload ?? (_ => Task.CompletedTask),
            new RecordingUnexpectedErrorReporter());
}
