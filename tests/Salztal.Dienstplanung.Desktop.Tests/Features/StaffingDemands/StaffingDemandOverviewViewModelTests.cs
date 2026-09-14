using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Features.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.StaffingDemands;

public sealed class StaffingDemandOverviewViewModelTests
{
    private static readonly DateOnly WeekMonday = new(2026, 9, 14);

    [Fact]
    public async Task LoadCommandShowsGroupedInitialWeekWithEveryRequiredSummary()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(23, viewModel.Demands.Count);
        Assert.Equal(2, viewModel.WorkLocations.Count);
        Assert.Equal(20_220, viewModel.TotalRequiredWorkMinutes);
        Assert.Equal("337 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.Equal("14.09.2026 bis 20.09.2026", viewModel.SelectedWeekRangeDisplay);
        Assert.False(viewModel.IsEmpty);
        Assert.False(viewModel.HasLoadError);

        StaffingDemandWorkLocationGroupViewModel cafeteria = Assert.Single(
            viewModel.WorkLocations,
            location => location.Id == InitialWorkLocationCatalog.Cafeteria.Id.Value);
        Assert.Equal("Cafeteria", cafeteria.Name);
        Assert.Equal("Gelb", cafeteria.ColorName);
        Assert.Equal("57 Stunden", cafeteria.RequiredWorkDisplay);
        Assert.Equal(7, cafeteria.Days.Count);
        Assert.Equal(
            "7 Stunden",
            Assert.Single(cafeteria.Days, day => day.Date == WeekMonday)
                .RequiredWorkDisplay);
        StaffingDemandDayGroupViewModel saturday = Assert.Single(
            cafeteria.Days,
            day => day.Date.DayOfWeek == DayOfWeek.Saturday);
        Assert.Equal(2, saturday.Demands.Count);
        Assert.Equal("11 Stunden", saturday.RequiredWorkDisplay);

        StaffingDemandWorkLocationGroupViewModel restaurant = Assert.Single(
            viewModel.WorkLocations,
            location => location.Id == InitialWorkLocationCatalog.Restaurant.Id.Value);
        Assert.Equal("280 Stunden", restaurant.RequiredWorkDisplay);
        Assert.All(restaurant.Days, day => Assert.Equal("40 Stunden", day.RequiredWorkDisplay));

        StaffingDemandItemViewModel mondayCafeteria = Assert.Single(
            viewModel.Demands,
            demand => demand.Date == WeekMonday
                && demand.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.Equal("Cafeteria-Dienst A", mondayCafeteria.ShiftTypeName);
        Assert.Equal("13:30–20:30 Uhr", mondayCafeteria.ActualTimeDisplay);
        Assert.Equal("1 Person", mondayCafeteria.RequiredEmployeeCountDisplay);
        Assert.Equal("7 Stunden", mondayCafeteria.RequiredWorkDisplay);
        Assert.Equal("Regelmäßiger Standard", mondayCafeteria.SourceDisplay);
        Assert.False(mondayCafeteria.IsDateException);
        Assert.Equal(420, mondayCafeteria.DurationMinutes);
        Assert.Equal(420, mondayCafeteria.RequiredWorkMinutes);
    }

    [Fact]
    public async Task LoadCommandShowsDateExceptionSourceAndUpdatedTotals()
    {
        StaffingDemandDateException replacement =
            Assert.IsType<StaffingDemandDateException>(
                StaffingDemandDateException.CreateReplacement(
                    new Guid("81000000-0000-4000-8000-000000000001"),
                    WeekMonday,
                    InitialWorkLocationCatalog.Cafeteria.Id.Value,
                    InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
                    new TimeOnly(14, 0),
                    new TimeOnly(20, 0),
                    2).Value);
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(
            new FakeStaffingDemandReader(
                FakeStaffingDemandReader.CreateData(dateExceptions: [replacement])));

        await viewModel.LoadCommand.ExecuteAsync(null);

        StaffingDemandItemViewModel changed = Assert.Single(
            viewModel.Demands,
            demand => demand.SourceId == replacement.Id.Value);
        Assert.True(changed.IsDateException);
        Assert.Equal("Einmalige Änderung", changed.SourceDisplay);
        Assert.Equal("14:00–20:00 Uhr", changed.ActualTimeDisplay);
        Assert.Equal("2 Personen", changed.RequiredEmployeeCountDisplay);
        Assert.Equal("12 Stunden", changed.RequiredWorkDisplay);
        Assert.Equal("342 Stunden", viewModel.TotalRequiredWorkDisplay);
    }

    [Fact]
    public async Task SelectedDateNormalizesToMondayAndWeekCommandsReloadAdjacentWeeks()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        viewModel.SelectedWeekMonday = new DateTime(2026, 9, 17);

        Assert.Equal(new DateTime(2026, 9, 14), viewModel.SelectedWeekMonday);
        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.NextWeekCommand.ExecuteAsync(null);
        Assert.Equal(new DateTime(2026, 9, 21), viewModel.SelectedWeekMonday);
        Assert.Equal("21.09.2026 bis 27.09.2026", viewModel.SelectedWeekRangeDisplay);
        await viewModel.PreviousWeekCommand.ExecuteAsync(null);

        Assert.Equal(new DateTime(2026, 9, 14), viewModel.SelectedWeekMonday);
        Assert.Equal(3, reader.LoadCallCount);
        Assert.Equal(23, viewModel.Demands.Count);
    }

    [Fact]
    public async Task LoadCommandWhileReaderWaitsShowsWorkingStateAndDisablesWeekActions()
    {
        TaskCompletionSource<StaffingDemandReadData> completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData())
        {
            LoadHandler = cancellationToken => completion.Task.WaitAsync(cancellationToken),
        };
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);

        Task loading = viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsLoading);
        Assert.False(viewModel.LoadCommand.CanExecute(null));
        Assert.False(viewModel.PreviousWeekCommand.CanExecute(null));
        Assert.False(viewModel.NextWeekCommand.CanExecute(null));
        Assert.False(viewModel.IsEmpty);
        completion.SetResult(FakeStaffingDemandReader.CreateData());
        await loading;
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadCommandWhenNoDemandsExistKeepsDaysAvailableForOneTimeAddition()
    {
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(
            new FakeStaffingDemandReader(
                FakeStaffingDemandReader.CreateData(standards: [])));

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.Demands);
        Assert.Equal(2, viewModel.WorkLocations.Count);
        Assert.All(viewModel.WorkLocations, location => Assert.Equal(7, location.Days.Count));
        Assert.False(viewModel.IsEmpty);
        Assert.True(viewModel.HasWorkLocations);
        Assert.False(viewModel.HasLoadError);
        Assert.Equal("0 Stunden", viewModel.TotalRequiredWorkDisplay);
    }

    [Fact]
    public async Task LoadCommandWhenStoredDataIsInvalidShowsApplicationMessageWithoutTechnicalReport()
    {
        StandardStaffingDemandRevision duplicate = InitialStaffingDemandCatalog.All[0];
        CollectingUnexpectedErrorReporter reporter = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(
            new FakeStaffingDemandReader(
                FakeStaffingDemandReader.CreateData(standards: [duplicate, duplicate])),
            reporter);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasLoadError);
        Assert.Contains("mehr als eine Revision", viewModel.LoadErrorMessage, StringComparison.Ordinal);
        Assert.Empty(viewModel.Demands);
        Assert.Empty(reporter.Reports);
    }

    [Fact]
    public async Task LoadCommandWhenReaderFailsShowsGermanErrorAndReportsTechnicalFailure()
    {
        InvalidOperationException failure = new("Synthetic staffing-demand load failure.");
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData())
        {
            LoadHandler = _ => Task.FromException<StaffingDemandReadData>(failure),
        };
        CollectingUnexpectedErrorReporter reporter = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, reporter);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasLoadError);
        Assert.Contains("nicht geladen", viewModel.LoadErrorMessage, StringComparison.Ordinal);
        Assert.Empty(viewModel.Demands);
        (Exception exception, string operation) = Assert.Single(reporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("LoadStaffingDemandWeek", operation);
    }

    [Fact]
    public async Task LoadCommandWhenCanceledClearsWorkingStateWithoutTechnicalReport()
    {
        TaskCompletionSource<StaffingDemandReadData> completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData())
        {
            LoadHandler = cancellationToken => completion.Task.WaitAsync(cancellationToken),
        };
        CollectingUnexpectedErrorReporter reporter = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, reporter);

        Task loading = viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.LoadCommand.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loading);
        Assert.False(viewModel.IsLoading);
        Assert.False(viewModel.HasLoadError);
        Assert.Empty(reporter.Reports);
    }

    [Fact]
    public async Task DateEditorCanReplaceExistingDemandRepeatedlyAndReloadTotals()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = CreateUpdatingDateStore(reader);
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, dateStore: store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StaffingDemandItemViewModel demand = Assert.Single(
            viewModel.Demands,
            item => item.Date == WeekMonday
                && item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        viewModel.DateEditor.OpenDemandCommand.Execute(demand);
        viewModel.DateEditor.SelectedStartTime = FindTime(viewModel.DateEditor, 14, 0);
        viewModel.DateEditor.SelectedEndTime = FindTime(viewModel.DateEditor, 20, 0);
        viewModel.DateEditor.RequiredEmployeeCountInput = "2";

        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        StaffingDemandDateException firstChange = Assert.IsType<StaffingDemandDateException>(
            store.WrittenException);
        Assert.Equal("342 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.True(viewModel.DateEditor.HasDateException);
        viewModel.DateEditor.RequiredEmployeeCountInput = "3";
        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(2, store.SaveCallCount);
        Assert.Same(firstChange, store.ExpectedCurrent);
        Assert.Equal("348 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.Equal(
            "Einmalige Änderung",
            Assert.Single(viewModel.Demands, item =>
                item.Date == WeekMonday
                && item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value)
                .SourceDisplay);
    }

    [Fact]
    public async Task DateEditorAddsDemandMissingFromRegularStandard()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = CreateUpdatingDateStore(reader);
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, dateStore: store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StaffingDemandDayGroupViewModel monday = GetCafeteriaDay(viewModel, WeekMonday);
        viewModel.DateEditor.OpenDayCommand.Execute(monday);
        viewModel.DateEditor.SelectedShiftType = Assert.Single(
            viewModel.DateEditor.ShiftTypes,
            shiftType => shiftType.Id == InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value);

        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(StaffingDemandDateExceptionKind.Add, store.WrittenException!.Kind);
        Assert.Equal("341 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.True(GetCafeteriaDay(viewModel, WeekMonday).HasDateException);
    }

    [Fact]
    public async Task DateEditorSetsNoDemandAndCanResetToRegularStandard()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = CreateUpdatingDateStore(reader);
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, dateStore: store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StaffingDemandItemViewModel demand = Assert.Single(
            viewModel.Demands,
            item => item.Date == WeekMonday
                && item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        viewModel.DateEditor.OpenDemandCommand.Execute(demand);

        await viewModel.DateEditor.NoDemandCommand.ExecuteAsync(null);

        Assert.Equal(StaffingDemandDateExceptionKind.Remove, store.WrittenException!.Kind);
        Assert.Equal("330 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.True(viewModel.DateEditor.IsNoDemandChange);
        Assert.True(GetCafeteriaDay(viewModel, WeekMonday).HasDateException);
        await viewModel.DateEditor.ResetCommand.ExecuteAsync(null);

        Assert.Equal(1, store.RemoveCallCount);
        Assert.Equal("337 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.False(viewModel.DateEditor.HasDateException);
        Assert.False(GetCafeteriaDay(viewModel, WeekMonday).HasDateException);
    }

    [Fact]
    public async Task DateEditorRejectsInvalidTimeAndPreservesIt()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, dateStore: store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.DateEditor.OpenDayCommand.Execute(GetCafeteriaDay(viewModel, WeekMonday));
        viewModel.DateEditor.SelectedStartTime = FindTime(viewModel.DateEditor, 20, 30);
        viewModel.DateEditor.SelectedEndTime = FindTime(viewModel.DateEditor, 20, 30);

        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("nach der Startzeit", viewModel.DateEditor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(new TimeOnly(20, 30), viewModel.DateEditor.SelectedStartTime.Value);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task DateEditorRejectsInvalidEmployeeCountAndPreservesIt()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, dateStore: store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.DateEditor.OpenDayCommand.Execute(GetCafeteriaDay(viewModel, WeekMonday));
        viewModel.DateEditor.RequiredEmployeeCountInput = "keine";

        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("Personenzahl", viewModel.DateEditor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal("keine", viewModel.DateEditor.RequiredEmployeeCountInput);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task DateEditorShowsConflictWithoutDiscardingInput()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = new()
        {
            SaveResult = StaffingDemandWriteStoreResult.Conflict,
        };
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, dateStore: store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.DateEditor.OpenDayCommand.Execute(GetCafeteriaDay(viewModel, WeekMonday));
        viewModel.DateEditor.RequiredEmployeeCountInput = "3";

        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("zwischenzeitlich geändert", viewModel.DateEditor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal("3", viewModel.DateEditor.RequiredEmployeeCountInput);
        Assert.Equal(1, store.SaveCallCount);
    }

    [Fact]
    public async Task DateEditorReportsTechnicalFailureWithoutDiscardingInput()
    {
        InvalidOperationException failure = new("Synthetic date write failure.");
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStaffingDemandDateExceptionStore store = new()
        {
            SaveHandler = (_, _, _) =>
                Task.FromException<StaffingDemandWriteStoreResult>(failure),
        };
        CollectingUnexpectedErrorReporter reporter = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(
            reader,
            reporter,
            store);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.DateEditor.OpenDayCommand.Execute(GetCafeteriaDay(viewModel, WeekMonday));
        viewModel.DateEditor.RequiredEmployeeCountInput = "3";

        await viewModel.DateEditor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("nicht gespeichert", viewModel.DateEditor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal("3", viewModel.DateEditor.RequiredEmployeeCountInput);
        (Exception exception, string operation) = Assert.Single(reporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("SaveStaffingDemandDateChange", operation);
    }

    [Fact]
    public async Task DateEditorOnlyOffersNormalShiftTypesOfSelectedLocation()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.DateEditor.OpenDayCommand.Execute(GetCafeteriaDay(viewModel, WeekMonday));

        Assert.Equal(2, viewModel.DateEditor.ShiftTypes.Count);
        Assert.All(
            viewModel.DateEditor.ShiftTypes,
            shiftType => Assert.Equal(
                InitialWorkLocationCatalog.Cafeteria.Id.Value,
                shiftType.WorkLocationId));
        Assert.DoesNotContain(
            viewModel.DateEditor.ShiftTypes,
            shiftType => shiftType.Name is "D" or "Spr");
    }

    [Fact]
    public async Task StandardEditorReplacesExistingStandardAndReloadsStoredWeek()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        store.AppendHandler = (_, revision, _) =>
        {
            reader.AppendStandardRevision(revision);
            return Task.FromResult(StaffingDemandWriteStoreResult.Succeeded);
        };
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.SelectedStartTime = FindTime(editor, 14, 0);
        editor.SelectedEndTime = FindTime(editor, 20, 0);
        editor.RequiredEmployeeCountInput = "2";

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(1, store.AppendCallCount);
        Assert.Equal(StandardStaffingDemandRevisionKind.Replace, store.WrittenRevision!.Kind);
        Assert.Equal(new TimeOnly(14, 0), store.WrittenRevision.ActualTime!.Start);
        Assert.Equal(new TimeOnly(20, 0), store.WrittenRevision.ActualTime.End);
        Assert.Equal(2, store.WrittenRevision.RequiredEmployeeCount!.Value);
        Assert.Equal("342 Stunden", viewModel.TotalRequiredWorkDisplay);
        Assert.Equal(5, reader.LoadCallCount);
        Assert.True(editor.HasSuccessMessage);
        Assert.True(editor.HasRevisionAtEffectiveMonday);
        Assert.True(editor.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task StandardEditorAllowsSecondCorrectionForSameEffectiveMonday()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        store.AppendHandler = (_, revision, _) =>
        {
            reader.AppendStandardRevision(revision);
            return Task.FromResult(StaffingDemandWriteStoreResult.Succeeded);
        };
        StaffingDemandOverviewViewModel overview = CreateViewModel(reader);
        await overview.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            overview,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);

        editor.RequiredEmployeeCountInput = "2";
        await editor.SaveCommand.ExecuteAsync(null);
        StandardStaffingDemandRevision firstCorrection = store.WrittenRevision!;
        editor.RequiredEmployeeCountInput = "3";
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(2, store.AppendCallCount);
        Assert.Equal(1, firstCorrection.CorrectionSequence);
        Assert.Equal(2, store.WrittenRevision!.CorrectionSequence);
        Assert.Same(firstCorrection, store.ExpectedCurrent);
        Assert.Equal(3, store.WrittenRevision.RequiredEmployeeCount!.Value);
        Assert.Equal("3", editor.RequiredEmployeeCountInput);
        Assert.True(editor.SaveCommand.CanExecute(null));
        Assert.True(editor.HasSuccessMessage);
    }

    [Fact]
    public async Task StandardEditorAddsMissingStandardWithShiftTypeDefaultTime()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.SelectedShiftType = Assert.Single(
            editor.ShiftTypes,
            shiftType => shiftType.Id == InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value);

        Assert.False(editor.HasEffectiveStandard);
        Assert.Equal(new TimeOnly(13, 30), editor.SelectedStartTime.Value);
        Assert.Equal(new TimeOnly(17, 30), editor.SelectedEndTime.Value);
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(StandardStaffingDemandRevisionKind.Add, store.WrittenRevision!.Kind);
        Assert.Null(store.ExpectedCurrent);
    }

    [Fact]
    public async Task StandardEditorAddsAfterRemovalWithRemovalAsExpectedCurrentRevision()
    {
        StandardStaffingDemandRevision removal = Assert.IsType<
            StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateRemoval(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                InitialWorkLocationCatalog.Cafeteria.Id.Value,
                InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
                WeekMonday.AddDays(-7)).Value);
        FakeStaffingDemandReader reader = new(
            FakeStaffingDemandReader.CreateData(
                standards: InitialStaffingDemandCatalog.All.Append(removal)));
        FakeStandardStaffingDemandRevisionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);

        Assert.False(editor.HasEffectiveStandard);
        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Equal(StandardStaffingDemandRevisionKind.Add, store.WrittenRevision!.Kind);
        Assert.Same(removal, store.ExpectedCurrent);
    }

    [Fact]
    public async Task StandardEditorRemovesExistingStandardWithExpectedCurrentRevision()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);

        await editor.RemoveCommand.ExecuteAsync(null);

        Assert.Equal(StandardStaffingDemandRevisionKind.Remove, store.WrittenRevision!.Kind);
        Assert.NotNull(store.ExpectedCurrent);
        Assert.Null(store.WrittenRevision.ActualTime);
        Assert.Null(store.WrittenRevision.RequiredEmployeeCount);
    }

    [Fact]
    public async Task StandardEditorRejectsInvalidTimeAndPreservesInput()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.SelectedStartTime = FindTime(editor, 20, 30);
        editor.SelectedEndTime = FindTime(editor, 20, 30);

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("nach der Startzeit", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(new TimeOnly(20, 30), editor.SelectedStartTime.Value);
        Assert.Equal(new TimeOnly(20, 30), editor.SelectedEndTime.Value);
        Assert.Equal(0, store.AppendCallCount);
    }

    [Theory]
    [InlineData("keine", "Personenzahl")]
    [InlineData("0", "größer als null")]
    public async Task StandardEditorRejectsInvalidEmployeeCountAndPreservesInput(
        string input,
        string expectedMessage)
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.RequiredEmployeeCountInput = input;

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Contains(expectedMessage, editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(input, editor.RequiredEmployeeCountInput);
        Assert.Equal(0, store.AppendCallCount);
    }

    [Fact]
    public async Task StandardEditorRejectsNonMondayAndPreservesInput()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.EffectiveFromMonday = new DateTime(2026, 9, 15);

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("Montag", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(new DateTime(2026, 9, 15), editor.EffectiveFromMonday);
        Assert.Equal(0, store.AppendCallCount);
    }

    [Fact]
    public async Task StandardEditorShowsConflictAndPreservesInput()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new()
        {
            Result = StaffingDemandWriteStoreResult.Conflict,
        };
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.RequiredEmployeeCountInput = "3";

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("zwischenzeitlich geändert", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal("3", editor.RequiredEmployeeCountInput);
        Assert.Equal(1, store.AppendCallCount);
        Assert.Equal(3, reader.LoadCallCount);
    }

    [Fact]
    public async Task StandardEditorReportsTechnicalFailureAndPreservesInput()
    {
        InvalidOperationException failure = new("Synthetic standard write failure.");
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new()
        {
            AppendHandler = (_, _, _) =>
                Task.FromException<StaffingDemandWriteStoreResult>(failure),
        };
        CollectingUnexpectedErrorReporter reporter = new();
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader, reporter);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            reporter,
            store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.RequiredEmployeeCountInput = "3";

        await editor.SaveCommand.ExecuteAsync(null);

        Assert.Contains("nicht gespeichert", editor.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal("3", editor.RequiredEmployeeCountInput);
        (Exception exception, string operation) = Assert.Single(reporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("ChangeStandardStaffingDemand", operation);
    }

    [Fact]
    public async Task FutureStandardRevisionLeavesPreviousWeekUnchanged()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        FakeStandardStaffingDemandRevisionStore store = new();
        store.AppendHandler = (_, revision, _) =>
        {
            reader.AppendStandardRevision(revision);
            return Task.FromResult(StaffingDemandWriteStoreResult.Succeeded);
        };
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(
            reader,
            viewModel,
            store: store);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.EffectiveFromMonday = new DateTime(2026, 9, 21);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);
        editor.RequiredEmployeeCountInput = "2";

        await editor.SaveCommand.ExecuteAsync(null);
        Assert.Equal("337 Stunden", viewModel.TotalRequiredWorkDisplay);
        viewModel.SelectedWeekMonday = new DateTime(2026, 9, 21);
        await viewModel.LoadCommand.ExecuteAsync(null);
        Assert.Equal("344 Stunden", viewModel.TotalRequiredWorkDisplay);
        await viewModel.PreviousWeekCommand.ExecuteAsync(null);

        Assert.Equal(new DateTime(2026, 9, 14), viewModel.SelectedWeekMonday);
        Assert.Equal("337 Stunden", viewModel.TotalRequiredWorkDisplay);
    }

    [Fact]
    public async Task StandardEditorOnlyOffersNormalShiftTypesOfSelectedLocation()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        StaffingDemandOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(reader, viewModel);
        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);

        Assert.Equal(2, editor.ShiftTypes.Count);
        Assert.All(
            editor.ShiftTypes,
            shiftType => Assert.Equal(editor.SelectedWorkLocation!.Id, shiftType.WorkLocationId));
        Assert.DoesNotContain(editor.ShiftTypes, shiftType => shiftType.Name is "D" or "Spr");
    }

    [Fact]
    public async Task StandardEditorShowsAllSevenDaysForSelectedLocation()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        StaffingDemandOverviewViewModel overview = CreateViewModel(reader);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(reader, overview);

        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);

        Assert.Equal(7, editor.WeekDays.Count);
        Assert.Equal(
            ["Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag"],
            editor.WeekDays.Select(day => day.DayName));
        Assert.All(editor.WeekDays, day => Assert.Equal(2, day.Items.Count));
        StandardStaffingDemandWeekDayViewModel monday = editor.WeekDays[0];
        Assert.Contains(
            monday.Items,
            item => item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value
                && item.DemandDisplay == "Kein regelmäßiger Bedarf");
        Assert.Contains(
            monday.Items,
            item => item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value
                && item.DemandDisplay == "13:30–20:30 Uhr · 1 Person");
        StandardStaffingDemandWeekDayViewModel saturday = editor.WeekDays[5];
        Assert.All(saturday.Items, item => Assert.Contains("1 Person", item.DemandDisplay));
    }

    [Fact]
    public async Task StandardEditorUsesWorkLocationSelectedByOuterCatalog()
    {
        FakeStaffingDemandReader reader = new(FakeStaffingDemandReader.CreateData());
        StaffingDemandOverviewViewModel overview = CreateViewModel(reader);
        StandardStaffingDemandEditorViewModel editor = CreateStandardEditor(reader, overview);
        editor.SelectWorkLocation(InitialWorkLocationCatalog.Restaurant.Id.Value);

        await editor.LoadEffectiveWeekCommand.ExecuteAsync(null);

        Assert.Equal(
            InitialWorkLocationCatalog.Restaurant.Id.Value,
            editor.SelectedWorkLocation!.Id);
        Assert.Equal(7, editor.WeekDays.Count);
        Assert.All(editor.WeekDays, day => Assert.Equal(2, day.Items.Count));
        Assert.All(
            editor.WeekDays.SelectMany(day => day.Items),
            item => Assert.Contains(
                item.ShiftTypeId,
                new[]
                {
                    InitialShiftTypeCatalog.EarlyShift.Id.Value,
                    InitialShiftTypeCatalog.LateShift.Id.Value,
                }));
    }

    private static StaffingDemandOverviewViewModel CreateViewModel(
        FakeStaffingDemandReader reader,
        IUnexpectedErrorReporter? reporter = null,
        FakeStaffingDemandDateExceptionStore? dateStore = null)
    {
        FakeStaffingDemandDateExceptionStore effectiveStore = dateStore ?? new();
        return new StaffingDemandOverviewViewModel(
            new GetStaffingDemandWeekQuery(reader),
            new SaveStaffingDemandDateExceptionCommand(reader, effectiveStore),
            new RemoveStaffingDemandDateExceptionCommand(reader, effectiveStore),
            WeekMonday,
            reporter ?? new CollectingUnexpectedErrorReporter());
    }

    private static StandardStaffingDemandEditorViewModel CreateStandardEditor(
        FakeStaffingDemandReader reader,
        StaffingDemandOverviewViewModel overview,
        IUnexpectedErrorReporter? reporter = null,
        FakeStandardStaffingDemandRevisionStore? store = null)
    {
        return new StandardStaffingDemandEditorViewModel(
            new GetStaffingDemandWeekQuery(reader),
            new ChangeStandardStaffingDemandCommand(
                reader,
                store ?? new FakeStandardStaffingDemandRevisionStore()),
            WeekMonday,
            overview.LoadAsync,
            reporter ?? new CollectingUnexpectedErrorReporter());
    }

    private static FakeStaffingDemandDateExceptionStore CreateUpdatingDateStore(
        FakeStaffingDemandReader reader)
    {
        FakeStaffingDemandDateExceptionStore store = new();
        store.SaveHandler = (_, replacement, _) =>
        {
            reader.SaveDateException(replacement);
            return Task.FromResult(StaffingDemandWriteStoreResult.Succeeded);
        };
        store.RemoveHandler = (expected, _) =>
        {
            reader.RemoveDateException(expected);
            return Task.FromResult(StaffingDemandWriteStoreResult.Succeeded);
        };
        return store;
    }

    private static StaffingDemandDayGroupViewModel GetCafeteriaDay(
        StaffingDemandOverviewViewModel viewModel,
        DateOnly date)
    {
        StaffingDemandWorkLocationGroupViewModel cafeteria = Assert.Single(
            viewModel.WorkLocations,
            location => location.Id == InitialWorkLocationCatalog.Cafeteria.Id.Value);
        return Assert.Single(cafeteria.Days, day => day.Date == date);
    }

    private static StaffingDemandTimeOption FindTime(
        DateStaffingDemandEditorViewModel editor,
        int hour,
        int minute)
    {
        TimeOnly expected = new(hour, minute);
        return editor.TimeOptions.Single(option => option.Value == expected);
    }

    private static StaffingDemandTimeOption FindTime(
        StandardStaffingDemandEditorViewModel editor,
        int hour,
        int minute)
    {
        TimeOnly expected = new(hour, minute);
        return editor.TimeOptions.Single(option => option.Value == expected);
    }
}
