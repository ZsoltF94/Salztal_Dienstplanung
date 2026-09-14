using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.StaffingDemands;

public sealed class GetStaffingDemandWeekQueryTests
{
    private static readonly DateOnly WeekMonday = new(2026, 9, 14);

    [Fact]
    public async Task InitialStandardsReturnEnrichedImmutableWeekSnapshot()
    {
        FakeStaffingDemandReader reader = new(CreateReadData(
            InitialStaffingDemandCatalog.All,
            []));
        GetStaffingDemandWeekQuery query = new(reader);

        StaffingDemandWeekQueryResult result = await query.ExecuteAsync(
            WeekMonday,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(StaffingDemandWeekQueryStatus.Succeeded, result.Status);
        Assert.Empty(result.Errors);
        StaffingDemandWeekSnapshot week = Assert.IsType<StaffingDemandWeekSnapshot>(
            result.Value);
        Assert.Equal(WeekMonday, week.WeekMonday);
        Assert.Equal(new DateOnly(2026, 9, 20), week.WeekSunday);
        Assert.Equal(23, week.Demands.Count);
        Assert.Equal(28, week.DateEditItems.Count);
        Assert.Equal(20_220, week.TotalRequiredWorkMinutes);
        Assert.True(((ICollection<StaffingDemandItemSnapshot>)week.Demands).IsReadOnly);
        Assert.True(
            ((ICollection<StaffingDemandDayWorkLocationSummarySnapshot>)week.DaySummaries)
                .IsReadOnly);
        Assert.True(
            ((ICollection<StaffingDemandWorkLocationWeekSummarySnapshot>)week
                .WorkLocationSummaries).IsReadOnly);
        Assert.True(
            ((ICollection<DateStaffingDemandEditItemSnapshot>)week.DateEditItems).IsReadOnly);

        StaffingDemandItemSnapshot mondayCafeteria = Assert.Single(
            week.Demands,
            demand => demand.Date == WeekMonday
                && demand.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.Equal(StaffingDemandSourceSnapshotKind.Standard, mondayCafeteria.SourceKind);
        Assert.Equal("Regelmäßiger Standard", mondayCafeteria.SourceDisplay);
        Assert.Equal("Cafeteria", mondayCafeteria.WorkLocationName);
        Assert.Equal("yellow", mondayCafeteria.WorkLocationColorCode);
        Assert.Equal("Cafeteria-Dienst A", mondayCafeteria.ShiftTypeName);
        Assert.True(mondayCafeteria.ShiftTypeUsesActualTimeAsDisplay);
        Assert.Null(mondayCafeteria.ShiftTypeAbbreviation);
        Assert.Equal(new TimeOnly(13, 30), mondayCafeteria.ActualStart);
        Assert.Equal(new TimeOnly(20, 30), mondayCafeteria.ActualEnd);
        Assert.Equal(1, mondayCafeteria.RequiredEmployeeCount);
        Assert.Equal(420, mondayCafeteria.DurationMinutes);
        Assert.Equal(420, mondayCafeteria.RequiredWorkMinutes);
        DateStaffingDemandEditItemSnapshot mondayCafeteriaEdit = Assert.Single(
            week.DateEditItems,
            item => item.Date == WeekMonday
                && item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.True(mondayCafeteriaEdit.HasRegularStandard);
        Assert.True(mondayCafeteriaEdit.HasEffectiveDemand);
        Assert.False(mondayCafeteriaEdit.HasDateException);
        Assert.Equal(new TimeOnly(13, 30), mondayCafeteriaEdit.RegularActualStart);

        Assert.Contains(
            week.WorkLocationSummaries,
            summary => summary.WorkLocationName == "Cafeteria"
                && summary.RequiredWorkMinutes == 3_420);
        Assert.Contains(
            week.WorkLocationSummaries,
            summary => summary.WorkLocationName == "Restaurant"
                && summary.RequiredWorkMinutes == 16_800);
        Assert.Equal(1, reader.LoadCallCount);
    }

    [Fact]
    public async Task DateReplacementOverridesStandardAndIdentifiesItsSource()
    {
        StaffingDemandDateException replacement = CreateDateReplacement(
            WeekMonday,
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
            new TimeOnly(14, 0),
            new TimeOnly(20, 0),
            2);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData(InitialStaffingDemandCatalog.All, [replacement])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        StaffingDemandWeekSnapshot week = Assert.IsType<StaffingDemandWeekSnapshot>(
            result.Value);
        StaffingDemandItemSnapshot changedDemand = Assert.Single(
            week.Demands,
            demand => demand.Date == WeekMonday
                && demand.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.Equal(replacement.Id.Value, changedDemand.SourceId);
        Assert.Equal(
            StaffingDemandSourceSnapshotKind.DateException,
            changedDemand.SourceKind);
        Assert.Equal("Einmalige Änderung", changedDemand.SourceDisplay);
        Assert.Equal(2, changedDemand.RequiredEmployeeCount);
        Assert.Equal(720, changedDemand.RequiredWorkMinutes);
        DateStaffingDemandEditItemSnapshot changedEdit = Assert.Single(
            week.DateEditItems,
            item => item.Date == WeekMonday
                && item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.True(changedEdit.HasDateException);
        Assert.False(changedEdit.IsNoDemandChange);
        Assert.Equal(replacement.Id.Value, changedEdit.ExpectedCurrentExceptionId);
        Assert.Equal(new TimeOnly(14, 0), changedEdit.ActualStart);
        Assert.Equal(20_520, week.TotalRequiredWorkMinutes);
    }

    [Fact]
    public async Task DateRemovalOmitsOnlyAffectedDemand()
    {
        StaffingDemandDateException removal = Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateRemoval(
                Guid.NewGuid(),
                WeekMonday,
                InitialWorkLocationCatalog.Cafeteria.Id.Value,
                InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value).Value);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData(InitialStaffingDemandCatalog.All, [removal])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        StaffingDemandWeekSnapshot week = Assert.IsType<StaffingDemandWeekSnapshot>(
            result.Value);
        Assert.Equal(22, week.Demands.Count);
        Assert.Equal(19_800, week.TotalRequiredWorkMinutes);
        Assert.DoesNotContain(
            week.Demands,
            demand => demand.Date == WeekMonday
                && demand.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        DateStaffingDemandEditItemSnapshot removedEdit = Assert.Single(
            week.DateEditItems,
            item => item.Date == WeekMonday
                && item.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.True(removedEdit.HasRegularStandard);
        Assert.False(removedEdit.HasEffectiveDemand);
        Assert.True(removedEdit.HasDateException);
        Assert.True(removedEdit.IsNoDemandChange);
        Assert.Equal(removal.Id.Value, removedEdit.ExpectedCurrentExceptionId);
    }

    [Fact]
    public async Task EmptyInventoryReturnsSuccessfulImmutableEmptyWeek()
    {
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([], [])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        StaffingDemandWeekSnapshot week = Assert.IsType<StaffingDemandWeekSnapshot>(
            result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(week.Demands);
        Assert.Empty(week.DaySummaries);
        Assert.Empty(week.WorkLocationSummaries);
        Assert.Equal(0, week.TotalRequiredWorkMinutes);
        Assert.True(((ICollection<StaffingDemandItemSnapshot>)week.Demands).IsReadOnly);
    }

    [Fact]
    public async Task NonMondayReturnsReadableValidationError()
    {
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([], [])));

        StaffingDemandWeekQueryResult result = await query.ExecuteAsync(
            WeekMonday.AddDays(1),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(StaffingDemandWeekQueryStatus.ValidationFailed, result.Status);
        StaffingDemandWeekQueryError error = Assert.Single(result.Errors);
        Assert.Equal(StaffingDemandWeekQueryErrorCode.WeekStartMustBeMonday, error.Code);
        Assert.Equal("Die ausgewählte Woche muss an einem Montag beginnen.", error.Message);
        Assert.True(((ICollection<StaffingDemandWeekQueryError>)result.Errors).IsReadOnly);
    }

    [Fact]
    public async Task UnknownWorkLocationReferenceReturnsVisibleCatalogError()
    {
        StandardStaffingDemandRevision revision = CreateAddition(
            Guid.NewGuid(),
            Guid.NewGuid(),
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([revision], [])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.CatalogInvalid, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandWeekQueryErrorCode.WorkLocationNotFound);
    }

    [Fact]
    public async Task UnknownShiftTypeReferenceReturnsVisibleCatalogError()
    {
        StandardStaffingDemandRevision revision = CreateAddition(
            Guid.NewGuid(),
            InitialWorkLocationCatalog.Restaurant.Id.Value,
            Guid.NewGuid());
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([revision], [])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.CatalogInvalid, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandWeekQueryErrorCode.ShiftTypeNotFound);
    }

    [Fact]
    public async Task ShiftTypeAtDifferentWorkLocationReturnsVisibleCatalogError()
    {
        StandardStaffingDemandRevision revision = CreateAddition(
            Guid.NewGuid(),
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([revision], [])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.CatalogInvalid, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == StaffingDemandWeekQueryErrorCode.ShiftTypeWorkLocationMismatch);
    }

    [Fact]
    public async Task DuplicateCatalogIdentifiersReturnVisibleErrors()
    {
        ServiceCatalogData catalog = new(
            [
                InitialWorkLocationCatalog.Cafeteria,
                InitialWorkLocationCatalog.Cafeteria,
                InitialWorkLocationCatalog.Restaurant,
            ],
            [
                InitialShiftTypeCatalog.EarlyShift,
                InitialShiftTypeCatalog.EarlyShift,
            ],
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            new StaffingDemandReadData([], [], catalog)));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.CatalogInvalid, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandWeekQueryErrorCode.DuplicateWorkLocation);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandWeekQueryErrorCode.DuplicateShiftType);
    }

    [Fact]
    public async Task DuplicateStandardRevisionReturnsVisibleStoredDataError()
    {
        StandardStaffingDemandRevision revision = InitialStaffingDemandCatalog.All[0];
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([revision, revision], [])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.StoredDataInvalid, result.Status);
        StaffingDemandWeekQueryError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandWeekQueryErrorCode.DuplicateStandardRevision,
            error.Code);
    }

    [Fact]
    public async Task ReplacementWithoutEarlierStandardReturnsVisibleStoredDataError()
    {
        StandardStaffingDemandRevision replacement = Assert.IsType<
            StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateReplacement(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                InitialWorkLocationCatalog.Restaurant.Id.Value,
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                WeekMonday,
                new TimeOnly(7, 0),
                new TimeOnly(13, 0),
                3).Value);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([replacement], [])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            StaffingDemandWeekQueryErrorCode.InvalidStandardRevisionSequence,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task DuplicateDateExceptionReturnsVisibleStoredDataError()
    {
        StaffingDemandDateException exception = CreateDateReplacement(
            WeekMonday,
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
            new TimeOnly(14, 0),
            new TimeOnly(20, 0),
            1);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData(InitialStaffingDemandCatalog.All, [exception, exception])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            StaffingDemandWeekQueryErrorCode.DuplicateDateException,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ReplacementWithoutEffectiveStandardReturnsVisibleStoredDataError()
    {
        StaffingDemandDateException exception = CreateDateReplacement(
            WeekMonday,
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
            new TimeOnly(14, 0),
            new TimeOnly(20, 0),
            1);
        GetStaffingDemandWeekQuery query = new(new FakeStaffingDemandReader(
            CreateReadData([], [exception])));

        StaffingDemandWeekQueryResult result = await ExecuteQueryAsync(query);

        Assert.Equal(StaffingDemandWeekQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            StaffingDemandWeekQueryErrorCode.InvalidDateExceptionApplication,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task CancellationBeforeReadDoesNotCallReader()
    {
        FakeStaffingDemandReader reader = new(CreateReadData([], []));
        GetStaffingDemandWeekQuery query = new(reader);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<StaffingDemandWeekQueryResult> execution = query.ExecuteAsync(
            WeekMonday,
            cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
    }

    [Fact]
    public async Task CancellationDuringReadStopsBeforeResolution()
    {
        using CancellationTokenSource cancellation = new();
        CancelDuringReadReader reader = new(CreateReadData([], []), cancellation);
        GetStaffingDemandWeekQuery query = new(reader);

        Task<StaffingDemandWeekQueryResult> execution = query.ExecuteAsync(
            WeekMonday,
            cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(1, reader.LoadCallCount);
    }

    [Fact]
    public void ApplicationAssemblyDoesNotReferenceForbiddenTechnicalModules()
    {
        string[] referencedAssemblies = typeof(GetStaffingDemandWeekQuery).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            referencedAssemblies,
            name => name.Contains("PresentationFramework", StringComparison.Ordinal)
                || name.Contains("EntityFrameworkCore", StringComparison.Ordinal)
                || name.Contains("Google.OrTools", StringComparison.Ordinal)
                || name.Contains("Microsoft.Data.Sqlite", StringComparison.Ordinal));
    }

    private static StaffingDemandReadData CreateReadData(
        IEnumerable<StandardStaffingDemandRevision> standardRevisions,
        IEnumerable<StaffingDemandDateException> dateExceptions)
    {
        ServiceCatalogData serviceCatalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);

        return new StaffingDemandReadData(
            standardRevisions,
            dateExceptions,
            serviceCatalog);
    }

    private static Task<StaffingDemandWeekQueryResult> ExecuteQueryAsync(
        GetStaffingDemandWeekQuery query)
    {
        return query.ExecuteAsync(WeekMonday, TestContext.Current.CancellationToken);
    }

    private static StandardStaffingDemandRevision CreateAddition(
        Guid id,
        Guid workLocationId,
        Guid shiftTypeId)
    {
        return Assert.IsType<StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateAddition(
                id,
                DayOfWeek.Monday,
                workLocationId,
                shiftTypeId,
                DateOnly.MinValue,
                new TimeOnly(6, 30),
                new TimeOnly(13, 30),
                1).Value);
    }

    private static StaffingDemandDateException CreateDateReplacement(
        DateOnly date,
        Guid workLocationId,
        Guid shiftTypeId,
        TimeOnly start,
        TimeOnly end,
        int requiredEmployeeCount)
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateReplacement(
                Guid.NewGuid(),
                date,
                workLocationId,
                shiftTypeId,
                start,
                end,
                requiredEmployeeCount).Value);
    }

    private sealed class FakeStaffingDemandReader : IStaffingDemandReader
    {
        private readonly StaffingDemandReadData _data;

        public FakeStaffingDemandReader(StaffingDemandReadData data)
        {
            _data = data;
        }

        public int LoadCallCount { get; private set; }

        public Task<StaffingDemandReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }
    }

    private sealed class CancelDuringReadReader : IStaffingDemandReader
    {
        private readonly StaffingDemandReadData _data;
        private readonly CancellationTokenSource _cancellation;

        public CancelDuringReadReader(
            StaffingDemandReadData data,
            CancellationTokenSource cancellation)
        {
            _data = data;
            _cancellation = cancellation;
        }

        public int LoadCallCount { get; private set; }

        public Task<StaffingDemandReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            _cancellation.Cancel();
            return Task.FromResult(_data);
        }
    }
}
