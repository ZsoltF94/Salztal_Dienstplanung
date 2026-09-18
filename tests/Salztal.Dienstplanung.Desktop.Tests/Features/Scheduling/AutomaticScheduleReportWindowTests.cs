using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

public sealed class AutomaticScheduleReportWindowTests
{
    [Theory]
    [InlineData(860, 560)]
    [InlineData(1180, 780)]
    public void CurrentReportRendersSeparatePlanningAndGenerationAreas(
        double width,
        double height)
    {
        RunInSta(() =>
        {
            AutomaticScheduleReportSource source = CreateSource();
            AutomaticScheduleReportWindowViewModel viewModel =
                AutomaticScheduleReportWindowViewModel.Current(source);
            AutomaticScheduleReportWindow window = Render(viewModel, width, height);
            try
            {
                TabItem[] tabs = Descendants(window).OfType<TabItem>().ToArray();
                DataGrid demandGrid = Assert.Single(
                    Descendants(window).OfType<DataGrid>());
                ComboBox demandWeek = Assert.Single(
                    Descendants(window).OfType<ComboBox>());

                Assert.True(viewModel.HasPlanningReport);
                Assert.True(viewModel.HasTechnicalDetails);
                Assert.Contains(tabs, tab => Equals(tab.Header, "Planung"));
                Assert.Contains(tabs, tab => Equals(tab.Header, "Generierung"));
                Assert.Contains(tabs, tab => Equals(tab.Header, "Bedarfe und Deckung"));
                Assert.Contains(tabs, tab => Equals(tab.Header, "Personen und Dienste"));
                Assert.Equal(
                    "Bedarfsdeckung",
                    System.Windows.Automation.AutomationProperties.GetName(demandGrid));
                Assert.True(demandWeek.Focusable);

                TabItem employeeTab = Assert.Single(
                    tabs,
                    tab => Equals(tab.Header, "Personen und Dienste"));
                employeeTab.IsSelected = true;
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);
                window.UpdateLayout();
                DataGrid employeeGrid = Assert.Single(
                    Descendants(window).OfType<DataGrid>());
                ComboBox[] employeeFilters = Descendants(window)
                    .OfType<ComboBox>()
                    .ToArray();
                Assert.Equal(
                    "Personen und Dienstverteilung",
                    System.Windows.Automation.AutomationProperties.GetName(employeeGrid));
                Assert.True(employeeGrid.Columns.Count >= 7);
                Assert.Equal(2, employeeFilters.Length);
                Assert.All(employeeFilters, filter => Assert.True(filter.Focusable));

                TabItem generationTab = Assert.Single(
                    tabs,
                    tab => Equals(tab.Header, "Generierung"));
                generationTab.IsSelected = true;
                Dispatcher.CurrentDispatcher.Invoke(
                    () => { },
                    DispatcherPriority.ApplicationIdle);
                window.UpdateLayout();

                Assert.True(window.ActualWidth >= window.MinWidth);
                Assert.True(window.ActualHeight >= window.MinHeight);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void EscapeClosesReportWindow()
    {
        RunInSta(() =>
        {
            AutomaticScheduleReportWindow window = Render(
                AutomaticScheduleReportWindowViewModel.Current(CreateSource()),
                860,
                560);
            KeyEventArgs args = new(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(window),
                Environment.TickCount,
                Key.Escape)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            };

            window.RaiseEvent(args);

            Assert.True(args.Handled);
            Assert.False(window.IsVisible);
        });
    }

    [Fact]
    public void FailedRunExplainsWhyPlanningValuesAreAbsent()
    {
        AutomaticScheduleGenerationReport report =
            AutomaticScheduleGenerationTestData.TechnicalFailureOutcome().Report;
        AutomaticScheduleReportWindowViewModel viewModel =
            AutomaticScheduleReportWindowViewModel.Current(new AutomaticScheduleReportSource(
                "attempt:failure",
                "Letzter Generierungsversuch ohne Vorschlag",
                report));

        Assert.False(viewModel.HasPlanningReport);
        Assert.True(viewModel.HasNoPlanningReport);
        Assert.Contains("keinen zulässigen Vorschlag", viewModel.PlanningUnavailableDisplay);
        Assert.NotNull(viewModel.Generation);
    }

    [Fact]
    public void TimeLimitedReportExposesExplanationAndIntermediateClassification()
    {
        AutomaticScheduleGenerationReportViewModel viewModel = new(
            AutomaticScheduleGenerationTestData.TimeLimitedFeasibleOutcome().Report);

        AutomaticScheduleGenerationPhaseViewModel input = Assert.Single(
            viewModel.Phases,
            phase => phase.Name == "Eingangs- und Strukturprüfung");
        Assert.Equal("Abgeschlossen", input.StatusDisplay);
        Assert.Contains("vollständig abgeschlossen", input.Explanation);
        AutomaticScheduleGenerationPhaseViewModel regular = Assert.Single(
            viewModel.Phases,
            phase => phase.Name == "Reguläre Bedarfsdeckung");
        Assert.Equal("Zulässig, nicht optimal bewiesen", regular.StatusDisplay);
        Assert.Contains("Zeitgrenze von 120,000 s", regular.Explanation);
        Assert.Contains(
            "vollständig berührte reguläre Bedarfsplätze",
            regular.Explanation);
        Assert.Contains("zulässige Zwischenstand wurde behalten", regular.Explanation);
        Assert.True(regular.HasMetricContext);
        Assert.Contains("nicht als Optimum bewiesen", regular.MetricContext);
        AutomaticScheduleGenerationMetricViewModel uncovered = Assert.Single(
            regular.Metrics,
            metric => metric.Name
                == "Nach regulärer Deckung offene Mitarbeiterminuten");
        Assert.Equal("4 Std.", uncovered.AchievedDisplay);
        AutomaticScheduleGenerationPhaseViewModel following = Assert.Single(
            viewModel.Phases,
            phase => phase.Name == "Zusätzliche Spr-Notfalldeckung");
        Assert.Equal("Keine aufgezeichnete Ausführung", following.StatusDisplay);
        Assert.Contains("Nicht begonnen", following.Explanation);
        Assert.Contains("Reguläre Bedarfsdeckung", following.Explanation);
    }

    [Fact]
    public void LegacyReportShowsMissingDetailsWithoutInventedTimeout()
    {
        AutomaticScheduleGenerationReportViewModel viewModel = new(
            AutomaticScheduleGenerationTestData.LegacyInterruptedOutcome().Report);

        AutomaticScheduleGenerationPhaseViewModel regular = Assert.Single(
            viewModel.Phases,
            phase => phase.Name == "Reguläre Bedarfsdeckung");
        Assert.Contains("älteren Lauf nicht gespeichert", regular.Explanation);
        Assert.DoesNotContain("Zeitgrenze", regular.Explanation);
        AutomaticScheduleGenerationPhaseViewModel following = Assert.Single(
            viewModel.Phases,
            phase => phase.Name == "Zusätzliche Spr-Notfalldeckung");
        Assert.Contains("Nicht begonnen", following.Explanation);
        Assert.Contains("älteren Lauf nicht gespeichert", following.Explanation);
        Assert.DoesNotContain("Zeitgrenze", following.Explanation);
    }

    [Theory]
    [InlineData(860, 560)]
    [InlineData(1180, 780)]
    public void InterruptedReportRendersExplanationsAndScrollableDetails(
        double width,
        double height)
    {
        RunInSta(() =>
        {
            AutomaticScheduleReportWindow window = Render(
                AutomaticScheduleReportWindowViewModel.Current(CreateSource(
                    AutomaticScheduleGenerationTestData
                        .TimeLimitedFeasibleOutcome())),
                width,
                height);
            try
            {
                SelectGenerationTab(window);
                Expander input = Assert.Single(
                    Descendants(window).OfType<Expander>(),
                    item => item.DataContext
                            is AutomaticScheduleGenerationPhaseViewModel phase
                        && phase.Name == "Eingangs- und Strukturprüfung");
                TextBlock inputHeader = Assert.IsType<TextBlock>(input.Header);
                Assert.Contains(
                    inputHeader.Inlines.OfType<Run>(),
                    run => run.Text == "Abgeschlossen");
                ExpandPhase(window, "Reguläre Bedarfsdeckung");
                ExpandPhase(window, "Zusätzliche Spr-Notfalldeckung");
                window.UpdateLayout();

                ScrollViewer generationScroll = Assert.Single(
                    Descendants(window).OfType<ScrollViewer>(),
                    scroll => System.Windows.Automation.AutomationProperties.GetName(
                        scroll) == "Generierungsbericht mit Phasendetails");
                Assert.Equal(
                    ScrollBarVisibility.Auto,
                    generationScroll.HorizontalScrollBarVisibility);
                Assert.Equal(
                    ScrollBarVisibility.Auto,
                    generationScroll.VerticalScrollBarVisibility);
                TextBlock[] explanations = Descendants(window)
                    .OfType<TextBlock>()
                    .Where(text => System.Windows.Automation.AutomationProperties.GetName(
                        text) == "Phasenerklärung")
                    .ToArray();
                Assert.Contains(explanations, text =>
                    text.Text.Contains("Zeitgrenze von 120,000 s", StringComparison.Ordinal)
                    && text.Text.Contains(
                        "zulässige Zwischenstand wurde behalten",
                        StringComparison.Ordinal));
                Assert.Contains(explanations, text =>
                    text.Text.Contains("Nicht begonnen", StringComparison.Ordinal)
                    && text.Text.Contains(
                        "Reguläre Bedarfsdeckung",
                        StringComparison.Ordinal));
                Assert.Contains(
                    Descendants(window).OfType<TextBlock>(),
                    text => System.Windows.Automation.AutomationProperties.GetName(
                            text) == "Zwischenstandseinordnung"
                        && text.Text.Contains(
                            "nicht als Optimum bewiesen",
                            StringComparison.Ordinal));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void LegacyReportRendersMissingDetailBoundaryAfterRealTabSwitch()
    {
        RunInSta(() =>
        {
            AutomaticScheduleReportWindow window = Render(
                AutomaticScheduleReportWindowViewModel.Current(CreateSource(
                    AutomaticScheduleGenerationTestData.LegacyInterruptedOutcome())),
                860,
                560);
            try
            {
                SelectGenerationTab(window);
                ExpandPhase(window, "Reguläre Bedarfsdeckung");
                ExpandPhase(window, "Zusätzliche Spr-Notfalldeckung");
                window.UpdateLayout();

                TextBlock[] explanations = Descendants(window)
                    .OfType<TextBlock>()
                    .Where(text => System.Windows.Automation.AutomationProperties.GetName(
                        text) == "Phasenerklärung")
                    .ToArray();
                Assert.Contains(explanations, text =>
                    text.Text.Contains(
                        "älteren Lauf nicht gespeichert",
                        StringComparison.Ordinal));
                Assert.DoesNotContain(explanations, text =>
                    text.Text.Contains("Zeitgrenze", StringComparison.Ordinal));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void CoordinatorReusesRestoresAndInvalidatesSingleWindow()
    {
        RunInSta(() =>
        {
            AutomaticScheduleReportWindowCoordinator coordinator = new();
            Window owner = new();
            owner.Show();
            coordinator.AttachOwner(owner);
            try
            {
                coordinator.SetSource(CreateSource(), string.Empty);
                coordinator.ShowCurrent();
                AutomaticScheduleReportWindow first = Assert.IsType<
                    AutomaticScheduleReportWindow>(coordinator.Window);

                Assert.Same(owner, first.Owner);
                Assert.True(owner.IsEnabled);
                first.WindowState = WindowState.Minimized;
                coordinator.ShowCurrent();

                Assert.Same(first, coordinator.Window);
                Assert.Equal(WindowState.Normal, first.WindowState);
                Assert.True(coordinator.IsOpen);
                Assert.True(owner.IsEnabled);

                coordinator.SetSource(
                    null,
                    "Der Entwurf wurde nach der automatischen Übernahme geändert.");

                Assert.False(coordinator.CurrentViewModel!.IsCurrent);
                Assert.Contains("geändert", coordinator.CurrentViewModel.UnavailableMessage);
                Assert.Same(first, coordinator.Window);

                coordinator.Close();
                Assert.False(coordinator.IsOpen);
            }
            finally
            {
                coordinator.Close();
                owner.Close();
            }
        });
    }

    private static AutomaticScheduleReportSource CreateSource(
        AutomaticScheduleGenerationOutcome? outcome = null)
    {
        AutomaticScheduleGenerationReport report = (outcome
            ?? AutomaticScheduleGenerationTestData.SuccessOutcome()).Report;
        return new AutomaticScheduleReportSource(
            "preview:synthetic",
            "Flüchtiger Generierungsvorschlag",
            report);
    }

    private static void SelectGenerationTab(AutomaticScheduleReportWindow window)
    {
        TabItem generationTab = Assert.Single(
            Descendants(window).OfType<TabItem>(),
            tab => Equals(tab.Header, "Generierung"));
        generationTab.IsSelected = true;
        Dispatcher.CurrentDispatcher.Invoke(
            () => { },
            DispatcherPriority.ApplicationIdle);
        window.UpdateLayout();
    }

    private static void ExpandPhase(
        AutomaticScheduleReportWindow window,
        string phaseName)
    {
        Expander expander = Assert.Single(
            Descendants(window).OfType<Expander>(),
            item => item.DataContext is AutomaticScheduleGenerationPhaseViewModel phase
                && phase.Name == phaseName);
        expander.IsExpanded = true;
        Dispatcher.CurrentDispatcher.Invoke(
            () => { },
            DispatcherPriority.ApplicationIdle);
    }

    private static AutomaticScheduleReportWindow Render(
        AutomaticScheduleReportWindowViewModel viewModel,
        double width,
        double height)
    {
        AutomaticScheduleReportWindow window = new()
        {
            DataContext = viewModel,
            Width = width,
            Height = height,
        };
        window.Show();
        Dispatcher.CurrentDispatcher.Invoke(
            () => { },
            DispatcherPriority.ApplicationIdle);
        window.UpdateLayout();
        return window;
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
