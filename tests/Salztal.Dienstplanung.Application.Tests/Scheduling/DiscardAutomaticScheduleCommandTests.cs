using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class DiscardAutomaticScheduleCommandTests
{
    [Fact]
    public async Task SuccessfulDiscardPassesOneExactChangeAndAdvancesOnce()
    {
        DiscardTestContext context = DiscardTestContext.Create();

        AutomaticScheduleDiscardResult result = await context.Command.ExecuteAsync(
            new DiscardAutomaticScheduleRequest(
                context.Draft.Id.Value,
                context.Draft.Version.Value,
                context.Draft.Period.StartMonday),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleDiscardStatus.Succeeded, result.Status);
        Assert.Equal(context.Draft.Version.Value + 1, result.Draft?.Version.Value);
        DiscardAutomaticScheduleChange change = Assert.IsType<
            DiscardAutomaticScheduleChange>(context.Store.Change);
        Assert.Equal(context.Draft.Version.Value, change.ExpectedDraftVersion);
        Assert.Null(change.ExpectedPreparedSnapshotId);
        Assert.DoesNotContain(change.UpdatedDraft.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.AutomaticGeneration);
        Assert.Contains(change.UpdatedDraft.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.ServiceManagement);
        Assert.Contains(change.UpdatedDraft.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.ManualEdit);
        Assert.Empty(change.UpdatedDraft.GeneratedDayOffMarkers);
        Assert.Empty(change.UpdatedDraft.AssignmentLocks);
        Assert.Equal(1, context.Store.DiscardCallCount);
    }

    [Fact]
    public async Task MissingAcceptedAutomaticScheduleIsRejectedWithoutWrite()
    {
        DiscardTestContext context = DiscardTestContext.Create(hasAutomaticRun: false);

        AutomaticScheduleDiscardResult result = await context.Command.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleDiscardStatus.NoAutomaticSchedule, result.Status);
        Assert.Equal(0, context.Store.DiscardCallCount);
    }

    [Fact]
    public async Task StaleVersionIsRejectedWithoutWrite()
    {
        DiscardTestContext context = DiscardTestContext.Create();

        AutomaticScheduleDiscardResult result = await context.Command.ExecuteAsync(
            context.Request with
            {
                ExpectedDraftVersion = context.Draft.Version.Value + 1,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleDiscardStatus.Conflict, result.Status);
        Assert.Equal(0, context.Store.DiscardCallCount);
    }

    [Fact]
    public async Task StoreConflictLeavesCurrentDraftUnchanged()
    {
        DiscardTestContext context = DiscardTestContext.Create();
        context.Store.ReturnConflict = true;

        AutomaticScheduleDiscardResult result = await context.Command.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleDiscardStatus.Conflict, result.Status);
        Assert.Equal(context.Draft.Version, context.Store.CurrentDraft.Version);
        Assert.Equal(1, context.Store.DiscardCallCount);
    }

    [Fact]
    public async Task PreCancelledRequestDoesNotReadOrWrite()
    {
        DiscardTestContext context = DiscardTestContext.Create();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        AutomaticScheduleDiscardResult result = await context.Command.ExecuteAsync(
            context.Request,
            cancellation.Token);

        Assert.Equal(AutomaticScheduleDiscardStatus.Cancelled, result.Status);
        Assert.Equal(0, context.Store.LoadCallCount);
        Assert.Equal(0, context.Store.DiscardCallCount);
    }

    private sealed class DiscardTestContext
    {
        private DiscardTestContext(ScheduleDraft draft, DiscardStore store)
        {
            Draft = draft;
            Store = store;
            Command = new DiscardAutomaticScheduleCommand(store, store);
            Request = new DiscardAutomaticScheduleRequest(
                draft.Id.Value,
                draft.Version.Value,
                draft.Period.StartMonday);
        }

        public ScheduleDraft Draft { get; }

        public DiscardStore Store { get; }

        public DiscardAutomaticScheduleCommand Command { get; }

        public DiscardAutomaticScheduleRequest Request { get; }

        public static DiscardTestContext Create(bool hasAutomaticRun = true)
        {
            ScheduleDraft empty = ScheduleWorkspaceTestContext.CreateDraft();
            EmployeeId.TryCreate(
                ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
                out EmployeeId? serviceManagementId);
            EmployeeId.TryCreate(
                ScheduleWorkspaceTestContext.StandardEmployeeId,
                out EmployeeId? standardId);
            ScheduleAssignment serviceManagement = CreateAssignment(
                empty,
                serviceManagementId!,
                0,
                AssignmentOrigin.ServiceManagement,
                InitialShiftTypeCatalog.EarlyShift);
            ScheduleAssignment manual = CreateAssignment(
                empty,
                standardId!,
                1,
                AssignmentOrigin.ManualEdit,
                InitialShiftTypeCatalog.LateShift);
            ScheduleAssignment automatic = CreateAssignment(
                empty,
                standardId!,
                2,
                AssignmentOrigin.AutomaticGeneration,
                InitialShiftTypeCatalog.EarlyShift);
            ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
                assignments: [serviceManagement, manual, automatic],
                version: 4,
                generatedDayOffMarkers:
                [
                    new GeneratedDayOffMarker(
                        standardId!,
                        empty.Period.StartMonday.AddDays(3)),
                ],
                assignmentLocks: [new AssignmentLock(automatic.Id)]);
            AutomaticScheduleRunRecord? run = hasAutomaticRun ? CreateRun() : null;
            ScheduleWorkspaceReadData data =
                ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                    draft,
                    automaticScheduleRun: run);
            return new DiscardTestContext(draft, new DiscardStore(data, draft));
        }

        private static ScheduleAssignment CreateAssignment(
            ScheduleDraft draft,
            EmployeeId employeeId,
            int dayOffset,
            AssignmentOrigin origin,
            ShiftType shiftType)
        {
            DemandSlot slot = Assert.Single(draft.DemandSlots.Slots, candidate =>
                candidate.Id.Date == draft.Period.StartMonday.AddDays(dayOffset)
                && candidate.Id.ShiftTypeId == shiftType.Id
                && candidate.Id.Ordinal == 1);
            return Assert.IsType<ScheduleAssignment>(
                ScheduleAssignment.CreateNormal(
                    Guid.NewGuid(),
                    employeeId,
                    slot,
                    origin).Value);
        }

        private static AutomaticScheduleRunRecord CreateRun()
        {
            AutomaticScheduleRunMetadata metadata = new(
                "Synthetischer Testsolver",
                "1.0",
                AutomaticSchedulePlanningStatus.Optimal,
                TimeSpan.FromMinutes(2),
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.Zero,
                []);
            AutomaticScheduleObjectiveSnapshot objective = new(
                0,
                0,
                [],
                [],
                [],
                [],
                []);
            return new AutomaticScheduleRunRecord(Guid.NewGuid(), metadata, objective);
        }
    }

    private sealed class DiscardStore :
        IScheduleWorkspaceReader,
        IDiscardAutomaticScheduleStore
    {
        private readonly ScheduleWorkspaceReadData _data;

        public DiscardStore(ScheduleWorkspaceReadData data, ScheduleDraft currentDraft)
        {
            _data = data;
            CurrentDraft = currentDraft;
        }

        public int LoadCallCount { get; private set; }

        public int DiscardCallCount { get; private set; }

        public bool ReturnConflict { get; set; }

        public DiscardAutomaticScheduleChange? Change { get; private set; }

        public ScheduleDraft CurrentDraft { get; private set; }

        public Task<ScheduleWorkspaceReadData> LoadAsync(
            SchedulePeriod period,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }

        public Task<DiscardAutomaticScheduleStoreResult> DiscardAsync(
            DiscardAutomaticScheduleChange change,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DiscardCallCount++;
            Change = change;
            if (ReturnConflict)
            {
                return Task.FromResult(
                    DiscardAutomaticScheduleStoreResult.Conflict());
            }

            CurrentDraft = change.UpdatedDraft;
            return Task.FromResult(
                DiscardAutomaticScheduleStoreResult.Success(CurrentDraft));
        }
    }
}
