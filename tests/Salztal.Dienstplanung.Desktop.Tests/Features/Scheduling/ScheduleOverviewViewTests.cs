using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class ScheduleOverviewViewTests
{
    [Fact]
    public void LoadedWorkspaceRendersStructuredStatesAndKeyboardBindings()
    {
        Exception? renderingException = null;
        IReadOnlyList<string> renderedTexts = [];
        IReadOnlyList<Key> shortcutKeys = [];
        Brush? fixedDayOffForeground = null;
        Thread thread = new(() =>
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.UnhandledException += OnUnhandledException;
            try
            {
                FakeScheduleDataAccess dataAccess = new();
                dataAccess.Add(
                    FakeScheduleDataAccess.NormalEmployeeId,
                    ScheduleOverviewViewModelTests.PeriodMonday,
                    AvailabilityEntryKind.Vacation);
                dataAccess.Add(
                    FakeScheduleDataAccess.NormalEmployeeId,
                    ScheduleOverviewViewModelTests.PeriodMonday.AddDays(1),
                    AvailabilityEntryKind.Sickness);
                dataAccess.Add(
                    FakeScheduleDataAccess.NormalEmployeeId,
                    ScheduleOverviewViewModelTests.PeriodMonday.AddDays(2),
                    AvailabilityEntryKind.FixedDayOff);
                ScheduleOverviewViewModel viewModel =
                    ScheduleOverviewViewModelTests.CreateViewModel(dataAccess);
                viewModel.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
                ScheduleOverviewViewModelTests.SelectCell(
                    viewModel,
                    FakeScheduleDataAccess.ServiceManagementEmployeeId,
                    ScheduleOverviewViewModelTests.PeriodMonday);
                ServiceManagementAssignmentOptionViewModel office = Assert.Single(
                    viewModel.Typ1Editor.Options,
                    option => option.Snapshot.Kind
                            == ServiceManagementAssignmentSelectionKind.OfficeTime
                        && option.Snapshot.FirstSlot.Ordinal == 1
                        && option.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");
                viewModel.Typ1Editor.SelectedOption = office;
                viewModel.SetTyp1AssignmentCommand.ExecuteAsync(null)
                    .GetAwaiter().GetResult();
                ScheduleOverviewView view = new()
                {
                    DataContext = viewModel,
                };
                Size size = new(1920, 1080);
                view.Measure(size);
                view.Arrange(new Rect(size));
                view.UpdateLayout();
                dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                view.UpdateLayout();

                renderedTexts = GetVisualDescendants(view)
                    .OfType<TextBlock>()
                    .Select(text => text.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .ToArray();
                shortcutKeys = view.InputBindings
                    .OfType<KeyBinding>()
                    .Select(binding => binding.Key)
                    .ToArray();
                Button fixedDayOff = Assert.Single(
                    GetVisualDescendants(view).OfType<Button>(),
                    button => button.Content as string == "X");
                fixedDayOffForeground = fixedDayOff.Foreground;
            }
            catch (Exception exception)
            {
                renderingException = exception;
            }
            finally
            {
                dispatcher.UnhandledException -= OnUnhandledException;
                dispatcher.InvokeShutdown();
            }

            void OnUnhandledException(
                object sender,
                DispatcherUnhandledExceptionEventArgs eventArgs)
            {
                renderingException ??= eventArgs.Exception;
                eventArgs.Handled = true;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);

        thread.Start();

        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(renderingException);
        Assert.Contains("Dienstplan SER", renderedTexts);
        Assert.Contains("Sarah Leitung", renderedTexts);
        Assert.Contains("Erika Muster", renderedTexts);
        Assert.Contains("U", renderedTexts);
        Assert.Contains("K", renderedTexts);
        Assert.Contains("X", renderedTexts);
        Assert.Contains("B", renderedTexts);
        Assert.Contains("Noch nicht vorbereitet", renderedTexts);
        Assert.Contains(renderedTexts, text => text.Contains("Typ1 fehlt", StringComparison.Ordinal));
        Assert.Equal([Key.U, Key.K, Key.X, Key.Delete], shortcutKeys);
        Assert.Equal(
            Color.FromRgb(0xC6, 0x28, 0x28),
            Assert.IsType<SolidColorBrush>(fixedDayOffForeground).Color);
    }

    private static IEnumerable<DependencyObject> GetVisualDescendants(
        DependencyObject parent)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (DependencyObject descendant in GetVisualDescendants(child))
            {
                yield return descendant;
            }
        }
    }
}
