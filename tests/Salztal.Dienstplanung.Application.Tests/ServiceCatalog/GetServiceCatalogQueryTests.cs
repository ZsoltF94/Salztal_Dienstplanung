using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.ServiceCatalog;

public sealed class GetServiceCatalogQueryTests
{
    [Fact]
    public async Task ExecuteAsyncWhenCatalogExistsReturnsCompleteImmutableSnapshot()
    {
        ServiceCatalogData data = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);
        FakeServiceCatalogReader reader = new(data);
        GetServiceCatalogQuery query = new(reader);

        ServiceCatalogSnapshot result = await query.ExecuteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.WorkLocations.Count);
        Assert.Equal(4, result.ShiftTypes.Count);
        Assert.True(((ICollection<WorkLocationSnapshot>)result.WorkLocations).IsReadOnly);
        Assert.True(((ICollection<ShiftTypeSnapshot>)result.ShiftTypes).IsReadOnly);

        WorkLocationSnapshot cafeteria = result.WorkLocations[0];
        Assert.Equal(InitialWorkLocationCatalog.Cafeteria.Id.Value, cafeteria.Id);
        Assert.Equal("Cafeteria", cafeteria.Name);
        Assert.Equal("yellow", cafeteria.ColorCode);

        ShiftTypeSnapshot cafeteriaShiftA = result.ShiftTypes[2];
        Assert.Equal(InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value, cafeteriaShiftA.Id);
        Assert.True(cafeteriaShiftA.UsesActualTimeAsDisplay);
        Assert.Null(cafeteriaShiftA.Abbreviation);
        Assert.Equal(new TimeOnly(13, 30), cafeteriaShiftA.StandardStart);
        Assert.Equal(new TimeOnly(20, 30), cafeteriaShiftA.StandardEnd);

        Assert.Equal("D", result.SplitShiftPattern.DisplayCode);
        Assert.Equal(180, result.SplitShiftPattern.StandardBreakMinutes);
        Assert.Equal(600, result.SplitShiftPattern.StandardWorkMinutes);
        Assert.Equal("Spr", result.ReliefShiftPattern.DisplayCode);
        Assert.Equal("blue", result.ReliefShiftPattern.DisplayColorCode);
        Assert.Equal(DayOfWeek.Saturday, result.ReliefShiftPattern.AllowedDay);
        Assert.True(result.ReliefShiftPattern.SwitchesAtEndOfFirstActualDemand);
        Assert.False(result.ReliefShiftPattern.HasInterruption);
        Assert.Equal(1, reader.LoadCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenCancelledDoesNotReadCatalog()
    {
        ServiceCatalogData data = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);
        FakeServiceCatalogReader reader = new(data);
        GetServiceCatalogQuery query = new(reader);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => query.ExecuteAsync(cancellation.Token));

        Assert.Equal(0, reader.LoadCallCount);
    }

    private sealed class FakeServiceCatalogReader : IServiceCatalogReader
    {
        private readonly ServiceCatalogData _data;

        public FakeServiceCatalogReader(ServiceCatalogData data)
        {
            _data = data;
        }

        public int LoadCallCount { get; private set; }

        public Task<ServiceCatalogData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }
    }
}
