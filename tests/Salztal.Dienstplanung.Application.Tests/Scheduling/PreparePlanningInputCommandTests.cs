using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class PreparePlanningInputCommandTests
{
    [Fact]
    public async Task InitialPreparationCreatesCompleteImmutableSolverInput()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningHistoryDayReadItem[] history = CreateCompleteHistory();
        FakePreparePlanningSnapshotStore store = new();

        PreparePlanningInputResult result = await Prepare(
            draft,
            history,
            store,
            PlanningRunOptions.Default);

        PlanningInputSnapshot snapshot = Assert.IsType<PlanningInputSnapshot>(result.Value);
        Assert.Equal(PreparePlanningInputStatus.Succeeded, result.Status);
        Assert.Equal(draft.Id.Value, snapshot.DraftId);
        Assert.Equal(draft.Version.Value, snapshot.DraftVersion);
        Assert.Equal(2, snapshot.Employees.Count);
        Assert.Equal(2, snapshot.EmployeeTypes.Count);
        Assert.Equal(2, snapshot.ServiceCatalog.WorkLocations.Count);
        Assert.Equal(4, snapshot.ServiceCatalog.ShiftTypes.Count);
        Assert.Empty(snapshot.AvailabilityEntries);
        Assert.Equal(195, snapshot.DemandSlots.Count);
        Assert.Equal(3, snapshot.ServiceManagementAssignments.Count);
        Assert.Equal(1, snapshot.RuleCatalog.Version);
        Assert.Equal(28, snapshot.RuleCatalog.Definitions.Count);
        Assert.False(snapshot.RunOptions.EnableAuxiliaryReliefShift);
        Assert.Equal(PlanningHistoryCompleteness.Complete, snapshot.History.Completeness);
        Assert.Equal(7, snapshot.History.AvailableDayCount);
        Assert.True(((ICollection<PlanningEmployeeSnapshot>)snapshot.Employees).IsReadOnly);
        Assert.True(((ICollection<ScheduleDemandSlotSnapshot>)snapshot.DemandSlots).IsReadOnly);
        Assert.Equal(draft.Version.Value, store.Change?.ExpectedDraftVersion);
        Assert.Null(store.Change?.ExpectedPreviousSnapshotId);
    }

    [Theory]
    [InlineData(0, PlanningHistoryCompleteness.Missing)]
    [InlineData(3, PlanningHistoryCompleteness.Partial)]
    public async Task PreparationAllowsMissingHistoryAndMarksCompleteness(
        int availableDays,
        PlanningHistoryCompleteness expectedCompleteness)
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningHistoryDayReadItem[] history = CreateCompleteHistory()
            .Take(availableDays)
            .ToArray();

        PlanningInputSnapshot snapshot = Assert.IsType<PlanningInputSnapshot>(
            (await Prepare(
                draft,
                history,
                new FakePreparePlanningSnapshotStore(),
                PlanningRunOptions.Default)).Value);

        Assert.Equal(expectedCompleteness, snapshot.History.Completeness);
        Assert.Equal(7, snapshot.History.Days.Count);
        Assert.Equal(availableDays, snapshot.History.AvailableDayCount);
    }

    [Fact]
    public async Task ExplicitAuxiliaryReliefOptionIsOffByDefaultAndCanBeStoredOnPurpose()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot defaultSnapshot = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        PlanningInputSnapshot enabledSnapshot = await PrepareSuccessfully(
            draft,
            new PlanningRunOptions(true));

        Assert.False(defaultSnapshot.RunOptions.EnableAuxiliaryReliefShift);
        Assert.True(enabledSnapshot.RunOptions.EnableAuxiliaryReliefShift);
    }

    [Fact]
    public async Task RefreshReplacesExpectedSnapshotAndPreservesValidTyp1Assignments()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot previous = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        FakePreparePlanningSnapshotStore store = new();

        PreparePlanningInputResult result = await Prepare(
            draft,
            CreateCompleteHistory(),
            store,
            new PlanningRunOptions(true),
            previous);

        PlanningInputSnapshot refreshed = Assert.IsType<PlanningInputSnapshot>(result.Value);
        Assert.NotEqual(previous.Id, refreshed.Id);
        Assert.Equal(
            previous.ServiceManagementAssignments.Select(item => item.AssignmentId),
            refreshed.ServiceManagementAssignments.Select(item => item.AssignmentId));
        Assert.True(refreshed.RunOptions.EnableAuxiliaryReliefShift);
        Assert.Equal(previous.Id, store.Change?.ExpectedPreviousSnapshotId);
        Assert.False(previous.RunOptions.EnableAuxiliaryReliefShift);
    }

    [Fact]
    public async Task InvalidTyp1AssignmentBlocksRefreshWithStructuredReason()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot previous = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        DateOnly assignmentDate = draft.Assignments[0].Date;
        AvailabilityEntryReadItem conflictingEntry =
            ScheduleWorkspaceTestContext.CreateEntry(
                ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
                assignmentDate,
                AvailabilityEntryKind.Vacation,
                2);
        FakePreparePlanningSnapshotStore store = new();
        ScheduleWorkspaceReadData workspace =
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                draft,
                entries: [conflictingEntry]);
        FakePlanningInputReader reader = new(
            new PlanningInputReadData(
                workspace,
                CreateCompleteHistory(),
                previous));

        PreparePlanningInputResult result = await new PreparePlanningInputCommand(
            reader,
            store).ExecuteAsync(
                CreateRequest(draft, previous),
                TestContext.Current.CancellationToken);

        PreparePlanningInputError error = Assert.Single(result.Errors);
        InvalidServiceManagementAssignment invalid = Assert.Single(
            error.InvalidAssignments);
        Assert.Equal(PreparePlanningInputStatus.ValidationFailed, result.Status);
        Assert.Equal(
            PreparePlanningInputErrorCode.InvalidServiceManagementAssignment,
            error.Code);
        Assert.Equal(
            InvalidServiceManagementAssignmentCode.AvailabilityConflict,
            invalid.Code);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task MissingTyp1WeekBlocksPreparationButFullyMissingHistoryDoesNot()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        FakePreparePlanningSnapshotStore store = new();

        PreparePlanningInputResult result = await Prepare(
            draft,
            [],
            store,
            PlanningRunOptions.Default);

        PreparePlanningInputError error = Assert.Single(result.Errors);
        Assert.Equal(PreparePlanningInputStatus.NotReady, result.Status);
        Assert.Equal(PreparePlanningInputErrorCode.ServiceManagementNotReady, error.Code);
        Assert.Equal(3, error.WeekMondays.Count);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task StoreConflictDoesNotReplacePreviousSnapshot()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot previous = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        FakePreparePlanningSnapshotStore store = new(
            PreparePlanningSnapshotStoreResult.Conflict());

        PreparePlanningInputResult result = await Prepare(
            draft,
            CreateCompleteHistory(),
            store,
            new PlanningRunOptions(true),
            previous);

        Assert.Equal(PreparePlanningInputStatus.Conflict, result.Status);
        Assert.False(previous.RunOptions.EnableAuxiliaryReliefShift);
        Assert.Equal(previous.Id, store.Change?.ExpectedPreviousSnapshotId);
    }

    [Fact]
    public async Task PreparationExpectationMismatchDoesNotCallStore()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot previous = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        FakePreparePlanningSnapshotStore store = new();
        FakePlanningInputReader reader = new(
            new PlanningInputReadData(
                ScheduleWorkspaceTestContext.CreateReadDataForDraft(draft),
                CreateCompleteHistory(),
                previous));
        PreparePlanningInputRequest request = new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            PlanningPreparationExpectation.NotPrepared,
            null,
            PlanningRunOptions.Default);

        PreparePlanningInputResult result = await new PreparePlanningInputCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(PreparePlanningInputStatus.Conflict, result.Status);
        Assert.Equal(
            PreparePlanningInputErrorCode.PreparationStateConflict,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Theory]
    [InlineData(PlanningInputChangeCategory.EmployeesAndTypes)]
    [InlineData(PlanningInputChangeCategory.ShiftEligibilities)]
    [InlineData(PlanningInputChangeCategory.ServiceCatalog)]
    [InlineData(PlanningInputChangeCategory.StaffingDemands)]
    [InlineData(PlanningInputChangeCategory.AvailabilityEntries)]
    [InlineData(PlanningInputChangeCategory.ServiceManagementAssignments)]
    [InlineData(PlanningInputChangeCategory.RuleCatalog)]
    [InlineData(PlanningInputChangeCategory.RunOptions)]
    [InlineData(PlanningInputChangeCategory.History)]
    public async Task ComparisonReportsEveryChangedComponentSeparately(
        PlanningInputChangeCategory category)
    {
        PlanningInputSnapshot baseline = await PrepareSuccessfully(
            CreateReadyDraft(),
            PlanningRunOptions.Default);
        PlanningInputSnapshot changed = ChangeOnly(baseline, category);

        PlanningInputComparison comparison = PlanningInputComparison.Compare(
            baseline,
            changed);

        Assert.False(comparison.IsCurrent);
        Assert.Equal([category], comparison.ChangedCategories);
    }

    [Fact]
    public async Task RepeatedWorkspaceReadsKeepPreparedSnapshotUnchanged()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot prepared = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        PlanningHistoryDayReadItem[] history = CreateCompleteHistory();
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                draft,
                historyDays: history,
                preparedSnapshot: prepared));
        GetScheduleWorkspaceQuery query = new(reader);

        ScheduleWorkspaceQueryResult first = await query.ExecuteAsync(
            draft.Period.StartMonday,
            TestContext.Current.CancellationToken);
        ScheduleWorkspaceQueryResult second = await query.ExecuteAsync(
            draft.Period.StartMonday,
            TestContext.Current.CancellationToken);

        Assert.Equal(SchedulePreparationStatus.Prepared, first.Value?.PreparationStatus);
        Assert.Equal(SchedulePreparationStatus.Prepared, second.Value?.PreparationStatus);
        Assert.Equal(prepared.Id, first.Value?.PreparedSnapshotId);
        Assert.Equal(prepared.Id, second.Value?.PreparedSnapshotId);
        Assert.Empty(first.Value!.ChangedCategories);
        Assert.Equal(1, prepared.DraftVersion);
        Assert.Equal(2, reader.LoadCallCount);
    }

    [Fact]
    public async Task WorkspaceReadMarksChangedAvailabilityAsOutdated()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningInputSnapshot prepared = await PrepareSuccessfully(
            draft,
            PlanningRunOptions.Default);
        AvailabilityEntryReadItem entry = ScheduleWorkspaceTestContext.CreateEntry(
            ScheduleWorkspaceTestContext.StandardEmployeeId,
            draft.Period.StartMonday,
            AvailabilityEntryKind.FixedDayOff,
            3);
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                draft,
                entries: [entry],
                historyDays: CreateCompleteHistory(),
                preparedSnapshot: prepared));

        ScheduleWorkspaceSnapshot snapshot = Assert.IsType<ScheduleWorkspaceSnapshot>(
            (await new GetScheduleWorkspaceQuery(reader).ExecuteAsync(
                draft.Period.StartMonday,
                TestContext.Current.CancellationToken)).Value);

        Assert.Equal(SchedulePreparationStatus.Outdated, snapshot.PreparationStatus);
        Assert.Equal(
            [PlanningInputChangeCategory.AvailabilityEntries],
            snapshot.ChangedCategories);
    }

    private static async Task<PlanningInputSnapshot> PrepareSuccessfully(
        ScheduleDraft draft,
        PlanningRunOptions options)
    {
        PreparePlanningInputResult result = await Prepare(
            draft,
            CreateCompleteHistory(),
            new FakePreparePlanningSnapshotStore(),
            options);
        return Assert.IsType<PlanningInputSnapshot>(result.Value);
    }

    private static Task<PreparePlanningInputResult> Prepare(
        ScheduleDraft draft,
        IEnumerable<PlanningHistoryDayReadItem> history,
        FakePreparePlanningSnapshotStore store,
        PlanningRunOptions options,
        PlanningInputSnapshot? previous = null)
    {
        ScheduleWorkspaceReadData workspace =
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(draft);
        FakePlanningInputReader reader = new(
            new PlanningInputReadData(workspace, history, previous));
        return new PreparePlanningInputCommand(reader, store).ExecuteAsync(
            CreateRequest(draft, previous, options),
            TestContext.Current.CancellationToken);
    }

    private static PreparePlanningInputRequest CreateRequest(
        ScheduleDraft draft,
        PlanningInputSnapshot? previous = null,
        PlanningRunOptions? options = null)
    {
        return new PreparePlanningInputRequest(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            previous is null
                ? PlanningPreparationExpectation.NotPrepared
                : PlanningPreparationExpectation.Prepared,
            previous?.Id,
            options ?? PlanningRunOptions.Default);
    }

    private static ScheduleDraft CreateReadyDraft()
    {
        ScheduleDraft empty = ScheduleWorkspaceTestContext.CreateDraft();
        List<ScheduleAssignment> assignments = [];
        for (int week = 0; week < 3; week++)
        {
            DateOnly monday = empty.Period.StartMonday.AddDays(week * 7);
            DemandSlot slot = Assert.Single(
                empty.DemandSlots.Slots,
                item => item.Id.Date == monday
                    && item.Id.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id
                    && item.Id.Ordinal == 1);
            assignments.Add(Assert.IsType<ScheduleAssignment>(
                ScheduleAssignment.CreateNormal(
                    Guid.Parse($"60000000-0000-4000-8000-{week + 1:D12}"),
                    ScheduleWorkspaceTestContext.ServiceManagementEmployee.Id,
                    slot,
                    AssignmentOrigin.ServiceManagement).Value));
        }

        return ScheduleWorkspaceTestContext.CreateDraft(assignments: assignments);
    }

    private static PlanningHistoryDayReadItem[] CreateCompleteHistory()
    {
        DateOnly first = ScheduleWorkspaceTestContext.PeriodMonday.AddDays(-7);
        return Enumerable.Range(0, 7)
            .Select(index => new PlanningHistoryDayReadItem(
                first.AddDays(index),
                [
                    new PlanningHistoryAssignmentReadItem(
                        ScheduleWorkspaceTestContext.StandardEmployeeId,
                        300),
                ]))
            .ToArray();
    }

    private static PlanningInputSnapshot ChangeOnly(
        PlanningInputSnapshot source,
        PlanningInputChangeCategory category)
    {
        PlanningEmployeeSnapshot[] employees = source.Employees.ToArray();
        PlanningEmployeeTypeSnapshot[] types = source.EmployeeTypes.ToArray();
        PlanningServiceCatalogSnapshot catalog = source.ServiceCatalog;
        PlanningAvailabilityEntrySnapshot[] entries = source.AvailabilityEntries.ToArray();
        ScheduleDemandSlotSnapshot[] slots = source.DemandSlots.ToArray();
        ScheduleAssignmentSnapshot[] assignments =
            source.ServiceManagementAssignments.ToArray();
        RuleCatalogSnapshot rules = source.RuleCatalog;
        PlanningRunOptions options = source.RunOptions;
        PlanningHistorySnapshot history = source.History;

        switch (category)
        {
            case PlanningInputChangeCategory.EmployeesAndTypes:
                employees[0] = employees[0] with { FirstName = "Geändert" };
                break;
            case PlanningInputChangeCategory.ShiftEligibilities:
                types[0] = CopyType(types[0], []);
                break;
            case PlanningInputChangeCategory.ServiceCatalog:
                PlanningWorkLocationSnapshot[] locations = catalog.WorkLocations.ToArray();
                locations[0] = locations[0] with { Name = "Geändert" };
                catalog = new PlanningServiceCatalogSnapshot(
                    locations,
                    catalog.ShiftTypes,
                    catalog.SplitShiftPattern,
                    catalog.ReliefShiftPattern);
                break;
            case PlanningInputChangeCategory.StaffingDemands:
                slots = slots.Skip(1).ToArray();
                break;
            case PlanningInputChangeCategory.AvailabilityEntries:
                entries =
                [
                    new PlanningAvailabilityEntrySnapshot(
                        source.Employees[0].Id,
                        source.PeriodMonday,
                        AvailabilityEntryKind.FixedDayOff,
                        1),
                ];
                break;
            case PlanningInputChangeCategory.ServiceManagementAssignments:
                assignments = assignments.Skip(1).ToArray();
                break;
            case PlanningInputChangeCategory.RuleCatalog:
                rules = new RuleCatalogSnapshot(
                    source.RuleCatalog.Version + 1,
                    source.RuleCatalog.Definitions);
                break;
            case PlanningInputChangeCategory.RunOptions:
                options = new PlanningRunOptions(
                    !source.RunOptions.EnableAuxiliaryReliefShift);
                break;
            case PlanningInputChangeCategory.History:
                history = new PlanningHistorySnapshot(
                    PlanningHistoryCompleteness.Partial,
                    source.History.Days);
                break;
            default:
                throw new InvalidOperationException();
        }

        return new PlanningInputSnapshot(
            Guid.NewGuid(),
            source.DraftId,
            source.DraftVersion + 1,
            source.PeriodMonday,
            source.PeriodSunday,
            employees,
            types,
            catalog,
            entries,
            slots,
            assignments,
            rules,
            options,
            history);
    }

    private static PlanningEmployeeTypeSnapshot CopyType(
        PlanningEmployeeTypeSnapshot source,
        IEnumerable<PlanningEmployeeTypeEligibilitySnapshot> eligibilities)
    {
        return new PlanningEmployeeTypeSnapshot(
            source.Id,
            source.Code,
            source.Name,
            source.WeeklyWorkTargetMinutes,
            source.AllowsVacationAndSickness,
            source.AbsenceDayValueMinutes,
            source.PlanningRole,
            source.AllowsAutomaticAssignment,
            source.RequiresWeeklyManualAssignment,
            source.PreservesManualAssignmentsOnGeneration,
            source.ManualSuggestionPriority,
            eligibilities);
    }
}

internal sealed class FakePlanningInputReader : IPlanningInputReader
{
    private readonly PlanningInputReadData _data;

    public FakePlanningInputReader(PlanningInputReadData data)
    {
        _data = data;
    }

    public int LoadCallCount { get; private set; }

    public Task<PlanningInputReadData> LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LoadCallCount++;
        return Task.FromResult(_data);
    }
}

internal sealed class FakePreparePlanningSnapshotStore : IPreparePlanningSnapshotStore
{
    private readonly PreparePlanningSnapshotStoreResult? _configuredResult;

    public FakePreparePlanningSnapshotStore(
        PreparePlanningSnapshotStoreResult? configuredResult = null)
    {
        _configuredResult = configuredResult;
    }

    public int SaveCallCount { get; private set; }

    public PreparePlanningSnapshotChange? Change { get; private set; }

    public Task<PreparePlanningSnapshotStoreResult> SaveAsync(
        PreparePlanningSnapshotChange change,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveCallCount++;
        Change = change;
        return Task.FromResult(
            _configuredResult
            ?? PreparePlanningSnapshotStoreResult.Success(change.Snapshot));
    }
}
