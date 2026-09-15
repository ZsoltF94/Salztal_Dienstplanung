using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.EmployeeTypes;

public sealed class EmployeeTypeOverviewViewTests
{
    [Fact]
    public void ConstructorLoadsAllStaticResources()
    {
        Exception? constructionException = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = new EmployeeTypeOverviewView();
            }
            catch (Exception exception)
            {
                constructionException = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);

        thread.Start();

        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(constructionException);
    }

    [Fact]
    public async Task LoadedReadOnlyTypeValuesRenderWithoutDispatcherFailure()
    {
        EmployeeTypeCatalogSnapshot catalog = await new GetEmployeeTypeCatalogQuery(
            new SingleEmployeeTypeReader()).ExecuteAsync(TestContext.Current.CancellationToken);
        EmployeeTypeOverviewItemViewModel item = new(Assert.Single(catalog.EmployeeTypes));
        Exception? renderingException = null;
        IReadOnlyList<(string TargetName, string AvailabilityDisplay)> renderedEligibilities = [];
        IReadOnlyList<string> renderedTexts = [];
        Thread thread = new(() =>
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.UnhandledException += OnUnhandledException;

            try
            {
                EmployeeTypeOverviewView view = new()
                {
                    DataContext = new RenderingDataContext(item),
                };
                Size availableSize = new(1160, 720);

                view.Measure(availableSize);
                view.Arrange(new Rect(availableSize));
                view.UpdateLayout();
                dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                view.UpdateLayout();
                renderedEligibilities = GetVisualDescendants(view)
                    .OfType<Grid>()
                    .Select(grid => grid.Children.OfType<TextBlock>()
                        .Select(textBlock => textBlock.Text)
                        .ToArray())
                    .Where(texts => texts.Length == 2)
                    .Select(texts => (
                        TargetName: texts[0],
                        AvailabilityDisplay: texts[1]))
                    .Where(pair => item.ShiftEligibilities.Any(
                        eligibility => eligibility.AvailabilityDisplay == pair.AvailabilityDisplay))
                    .ToArray();
                renderedTexts = GetVisualDescendants(view)
                    .OfType<TextBlock>()
                    .Select(textBlock => textBlock.Text)
                    .ToArray();
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
        Assert.All(
            item.ShiftEligibilities,
            expected => Assert.Contains(
                renderedEligibilities,
                actual => actual == (expected.TargetName, expected.AvailabilityDisplay)));
        Assert.Contains(RenderingDataContext.ExpectedSuccessMessage, renderedTexts);
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

    private sealed class RenderingDataContext
    {
        public const string ExpectedSuccessMessage =
            "Die Änderungen am Mitarbeitertyp wurden gespeichert.";

        public RenderingDataContext(EmployeeTypeOverviewItemViewModel employeeType)
        {
            EmployeeTypes = [employeeType];
            SelectedEmployeeType = employeeType;
        }

        public IReadOnlyList<EmployeeTypeOverviewItemViewModel> EmployeeTypes { get; }

        public EmployeeTypeOverviewItemViewModel SelectedEmployeeType { get; set; }

        public bool IsLoading { get; }

        public bool HasLoadError { get; }

        public bool IsEmpty { get; }

        public bool ShowSelectedDetails { get; } = true;

        public bool IsEditorOpen { get; }

        public bool IsDeletionConfirmationOpen { get; }

        public bool HasSuccessMessage { get; } = true;

        public string SuccessMessage { get; } = ExpectedSuccessMessage;
    }

    private sealed class SingleEmployeeTypeReader : IEmployeeReader
    {
        public Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new EmployeeReadData(
                [],
                [InitialEmployeeTypeCatalog.Type25],
                new ServiceCatalogData(
                    InitialWorkLocationCatalog.All,
                    InitialShiftTypeCatalog.All,
                    InitialShiftPatternCatalog.SplitShift,
                    InitialShiftPatternCatalog.ReliefShift)));
        }
    }
}
