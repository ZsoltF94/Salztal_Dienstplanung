using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;
using Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

namespace Salztal.Dienstplanung.Infrastructure.Tests.Scheduling;

public sealed class SqliteScheduleStoreTests
{
    private const string AvailabilityMigration =
        "20260915183307_AddAvailabilityEntries";
    private const string AutomaticScheduleRunsMigration =
        "20260917125952_AddAutomaticScheduleRuns";

    private static readonly DateOnly PeriodMonday = new(2026, 9, 14);

    private static readonly Guid ServiceManagementEmployeeId =
        new("aae194ab-5147-4760-898f-f3d407c29ae9");

    private static readonly Guid StandardEmployeeId =
        new("706b1355-fadf-4646-9a02-b380506e9064");

    [Fact]
    public async Task InitializeUpgradesSystem06DatabaseAndKeepsExistingData()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateAsync(database.Path, AvailabilityMigration);
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO Employees (Id, FirstName, LastName, EmployeeTypeId, IsActive) "
            + $"VALUES ('{DatabaseId(ServiceManagementEmployeeId)}', 'Sarah', 'Leitung', "
            + $"'{DatabaseId(InitialEmployeeTypeCatalog.Type1.Id.Value)}', 1); "
            + "INSERT INTO AvailabilityEntries "
            + "(EmployeeId, Date, Kind, ChangeVersion) "
            + $"VALUES ('{DatabaseId(ServiceManagementEmployeeId)}', "
            + "'2026-09-15', 0, 1);");
        SqliteScheduleStore store = new(database.Path);

        await store.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(10L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM Employees;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM AvailabilityEntries;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM sqlite_schema "
            + "WHERE type = 'table' AND name = 'SchedulePeriodDays';"));
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(database.Path).Create();
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(
            TestContext.Current.CancellationToken));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task InitializeUpgradesSystem08DatabaseAndKeepsDraft()
    {
        using TemporarySqliteDatabase database = new();
        const string SchedulingMigration = "20260916163137_AddScheduling";
        Guid draftId = new("a77b5cab-e1a1-4cd3-8aa8-dd16159498a2");
        await MigrateAsync(database.Path, SchedulingMigration);
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO ScheduleDrafts "
            + "(Id, Version, StartMonday, EndSunday, PreparedSnapshotId) "
            + $"VALUES ('{DatabaseId(draftId)}', 3, '2026-09-14', "
            + "'2026-10-04', NULL);");

        await new SqliteScheduleStore(database.Path).InitializeAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(10L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM ScheduleDrafts WHERE Version = 3;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM sqlite_schema "
            + "WHERE type = 'table' AND name = 'AutomaticScheduleRuns';"));
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(database.Path).Create();
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(
            TestContext.Current.CancellationToken));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task UpgradeFromAutomaticScheduleRunsPreservesDurationsAndInitializesEmptyPhases()
    {
        using TemporarySqliteDatabase database = new();
        Guid draftId = new("a77b5cab-e1a1-4cd3-8aa8-dd16159498a3");
        await MigrateAsync(database.Path, AutomaticScheduleRunsMigration);
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO ScheduleDrafts "
            + "(Id, Version, StartMonday, EndSunday, PreparedSnapshotId) "
            + $"VALUES ('{DatabaseId(draftId)}', 3, '2026-09-14', "
            + "'2026-10-04', NULL); "
            + "INSERT INTO AutomaticScheduleRuns "
            + "(DraftId, SnapshotId, SolverName, SolverVersion, ResultStatus, "
            + "TimeLimitTicks, ModelBuildDurationTicks, NonAuxiliarySolveDurationTicks, "
            + "AuxiliarySolveDurationTicks, ResultMappingDurationTicks, TotalDurationTicks, "
            + "SettingsPayload, ObjectivePayload) "
            + $"VALUES ('{DatabaseId(draftId)}', "
            + "'11111111-1111-1111-1111-111111111111', 'Synthetic', '1.0', 0, "
            + "1200000000, 100, 200, 300, 400, 1000, '[]', "
            + "'{\"UncoveredEmployeeMinutes\":0,\"FullyUncoveredDemandSlotCount\":0,"
            + "\"HighPriorityViolations\":[],\"MediumPriorityViolations\":[],"
            + "\"LowPriorityViolations\":[],\"StabilityViolations\":[],"
            + "\"TechnicalTieBreakerKeys\":[]}');");

        await new SqliteScheduleStore(database.Path).InitializeAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(200L, await ExecuteScalarAsync(
            database.Path,
            "SELECT OptimizationDurationTicks FROM AutomaticScheduleRuns;"));
        Assert.Equal(300L, await ExecuteScalarAsync(
            database.Path,
            "SELECT LegacyPhaseDurationTicks FROM AutomaticScheduleRuns;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM AutomaticScheduleRuns WHERE PhasesPayload = '[]';"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM ScheduleDrafts WHERE Version = 3;"));
    }

    [Fact]
    public async Task CreateWhenStoreRestartsRestoresTwentyOneDaysAndDemandSlots()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(database.Path);
        ScheduleDraft created = await OpenDraftAsync(store, PeriodMonday);

        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                created.Period,
                TestContext.Current.CancellationToken);

        ScheduleDraft draft = Assert.IsType<ScheduleDraft>(reloaded.ExactDraft);
        Assert.Equal(created.Id, draft.Id);
        Assert.Equal(1, draft.Version.Value);
        Assert.Equal(195, draft.DemandSlots.Slots.Count);
        Assert.Equal(21L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM SchedulePeriodDays;"));
    }

    [Fact]
    public async Task StaleCreateCannotInsertOverlappingPeriod()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(database.Path);
        SchedulePeriod overlapPeriod = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(PeriodMonday.AddDays(14)).Value);
        ScheduleWorkspaceReadData staleRead = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(overlapPeriod, TestContext.Current.CancellationToken);
        ScheduleDraft staleDraft = CreateDraft(overlapPeriod, staleRead);
        await OpenDraftAsync(store, PeriodMonday);

        OpenScheduleDraftStoreResult result = await store.CreateAsync(
            staleDraft,
            TestContext.Current.CancellationToken);

        Assert.Equal(OpenScheduleDraftStoreStatus.Overlap, result.Status);
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM ScheduleDrafts;"));
        Assert.Equal(21L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM SchedulePeriodDays;"));
    }

    [Fact]
    public async Task RestartPreservesTyp1KindsSegmentsAndPreparedSnapshot()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await OpenDraftAsync(store, PeriodMonday);
        draft = await SetAssignmentAsync(
            store,
            draft,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);
        draft = await SetAssignmentAsync(
            store,
            draft,
            ServiceManagementAssignmentSelectionKind.OfficeTime,
            PeriodMonday.AddDays(1),
            InitialShiftTypeCatalog.LateShift);
        draft = await SetAssignmentAsync(
            store,
            draft,
            ServiceManagementAssignmentSelectionKind.SplitShiftPattern,
            PeriodMonday.AddDays(2),
            InitialShiftTypeCatalog.EarlyShift,
            InitialShiftTypeCatalog.LateShift);
        draft = await SetAssignmentAsync(
            store,
            draft,
            ServiceManagementAssignmentSelectionKind.ReliefShiftPattern,
            PeriodMonday.AddDays(5),
            InitialShiftTypeCatalog.CafeteriaShiftB,
            InitialShiftTypeCatalog.LateShift);
        draft = await SetAssignmentAsync(
            store,
            draft,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            PeriodMonday.AddDays(7),
            InitialShiftTypeCatalog.EarlyShift);
        draft = await SetAssignmentAsync(
            store,
            draft,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            PeriodMonday.AddDays(14),
            InitialShiftTypeCatalog.EarlyShift);
        PlanningInputSnapshot prepared = await PrepareAsync(
            store,
            draft,
            null,
            new PlanningRunOptions(true));

        SqliteScheduleStore restarted = new(database.Path);
        PlanningInputReadData reloaded = await ((IPlanningInputReader)restarted).LoadAsync(
            draft.Period,
            TestContext.Current.CancellationToken);

        ScheduleDraft storedDraft = Assert.IsType<ScheduleDraft>(
            reloaded.Workspace.ExactDraft);
        PlanningInputSnapshot storedSnapshot = Assert.IsType<PlanningInputSnapshot>(
            reloaded.PreparedSnapshot);
        Assert.Equal(6, storedDraft.Assignments.Count);
        Assert.Contains(storedDraft.Assignments, item =>
            item.Kind == ScheduleAssignmentKind.OfficeTime
            && item.Coverages.Count == 0);
        Assert.Contains(storedDraft.Assignments, item =>
            item.Kind == ScheduleAssignmentKind.SplitShiftPattern
            && item.Segments.Count == 2);
        Assert.Contains(storedDraft.Assignments, item =>
            item.Kind == ScheduleAssignmentKind.ReliefShiftPattern
            && item.Coverages.Any(coverage =>
                coverage.Kind == DemandCoverageKind.PartialReliefShift));
        Assert.Equal(prepared.Id, storedSnapshot.Id);
        Assert.True(storedSnapshot.RunOptions.EnableAuxiliaryReliefShift);
        Assert.Equal(30, storedSnapshot.RuleCatalog.Definitions.Count);
        Assert.Equal(195, storedSnapshot.DemandSlots.Count);
        Assert.Equal(6, storedSnapshot.ServiceManagementAssignments.Count);
        Assert.Equal(PlanningHistoryCompleteness.Missing, storedSnapshot.History.Completeness);
        Assert.True(PlanningInputComparison.Compare(prepared, storedSnapshot).IsCurrent);
    }

    [Fact]
    public async Task RestartReadsVersionOneSnapshotAndMarksItDifferentFromCurrentCatalog()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot current = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        RuleCatalogSnapshot versionOne = await GetRuleCatalogQuery.ExecuteAsync(
            1,
            TestContext.Current.CancellationToken);
        PlanningInputSnapshot historical = CopyPlanningSnapshot(
            current,
            PlanningRunOptions.Default,
            versionOne);

        PreparePlanningSnapshotStoreResult saveResult = await store.SaveAsync(
            new PreparePlanningSnapshotChange(
                historical,
                draft.Version.Value,
                current.Id),
            TestContext.Current.CancellationToken);
        PlanningInputReadData reloaded = await ((IPlanningInputReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);

        Assert.Equal(PreparePlanningSnapshotStoreStatus.Succeeded, saveResult.Status);
        PlanningInputSnapshot restored = Assert.IsType<PlanningInputSnapshot>(
            reloaded.PreparedSnapshot);
        Assert.Equal(1, restored.RuleCatalog.Version);
        Assert.Equal(28, restored.RuleCatalog.Definitions.Count);
        Assert.Equal(
            [PlanningInputChangeCategory.RuleCatalog],
            PlanningInputComparison.Compare(restored, current).ChangedCategories);
    }

    [Fact]
    public async Task RefreshAtomicallyReplacesPreviousSnapshot()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot first = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);

        PlanningInputSnapshot second = await PrepareAsync(
            store,
            draft,
            first,
            new PlanningRunOptions(true));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM PlanningSnapshots;"));
        Assert.Equal(0L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM PlanningSnapshots "
            + $"WHERE Id = '{DatabaseId(first.Id)}';"));
        PlanningInputReadData reloaded = await ((IPlanningInputReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);
        Assert.Equal(second.Id, reloaded.PreparedSnapshot?.Id);
        Assert.True(reloaded.PreparedSnapshot?.RunOptions.EnableAuxiliaryReliefShift);
    }

    [Fact]
    public async Task FailedRefreshKeepsPreviousSnapshotAndDraftReference()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot first = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER FailPlanningSnapshot BEFORE INSERT ON PlanningSnapshots "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic failure'); END;");
        PlanningInputSnapshot replacement = CopyPlanningSnapshot(
            first,
            new PlanningRunOptions(true));
        PreparePlanningSnapshotChange change = new(
            replacement,
            draft.Version.Value,
            first.Id);

        PreparePlanningSnapshotStoreResult result = await store.SaveAsync(
            change,
            TestContext.Current.CancellationToken);

        Assert.Equal(PreparePlanningSnapshotStoreStatus.Conflict, result.Status);
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM PlanningSnapshots;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM PlanningSnapshots "
            + $"WHERE Id = '{DatabaseId(first.Id)}';"));
        PlanningInputReadData reloaded = await ((IPlanningInputReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);
        Assert.Equal(first.Id, reloaded.PreparedSnapshot?.Id);
        Assert.False(reloaded.PreparedSnapshot?.RunOptions.EnableAuxiliaryReliefShift);
    }

    [Fact]
    public async Task AcceptanceRoundTripsCompleteGenerationAndRunAfterRestart()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange change = CreateAutomaticChange(
            draft,
            snapshot.Id,
            dayOffset: 2,
            seed: 2);

        AcceptAutomaticScheduleProposalStoreResult result = await store.AcceptAsync(
            change,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Succeeded, result.Status);
        PlanningInputReadData reloaded = await ((IPlanningInputReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);
        ScheduleDraft stored = Assert.IsType<ScheduleDraft>(
            reloaded.Workspace.ExactDraft);
        Assert.Equal(draft.Version.Value + 1, stored.Version.Value);
        Assert.Equal(3, stored.Assignments.Count(assignment =>
            assignment.Origin == AssignmentOrigin.ServiceManagement));
        Assert.Equal(
            Assert.Single(change.UpdatedDraft.Assignments, assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration).Id,
            Assert.Single(stored.Assignments, assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration).Id);
        Assert.Equal(
            Assert.Single(change.UpdatedDraft.GeneratedDayOffMarkers),
            Assert.Single(stored.GeneratedDayOffMarkers));
        Assert.Equal(snapshot.Id, reloaded.PreparedSnapshot?.Id);
        AssertRunEqual(
            change.Run,
            Assert.IsType<AutomaticScheduleRunRecord>(
                reloaded.Workspace.AutomaticScheduleRun));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM AutomaticScheduleRuns;"));
        Assert.Equal(195L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM ScheduleDemandSlots;"));
    }

    [Fact]
    public async Task LegacyPhasePayloadWithoutTerminationRemainsReadable()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange change = CreateAutomaticChange(
            draft,
            snapshot.Id,
            dayOffset: 2,
            seed: 2);
        await store.AcceptAsync(change, TestContext.Current.CancellationToken);
        const string legacyPayload =
            "[{\"Kind\":0,\"Status\":0,\"DurationTicks\":100000,\"Values\":[]},"
            + "{\"Kind\":3,\"Status\":2,\"DurationTicks\":1200000000,"
            + "\"Values\":[{\"Key\":\"required_minutes\",\"Value\":240},"
            + "{\"Key\":\"uncovered_minutes\",\"Value\":120}]}]";
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE AutomaticScheduleRuns SET PhasesPayload = '"
            + legacyPayload
            + "';");

        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);

        AutomaticScheduleRunRecord run = Assert.IsType<AutomaticScheduleRunRecord>(
            reloaded.AutomaticScheduleRun);
        Assert.Equal(
            AutomaticSchedulePhaseTraceCompleteness.Complete,
            run.Metadata.PhaseTraceCompleteness);
        AutomaticSchedulePhaseSnapshot interrupted = Assert.Single(
            run.Metadata.Phases,
            phase => phase.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Equal(AutomaticSchedulePhaseStatus.Interrupted, interrupted.Status);
        Assert.Null(interrupted.Termination);
        Assert.Equal(
            AutomaticSchedulePhaseDetailAvailability.TerminationDetailsNotRecorded,
            interrupted.DetailAvailability);
    }

    [Fact]
    public async Task GenerationAcceptanceAndWorkspaceReloadKeepTerminationDetails()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AutomaticScheduleProposal proposal = CreateInterruptedProposal(snapshot);
        GenerateAutomaticScheduleCommand generationCommand = new(
            store,
            new FixedPlanner(AutomaticSchedulePlanningResult.Success(
                AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
                proposal)));

        AutomaticScheduleGenerationResult generation = await generationCommand.ExecuteAsync(
            new GenerateAutomaticScheduleRequest(
                draft.Id.Value,
                draft.Version.Value,
                draft.Period.StartMonday,
                snapshot.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal,
            generation.Status);
        AutomaticSchedulePreview preview = Assert.IsType<AutomaticSchedulePreview>(
            generationCommand.CurrentPreview);
        AssertPhaseTerminationEqual(
            proposal.Metadata.Phases,
            preview.Proposal.Metadata.Phases);
        AutomaticScheduleAcceptanceResult acceptance = await
            new AcceptAutomaticScheduleProposalCommand(store, store).ExecuteAsync(
                new AcceptAutomaticScheduleProposalRequest(
                    draft.Period.StartMonday,
                    preview.Proposal),
                TestContext.Current.CancellationToken);
        Assert.Equal(AutomaticScheduleAcceptanceStatus.Succeeded, acceptance.Status);

        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);

        AutomaticScheduleRunRecord run = Assert.IsType<AutomaticScheduleRunRecord>(
            reloaded.AutomaticScheduleRun);
        AssertPhaseTerminationEqual(
            preview.Proposal.Metadata.Phases,
            run.Metadata.Phases);
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM AutomaticScheduleRuns;"));
    }

    [Fact]
    public async Task AcceptancePreservesLockedAutomaticAssignmentAndItsChildren()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        EmployeeId.TryCreate(StandardEmployeeId, out EmployeeId? employeeId);
        DemandSlot lockedSlot = FindSlot(
            draft,
            draft.Period.StartMonday.AddDays(1),
            InitialShiftTypeCatalog.EarlyShift);
        ScheduleAssignment lockedAssignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.Parse("8914cdf0-6440-4f50-b82f-c9356fb446d0"),
                employeeId,
                lockedSlot,
                AssignmentOrigin.AutomaticGeneration).Value);
        ScheduleDraft withLock = Assert.IsType<ScheduleDraft>(ScheduleDraft.Create(
            draft.Id.Value,
            draft.Version.Value + 1,
            draft.Period,
            draft.DemandSlots,
            draft.AvailabilityEntries,
            draft.Assignments.Append(lockedAssignment),
            draft.GeneratedDayOffMarkers,
            [new AssignmentLock(lockedAssignment.Id)]).Value);
        ScheduleDayChangeStoreResult seedResult = await store.ChangeAsync(
            new ScheduleDayChange(
                withLock,
                draft.Version.Value,
                ScheduleAvailabilityMutation.None(),
                SchedulePreparationImpact.PotentiallyOutdated),
            TestContext.Current.CancellationToken);
        Assert.Equal(ScheduleDayChangeStoreStatus.Succeeded, seedResult.Status);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            withLock,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange change = CreateAutomaticChange(
            withLock,
            snapshot.Id,
            dayOffset: 3,
            seed: 2);

        AcceptAutomaticScheduleProposalStoreResult result = await store.AcceptAsync(
            change,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Succeeded, result.Status);
        ScheduleDraft stored = Assert.IsType<ScheduleDraft>(
            (await ((IScheduleWorkspaceReader)new SqliteScheduleStore(database.Path))
                .LoadAsync(draft.Period, TestContext.Current.CancellationToken)).ExactDraft);
        Assert.Contains(stored.Assignments, assignment =>
            assignment.Id == lockedAssignment.Id
            && assignment.Segments.Count == lockedAssignment.Segments.Count
            && assignment.Coverages.Count == lockedAssignment.Coverages.Count);
        Assert.Contains(stored.AssignmentLocks, assignmentLock =>
            assignmentLock.AssignmentId == lockedAssignment.Id);
        Assert.Equal(2, stored.Assignments.Count(assignment =>
            assignment.Origin == AssignmentOrigin.AutomaticGeneration));
    }

    [Fact]
    public async Task LaterAcceptanceReplacesGenerationAndCurrentRunWithoutHistory()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot firstSnapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange first = CreateAutomaticChange(
            draft,
            firstSnapshot.Id,
            dayOffset: 2,
            seed: 1);
        await store.AcceptAsync(first, TestContext.Current.CancellationToken);
        ScheduleDraft afterFirst = Assert.IsType<ScheduleDraft>(
            (await ((IScheduleWorkspaceReader)store).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken)).ExactDraft);
        PlanningInputSnapshot secondSnapshot = await PrepareAsync(
            store,
            afterFirst,
            firstSnapshot,
            new PlanningRunOptions(true));
        AcceptAutomaticScheduleProposalChange second = CreateAutomaticChange(
            afterFirst,
            secondSnapshot.Id,
            dayOffset: 4,
            seed: 2);

        AcceptAutomaticScheduleProposalStoreResult result = await store.AcceptAsync(
            second,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Succeeded, result.Status);
        PlanningInputReadData reloaded = await ((IPlanningInputReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);
        ScheduleDraft stored = Assert.IsType<ScheduleDraft>(
            reloaded.Workspace.ExactDraft);
        Assert.DoesNotContain(stored.Assignments, assignment =>
            assignment.Id == Assert.Single(first.UpdatedDraft.Assignments, item =>
                item.Origin == AssignmentOrigin.AutomaticGeneration).Id);
        Assert.Contains(stored.Assignments, assignment =>
            assignment.Id == Assert.Single(second.UpdatedDraft.Assignments, item =>
                item.Origin == AssignmentOrigin.AutomaticGeneration).Id);
        Assert.Equal(
            Assert.Single(second.UpdatedDraft.GeneratedDayOffMarkers),
            Assert.Single(stored.GeneratedDayOffMarkers));
        AssertRunEqual(
            second.Run,
            Assert.IsType<AutomaticScheduleRunRecord>(
                reloaded.Workspace.AutomaticScheduleRun));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM AutomaticScheduleRuns;"));
    }

    [Fact]
    public async Task VersionConflictKeepsDraftAndCreatesNoRun()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange valid = CreateAutomaticChange(
            draft,
            snapshot.Id,
            dayOffset: 2,
            seed: 1);
        AcceptAutomaticScheduleProposalChange stale = valid with
        {
            ExpectedDraftVersion = draft.Version.Value + 1,
        };

        AcceptAutomaticScheduleProposalStoreResult result = await store.AcceptAsync(
            stale,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Conflict, result.Status);
        await AssertUnchangedWithoutRunAsync(store, draft);
    }

    [Fact]
    public async Task SnapshotConflictKeepsDraftAndCreatesNoRun()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange valid = CreateAutomaticChange(
            draft,
            snapshot.Id,
            dayOffset: 2,
            seed: 1);
        AcceptAutomaticScheduleProposalChange stale = valid with
        {
            Run = new AutomaticScheduleRunRecord(
                Guid.NewGuid(),
                valid.Run.Metadata,
                valid.Run.Objective),
        };

        AcceptAutomaticScheduleProposalStoreResult result = await store.AcceptAsync(
            stale,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Conflict, result.Status);
        await AssertUnchangedWithoutRunAsync(store, draft);
    }

    [Fact]
    public async Task DuplicateAcceptanceIsRejectedAndKeepsFirstResult()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange change = CreateAutomaticChange(
            draft,
            snapshot.Id,
            dayOffset: 2,
            seed: 1);

        AcceptAutomaticScheduleProposalStoreResult first = await store.AcceptAsync(
            change,
            TestContext.Current.CancellationToken);
        AcceptAutomaticScheduleProposalStoreResult second = await store.AcceptAsync(
            change,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Succeeded, first.Status);
        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Conflict, second.Status);
        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(draft.Period, TestContext.Current.CancellationToken);
        Assert.Equal(change.UpdatedDraft.Version, reloaded.ExactDraft?.Version);
        AssertRunEqual(
            change.Run,
            Assert.IsType<AutomaticScheduleRunRecord>(reloaded.AutomaticScheduleRun));
    }

    [Fact]
    public async Task FailureAfterDeletesRollsBackDraftGenerationAndPreviousRun()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot firstSnapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange first = CreateAutomaticChange(
            draft,
            firstSnapshot.Id,
            dayOffset: 2,
            seed: 1);
        await store.AcceptAsync(first, TestContext.Current.CancellationToken);
        ScheduleDraft afterFirst = Assert.IsType<ScheduleDraft>(
            (await ((IScheduleWorkspaceReader)store).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken)).ExactDraft);
        PlanningInputSnapshot secondSnapshot = await PrepareAsync(
            store,
            afterFirst,
            firstSnapshot,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange second = CreateAutomaticChange(
            afterFirst,
            secondSnapshot.Id,
            dayOffset: 4,
            seed: 2);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER FailAutomaticRun BEFORE INSERT ON AutomaticScheduleRuns "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic failure'); END;");

        AcceptAutomaticScheduleProposalStoreResult result = await store.AcceptAsync(
            second,
            TestContext.Current.CancellationToken);

        Assert.Equal(AcceptAutomaticScheduleProposalStoreStatus.Conflict, result.Status);
        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);
        ScheduleDraft stored = Assert.IsType<ScheduleDraft>(reloaded.ExactDraft);
        Assert.Equal(afterFirst.Version, stored.Version);
        Assert.Equal(
            Assert.Single(afterFirst.Assignments, assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration).Id,
            Assert.Single(stored.Assignments, assignment =>
                assignment.Origin == AssignmentOrigin.AutomaticGeneration).Id);
        Assert.Equal(
            Assert.Single(afterFirst.GeneratedDayOffMarkers),
            Assert.Single(stored.GeneratedDayOffMarkers));
        AssertRunEqual(
            first.Run,
            Assert.IsType<AutomaticScheduleRunRecord>(reloaded.AutomaticScheduleRun));
    }

    [Fact]
    public async Task DiscardAutomaticScheduleRemovesGenerationPreparationAndLocksAfterRestart()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft initial = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot prepared = await PrepareAsync(
            store,
            initial,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange acceptedChange = CreateAutomaticChange(
            initial,
            prepared.Id,
            dayOffset: 2,
            seed: 8);
        await store.AcceptAsync(acceptedChange, TestContext.Current.CancellationToken);
        ScheduleWorkspaceReadData accepted = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(initial.Period, TestContext.Current.CancellationToken);
        ScheduleAssignment automatic = Assert.Single(
            accepted.ExactDraft!.Assignments,
            assignment => assignment.Origin == AssignmentOrigin.AutomaticGeneration);
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO ScheduleAssignmentLocks (DraftId, AssignmentId) VALUES "
            + $"('{DatabaseId(initial.Id.Value)}', '{DatabaseId(automatic.Id.Value)}');");
        accepted = await ((IScheduleWorkspaceReader)store).LoadAsync(
            initial.Period,
            TestContext.Current.CancellationToken);
        ScheduleDraft current = Assert.IsType<ScheduleDraft>(accepted.ExactDraft);
        Assert.Equal(automatic.Id, Assert.Single(current.AssignmentLocks).AssignmentId);
        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(
            current.DiscardAutomaticGeneration().Value);

        SqliteAutomaticScheduleDiscardStore discardStore = new(database.Path);
        DiscardAutomaticScheduleStoreResult result = await discardStore.DiscardAsync(
            new DiscardAutomaticScheduleChange(
                updated,
                current.Version.Value,
                accepted.PreparedSnapshot?.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(DiscardAutomaticScheduleStoreStatus.Succeeded, result.Status);
        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                initial.Period,
                TestContext.Current.CancellationToken);
        ScheduleDraft stored = Assert.IsType<ScheduleDraft>(reloaded.ExactDraft);
        Assert.Equal(current.Version.Value + 1, stored.Version.Value);
        Assert.DoesNotContain(stored.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.AutomaticGeneration);
        Assert.Contains(stored.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.ServiceManagement);
        Assert.Empty(stored.GeneratedDayOffMarkers);
        Assert.Empty(stored.AssignmentLocks);
        Assert.Null(reloaded.AutomaticScheduleRun);
        Assert.Null(reloaded.PreparedSnapshot);
    }

    [Fact]
    public async Task StaleDiscardIsRejectedAndLeavesAcceptedPlanUnchanged()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft initial = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot prepared = await PrepareAsync(
            store,
            initial,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange acceptedChange = CreateAutomaticChange(
            initial,
            prepared.Id,
            dayOffset: 2,
            seed: 9);
        await store.AcceptAsync(acceptedChange, TestContext.Current.CancellationToken);
        ScheduleWorkspaceReadData accepted = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(initial.Period, TestContext.Current.CancellationToken);
        ScheduleDraft current = Assert.IsType<ScheduleDraft>(accepted.ExactDraft);
        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(
            current.DiscardAutomaticGeneration().Value);

        SqliteAutomaticScheduleDiscardStore discardStore = new(database.Path);
        DiscardAutomaticScheduleStoreResult result = await discardStore.DiscardAsync(
            new DiscardAutomaticScheduleChange(
                updated,
                current.Version.Value - 1,
                accepted.PreparedSnapshot?.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(DiscardAutomaticScheduleStoreStatus.Conflict, result.Status);
        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(initial.Period, TestContext.Current.CancellationToken);
        Assert.Equal(current.Version, reloaded.ExactDraft?.Version);
        Assert.Single(reloaded.ExactDraft!.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.AutomaticGeneration);
        Assert.NotNull(reloaded.AutomaticScheduleRun);
        Assert.NotNull(reloaded.PreparedSnapshot);
    }

    [Fact]
    public async Task FailureDuringDiscardRollsBackEveryDeletedAutomaticValue()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft initial = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot prepared = await PrepareAsync(
            store,
            initial,
            null,
            PlanningRunOptions.Default);
        AcceptAutomaticScheduleProposalChange acceptedChange = CreateAutomaticChange(
            initial,
            prepared.Id,
            dayOffset: 2,
            seed: 10);
        await store.AcceptAsync(acceptedChange, TestContext.Current.CancellationToken);
        ScheduleWorkspaceReadData accepted = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(initial.Period, TestContext.Current.CancellationToken);
        ScheduleDraft current = Assert.IsType<ScheduleDraft>(accepted.ExactDraft);
        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(
            current.DiscardAutomaticGeneration().Value);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER FailAutomaticDiscard BEFORE DELETE ON AutomaticScheduleRuns "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic discard failure'); END;");

        SqliteAutomaticScheduleDiscardStore discardStore = new(database.Path);
        await Assert.ThrowsAsync<SqliteException>(() => discardStore.DiscardAsync(
            new DiscardAutomaticScheduleChange(
                updated,
                current.Version.Value,
                accepted.PreparedSnapshot?.Id),
            TestContext.Current.CancellationToken));

        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                initial.Period,
                TestContext.Current.CancellationToken);
        Assert.Equal(current.Version, reloaded.ExactDraft?.Version);
        Assert.Single(reloaded.ExactDraft!.Assignments, assignment =>
            assignment.Origin == AssignmentOrigin.AutomaticGeneration);
        Assert.Single(reloaded.ExactDraft.GeneratedDayOffMarkers);
        Assert.NotNull(reloaded.AutomaticScheduleRun);
        Assert.NotNull(reloaded.PreparedSnapshot);
    }

    [Fact]
    public async Task DatabaseRejectsOrphanAndInvalidSnapshotComponents()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await CreateReadyDraftAsync(store);
        PlanningInputSnapshot snapshot = await PrepareAsync(
            store,
            draft,
            null,
            PlanningRunOptions.Default);

        SqliteException orphan = await Assert.ThrowsAsync<SqliteException>(() =>
            ExecuteNonQueryAsync(
                database.Path,
                "INSERT INTO PlanningSnapshotComponents "
                + "(SnapshotId, Kind, Sequence, Payload) "
                + $"VALUES ('{DatabaseId(Guid.NewGuid())}', 0, 0, '{{}}');"));
        SqliteException invalidKind = await Assert.ThrowsAsync<SqliteException>(() =>
            ExecuteNonQueryAsync(
                database.Path,
                "INSERT INTO PlanningSnapshotComponents "
                + "(SnapshotId, Kind, Sequence, Payload) "
                + $"VALUES ('{DatabaseId(snapshot.Id)}', 99, 0, '{{}}');"));

        Assert.Equal(19, orphan.SqliteErrorCode);
        Assert.Equal(19, invalidKind.SqliteErrorCode);
    }

    [Fact]
    public async Task FailedCombinedTyp1ChangeRollsBackAvailabilityAndDraft()
    {
        using TemporarySqliteDatabase database = new();
        SqliteScheduleStore store = await InitializeAsync(
            database.Path,
            createServiceManagementEmployee: true);
        ScheduleDraft draft = await OpenDraftAsync(store, PeriodMonday);
        AvailabilityEntry entry = Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(
                ServiceManagementEmployeeId,
                PeriodMonday,
                AvailabilityEntryKind.Vacation).Value);
        AvailabilityEntryWriteStoreResult availabilityResult = await
            ((ISetAvailabilityEntryStore)new SqliteAvailabilityStore(database.Path)).SaveAsync(
                null,
                null,
                entry,
                TestContext.Current.CancellationToken);
        Assert.Equal(AvailabilityEntryWriteStoreStatus.Succeeded, availabilityResult.Status);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER FailScheduleAssignment BEFORE INSERT ON ScheduleAssignments "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic failure'); END;");
        ScheduleWorkspaceReadData workspace = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(draft.Period, TestContext.Current.CancellationToken);
        DemandSlot slot = FindSlot(
            Assert.IsType<ScheduleDraft>(workspace.ExactDraft),
            PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift);
        SetServiceManagementAssignmentRequest request = CreateAssignmentRequest(
            draft,
            slot,
            null,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            1,
            ScheduleReplacementConfirmation.Confirmed);

        ScheduleDayChangeResult result = await new SetServiceManagementAssignmentCommand(
            store,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(ScheduleDayChangeStatus.Conflict, result.Status);
        AvailabilityReadData availability = await ((IAvailabilityReader)
            new SqliteAvailabilityStore(database.Path)).LoadAsync(
                PeriodMonday,
                PeriodMonday.AddDays(6),
                TestContext.Current.CancellationToken);
        Assert.Single(availability.Entries);
        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)
            new SqliteScheduleStore(database.Path)).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken);
        Assert.Equal(1, reloaded.ExactDraft?.Version.Value);
        Assert.Empty(reloaded.ExactDraft!.Assignments);
    }

    private static AutomaticScheduleProposal CreateInterruptedProposal(
        PlanningInputSnapshot snapshot)
    {
        HashSet<(Guid SourceId, DateOnly Date, Guid WorkLocationId,
            Guid ShiftTypeId, int Ordinal)> covered = snapshot
            .ServiceManagementAssignments
            .SelectMany(assignment => assignment.Coverages)
            .Select(coverage => (
                coverage.DemandSourceId,
                coverage.Date,
                coverage.WorkLocationId,
                coverage.ShiftTypeId,
                coverage.Ordinal))
            .ToHashSet();
        AutomaticScheduleOpenDemand[] openDemands = snapshot.DemandSlots
            .Where(demand => !covered.Contains((
                demand.SourceId,
                demand.Date,
                demand.WorkLocationId,
                demand.ShiftTypeId,
                demand.Ordinal)))
            .Select(demand => new AutomaticScheduleOpenDemand(
                demand.SourceId,
                demand.Date,
                demand.WorkLocationId,
                demand.ShiftTypeId,
                demand.Ordinal,
                demand.ActualStart,
                demand.ActualEnd,
                demand.DurationMinutes,
                AutomaticScheduleOpenDemandKind.FullyUncovered))
            .ToArray();
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
        ScheduleRuleEvaluationSet evaluations = new(
            catalog,
            catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.Satisfied,
                NoRuleResultParameters.Instance)));
        ScheduleObjectiveVector objective = new(
            openDemands.Sum(demand => demand.UncoveredMinutes),
            openDemands.Length,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            []);
        AutomaticScheduleRunMetadata metadata = new(
            "Synthetic interrupted solver",
            "10.0.3",
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
            TimeSpan.FromSeconds(120),
            TimeSpan.FromMilliseconds(25),
            TimeSpan.FromSeconds(120),
            TimeSpan.FromMilliseconds(5),
            TimeSpan.FromMilliseconds(120_050),
            [new AutomaticScheduleSetting("workers", "1")],
            [
                new AutomaticSchedulePhaseSnapshot(
                    AutomaticSchedulePhaseKind.InputValidation,
                    AutomaticSchedulePhaseStatus.Completed,
                    TimeSpan.FromMilliseconds(20)),
                new AutomaticSchedulePhaseSnapshot(
                    AutomaticSchedulePhaseKind.RegularCoverage,
                    AutomaticSchedulePhaseStatus.Interrupted,
                    TimeSpan.FromSeconds(120),
                    [
                        new AutomaticSchedulePhaseValue(
                            "required_minutes",
                            snapshot.DemandSlots.Sum(demand => demand.DurationMinutes)),
                        new AutomaticSchedulePhaseValue(
                            "uncovered_minutes",
                            openDemands.Sum(demand => demand.UncoveredMinutes)),
                    ],
                    new AutomaticSchedulePhaseTerminationSnapshot(
                        AutomaticSchedulePhaseTerminationReason
                            .TimeLimitWithFeasibleSelection,
                        AutomaticScheduleOptimizationTargetKind
                            .RegularTouchedDemandSlots,
                        TimeSpan.FromSeconds(120),
                        TimeSpan.FromMilliseconds(120_005))),
            ]);
        return new AutomaticScheduleProposal(
            snapshot.Id,
            snapshot.DraftId,
            snapshot.DraftVersion,
            [],
            [],
            openDemands,
            objective,
            evaluations,
            metadata);
    }

    private static AcceptAutomaticScheduleProposalChange CreateAutomaticChange(
        ScheduleDraft current,
        Guid snapshotId,
        int dayOffset,
        int seed)
    {
        EmployeeId.TryCreate(StandardEmployeeId, out EmployeeId? employeeId);
        DemandSlot slot = FindSlot(
            current,
            current.Period.StartMonday.AddDays(dayOffset),
            InitialShiftTypeCatalog.EarlyShift);
        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                new Guid($"8c000000-0000-4000-8000-{seed:D12}"),
                employeeId,
                slot,
                AssignmentOrigin.AutomaticGeneration).Value);
        GeneratedDayOffMarker marker = new(
            employeeId!,
            current.Period.StartMonday.AddDays(dayOffset + 1));
        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(
            current.ReplaceAutomaticGeneration([assignment], [marker]).Value);
        AutomaticScheduleRunMetadata metadata = new(
            $"Synthetic solver {seed}",
            $"10.0.{seed}",
            seed % 2 == 0
                ? AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal
                : AutomaticSchedulePlanningStatus.Optimal,
            TimeSpan.FromSeconds(120),
            TimeSpan.FromMilliseconds(100 + seed),
            TimeSpan.FromMilliseconds(500 + (seed * 2)),
            TimeSpan.FromMilliseconds(40 + seed),
            TimeSpan.FromMilliseconds(700 + (seed * 10)),
            [
                new AutomaticScheduleSetting("random_seed", seed.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)),
                new AutomaticScheduleSetting("workers", "1"),
            ],
            [
                new AutomaticSchedulePhaseSnapshot(
                    AutomaticSchedulePhaseKind.InputValidation,
                    AutomaticSchedulePhaseStatus.Completed,
                    TimeSpan.FromMilliseconds(10 + seed)),
                new AutomaticSchedulePhaseSnapshot(
                    AutomaticSchedulePhaseKind.RegularCoverage,
                    seed % 2 == 0
                        ? AutomaticSchedulePhaseStatus.Interrupted
                        : AutomaticSchedulePhaseStatus.Completed,
                    TimeSpan.FromMilliseconds(20 + seed),
                    [
                        new AutomaticSchedulePhaseValue(
                            "required_minutes",
                            120 * seed),
                        new AutomaticSchedulePhaseValue(
                            "uncovered_minutes",
                            60 * seed),
                    ],
                    seed % 2 == 0
                        ? new AutomaticSchedulePhaseTerminationSnapshot(
                            AutomaticSchedulePhaseTerminationReason
                                .TimeLimitWithFeasibleSelection,
                            AutomaticScheduleOptimizationTargetKind
                                .RegularTouchedDemandSlots,
                            TimeSpan.FromSeconds(120),
                            TimeSpan.FromMilliseconds(120_000 + seed))
                        : null),
            ]);
        AutomaticScheduleObjectiveSnapshot objective = new(
            120 * seed,
            seed,
            [new AutomaticScheduleRuleViolation(
                "HO-01",
                RuleFamily.Soft,
                RulePriority.High,
                $"high-{seed}",
                seed)],
            seed,
            seed + 1,
            [new AutomaticScheduleAuxiliaryMinimumCase(
                StandardEmployeeId,
                PeriodMonday,
                180 + seed,
                true)],
            [new AutomaticScheduleRelativeWeeklyTargetCase(
                StandardEmployeeId,
                PeriodMonday,
                180 + seed,
                600)],
            [new AutomaticScheduleRuleViolation(
                "MI-01",
                RuleFamily.Soft,
                RulePriority.Medium,
                $"medium-{seed}",
                seed + 1)],
            [new AutomaticScheduleRuleViolation(
                "LO-01",
                RuleFamily.Soft,
                RulePriority.Low,
                $"low-{seed}",
                seed + 2)],
            [new AutomaticScheduleRuleViolation(
                "ST-01",
                RuleFamily.Stability,
                null,
                $"stability-{seed}",
                seed + 3)],
            [$"assignment-{seed}"]);
        return new AcceptAutomaticScheduleProposalChange(
            updated,
            current.Version.Value,
            new AutomaticScheduleRunRecord(snapshotId, metadata, objective));
    }

    private static async Task AssertUnchangedWithoutRunAsync(
        SqliteScheduleStore store,
        ScheduleDraft expected)
    {
        ScheduleWorkspaceReadData reloaded = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(expected.Period, TestContext.Current.CancellationToken);
        ScheduleDraft stored = Assert.IsType<ScheduleDraft>(reloaded.ExactDraft);
        Assert.Equal(expected.Version, stored.Version);
        Assert.Equal(expected.Assignments.Select(item => item.Id),
            stored.Assignments.Select(item => item.Id));
        Assert.Equal(expected.GeneratedDayOffMarkers, stored.GeneratedDayOffMarkers);
        Assert.Null(reloaded.AutomaticScheduleRun);
    }

    private static void AssertRunEqual(
        AutomaticScheduleRunRecord expected,
        AutomaticScheduleRunRecord actual)
    {
        Assert.Equal(expected.SnapshotId, actual.SnapshotId);
        Assert.Equal(expected.Metadata.SolverName, actual.Metadata.SolverName);
        Assert.Equal(expected.Metadata.SolverVersion, actual.Metadata.SolverVersion);
        Assert.Equal(expected.Metadata.ResultStatus, actual.Metadata.ResultStatus);
        Assert.Equal(expected.Metadata.TimeLimit, actual.Metadata.TimeLimit);
        Assert.Equal(
            expected.Metadata.ModelBuildDuration,
            actual.Metadata.ModelBuildDuration);
        Assert.Equal(
            expected.Metadata.OptimizationDuration,
            actual.Metadata.OptimizationDuration);
        Assert.Equal(
            expected.Metadata.ResultMappingDuration,
            actual.Metadata.ResultMappingDuration);
        Assert.Equal(expected.Metadata.TotalDuration, actual.Metadata.TotalDuration);
        Assert.Equal(expected.Metadata.Settings, actual.Metadata.Settings);
        Assert.Equal(expected.Metadata.Phases.Count, actual.Metadata.Phases.Count);
        for (int index = 0; index < expected.Metadata.Phases.Count; index++)
        {
            AutomaticSchedulePhaseSnapshot expectedPhase = expected.Metadata.Phases[index];
            AutomaticSchedulePhaseSnapshot actualPhase = actual.Metadata.Phases[index];
            Assert.Equal(expectedPhase.Kind, actualPhase.Kind);
            Assert.Equal(expectedPhase.Status, actualPhase.Status);
            Assert.Equal(expectedPhase.Duration, actualPhase.Duration);
            Assert.Equal(expectedPhase.Values, actualPhase.Values);
            Assert.Equal(
                expectedPhase.DetailAvailability,
                actualPhase.DetailAvailability);
            Assert.Equal(
                expectedPhase.Termination?.Reason,
                actualPhase.Termination?.Reason);
            Assert.Equal(
                expectedPhase.Termination?.ActiveTarget,
                actualPhase.Termination?.ActiveTarget);
            Assert.Equal(
                expectedPhase.Termination?.TimeLimit,
                actualPhase.Termination?.TimeLimit);
            Assert.Equal(
                expectedPhase.Termination?.BudgetElapsed,
                actualPhase.Termination?.BudgetElapsed);
        }

        Assert.Equal(
            expected.Objective.UncoveredEmployeeMinutes,
            actual.Objective.UncoveredEmployeeMinutes);
        Assert.Equal(
            expected.Objective.FullyUncoveredDemandSlotCount,
            actual.Objective.FullyUncoveredDemandSlotCount);
        Assert.Equal(
            expected.Objective.HighPriorityViolations,
            actual.Objective.HighPriorityViolations);
        Assert.Equal(
            expected.Objective.ReliefShiftAssignmentCount,
            actual.Objective.ReliefShiftAssignmentCount);
        Assert.Equal(
            expected.Objective.SplitShiftAssignmentCount,
            actual.Objective.SplitShiftAssignmentCount);
        Assert.Equal(
            expected.Objective.AuxiliaryMinimumCases,
            actual.Objective.AuxiliaryMinimumCases);
        Assert.Equal(
            expected.Objective.RelativeWeeklyTargetCases,
            actual.Objective.RelativeWeeklyTargetCases);
        Assert.Equal(
            expected.Objective.MediumPriorityViolations,
            actual.Objective.MediumPriorityViolations);
        Assert.Equal(
            expected.Objective.LowPriorityViolations,
            actual.Objective.LowPriorityViolations);
        Assert.Equal(
            expected.Objective.StabilityViolations,
            actual.Objective.StabilityViolations);
        Assert.Equal(
            expected.Objective.TechnicalTieBreakerKeys,
            actual.Objective.TechnicalTieBreakerKeys);
    }

    private static void AssertPhaseTerminationEqual(
        ReadOnlyCollection<AutomaticSchedulePhaseSnapshot> expected,
        ReadOnlyCollection<AutomaticSchedulePhaseSnapshot> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].Status, actual[index].Status);
            Assert.Equal(expected[index].Duration, actual[index].Duration);
            Assert.Equal(expected[index].Values, actual[index].Values);
            Assert.Equal(
                expected[index].DetailAvailability,
                actual[index].DetailAvailability);
            Assert.Equal(
                expected[index].Termination?.Reason,
                actual[index].Termination?.Reason);
            Assert.Equal(
                expected[index].Termination?.ActiveTarget,
                actual[index].Termination?.ActiveTarget);
            Assert.Equal(
                expected[index].Termination?.TimeLimit,
                actual[index].Termination?.TimeLimit);
            Assert.Equal(
                expected[index].Termination?.BudgetElapsed,
                actual[index].Termination?.BudgetElapsed);
        }
    }

    private static async Task<SqliteScheduleStore> InitializeAsync(
        string databasePath,
        bool createServiceManagementEmployee = false)
    {
        await new SqliteServiceCatalogStore(databasePath).InitializeAsync(
            TestContext.Current.CancellationToken);
        await new SqliteStaffingDemandStore(databasePath).InitializeAsync(
            TestContext.Current.CancellationToken);
        SqliteScheduleStore store = new(databasePath);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        if (createServiceManagementEmployee)
        {
            Employee serviceManagementEmployee = Assert.IsType<Employee>(Employee.Create(
                ServiceManagementEmployeeId,
                "Sarah",
                "Leitung",
                InitialEmployeeTypeCatalog.Type1.Id.Value).Value);
            EmployeeWriteStoreResult serviceManagementResult = await ((ICreateEmployeeStore)
                new SqliteEmployeeStore(databasePath)).CreateAsync(
                    serviceManagementEmployee,
                    InitialEmployeeTypeCatalog.Type1.Id,
                    TestContext.Current.CancellationToken);
            Assert.Equal(EmployeeWriteStoreResult.Succeeded, serviceManagementResult);
            Employee standardEmployee = Assert.IsType<Employee>(Employee.Create(
                StandardEmployeeId,
                "Erika",
                "Muster",
                InitialEmployeeTypeCatalog.Type20.Id.Value).Value);
            EmployeeWriteStoreResult standardResult = await ((ICreateEmployeeStore)
                new SqliteEmployeeStore(databasePath)).CreateAsync(
                    standardEmployee,
                    InitialEmployeeTypeCatalog.Type20.Id,
                    TestContext.Current.CancellationToken);
            Assert.Equal(EmployeeWriteStoreResult.Succeeded, standardResult);
        }

        return store;
    }

    private static async Task<ScheduleDraft> OpenDraftAsync(
        SqliteScheduleStore store,
        DateOnly selectedDate)
    {
        OpenOrCreateScheduleDraftResult result = await
            new OpenOrCreateScheduleDraftCommand(store, store).ExecuteAsync(
                selectedDate,
                TestContext.Current.CancellationToken);
        Assert.Equal(OpenOrCreateScheduleDraftStatus.Succeeded, result.Status);
        SchedulePeriod period = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(result.Value!.PeriodMonday).Value);
        return Assert.IsType<ScheduleDraft>(
            (await ((IScheduleWorkspaceReader)store).LoadAsync(
                period,
                TestContext.Current.CancellationToken)).ExactDraft);
    }

    private static async Task<ScheduleDraft> CreateReadyDraftAsync(
        SqliteScheduleStore store)
    {
        ScheduleDraft draft = await OpenDraftAsync(store, PeriodMonday);
        for (int week = 0; week < 3; week++)
        {
            draft = await SetAssignmentAsync(
                store,
                draft,
                ServiceManagementAssignmentSelectionKind.NormalDemand,
                PeriodMonday.AddDays(week * 7),
                InitialShiftTypeCatalog.EarlyShift);
        }

        return draft;
    }

    private static async Task<ScheduleDraft> SetAssignmentAsync(
        SqliteScheduleStore store,
        ScheduleDraft current,
        ServiceManagementAssignmentSelectionKind kind,
        DateOnly date,
        ShiftType firstShift,
        ShiftType? secondShift = null)
    {
        ScheduleWorkspaceReadData data = await ((IScheduleWorkspaceReader)store)
            .LoadAsync(current.Period, TestContext.Current.CancellationToken);
        ScheduleDraft draft = Assert.IsType<ScheduleDraft>(data.ExactDraft);
        DemandSlot first = FindSlot(draft, date, firstShift);
        DemandSlot? second = secondShift is null
            ? null
            : FindSlot(draft, date, secondShift);
        SetServiceManagementAssignmentRequest request = CreateAssignmentRequest(
            draft,
            first,
            second,
            kind,
            null,
            ScheduleReplacementConfirmation.NotConfirmed);

        ScheduleDayChangeResult result = await new SetServiceManagementAssignmentCommand(
            store,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        return Assert.IsType<ScheduleDraft>(
            (await ((IScheduleWorkspaceReader)store).LoadAsync(
                draft.Period,
                TestContext.Current.CancellationToken)).ExactDraft);
    }

    private static SetServiceManagementAssignmentRequest CreateAssignmentRequest(
        ScheduleDraft draft,
        DemandSlot first,
        DemandSlot? second,
        ServiceManagementAssignmentSelectionKind kind,
        long? expectedEntryVersion,
        ScheduleReplacementConfirmation confirmation)
    {
        return new SetServiceManagementAssignmentRequest(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            ServiceManagementEmployeeId,
            kind,
            CreateSelection(first),
            second is null ? null : CreateSelection(second),
            expectedEntryVersion,
            confirmation);
    }

    private static ScheduleDemandSlotSelection CreateSelection(DemandSlot slot)
    {
        return new ScheduleDemandSlotSelection(
            slot.Id.SourceId.Value,
            slot.Id.SourceKind == StaffingDemandSourceKind.Standard
                ? ScheduleDemandSourceKindSnapshot.Standard
                : ScheduleDemandSourceKindSnapshot.DateException,
            slot.Id.Date,
            slot.Id.WorkLocationId.Value,
            slot.Id.ShiftTypeId.Value,
            slot.Id.Ordinal,
            slot.ActualTime.Start,
            slot.ActualTime.End);
    }

    private static DemandSlot FindSlot(
        ScheduleDraft draft,
        DateOnly date,
        ShiftType shiftType)
    {
        return Assert.Single(
            draft.DemandSlots.Slots,
            slot => slot.Id.Date == date
                && slot.Id.ShiftTypeId == shiftType.Id
                && slot.Id.Ordinal == 1);
    }

    private static async Task<PlanningInputSnapshot> PrepareAsync(
        SqliteScheduleStore store,
        ScheduleDraft draft,
        PlanningInputSnapshot? previous,
        PlanningRunOptions options)
    {
        PreparePlanningInputRequest request = new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            previous is null
                ? PlanningPreparationExpectation.NotPrepared
                : PlanningPreparationExpectation.Prepared,
            previous?.Id,
            options);
        PreparePlanningInputResult result = await new PreparePlanningInputCommand(
            store,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(PreparePlanningInputStatus.Succeeded, result.Status);
        return Assert.IsType<PlanningInputSnapshot>(result.Value);
    }

    private static PlanningInputSnapshot CopyPlanningSnapshot(
        PlanningInputSnapshot source,
        PlanningRunOptions options,
        RuleCatalogSnapshot? ruleCatalog = null)
    {
        return new PlanningInputSnapshot(
            Guid.NewGuid(),
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            source.Employees,
            source.EmployeeTypes,
            source.ServiceCatalog,
            source.AvailabilityEntries,
            source.DemandSlots,
            source.ServiceManagementAssignments,
            ruleCatalog ?? source.RuleCatalog,
            options,
            source.History);
    }

    private static ScheduleDraft CreateDraft(
        SchedulePeriod period,
        ScheduleWorkspaceReadData data)
    {
        StandardStaffingDemandRevisionSet revisions =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    data.StaffingDemands.StandardRevisions).Value);
        StaffingDemandDateExceptionSet exceptions =
            Assert.IsType<StaffingDemandDateExceptionSet>(
                StaffingDemandDateExceptionSet.Create(
                    data.StaffingDemands.DateExceptions).Value);
        List<EffectiveStaffingDemand> demands = [];
        for (int week = 0; week < 3; week++)
        {
            demands.AddRange(Assert.IsType<StaffingDemandWeek>(
                StaffingDemandWeek.Resolve(
                    period.StartMonday.AddDays(week * 7),
                    revisions,
                    exceptions).Value).Demands);
        }

        DemandSlotSet slots = Assert.IsType<DemandSlotSet>(DemandSlotSet.Create(
            period,
            demands,
            data.StaffingDemands.ServiceCatalog.WorkLocations.Select(item => item.Id),
            data.StaffingDemands.ServiceCatalog.ShiftTypes.Select(item => item.Id)).Value);
        AvailabilityEntrySet availability = Assert.IsType<AvailabilityEntrySet>(
            AvailabilityEntrySet.Create(
                data.Availability.Entries.Select(item => item.Entry)).Value);
        return Assert.IsType<ScheduleDraft>(ScheduleDraft.Create(
            Guid.NewGuid(),
            1,
            period,
            slots,
            availability,
            [],
            [],
            []).Value);
    }

    private static async Task MigrateAsync(string databasePath, string migration)
    {
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(databasePath).Create();
        IMigrator migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(migration, TestContext.Current.CancellationToken);
    }

    private static async Task<long> ExecuteScalarAsync(
        string databasePath,
        string commandText)
    {
        await using SqliteConnection connection = CreateConnection(databasePath);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        object? value = await command.ExecuteScalarAsync(
            TestContext.Current.CancellationToken);
        return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteNonQueryAsync(
        string databasePath,
        string commandText)
    {
        await using SqliteConnection connection = CreateConnection(databasePath);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }

    private static SqliteConnection CreateConnection(string databasePath)
    {
        return new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWrite,
            ForeignKeys = true,
            Pooling = false,
        }.ToString());
    }

    private static string DatabaseId(Guid id)
    {
        return id.ToString().ToUpperInvariant();
    }

    private sealed class FixedPlanner(AutomaticSchedulePlanningResult result)
        : IAutomaticSchedulePlanner
    {
        public Task<AutomaticSchedulePlanningResult> PlanAsync(
            AutomaticSchedulePlanningRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(result);
        }
    }

    private sealed class TemporarySqliteDatabase : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "Salztal.Dienstplanung.Tests",
            Guid.NewGuid().ToString("N"));

        public TemporarySqliteDatabase()
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "scheduling.db");
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }
    }
}
