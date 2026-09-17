using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class ScheduleOverviewViewTests
{
    public static TheoryData<double, double> SupportedWindowSizes => new()
    {
        { 1160, 760 },
        { 900, 600 },
        { 1920, 1080 },
    };

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
                ScheduleCellViewModel serviceManagementCell =
                    ScheduleOverviewViewModelTests.SelectCell(
                    viewModel,
                    FakeScheduleDataAccess.ServiceManagementEmployeeId,
                    ScheduleOverviewViewModelTests.PeriodMonday);
                ServiceManagementAssignmentOptionViewModel office = Assert.Single(
                    serviceManagementCell.AssignmentOptions,
                    option => option.Snapshot.Kind
                            == ServiceManagementAssignmentSelectionKind.OfficeTime
                        && option.Snapshot.FirstSlot.Ordinal == 1
                        && option.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");
                viewModel.SetTyp1AssignmentCommand.ExecuteAsync(office)
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

    [Fact]
    public void Typ1PopupIsAnchoredToCellSelectsDirectlyAndClosesWithEscape()
    {
        RunInSta(() =>
        {
            FakeScheduleDataAccess dataAccess = new();
            ScheduleOverviewViewModel viewModel =
                ScheduleOverviewViewModelTests.CreateViewModel(dataAccess);
            viewModel.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            ScheduleCellViewModel cell = Assert.Single(
                Assert.Single(
                    viewModel.Employees,
                    employee => employee.EmployeeId
                        == FakeScheduleDataAccess.ServiceManagementEmployeeId).Cells,
                candidate => candidate.Date == ScheduleOverviewViewModelTests.PeriodMonday);
            ScheduleOverviewView view = new()
            {
                DataContext = viewModel,
            };
            Window window = new()
            {
                Content = view,
                Width = 1160,
                Height = 760,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
            };
            try
            {
                window.Show();
                window.Activate();
                view.UpdateLayout();
                Button cellButton = Assert.Single(
                    GetVisualDescendants(view).OfType<Button>(),
                    button => AutomationProperties.GetName(button) == cell.AutomationName);
                Popup popup = Assert.Single(
                    GetVisualDescendants(view).OfType<Popup>(),
                    candidate => ReferenceEquals(candidate.PlacementTarget, cellButton));
                popup.StaysOpen = true;

                Assert.True(cellButton.Command.CanExecute(cellButton.CommandParameter));
                cellButton.Command.Execute(cellButton.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                Assert.True(popup.IsOpen);
                Assert.Same(cellButton, popup.PlacementTarget);
                Assert.IsType<Button>(FocusManager.GetFocusedElement(popup.Child));
                Button office = Assert.Single(
                    GetVisualDescendants(popup.Child).OfType<Button>(),
                    button => button.DataContext
                            is ServiceManagementAssignmentOptionViewModel option
                        && option.Snapshot.Kind
                            == ServiceManagementAssignmentSelectionKind.OfficeTime
                        && option.Snapshot.FirstSlot.Ordinal == 1
                        && option.Snapshot.FirstSlot.ShiftTypeName == "Frühdienst");

                Assert.True(office.Command.CanExecute(office.CommandParameter));
                office.Command.Execute(office.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                Assert.Equal("B", viewModel.SelectedCell?.EntryDisplay);
                Assert.False(popup.IsOpen);

                ScheduleCellViewModel reloadedCell = Assert.IsType<ScheduleCellViewModel>(
                    viewModel.SelectedCell);
                view.UpdateLayout();
                Button reloadedButton = Assert.Single(
                    GetVisualDescendants(view).OfType<Button>(),
                    button => AutomationProperties.GetName(button)
                        == reloadedCell.AutomationName);
                Popup reopenedPopup = Assert.Single(
                    GetVisualDescendants(view).OfType<Popup>(),
                    candidate => ReferenceEquals(candidate.PlacementTarget, reloadedButton));
                reopenedPopup.StaysOpen = true;
                Assert.True(reloadedButton.Command.CanExecute(reloadedButton.CommandParameter));
                reloadedButton.Command.Execute(reloadedButton.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);
                Assert.True(reopenedPopup.IsOpen);
                Button currentOption = Assert.Single(
                    GetVisualDescendants(reopenedPopup.Child).OfType<Button>(),
                    button => button.DataContext
                        is ServiceManagementAssignmentOptionViewModel { IsCurrent: true });
                Assert.Same(
                    currentOption,
                    FocusManager.GetFocusedElement(reopenedPopup.Child));
                FrameworkElement popupContent = Assert.IsAssignableFrom<FrameworkElement>(
                    reopenedPopup.Child);
                PresentationSource source = Assert.IsAssignableFrom<PresentationSource>(
                    PresentationSource.FromVisual(popupContent));
                KeyEventArgs escape = new(
                    Keyboard.PrimaryDevice,
                    source,
                    Environment.TickCount,
                    Key.Escape)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent,
                };

                popupContent.RaiseEvent(escape);

                Assert.False(reloadedCell.IsAssignmentEditorOpen);
                Assert.False(reopenedPopup.IsOpen);
                Assert.True(escape.Handled);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void FeedbackOverlayKeepsLayoutStableAndUsesDistinctAccessibleStates()
    {
        RunInSta(() =>
        {
            ControlledScheduleFeedbackDelay delay = new();
            FakeScheduleDataAccess dataAccess = new();
            ScheduleOverviewViewModel viewModel =
                ScheduleOverviewViewModelTests.CreateViewModel(
                    dataAccess,
                    feedbackDelay: delay);
            viewModel.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            ScheduleOverviewView view = new()
            {
                DataContext = viewModel,
            };
            Size size = new(1160, 760);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();

            FrameworkElement selection = FindNamedElement(
                view,
                "Auswahl und Tagesaktionen");
            Border overlay = Assert.IsType<Border>(
                FindNamedElement(view, "Dienstplanmeldung Hinweis"));
            double selectionTopWithSuccess = GetTop(selection, view);

            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.False(overlay.IsHitTestVisible);
            Assert.True(Panel.GetZIndex(overlay) > 0);
            Assert.Equal(
                AutomationLiveSetting.Polite,
                AutomationProperties.GetLiveSetting(overlay));
            Assert.Equal(
                Color.FromRgb(0xEA, 0xF7, 0xEE),
                Assert.IsType<SolidColorBrush>(overlay.Background).Color);
            Assert.Contains(
                GetVisualDescendants(overlay).OfType<TextBlock>(),
                text => text.Text == "Hinweis");

            ScheduleOverviewViewModelTests.SelectCell(
                viewModel,
                FakeScheduleDataAccess.NormalEmployeeId,
                ScheduleOverviewViewModelTests.PeriodMonday);
            view.UpdateLayout();

            Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            Assert.Equal(selectionTopWithSuccess, GetTop(selection, view), 3);

            dataAccess.RejectNextChange = true;
            viewModel.SetFixedDayOffCommand.ExecuteAsync(null)
                .GetAwaiter().GetResult();
            view.UpdateLayout();

            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.Equal("Dienstplanmeldung Fehler", AutomationProperties.GetName(overlay));
            Assert.Equal(
                Color.FromRgb(0xFF, 0xF0, 0xF0),
                Assert.IsType<SolidColorBrush>(overlay.Background).Color);
            Assert.Contains(
                GetVisualDescendants(overlay).OfType<TextBlock>(),
                text => text.Text == "Fehler");
            Assert.Equal(selectionTopWithSuccess, GetTop(selection, view), 3);
        });
    }

    [Fact]
    public void UnifiedConfirmationOverlayTrapsFocusCancelsWithEscapeAndServesAllActions()
    {
        RunInSta(() =>
        {
            FakeScheduleDataAccess dataAccess = new();
            dataAccess.Add(
                FakeScheduleDataAccess.NormalEmployeeId,
                ScheduleOverviewViewModelTests.PeriodMonday,
                AvailabilityEntryKind.Vacation);
            RecordingAutomaticScheduleResetActions resetActions = new();
            ScheduleOverviewViewModel viewModel =
                ScheduleOverviewViewModelTests.CreateViewModel(
                    dataAccess,
                    resetActions: resetActions);
            viewModel.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            ScheduleOverviewViewModelTests.SelectCell(
                viewModel,
                FakeScheduleDataAccess.NormalEmployeeId,
                ScheduleOverviewViewModelTests.PeriodMonday);
            ScheduleOverviewView view = new()
            {
                DataContext = viewModel,
            };
            Window window = new()
            {
                Content = view,
                Width = 1160,
                Height = 760,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
            };
            try
            {
                window.Show();
                window.Activate();
                view.UpdateLayout();
                Button remove = Assert.Single(
                    GetVisualDescendants(view).OfType<Button>(),
                    button => Equals(button.Content, "Leeren (Entf)"));
                FocusManager.SetFocusedElement(view, remove);
                remove.Focus();
                remove.Command.Execute(remove.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                FrameworkElement overlay = FindNamedElement(
                    view,
                    "Modale Bestätigung für eine Änderung am Tagesfeld");
                Button cancel = Assert.Single(
                    GetVisualDescendants(overlay).OfType<Button>(),
                    button => AutomationProperties.GetName(button)
                        == "Bestätigung abbrechen");
                Button confirm = Assert.Single(
                    GetVisualDescendants(overlay).OfType<Button>(),
                    button => AutomationProperties.GetName(button)
                        == "Änderung am Tagesfeld bestätigen");
                Button load = Assert.Single(
                    GetVisualDescendants(view).OfType<Button>(),
                    button => Equals(button.Content, "Zeitraum anzeigen"));

                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.Contains(
                    "übrige Dienstplan",
                    AutomationProperties.GetHelpText(overlay),
                    StringComparison.Ordinal);
                Assert.Same(cancel, FocusManager.GetFocusedElement(overlay));
                Assert.False(load.IsEnabled);
                Assert.Equal("U", viewModel.SelectedCell?.EntryDisplay);
                Assert.Equal(0, dataAccess.ChangeCallCount);

                Assert.Contains(
                    GetVisualDescendants(overlay).OfType<Border>(),
                    border => KeyboardNavigation.GetTabNavigation(border)
                        == KeyboardNavigationMode.Cycle);

                RaiseEscape(overlay);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
                Assert.Same(remove, FocusManager.GetFocusedElement(view));
                Assert.Equal("U", viewModel.SelectedCell?.EntryDisplay);
                Assert.Equal(0, dataAccess.ChangeCallCount);

                remove.Command.Execute(remove.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);
                confirm.Command.Execute(confirm.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
                Assert.Same(remove, FocusManager.GetFocusedElement(view));
                Assert.Equal(string.Empty, viewModel.SelectedCell?.EntryDisplay);
                Assert.Equal(1, dataAccess.ChangeCallCount);

                viewModel.AutomaticReset.ApplyContext(
                    AutomaticScheduleResetTestData.AcceptedContext(9, 2),
                    false);
                view.UpdateLayout();
                Button requestReset = Assert.Single(
                    GetVisualDescendants(view).OfType<Button>(),
                    button => AutomationProperties.GetName(button)
                        == "Übernommenen automatischen Plan vollständig verwerfen");
                FocusManager.SetFocusedElement(view, requestReset);
                requestReset.Focus();
                requestReset.Command.Execute(requestReset.CommandParameter);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                Assert.Same(
                    overlay,
                    FindNamedElement(
                        view,
                        "Modale Bestätigung zum vollständigen Verwerfen des automatischen Plans"));
                Assert.Contains(
                    GetVisualDescendants(overlay).OfType<TextBlock>(),
                    text => text.Text.Contains(
                        "9 automatisch erzeugte Einteilungen",
                        StringComparison.Ordinal));
                Assert.Contains(
                    GetVisualDescendants(overlay).OfType<TextBlock>(),
                    text => text.Text.Contains("2 schwarze X", StringComparison.Ordinal));
                Assert.Contains(
                    GetVisualDescendants(overlay).OfType<Button>(),
                    button => Equals(button.Content, "Vollständig verwerfen"));
                Assert.False(requestReset.IsEnabled);

                RaiseEscape(overlay);
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);

                Assert.Equal(0, resetActions.CallCount);
                Assert.Same(requestReset, FocusManager.GetFocusedElement(view));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [MemberData(nameof(SupportedWindowSizes))]
    public void LoadedWorkspaceKeepsPlanningBeforePreparationAndLowerActionsReachable(
        double width,
        double height)
    {
        RunInSta(() =>
        {
            FakeScheduleDataAccess dataAccess = new(additionalNormalEmployees: 20);
            ScheduleOverviewViewModel viewModel =
                ScheduleOverviewViewModelTests.CreateViewModel(dataAccess);
            viewModel.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
            ScheduleOverviewView view = new()
            {
                DataContext = viewModel,
            };
            Size size = new(width, height);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(
                () => { },
                DispatcherPriority.ApplicationIdle);
            view.UpdateLayout();

            FrameworkElement selection = FindNamedElement(
                view,
                "Auswahl und Tagesaktionen");
            FrameworkElement planning = FindNamedElement(
                view,
                "Drei-Wochen-Planung");
            FrameworkElement preparation = FindNamedElement(
                view,
                "Planungsvorbereitung");
            FrameworkElement generation = FindNamedElement(
                view,
                "Automatische Plangenerierung und Vorschau");
            ScrollViewer workspace = Assert.IsType<ScrollViewer>(
                FindNamedElement(view, "Dienstplan Arbeitsbereich"));
            ScrollViewer planningScroller = Assert.Single(
                GetVisualDescendants(planning)
                    .OfType<ScrollViewer>(),
                scroller => !ReferenceEquals(scroller, workspace));

            Assert.True(GetTop(selection, view) < GetTop(planning, view));
            Assert.True(GetTop(planning, view) < GetTop(preparation, view));
            Assert.True(GetTop(preparation, view) < GetTop(generation, view));
            Assert.Equal(ScrollBarVisibility.Auto, workspace.VerticalScrollBarVisibility);
            Assert.Equal(
                ScrollBarVisibility.Disabled,
                planningScroller.VerticalScrollBarVisibility);
            Assert.True(workspace.ScrollableHeight > 0);
            Assert.Contains(
                GetVisualDescendants(planning).OfType<TextBlock>(),
                text => text.Text == "Test Person 20");

            workspace.ScrollToEnd();
            view.UpdateLayout();

            Assert.Equal(workspace.ScrollableHeight, workspace.VerticalOffset, 3);
            Assert.True(GetTop(generation, view) < view.ActualHeight);
            Assert.True(GetTop(generation, view) + generation.ActualHeight > 0);
        });
    }

    private static FrameworkElement FindNamedElement(
        DependencyObject parent,
        string automationName)
    {
        return Assert.Single(
            GetVisualDescendants(parent).OfType<FrameworkElement>(),
            element => AutomationProperties.GetName(element) == automationName);
    }

    private static double GetTop(
        FrameworkElement element,
        Visual ancestor)
    {
        return element.TransformToAncestor(ancestor).Transform(new Point()).Y;
    }

    private static void RaiseEscape(FrameworkElement target)
    {
        PresentationSource source = Assert.IsAssignableFrom<PresentationSource>(
            PresentationSource.FromVisual(target));
        KeyEventArgs escape = new(
            Keyboard.PrimaryDevice,
            source,
            Environment.TickCount,
            Key.Escape)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent,
        };

        target.RaiseEvent(escape);

        Assert.True(escape.Handled);
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
