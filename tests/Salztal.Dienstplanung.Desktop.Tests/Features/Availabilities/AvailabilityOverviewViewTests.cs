using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Desktop.Features.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Availabilities;

public sealed class AvailabilityOverviewViewTests
{
    private static readonly DateOnly PeriodMonday = new(2026, 12, 21);

    [Fact]
    public void LoadedPeriodRendersTableValuesRedXAndKeyboardBindings()
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
                FakeAvailabilityDataAccess dataAccess = new();
                dataAccess.Add(
                    FakeAvailabilityDataAccess.NormalEmployeeId,
                    PeriodMonday,
                    AvailabilityEntryKind.Vacation);
                dataAccess.Add(
                    FakeAvailabilityDataAccess.NormalEmployeeId,
                    PeriodMonday.AddDays(1),
                    AvailabilityEntryKind.Sickness);
                dataAccess.Add(
                    FakeAvailabilityDataAccess.NormalEmployeeId,
                    PeriodMonday.AddDays(2),
                    AvailabilityEntryKind.FixedDayOff);
                AvailabilityOverviewViewModel viewModel = new(
                    new GetAvailabilityPeriodQuery(dataAccess),
                    new SaveAvailabilityEntryCommand(dataAccess, dataAccess),
                    new RemoveAvailabilityEntryCommand(dataAccess, dataAccess),
                    PeriodMonday,
                    new RecordingUnexpectedErrorReporter());
                viewModel.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
                AvailabilityOverviewView view = new()
                {
                    DataContext = viewModel,
                };
                Size size = new(1920, 900);
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
        Assert.Contains("Erika Muster", renderedTexts);
        Assert.Contains("Typ25 – Restaurant - 25 Stunden", renderedTexts);
        Assert.Contains("U", renderedTexts);
        Assert.Contains("K", renderedTexts);
        Assert.Contains("X", renderedTexts);
        Assert.Contains("Soll: 25:00 Std.", renderedTexts);
        Assert.True(
            renderedTexts.Contains("Wirksam: 15:00 Std."),
            string.Join(Environment.NewLine, renderedTexts));
        Assert.Equal([Key.U, Key.K, Key.X, Key.Delete], shortcutKeys);
        Assert.Equal(Color.FromRgb(0xC6, 0x28, 0x28),
            Assert.IsType<SolidColorBrush>(fixedDayOffForeground).Color);
    }

    private static IEnumerable<DependencyObject> GetVisualDescendants(DependencyObject parent)
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
