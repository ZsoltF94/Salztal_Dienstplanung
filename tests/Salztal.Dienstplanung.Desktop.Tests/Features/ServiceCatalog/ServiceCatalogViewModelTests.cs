using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.ServiceCatalog;

public sealed class ServiceCatalogViewModelTests
{
    [Fact]
    public async Task LoadCommandWhenCatalogExistsShowsLocationsAndSelectsFirstEntry()
    {
        FakeServiceCatalogReader reader = new();
        FakeWorkLocationUpdateStore updateStore = new();
        CollectingUnexpectedErrorReporter errorReporter = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(reader, updateStore, errorReporter);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.WorkLocations.Count);
        Assert.NotNull(viewModel.SelectedWorkLocation);
        Assert.Equal("Cafeteria", viewModel.SelectedWorkLocation.Name);
        Assert.Equal(2, viewModel.SelectedWorkLocation.ShiftTypes.Count);
        Assert.NotNull(viewModel.SplitShiftPattern);
        Assert.Equal("D", viewModel.SplitShiftPattern.DisplayCode);
        Assert.Equal("3 Stunden", viewModel.SplitShiftPattern.StandardBreakText);
        Assert.NotNull(viewModel.ReliefShiftPattern);
        Assert.Equal("Spr", viewModel.ReliefShiftPattern.DisplayCode);
        Assert.Equal("Blau", viewModel.ReliefShiftPattern.DisplayColorName);
        Assert.Equal("Samstag", viewModel.ReliefShiftPattern.AllowedDayName);
        Assert.True(viewModel.HasWorkLocations);
        Assert.False(viewModel.IsEmpty);
        Assert.False(viewModel.HasLoadError);
        Assert.Empty(errorReporter.Reports);
    }

    [Fact]
    public async Task LoadCommandWhileReaderWaitsShowsWorkingState()
    {
        TaskCompletionSource<ServiceCatalogData> completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        FakeServiceCatalogReader reader = new()
        {
            LoadHandler = cancellationToken => completion.Task.WaitAsync(cancellationToken),
        };
        ServiceCatalogViewModel viewModel = CreateViewModel(
            reader,
            new FakeWorkLocationUpdateStore(),
            new CollectingUnexpectedErrorReporter());

        Task loading = viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsLoading);
        Assert.False(viewModel.IsEmpty);
        completion.SetResult(FakeServiceCatalogReader.CreateInitialData());
        await loading;
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadCommandWhenCatalogIsEmptyShowsEmptyState()
    {
        FakeServiceCatalogReader reader = new()
        {
            LoadHandler = _ => Task.FromResult(
                FakeServiceCatalogReader.CreateInitialData([])),
        };
        ServiceCatalogViewModel viewModel = CreateViewModel(
            reader,
            new FakeWorkLocationUpdateStore(),
            new CollectingUnexpectedErrorReporter());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.WorkLocations);
        Assert.Null(viewModel.SelectedWorkLocation);
        Assert.True(viewModel.IsEmpty);
        Assert.False(viewModel.HasWorkLocations);
    }

    [Fact]
    public async Task LoadCommandWhenReaderFailsShowsGermanErrorAndReportsTechnicalFailure()
    {
        InvalidOperationException failure = new("Synthetic load failure.");
        FakeServiceCatalogReader reader = new()
        {
            LoadHandler = _ => Task.FromException<ServiceCatalogData>(failure),
        };
        CollectingUnexpectedErrorReporter errorReporter = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            reader,
            new FakeWorkLocationUpdateStore(),
            errorReporter);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasLoadError);
        Assert.Contains("nicht geladen", viewModel.LoadErrorMessage, StringComparison.Ordinal);
        Assert.False(viewModel.IsEmpty);
        Assert.Empty(viewModel.WorkLocations);
        (Exception exception, string operation) = Assert.Single(errorReporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("LoadServiceCatalog", operation);
    }

    [Fact]
    public async Task SaveCommandWhenInputIsValidUpdatesLocationAndShowsConfirmation()
    {
        FakeWorkLocationUpdateStore updateStore = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            updateStore,
            new CollectingUnexpectedErrorReporter());
        await viewModel.LoadCommand.ExecuteAsync(null);
        WorkLocationEditorViewModel editor = Assert.IsType<WorkLocationEditorViewModel>(
            viewModel.SelectedWorkLocation);
        editor.Name = "Cafeteria am Park";
        editor.SelectedColor = Assert.Single(
            editor.ColorOptions,
            color => color.Code == "red");

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Cafeteria am Park", updateStore.Replacement?.Name.Value);
        Assert.Equal("red", updateStore.Replacement?.Color.Code);
        Assert.True(editor.HasSuccessMessage);
        Assert.False(editor.HasError);
        Assert.False(editor.HasChanges);
        Assert.False(editor.IsSaving);
    }

    [Fact]
    public async Task SaveCommandWhenNameIsEmptyShowsValidationWithoutStoreAccess()
    {
        FakeWorkLocationUpdateStore updateStore = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            updateStore,
            new CollectingUnexpectedErrorReporter());
        await viewModel.LoadCommand.ExecuteAsync(null);
        WorkLocationEditorViewModel editor = Assert.IsType<WorkLocationEditorViewModel>(
            viewModel.SelectedWorkLocation);
        editor.Name = " ";

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasError);
        Assert.Contains("Namen", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(0, updateStore.FindCallCount);
        Assert.True(editor.HasChanges);
    }

    [Fact]
    public async Task SaveCommandWhenStoreFailsKeepsInputAndShowsGermanTechnicalError()
    {
        InvalidOperationException failure = new("Synthetic save failure.");
        FakeWorkLocationUpdateStore updateStore = new()
        {
            FindException = failure,
        };
        CollectingUnexpectedErrorReporter errorReporter = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            updateStore,
            errorReporter);
        await viewModel.LoadCommand.ExecuteAsync(null);
        WorkLocationEditorViewModel editor = Assert.IsType<WorkLocationEditorViewModel>(
            viewModel.SelectedWorkLocation);
        editor.Name = "Neue Bezeichnung";

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Neue Bezeichnung", editor.Name);
        Assert.True(editor.HasChanges);
        Assert.True(editor.HasError);
        Assert.Contains("nicht gespeichert", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.False(editor.IsSaving);
        (Exception exception, string operation) = Assert.Single(errorReporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("SaveWorkLocation", operation);
    }

    [Fact]
    public async Task SaveCommandWhileStoreWaitsShowsSavingStateAndDisablesItself()
    {
        TaskCompletionSource<CatalogEntryUpdateStoreResult> completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        FakeWorkLocationUpdateStore updateStore = new()
        {
            UpdateHandler = (_, _, cancellationToken) =>
                completion.Task.WaitAsync(cancellationToken),
        };
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            updateStore,
            new CollectingUnexpectedErrorReporter());
        await viewModel.LoadCommand.ExecuteAsync(null);
        WorkLocationEditorViewModel editor = Assert.IsType<WorkLocationEditorViewModel>(
            viewModel.SelectedWorkLocation);
        editor.Name = "Cafeteria Nord";

        Task saving = editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.IsSaving);
        Assert.False(editor.SaveCommand.CanExecute(null));
        completion.SetResult(CatalogEntryUpdateStoreResult.Updated);
        await saving;
        Assert.False(editor.IsSaving);
    }

    [Fact]
    public async Task ShiftTypeSaveWhenTimeIsValidPersistsAndUpdatesActualTimeDisplay()
    {
        FakeServiceCatalogReader reader = new();
        FakeShiftTypeStandardTimeUpdateStore updateStore = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            reader,
            new FakeWorkLocationUpdateStore(),
            new CollectingUnexpectedErrorReporter(),
            updateStore);
        await viewModel.LoadCommand.ExecuteAsync(null);
        ShiftTypeEditorViewModel editor = Assert.IsType<ShiftTypeEditorViewModel>(
            viewModel.SelectedWorkLocation?.ShiftTypes[0]);
        editor.SelectedEndTime = Assert.Single(
            editor.TimeOptions,
            option => option.Value == new TimeOnly(20, 0));

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(new TimeOnly(20, 0), updateStore.Replacement?.StandardTime.End);
        Assert.Equal("13:30-20:00", editor.DisplayCode);
        Assert.Equal("13:30 bis 20:00 Uhr", editor.StandardTimeText);
        Assert.True(editor.HasSuccessMessage);
        Assert.False(editor.HasError);
        Assert.False(editor.HasChanges);
        Assert.Equal(2, reader.LoadCallCount);
    }

    [Fact]
    public async Task ShiftTypeSaveWhenEndIsNotAfterStartShowsValidationWithoutStoreAccess()
    {
        FakeShiftTypeStandardTimeUpdateStore updateStore = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            new FakeWorkLocationUpdateStore(),
            new CollectingUnexpectedErrorReporter(),
            updateStore);
        await viewModel.LoadCommand.ExecuteAsync(null);
        ShiftTypeEditorViewModel editor = Assert.IsType<ShiftTypeEditorViewModel>(
            viewModel.SelectedWorkLocation?.ShiftTypes[0]);
        editor.SelectedEndTime = Assert.Single(
            editor.TimeOptions,
            option => option.Value == new TimeOnly(13, 0));

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasError);
        Assert.Contains("nach der Startzeit", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(0, updateStore.FindCallCount);
        Assert.True(editor.HasChanges);
    }

    [Fact]
    public async Task EarlyShiftSaveWhenItWouldOverlapLateShiftShowsSplitShiftValidation()
    {
        FakeShiftTypeStandardTimeUpdateStore updateStore = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            new FakeWorkLocationUpdateStore(),
            new CollectingUnexpectedErrorReporter(),
            updateStore);
        await viewModel.LoadCommand.ExecuteAsync(null);
        WorkLocationEditorViewModel restaurant = Assert.Single(
            viewModel.WorkLocations,
            location => location.Name == "Restaurant");
        ShiftTypeEditorViewModel earlyShift = Assert.Single(
            restaurant.ShiftTypes,
            shiftType => shiftType.Abbreviation == "F");
        earlyShift.SelectedEndTime = Assert.Single(
            earlyShift.TimeOptions,
            option => option.Value == new TimeOnly(17, 0));

        await earlyShift.SaveCommand.ExecuteAsync(null);

        Assert.True(earlyShift.HasError);
        Assert.Contains("nicht überschneiden", earlyShift.ErrorMessage, StringComparison.Ordinal);
        Assert.Null(updateStore.Replacement);
        Assert.True(earlyShift.HasChanges);
    }

    [Fact]
    public async Task ShiftTypeSaveWhenStoreReportsConflictKeepsInputAndShowsMessage()
    {
        FakeShiftTypeStandardTimeUpdateStore updateStore = new()
        {
            UpdateResult = CatalogEntryUpdateStoreResult.Conflict,
        };
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            new FakeWorkLocationUpdateStore(),
            new CollectingUnexpectedErrorReporter(),
            updateStore);
        await viewModel.LoadCommand.ExecuteAsync(null);
        ShiftTypeEditorViewModel editor = Assert.IsType<ShiftTypeEditorViewModel>(
            viewModel.SelectedWorkLocation?.ShiftTypes[1]);
        editor.SelectedEndTime = Assert.Single(
            editor.TimeOptions,
            option => option.Value == new TimeOnly(17, 0));

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.True(editor.HasError);
        Assert.Contains("zwischenzeitlich geändert", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(new TimeOnly(17, 0), editor.SelectedEndTime.Value);
        Assert.True(editor.HasChanges);
    }

    [Fact]
    public async Task ShiftTypeSaveWhenStoreFailsKeepsInputAndReportsTechnicalFailure()
    {
        InvalidOperationException failure = new("Synthetic shift-type save failure.");
        FakeShiftTypeStandardTimeUpdateStore updateStore = new()
        {
            FindException = failure,
        };
        CollectingUnexpectedErrorReporter errorReporter = new();
        ServiceCatalogViewModel viewModel = CreateViewModel(
            new FakeServiceCatalogReader(),
            new FakeWorkLocationUpdateStore(),
            errorReporter,
            updateStore);
        await viewModel.LoadCommand.ExecuteAsync(null);
        ShiftTypeEditorViewModel editor = Assert.IsType<ShiftTypeEditorViewModel>(
            viewModel.SelectedWorkLocation?.ShiftTypes[0]);
        editor.SelectedEndTime = Assert.Single(
            editor.TimeOptions,
            option => option.Value == new TimeOnly(20, 0));

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(new TimeOnly(20, 0), editor.SelectedEndTime.Value);
        Assert.True(editor.HasChanges);
        Assert.True(editor.HasError);
        Assert.Contains("nicht gespeichert", editor.ErrorMessage, StringComparison.Ordinal);
        (Exception exception, string operation) = Assert.Single(errorReporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("SaveShiftTypeStandardTime", operation);
    }

    private static ServiceCatalogViewModel CreateViewModel(
        FakeServiceCatalogReader reader,
        FakeWorkLocationUpdateStore updateStore,
        CollectingUnexpectedErrorReporter errorReporter,
        FakeShiftTypeStandardTimeUpdateStore? shiftTypeUpdateStore = null)
    {
        return new ServiceCatalogViewModel(
            new GetServiceCatalogQuery(reader),
            new UpdateWorkLocationCommand(updateStore),
            new UpdateShiftTypeStandardTimeCommand(
                shiftTypeUpdateStore ?? new FakeShiftTypeStandardTimeUpdateStore()),
            errorReporter);
    }
}
