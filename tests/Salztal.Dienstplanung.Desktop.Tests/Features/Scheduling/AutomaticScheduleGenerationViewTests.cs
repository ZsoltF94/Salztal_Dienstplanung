using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class AutomaticScheduleGenerationViewTests
{
    [Fact]
    public void BlockedStateRendersReasonAndKeyboardBindings()
    {
        RunInSta(() =>
        {
            AutomaticScheduleGenerationViewModel viewModel = CreateViewModel(
                new RecordingAutomaticScheduleGenerationActions());
            AutomaticScheduleGenerationView view = Render(viewModel);
            string[] texts = ReadTexts(view);
            Button start = Assert.Single(
                Descendants(view).OfType<Button>(),
                button => Equals(button.Content, "_Plan erzeugen"));
            KeyBinding[] bindings = view.InputBindings.OfType<KeyBinding>().ToArray();

            Assert.Contains("Automatische Planung", texts);
            Assert.Contains(texts, text => text.Contains("Lade zuerst", StringComparison.Ordinal));
            Assert.False(start.IsEnabled);
            Assert.Contains(bindings, binding =>
                binding.Key == Key.G && binding.Modifiers == ModifierKeys.Alt);
            Assert.Contains(bindings, binding => binding.Key == Key.Escape);
        });
    }

    [Fact]
    public void RunningStateRendersIndeterminateProgressAndCancellation()
    {
        RunInSta(() =>
        {
            RecordingAutomaticScheduleGenerationActions actions = new();
            AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(actions);
            Task operation = viewModel.StartCommand.ExecuteAsync(null);
            SpinWait.SpinUntil(() => actions.GenerateCallCount == 1, TimeSpan.FromSeconds(2));
            AutomaticScheduleGenerationView view = Render(viewModel);
            ProgressBar progress = Assert.Single(Descendants(view).OfType<ProgressBar>());
            Button cancel = Assert.Single(
                Descendants(view).OfType<Button>(),
                button => Equals(button.Content, "Abbrechen"));

            Assert.True(progress.IsIndeterminate);
            Assert.True(cancel.IsEnabled);
            Assert.DoesNotContain(ReadTexts(view), text => text.Contains('%'));

            viewModel.CancelCommand.Execute(null);
            WaitWithDispatcher(operation);
        });
    }

    [Fact]
    public void PreviewRendersSummaryAndBothConsciousActions()
    {
        RunInSta(() =>
        {
            RecordingAutomaticScheduleGenerationActions actions = new();
            actions.CompleteGeneration(AutomaticScheduleGenerationTestData.SuccessOutcome());
            AutomaticScheduleGenerationViewModel viewModel = CreateReadyViewModel(actions);
            viewModel.StartCommand.ExecuteAsync(null).GetAwaiter().GetResult();
            AutomaticScheduleGenerationView view = Render(viewModel);
            string[] texts = ReadTexts(view);
            Button[] buttons = Descendants(view).OfType<Button>().ToArray();

            Assert.Contains("Optimaler Vorschlag", texts);
            Assert.Contains(texts, text => text.Contains("Offener Bedarf", StringComparison.Ordinal));
            Assert.Contains(texts, text => text.Contains("System 10", StringComparison.Ordinal));
            Assert.Contains(buttons, button => Equals(button.Content, "Plan übernehmen"));
            Assert.Contains(buttons, button => Equals(button.Content, "Vorschlag verwerfen"));
        });
    }

    private static AutomaticScheduleGenerationViewModel CreateReadyViewModel(
        RecordingAutomaticScheduleGenerationActions actions)
    {
        AutomaticScheduleGenerationViewModel viewModel = CreateViewModel(actions);
        viewModel.ApplyContext(
            AutomaticScheduleGenerationTestData.ReadyContext(),
            false,
            false);
        return viewModel;
    }

    private static AutomaticScheduleGenerationViewModel CreateViewModel(
        RecordingAutomaticScheduleGenerationActions actions)
    {
        return new AutomaticScheduleGenerationViewModel(
            actions,
            _ => Task.CompletedTask,
            new RecordingUnexpectedErrorReporter());
    }

    private static AutomaticScheduleGenerationView Render(
        AutomaticScheduleGenerationViewModel viewModel)
    {
        AutomaticScheduleGenerationView view = new()
        {
            DataContext = viewModel,
        };
        Size size = new(1200, 500);
        view.Measure(size);
        view.Arrange(new Rect(size));
        view.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        view.UpdateLayout();
        return view;
    }

    private static string[] ReadTexts(DependencyObject view)
    {
        return Descendants(view)
            .OfType<TextBlock>()
            .Select(text => text.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static void RunInSta(Action action)
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(dispatcher));
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                dispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(failure);
    }

    private static void WaitWithDispatcher(Task task)
    {
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        DispatcherFrame frame = new();
        _ = task.ContinueWith(
            _ => dispatcher.BeginInvoke(() => frame.Continue = false),
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);
        Dispatcher.PushFrame(frame);
        task.GetAwaiter().GetResult();
    }
}
