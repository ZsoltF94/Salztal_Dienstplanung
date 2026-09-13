using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.ServiceCatalog;

public sealed class UpdateShiftTypeStandardTimeCommandTests
{
    [Fact]
    public async Task ExecuteAsyncWhenInputIsValidUpdatesOnlyStandardTime()
    {
        FakeShiftTypeStandardTimeUpdateStore store = new(InitialShiftTypeCatalog.EarlyShift);
        UpdateShiftTypeStandardTimeCommand command = new(store);
        UpdateShiftTypeStandardTimeRequest request = new(
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            new TimeOnly(7, 0),
            new TimeOnly(14, 0));

        UpdateShiftTypeStandardTimeResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateShiftTypeStandardTimeStatus.Succeeded, result.Status);
        ShiftTypeSnapshot snapshot = Assert.IsType<ShiftTypeSnapshot>(result.Value);
        Assert.Equal(request.ShiftTypeId, snapshot.Id);
        Assert.Equal("Frühdienst", snapshot.Name);
        Assert.Equal("F", snapshot.Abbreviation);
        Assert.Equal(new TimeOnly(7, 0), snapshot.StandardStart);
        Assert.Equal(new TimeOnly(14, 0), snapshot.StandardEnd);
        Assert.Equal(420, snapshot.StandardDurationMinutes);
        Assert.Empty(result.Errors);
        Assert.Same(InitialShiftTypeCatalog.EarlyShift, store.ExpectedValue);
        Assert.Equal(new TimeOnly(7, 0), store.ReplacementValue?.StandardTime.Start);
    }

    [Theory]
    [InlineData(6, 15, 0, 6, 0, 0)]
    [InlineData(6, 15, 1, 6, 15, 2)]
    public async Task ExecuteAsyncWhenInputIsInvalidReturnsGermanValidationErrorsWithoutStoreAccess(
        int startHour,
        int startMinute,
        int startSecond,
        int endHour,
        int endMinute,
        int endSecond)
    {
        FakeShiftTypeStandardTimeUpdateStore store = new(InitialShiftTypeCatalog.EarlyShift);
        UpdateShiftTypeStandardTimeCommand command = new(store);
        UpdateShiftTypeStandardTimeRequest request = new(
            Guid.Empty,
            new TimeOnly(startHour, startMinute, startSecond),
            new TimeOnly(endHour, endMinute, endSecond));

        UpdateShiftTypeStandardTimeResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateShiftTypeStandardTimeStatus.ValidationFailed, result.Status);
        Assert.Null(result.Value);
        Assert.All(result.Errors, error => Assert.False(string.IsNullOrWhiteSpace(error.Message)));
        Assert.Contains(
            result.Errors,
            error => error.Code == UpdateShiftTypeStandardTimeErrorCode.IdentifierRequired);
        Assert.Equal(0, store.FindCallCount);
        Assert.Equal(0, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenShiftTypeDoesNotExistReturnsNotFound()
    {
        FakeShiftTypeStandardTimeUpdateStore store = new(null);
        UpdateShiftTypeStandardTimeCommand command = new(store);

        UpdateShiftTypeStandardTimeResult result = await command.ExecuteAsync(
            new UpdateShiftTypeStandardTimeRequest(
                Guid.NewGuid(),
                new TimeOnly(7, 0),
                new TimeOnly(14, 0)),
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateShiftTypeStandardTimeStatus.NotFound, result.Status);
        UpdateShiftTypeStandardTimeError error = Assert.Single(result.Errors);
        Assert.Equal(UpdateShiftTypeStandardTimeErrorCode.NotFound, error.Code);
        Assert.Equal("Der Diensttyp wurde nicht gefunden.", error.Message);
        Assert.Equal(0, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenNewTimeOverlapsSplitShiftReturnsValidationError()
    {
        FakeShiftTypeStandardTimeUpdateStore store = new(InitialShiftTypeCatalog.EarlyShift);
        UpdateShiftTypeStandardTimeCommand command = new(store);

        UpdateShiftTypeStandardTimeResult result = await command.ExecuteAsync(
            new UpdateShiftTypeStandardTimeRequest(
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                new TimeOnly(6, 30),
                new TimeOnly(17, 0)),
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateShiftTypeStandardTimeStatus.ValidationFailed, result.Status);
        UpdateShiftTypeStandardTimeError error = Assert.Single(result.Errors);
        Assert.Equal(
            UpdateShiftTypeStandardTimeErrorCode.SplitShiftSegmentsMustNotOverlap,
            error.Code);
        Assert.Contains("nicht überschneiden", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenStoredValueChangedReturnsConflict()
    {
        FakeShiftTypeStandardTimeUpdateStore store = new(InitialShiftTypeCatalog.LateShift)
        {
            UpdateResult = CatalogEntryUpdateStoreResult.Conflict,
        };
        UpdateShiftTypeStandardTimeCommand command = new(store);

        UpdateShiftTypeStandardTimeResult result = await command.ExecuteAsync(
            new UpdateShiftTypeStandardTimeRequest(
                InitialShiftTypeCatalog.LateShift.Id.Value,
                new TimeOnly(17, 0),
                new TimeOnly(20, 0)),
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateShiftTypeStandardTimeStatus.Conflict, result.Status);
        UpdateShiftTypeStandardTimeError error = Assert.Single(result.Errors);
        Assert.Equal(UpdateShiftTypeStandardTimeErrorCode.Conflict, error.Code);
        Assert.Contains("Bitte laden Sie die Daten neu", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenCancelledDoesNotAccessStore()
    {
        FakeShiftTypeStandardTimeUpdateStore store = new(InitialShiftTypeCatalog.EarlyShift);
        UpdateShiftTypeStandardTimeCommand command = new(store);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => command.ExecuteAsync(
            new UpdateShiftTypeStandardTimeRequest(
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                new TimeOnly(7, 0),
                new TimeOnly(14, 0)),
            cancellation.Token));

        Assert.Equal(0, store.FindCallCount);
        Assert.Equal(0, store.UpdateCallCount);
    }

    private sealed class FakeShiftTypeStandardTimeUpdateStore :
        IShiftTypeStandardTimeUpdateStore
    {
        private readonly ShiftType? _current;

        public FakeShiftTypeStandardTimeUpdateStore(ShiftType? current)
        {
            _current = current;
        }

        public CatalogEntryUpdateStoreResult UpdateResult { get; init; } =
            CatalogEntryUpdateStoreResult.Updated;

        public int FindCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public ShiftType? ExpectedValue { get; private set; }

        public ShiftType? ReplacementValue { get; private set; }

        public Task<ShiftTypeStandardTimeUpdateData?> FindAsync(
            ShiftTypeId id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FindCallCount++;
            ShiftTypeStandardTimeUpdateData? data = _current?.Id == id
                ? new ShiftTypeStandardTimeUpdateData(
                    _current,
                    InitialShiftTypeCatalog.EarlyShift,
                    InitialShiftTypeCatalog.LateShift,
                    InitialShiftPatternCatalog.SplitShift)
                : null;
            return Task.FromResult(data);
        }

        public Task<CatalogEntryUpdateStoreResult> UpdateAsync(
            ShiftTypeStandardTimeUpdateData expected,
            ShiftType replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateCallCount++;
            ExpectedValue = expected.Current;
            ReplacementValue = replacement;
            return Task.FromResult(UpdateResult);
        }
    }
}
