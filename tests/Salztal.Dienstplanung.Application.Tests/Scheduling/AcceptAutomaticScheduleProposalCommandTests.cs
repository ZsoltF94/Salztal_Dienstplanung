using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class AcceptAutomaticScheduleProposalCommandTests
{
    [Fact]
    public async Task SuccessfulAcceptancePassesOneCompleteChangeAndAdvancesOnce()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        ScheduleAssignment replacement = CreateAutomaticAssignment(
            context.Draft,
            ScheduleWorkspaceTestContext.StandardEmployeeId,
            context.Draft.Period.StartMonday.AddDays(2));
        AutomaticScheduleDayOffProposal marker = new(
            ScheduleWorkspaceTestContext.StandardEmployeeId,
            context.Draft.Period.StartMonday.AddDays(3));
        AutomaticScheduleProposal proposal = context.CreateProposal(
            [CreateSnapshot(replacement)],
            [marker]);

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                proposal),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Succeeded, result.Status);
        Assert.Equal(context.Draft.Version.Value + 1, result.Draft?.Version.Value);
        AcceptAutomaticScheduleProposalChange change =
            Assert.IsType<AcceptAutomaticScheduleProposalChange>(context.Store.Change);
        Assert.Equal(context.Draft.Id, change.UpdatedDraft.Id);
        Assert.Equal(context.Draft.Version.Value, change.ExpectedDraftVersion);
        Assert.Equal(context.Prepared.Id, change.ExpectedSnapshotId);
        Assert.Same(proposal.Metadata, change.Metadata);
        Assert.Equal(
            proposal.ObjectiveVector.UncoveredEmployeeMinutes,
            change.Run.Objective.UncoveredEmployeeMinutes);
        Assert.Equal(replacement.Id, Assert.Single(
            change.UpdatedDraft.Assignments,
            assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration).Id);
        GeneratedDayOffMarker storedMarker = Assert.Single(
            change.UpdatedDraft.GeneratedDayOffMarkers);
        Assert.Equal(marker.EmployeeId, storedMarker.EmployeeId.Value);
        Assert.Equal(marker.Date, storedMarker.Date);
        Assert.Equal(1, context.Store.CallCount);
    }

    [Fact]
    public async Task SecondAcceptanceOfSameProposalIsRejectedWithoutSecondWrite()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        AutomaticScheduleProposal proposal = context.CreateProposal([], []);
        AcceptAutomaticScheduleProposalRequest request = new(
            context.Draft.Period.StartMonday,
            proposal);

        AutomaticScheduleAcceptanceResult first = await context.Command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);
        AutomaticScheduleAcceptanceResult second = await context.Command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Succeeded, first.Status);
        Assert.Equal(AutomaticScheduleAcceptanceStatus.Conflict, second.Status);
        Assert.Equal(1, context.Store.CallCount);
        Assert.Equal(context.Draft.Version.Value + 1, context.Store.CurrentDraft.Version.Value);
    }

    [Fact]
    public async Task DifferentDraftIdentifierIsRejectedBeforeWrite()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        AutomaticScheduleProposal proposal = context.CreateProposal(
            [],
            [],
            draftId: Guid.NewGuid());

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                proposal),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.DraftNotFound, result.Status);
        Assert.Equal(0, context.Store.CallCount);
    }

    [Fact]
    public async Task DifferentDraftVersionIsRejectedBeforeWrite()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        AutomaticScheduleProposal proposal = context.CreateProposal(
            [],
            [],
            expectedDraftVersion: context.Draft.Version.Value + 1);

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                proposal),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Conflict, result.Status);
        Assert.Equal(0, context.Store.CallCount);
    }

    [Fact]
    public async Task DifferentSnapshotIdentifierIsRejectedBeforeWrite()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        AutomaticScheduleProposal proposal = context.CreateProposal(
            [],
            [],
            snapshotId: Guid.NewGuid());

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                proposal),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Conflict, result.Status);
        Assert.Equal(0, context.Store.CallCount);
    }

    [Fact]
    public async Task StoreConflictReturnsNoAcceptedDraftAndDoesNotMerge()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        context.Store.ReturnConflict = true;
        AutomaticScheduleProposal proposal = context.CreateProposal([], []);

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                proposal),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Conflict, result.Status);
        Assert.Null(result.Draft);
        Assert.Equal(context.Draft.Version, context.Store.CurrentDraft.Version);
        Assert.Equal(1, context.Store.CallCount);
    }

    [Fact]
    public async Task AlteredAssignmentSnapshotIsRejectedBeforeWrite()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        ScheduleAssignment assignment = CreateAutomaticAssignment(
            context.Draft,
            ScheduleWorkspaceTestContext.StandardEmployeeId,
            context.Draft.Period.StartMonday.AddDays(2));
        ScheduleAssignmentSnapshot valid = CreateSnapshot(assignment);
        ScheduleAssignmentSnapshot altered = new(
            valid.AssignmentId,
            valid.EmployeeId,
            valid.Date,
            valid.Kind,
            valid.Origin,
            valid.PatternId,
            valid.WorkMinutes + 1,
            valid.IsProtectedFromAutomaticGeneration,
            valid.Segments,
            valid.Coverages);

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                context.CreateProposal([altered], [])),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.ProposalInvalid, result.Status);
        Assert.Equal(0, context.Store.CallCount);
    }

    [Theory]
    [InlineData(ScheduleAssignmentKind.SplitShiftPattern)]
    [InlineData(ScheduleAssignmentKind.ReliefShiftPattern)]
    public async Task CompositeAutomaticAssignmentIsRebuiltWithoutTechnicalLookup(
        ScheduleAssignmentKind kind)
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        ScheduleAssignment assignment = CreateCompositeAutomaticAssignment(
            context.Draft,
            kind);

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                context.CreateProposal([CreateSnapshot(assignment)], [])),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Succeeded, result.Status);
        ScheduleAssignment accepted = Assert.Single(
            Assert.IsType<ScheduleDraft>(result.Draft).Assignments,
            candidate => candidate.Origin == AssignmentOrigin.AutomaticGeneration);
        Assert.Equal(kind, accepted.Kind);
        Assert.Equal(assignment.Segments, accepted.Segments);
        Assert.Equal(assignment.Coverages, accepted.Coverages);
    }

    [Fact]
    public async Task MarkerConflictingWithProtectedAssignmentRejectsWholeChange()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        ScheduleAssignment protectedAssignment = context.Draft.Assignments[0];
        AutomaticScheduleDayOffProposal marker = new(
            protectedAssignment.EmployeeId.Value,
            protectedAssignment.Date);

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                context.CreateProposal([], [marker])),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleAcceptanceStatus.ProposalInvalid, result.Status);
        Assert.Contains(
            result.ValidationErrors,
            error => error.Code
                == ScheduleDraftValidationCode.GeneratedDayOffConflictsAssignment);
        Assert.Equal(0, context.Store.CallCount);
        Assert.Equal(context.Draft.Version, context.Store.CurrentDraft.Version);
    }

    [Fact]
    public async Task CancellationBeforeStartDoesNotReadOrWrite()
    {
        AcceptanceTestContext context = await AcceptanceTestContext.CreateAsync();
        int readsBefore = context.Store.ReadCallCount;

        AutomaticScheduleAcceptanceResult result = await context.Command.ExecuteAsync(
            new AcceptAutomaticScheduleProposalRequest(
                context.Draft.Period.StartMonday,
                context.CreateProposal([], [])),
            new CancellationToken(true));

        Assert.Equal(AutomaticScheduleAcceptanceStatus.Cancelled, result.Status);
        Assert.Equal(readsBefore, context.Store.ReadCallCount);
        Assert.Equal(0, context.Store.CallCount);
    }

    private static ScheduleAssignment CreateAutomaticAssignment(
        ScheduleDraft draft,
        Guid employeeId,
        DateOnly date)
    {
        DemandSlot slot = Assert.Single(
            draft.DemandSlots.Slots,
            candidate => candidate.Id.Date == date
                && candidate.Id.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id
                && candidate.Id.Ordinal == 1);
        Salztal.Dienstplanung.Domain.Employees.EmployeeId.TryCreate(
            employeeId,
            out Salztal.Dienstplanung.Domain.Employees.EmployeeId? validatedEmployeeId);
        return Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                validatedEmployeeId,
                slot,
                AssignmentOrigin.AutomaticGeneration).Value);
    }

    private static ScheduleAssignment CreateCompositeAutomaticAssignment(
        ScheduleDraft draft,
        ScheduleAssignmentKind kind)
    {
        DateOnly date = kind == ScheduleAssignmentKind.ReliefShiftPattern
            ? draft.Period.StartMonday.AddDays(5)
            : draft.Period.StartMonday.AddDays(1);
        ShiftType firstType = kind == ScheduleAssignmentKind.ReliefShiftPattern
            ? InitialShiftTypeCatalog.CafeteriaShiftB
            : InitialShiftTypeCatalog.EarlyShift;
        DemandSlot first = Assert.Single(
            draft.DemandSlots.Slots,
            candidate => candidate.Id.Date == date
                && candidate.Id.ShiftTypeId == firstType.Id
                && candidate.Id.Ordinal == 1);
        DemandSlot second = Assert.Single(
            draft.DemandSlots.Slots,
            candidate => candidate.Id.Date == date
                && candidate.Id.ShiftTypeId == InitialShiftTypeCatalog.LateShift.Id
                && candidate.Id.Ordinal == 1);
        Salztal.Dienstplanung.Domain.Employees.EmployeeId.TryCreate(
            ScheduleWorkspaceTestContext.StandardEmployeeId,
            out Salztal.Dienstplanung.Domain.Employees.EmployeeId? employeeId);
        ScheduleAssignmentValidationResult result = kind switch
        {
            ScheduleAssignmentKind.SplitShiftPattern =>
                ScheduleAssignment.CreateSplitShift(
                    Guid.NewGuid(),
                    employeeId,
                    InitialShiftPatternCatalog.SplitShift,
                    first,
                    second,
                    AssignmentOrigin.AutomaticGeneration),
            ScheduleAssignmentKind.ReliefShiftPattern =>
                ScheduleAssignment.CreateReliefShift(
                    Guid.NewGuid(),
                    employeeId,
                    InitialShiftPatternCatalog.ReliefShift,
                    first,
                    second,
                    AssignmentOrigin.AutomaticGeneration),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        return Assert.IsType<ScheduleAssignment>(result.Value);
    }

    private static ScheduleAssignmentSnapshot CreateSnapshot(
        ScheduleAssignment assignment)
    {
        return new ScheduleAssignmentSnapshot(
            assignment.Id.Value,
            assignment.EmployeeId.Value,
            assignment.Date,
            assignment.Kind switch
            {
                ScheduleAssignmentKind.NormalDemand =>
                    ScheduleAssignmentKindSnapshot.NormalDemand,
                ScheduleAssignmentKind.SplitShiftPattern =>
                    ScheduleAssignmentKindSnapshot.SplitShiftPattern,
                ScheduleAssignmentKind.ReliefShiftPattern =>
                    ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
                _ => throw new ArgumentOutOfRangeException(nameof(assignment)),
            },
            assignment.Origin switch
            {
                AssignmentOrigin.AutomaticGeneration =>
                    ScheduleAssignmentOriginSnapshot.AutomaticGeneration,
                _ => throw new ArgumentOutOfRangeException(nameof(assignment)),
            },
            assignment.PatternId?.Value,
            assignment.WorkMinutes,
            false,
            assignment.Segments.Select(segment =>
                new ScheduleAssignmentSegmentSnapshot(
                    segment.AnchorSlotId.SourceId.Value,
                    segment.Date,
                    segment.WorkLocationId.Value,
                    segment.ShiftTypeId.Value,
                    segment.ActualTime.Start,
                    segment.ActualTime.End,
                    segment.WorkMinutes)),
            assignment.Coverages.Select(coverage =>
                new ScheduleDemandCoverageSnapshot(
                    coverage.SlotId.SourceId.Value,
                    coverage.SlotId.Date,
                    coverage.SlotId.WorkLocationId.Value,
                    coverage.SlotId.ShiftTypeId.Value,
                    coverage.SlotId.Ordinal,
                    coverage.CoveredTime.Start,
                    coverage.CoveredTime.End,
                    coverage.CoveredMinutes,
                    coverage.Kind switch
                    {
                        DemandCoverageKind.Full =>
                            ScheduleDemandCoverageKindSnapshot.Full,
                        DemandCoverageKind.PartialReliefShift =>
                            ScheduleDemandCoverageKindSnapshot.PartialReliefShift,
                        _ => throw new ArgumentOutOfRangeException(nameof(assignment)),
                    })));
    }

    private sealed class AcceptanceTestContext(
        ScheduleDraft draft,
        AcceptanceStoreDouble store)
    {
        public ScheduleDraft Draft { get; } = draft;

        public AcceptanceStoreDouble Store { get; } = store;

        public PlanningInputSnapshot Prepared => Assert.IsType<PlanningInputSnapshot>(
            Store.PreparedSnapshot);

        public AcceptAutomaticScheduleProposalCommand Command => new(Store, Store);

        public static async Task<AcceptanceTestContext> CreateAsync()
        {
            ScheduleDraft draft = CreateReadyDraft();
            AcceptanceStoreDouble store = new(draft);
            PreparePlanningInputResult preparation = await new PreparePlanningInputCommand(
                store,
                store).ExecuteAsync(
                    new PreparePlanningInputRequest(
                        draft.Id.Value,
                        draft.Version.Value,
                        draft.Period.StartMonday,
                        PlanningPreparationExpectation.NotPrepared,
                        null,
                        PlanningRunOptions.Default),
                    TestContext.Current.CancellationToken);
            Assert.Equal(PreparePlanningInputStatus.Succeeded, preparation.Status);
            return new AcceptanceTestContext(draft, store);
        }

        public AutomaticScheduleProposal CreateProposal(
            IEnumerable<ScheduleAssignmentSnapshot> assignments,
            IEnumerable<AutomaticScheduleDayOffProposal> markers,
            Guid? draftId = null,
            long? expectedDraftVersion = null,
            Guid? snapshotId = null)
        {
            RuleCatalog catalog = Assert.IsType<RuleCatalog>(
                InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
            ScheduleRuleEvaluationSet evaluations = new(
                catalog,
                catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                    definition.Id,
                    RuleEvaluationStatus.Satisfied,
                    NoRuleResultParameters.Instance)));
            ScheduleObjectiveVector objective = new(
                0,
                0,
                RuleViolationSet.Empty,
                RuleViolationSet.Empty,
                RuleViolationSet.Empty,
                RuleViolationSet.Empty,
                []);
            AutomaticScheduleRunMetadata metadata = new(
                "Synthetic solver",
                "1.0",
                AutomaticSchedulePlanningStatus.Optimal,
                AutomaticSchedulePlanningRequest.ProductiveTimeLimit,
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.Zero,
                [new AutomaticScheduleSetting("workers", "1")]);
            return new AutomaticScheduleProposal(
                snapshotId ?? Prepared.Id,
                draftId ?? Draft.Id.Value,
                expectedDraftVersion ?? Draft.Version.Value,
                assignments,
                markers,
                [],
                objective,
                evaluations,
                metadata);
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
                        && item.Id.ShiftTypeId
                            == InitialShiftTypeCatalog.EarlyShift.Id
                        && item.Id.Ordinal == 1);
                assignments.Add(Assert.IsType<ScheduleAssignment>(
                    ScheduleAssignment.CreateNormal(
                        Guid.Parse($"62000000-0000-4000-8000-{week + 1:D12}"),
                        ScheduleWorkspaceTestContext.ServiceManagementEmployee.Id,
                        slot,
                        AssignmentOrigin.ServiceManagement).Value));
            }

            return ScheduleWorkspaceTestContext.CreateDraft(assignments: assignments);
        }
    }

    private sealed class AcceptanceStoreDouble(ScheduleDraft draft)
        : IScheduleWorkspaceReader,
          IPlanningInputReader,
          IPreparePlanningSnapshotStore,
          IAcceptAutomaticScheduleProposalStore
    {
        public ScheduleDraft CurrentDraft { get; private set; } = draft;

        public PlanningInputSnapshot? PreparedSnapshot { get; private set; }

        public AcceptAutomaticScheduleProposalChange? Change { get; private set; }

        public bool ReturnConflict { get; set; }

        public int ReadCallCount { get; private set; }

        public int CallCount { get; private set; }

        public Task<ScheduleWorkspaceReadData> LoadAsync(
            SchedulePeriod period,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCallCount++;
            return Task.FromResult(CreateWorkspace());
        }

        async Task<PlanningInputReadData> IPlanningInputReader.LoadAsync(
            SchedulePeriod period,
            CancellationToken cancellationToken)
        {
            ScheduleWorkspaceReadData workspace = await LoadAsync(
                period,
                cancellationToken);
            return new PlanningInputReadData(workspace, CreateHistory(), PreparedSnapshot);
        }

        public Task<PreparePlanningSnapshotStoreResult> SaveAsync(
            PreparePlanningSnapshotChange change,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PreparedSnapshot = change.Snapshot;
            return Task.FromResult(
                PreparePlanningSnapshotStoreResult.Success(change.Snapshot));
        }

        public Task<AcceptAutomaticScheduleProposalStoreResult> AcceptAsync(
            AcceptAutomaticScheduleProposalChange change,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Change = change;
            if (ReturnConflict
                || CurrentDraft.Id != change.UpdatedDraft.Id
                || CurrentDraft.Version.Value != change.ExpectedDraftVersion
                || PreparedSnapshot?.Id != change.ExpectedSnapshotId)
            {
                return Task.FromResult(
                    AcceptAutomaticScheduleProposalStoreResult.Conflict());
            }

            CurrentDraft = change.UpdatedDraft;
            return Task.FromResult(
                AcceptAutomaticScheduleProposalStoreResult.Success(CurrentDraft));
        }

        private ScheduleWorkspaceReadData CreateWorkspace() =>
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                CurrentDraft,
                historyDays: CreateHistory(),
                preparedSnapshot: PreparedSnapshot);

        private static PlanningHistoryDayReadItem[] CreateHistory()
        {
            DateOnly first = ScheduleWorkspaceTestContext.PeriodMonday.AddDays(-7);
            return Enumerable.Range(0, 7)
                .Select(index => new PlanningHistoryDayReadItem(
                    first.AddDays(index),
                    [new PlanningHistoryAssignmentReadItem(
                        ScheduleWorkspaceTestContext.StandardEmployeeId,
                        300)]))
                .ToArray();
        }
    }
}
