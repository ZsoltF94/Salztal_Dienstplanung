using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
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

    private static readonly DateOnly PeriodMonday = new(2026, 9, 14);

    private static readonly Guid ServiceManagementEmployeeId =
        new("aae194ab-5147-4760-898f-f3d407c29ae9");

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

        Assert.Equal(7L, await ExecuteScalarAsync(
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
        Assert.Equal(28, storedSnapshot.RuleCatalog.Definitions.Count);
        Assert.Equal(195, storedSnapshot.DemandSlots.Count);
        Assert.Equal(6, storedSnapshot.ServiceManagementAssignments.Count);
        Assert.Equal(PlanningHistoryCompleteness.Missing, storedSnapshot.History.Completeness);
        Assert.True(PlanningInputComparison.Compare(prepared, storedSnapshot).IsCurrent);
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
            Employee employee = Assert.IsType<Employee>(Employee.Create(
                ServiceManagementEmployeeId,
                "Sarah",
                "Leitung",
                InitialEmployeeTypeCatalog.Type1.Id.Value).Value);
            EmployeeWriteStoreResult result = await ((ICreateEmployeeStore)
                new SqliteEmployeeStore(databasePath)).CreateAsync(
                    employee,
                    InitialEmployeeTypeCatalog.Type1.Id,
                    TestContext.Current.CancellationToken);
            Assert.Equal(EmployeeWriteStoreResult.Succeeded, result);
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
        PlanningRunOptions options)
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
            source.RuleCatalog,
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
