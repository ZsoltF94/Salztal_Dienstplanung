using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class OpenOrCreateScheduleDraftCommandTests
{
    [Fact]
    public async Task ExecuteForWednesdayCreatesNormalizedTwentyOneDayDraft()
    {
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadData());
        FakeOpenScheduleDraftStore store = new();

        OpenOrCreateScheduleDraftResult result = await Execute(
            reader,
            store,
            ScheduleWorkspaceTestContext.PeriodMonday.AddDays(2));

        OpenedScheduleDraftSnapshot value = Assert.IsType<OpenedScheduleDraftSnapshot>(
            result.Value);
        ScheduleDraft created = Assert.IsType<ScheduleDraft>(store.CreatedDraft);
        Assert.Equal(OpenOrCreateScheduleDraftStatus.Succeeded, result.Status);
        Assert.Equal(ScheduleDraftOpenOutcome.Created, value.Outcome);
        Assert.Equal(ScheduleWorkspaceTestContext.PeriodMonday, value.PeriodMonday);
        Assert.Equal(ScheduleWorkspaceTestContext.PeriodMonday.AddDays(20), value.PeriodSunday);
        Assert.Equal(195, created.DemandSlots.Slots.Count);
        Assert.Empty(created.AvailabilityEntries.Entries);
        Assert.Equal([ScheduleWorkspaceTestContext.Period], reader.RequestedPeriods);
        Assert.Equal(1, store.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteWhenExactDraftExistsOpensItWithoutCreatingAnother()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(version: 4);
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadData(
                draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(draft)],
                exactDraft: draft));
        FakeOpenScheduleDraftStore store = new();

        OpenOrCreateScheduleDraftResult result = await Execute(reader, store);

        OpenedScheduleDraftSnapshot value = Assert.IsType<OpenedScheduleDraftSnapshot>(
            result.Value);
        Assert.Equal(ScheduleDraftOpenOutcome.OpenedExisting, value.Outcome);
        Assert.Equal(draft.Id.Value, value.DraftId);
        Assert.Equal(4, value.Version);
        Assert.Equal(0, store.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteWhenStoredPeriodOverlapsRejectsCreation()
    {
        SchedulePeriod overlappingPeriod = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(
                ScheduleWorkspaceTestContext.PeriodMonday.AddDays(14)).Value);
        ScheduleDraft overlappingDraft = ScheduleWorkspaceTestContext.CreateDraft(
            overlappingPeriod);
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadData(
                draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(overlappingDraft)]));
        FakeOpenScheduleDraftStore store = new();

        OpenOrCreateScheduleDraftResult result = await Execute(reader, store);

        ScheduleWorkspaceError error = Assert.Single(result.Errors);
        Assert.Equal(OpenOrCreateScheduleDraftStatus.Overlap, result.Status);
        Assert.Equal(ScheduleWorkspaceErrorCode.OverlappingPeriod, error.Code);
        Assert.Equal(overlappingPeriod.StartMonday, error.ConflictingPeriodMonday);
        Assert.Equal(overlappingPeriod.EndSunday, error.ConflictingPeriodSunday);
        Assert.Equal(0, store.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteWhenStoredPeriodIsAdjacentCreatesDraft()
    {
        SchedulePeriod adjacentPeriod = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(
                ScheduleWorkspaceTestContext.PeriodMonday.AddDays(21)).Value);
        ScheduleDraft adjacentDraft = ScheduleWorkspaceTestContext.CreateDraft(adjacentPeriod);
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadData(
                draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(adjacentDraft)]));
        FakeOpenScheduleDraftStore store = new();

        OpenOrCreateScheduleDraftResult result = await Execute(reader, store);

        Assert.Equal(OpenOrCreateScheduleDraftStatus.Succeeded, result.Status);
        Assert.Equal(1, store.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteWhenStoreDetectsConcurrentOverlapReturnsThatPeriod()
    {
        SchedulePeriod overlappingPeriod = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(
                ScheduleWorkspaceTestContext.PeriodMonday.AddDays(-14)).Value);
        FakeOpenScheduleDraftStore store = new(
            OpenScheduleDraftStoreResult.Overlap(overlappingPeriod));

        OpenOrCreateScheduleDraftResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData()),
            store);

        ScheduleWorkspaceError error = Assert.Single(result.Errors);
        Assert.Equal(OpenOrCreateScheduleDraftStatus.Overlap, result.Status);
        Assert.Equal(overlappingPeriod.StartMonday, error.ConflictingPeriodMonday);
        Assert.Equal(overlappingPeriod.EndSunday, error.ConflictingPeriodSunday);
    }

    [Fact]
    public async Task ExecuteWhenStoreDetectsConcurrentWriteReturnsConflict()
    {
        FakeOpenScheduleDraftStore store = new(OpenScheduleDraftStoreResult.Conflict());

        OpenOrCreateScheduleDraftResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData()),
            store);

        ScheduleWorkspaceError error = Assert.Single(result.Errors);
        Assert.Equal(OpenOrCreateScheduleDraftStatus.Conflict, result.Status);
        Assert.Equal(ScheduleWorkspaceErrorCode.Conflict, error.Code);
    }

    [Fact]
    public async Task ExecuteWhenExactHeadersAreDuplicatedReturnsStoredDataError()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        ScheduleDraftHeader header = ScheduleWorkspaceTestContext.CreateHeader(draft);

        OpenOrCreateScheduleDraftResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(
                    draftHeaders: [header, header],
                    exactDraft: draft)),
            new FakeOpenScheduleDraftStore());

        Assert.Equal(OpenOrCreateScheduleDraftStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            ScheduleWorkspaceErrorCode.StoredDataInvalid,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenEmployeeTypeIsMissingReturnsCatalogError()
    {
        OpenOrCreateScheduleDraftResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(employeeTypes: [])),
            new FakeOpenScheduleDraftStore());

        Assert.Equal(OpenOrCreateScheduleDraftStatus.CatalogInvalid, result.Status);
        Assert.Equal(ScheduleWorkspaceErrorCode.CatalogInvalid, Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenAvailabilityChangeVersionIsInvalidReturnsStoredDataError()
    {
        OpenOrCreateScheduleDraftResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(
                    entries:
                    [
                        ScheduleWorkspaceTestContext.CreateEntry(
                            ScheduleWorkspaceTestContext.StandardEmployeeId,
                            ScheduleWorkspaceTestContext.PeriodMonday,
                            AvailabilityEntryKind.FixedDayOff,
                            0),
                    ])),
            new FakeOpenScheduleDraftStore());

        Assert.Equal(OpenOrCreateScheduleDraftStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            ScheduleWorkspaceErrorCode.StoredDataInvalid,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenAlreadyCancelledDoesNotReadOrWrite()
    {
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadData());
        FakeOpenScheduleDraftStore store = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<OpenOrCreateScheduleDraftResult> execution =
            new OpenOrCreateScheduleDraftCommand(reader, store).ExecuteAsync(
                ScheduleWorkspaceTestContext.PeriodMonday,
                cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.CreateCallCount);
    }

    private static Task<OpenOrCreateScheduleDraftResult> Execute(
        FakeScheduleWorkspaceReader reader,
        FakeOpenScheduleDraftStore store,
        DateOnly? selectedDate = null)
    {
        return new OpenOrCreateScheduleDraftCommand(reader, store).ExecuteAsync(
            selectedDate ?? ScheduleWorkspaceTestContext.PeriodMonday,
            TestContext.Current.CancellationToken);
    }
}

internal sealed class FakeScheduleWorkspaceReader : IScheduleWorkspaceReader
{
    private readonly ScheduleWorkspaceReadData _data;

    public FakeScheduleWorkspaceReader(ScheduleWorkspaceReadData data)
    {
        _data = data;
    }

    public int LoadCallCount { get; private set; }

    public List<SchedulePeriod> RequestedPeriods { get; } = [];

    public Task<ScheduleWorkspaceReadData> LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LoadCallCount++;
        RequestedPeriods.Add(period);
        return Task.FromResult(_data);
    }
}

internal sealed class FakeOpenScheduleDraftStore : IOpenScheduleDraftStore
{
    private readonly OpenScheduleDraftStoreResult? _configuredResult;

    public FakeOpenScheduleDraftStore(
        OpenScheduleDraftStoreResult? configuredResult = null)
    {
        _configuredResult = configuredResult;
    }

    public int CreateCallCount { get; private set; }

    public ScheduleDraft? CreatedDraft { get; private set; }

    public Task<OpenScheduleDraftStoreResult> CreateAsync(
        ScheduleDraft draft,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CreateCallCount++;
        CreatedDraft = draft;
        return Task.FromResult(_configuredResult ?? OpenScheduleDraftStoreResult.Success(draft));
    }
}
