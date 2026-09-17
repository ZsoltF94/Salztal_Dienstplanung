using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class AutomaticScheduleResetViewTests
{
    [Fact]
    public void AcceptedPlanRendersRequestAndLeavesConfirmationToParentOverlay()
    {
        RunInSta(() =>
        {
            AutomaticScheduleResetViewModel viewModel = new(
                new RecordingAutomaticScheduleResetActions(),
                _ => Task.CompletedTask,
                new RecordingUnexpectedErrorReporter());
            viewModel.ApplyContext(
                AutomaticScheduleResetTestData.AcceptedContext(9, 2),
                false);
            AutomaticScheduleResetView view = Render(viewModel);
            Button request = Assert.Single(
                Descendants(view).OfType<Button>(),
                button => Equals(button.Content, "Automatischen Plan _verwerfen"));
            KeyBinding[] bindings = view.InputBindings.OfType<KeyBinding>().ToArray();

            Assert.True(request.IsEnabled);
            Assert.Contains(bindings, binding =>
                binding.Key == Key.R && binding.Modifiers == ModifierKeys.Alt);
            viewModel.RequestCommand.Execute(null);
            view.UpdateLayout();
            string[] texts = Descendants(view)
                .OfType<TextBlock>()
                .Select(text => text.Text)
                .ToArray();
            Button[] buttons = Descendants(view).OfType<Button>().ToArray();

            Assert.DoesNotContain(texts, text => text.Contains(
                "9 automatisch erzeugte Einteilungen",
                StringComparison.Ordinal));
            Assert.DoesNotContain(buttons, button =>
                Equals(button.Content, "Vollst\u00e4ndig verwerfen"));
            Assert.DoesNotContain(buttons, button => Equals(button.Content, "Abbrechen"));
        });
    }

    private static AutomaticScheduleResetView Render(
        AutomaticScheduleResetViewModel viewModel)
    {
        AutomaticScheduleResetView view = new()
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
}
