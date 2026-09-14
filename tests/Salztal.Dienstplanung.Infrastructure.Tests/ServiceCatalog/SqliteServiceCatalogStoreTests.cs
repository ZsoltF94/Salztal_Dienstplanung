using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Tests.ServiceCatalog;

public sealed class SqliteServiceCatalogStoreTests
{
    private const string InitialServiceCatalogMigration =
        "20260913202156_InitialServiceCatalog";
    private const string EmployeeMigration =
        "20260914005039_AddEmployeesAndEmployeeTypes";
    private const string StaffingDemandMigration =
        "20260914144923_AddStaffingDemands";
    private const string StaffingDemandCorrectionMigration =
        "20260914185047_AddStandardDemandCorrectionSequence";

    [Fact]
    public async Task InitializeAsyncOnEmptyDatabaseCreatesMigrationAndCompleteInitialCatalog()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore store = new(database.Path);

        await store.InitializeAsync(TestContext.Current.CancellationToken);
        ServiceCatalogData data = await store.LoadAsync(TestContext.Current.CancellationToken);

        Assert.True(File.Exists(database.Path));
        Assert.Equal(2, data.WorkLocations.Count);
        Assert.Equal(4, data.ShiftTypes.Count);
        Assert.Contains(
            data.WorkLocations,
            location => location.Id == InitialWorkLocationCatalog.Cafeteria.Id
                && location.Color.Code == "yellow");
        Assert.Contains(
            data.WorkLocations,
            location => location.Id == InitialWorkLocationCatalog.Restaurant.Id
                && location.Color.Code == "red");
        Assert.Equal("D", data.SplitShiftPattern.DisplayCode);
        Assert.Equal(180, data.SplitShiftPattern.StandardBreakMinutes);
        Assert.Equal(600, data.SplitShiftPattern.StandardWorkMinutes);
        Assert.Equal("Spr", data.ReliefShiftPattern.DisplayCode);
        Assert.Equal("blue", data.ReliefShiftPattern.DisplayColorCode);
        Assert.Equal(DayOfWeek.Saturday, data.ReliefShiftPattern.AllowedDay);
        Assert.Equal(
            InitialShiftTypeCatalog.CafeteriaShiftB.Id,
            data.ReliefShiftPattern.FirstShiftTypeId);
        Assert.Equal(
            InitialShiftTypeCatalog.LateShift.Id,
            data.ReliefShiftPattern.SecondShiftTypeId);
    }

    [Fact]
    public async Task InitializeAsyncWhenRepeatedKeepsAllMigrationsAndOneSetOfInitialValues()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore store = new(database.Path);

        await store.InitializeAsync(TestContext.Current.CancellationToken);
        await store.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
        Assert.Equal(2L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM WorkLocations;"));
        Assert.Equal(4L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM ShiftTypes;"));
        Assert.Equal(2L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM ShiftPatterns;"));
    }

    [Fact]
    public async Task PublishedSystem04MigrationTargetCreatesExactOriginalSchema()
    {
        using TemporarySqliteDatabase database = new();

        await MigrateToSystem04Async(database.Path);

        Assert.Equal(
            [InitialServiceCatalogMigration],
            await ExecuteStringColumnAsync(
                database.Path,
                "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"));
        Assert.Equal(
            [
                "ShiftPatterns",
                "ShiftTypes",
                "WorkLocations",
                "__EFMigrationsHistory",
                "__EFMigrationsLock",
            ],
            await ExecuteStringColumnAsync(
                database.Path,
                "SELECT name FROM sqlite_schema "
                + "WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;"));
    }

    [Fact]
    public async Task ExistingSystem04DatabaseWhenUpgradedKeepsCatalogSchemaAndValuesReadable()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateToSystem04Async(database.Path);
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE WorkLocations SET Name = 'Synthetischer Bestand' "
            + $"WHERE lower(Id) = lower('{InitialWorkLocationCatalog.Cafeteria.Id.Value:D}');");
        IReadOnlyList<string> schemaBefore = await ReadServiceCatalogSchemaAsync(database.Path);
        IReadOnlyList<string> migrationHistoryBefore = await ReadMigrationHistoryAsync(database.Path);

        SqliteServiceCatalogStore restartedStore = new(database.Path);
        await restartedStore.InitializeAsync(TestContext.Current.CancellationToken);
        ServiceCatalogData reloaded = await restartedStore.LoadAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(schemaBefore, await ReadServiceCatalogSchemaAsync(database.Path));
        Assert.Equal([InitialServiceCatalogMigration + "|10.0.12"], migrationHistoryBefore);
        Assert.Equal(
            [
                InitialServiceCatalogMigration + "|10.0.12",
                EmployeeMigration + "|10.0.12",
                StaffingDemandMigration + "|10.0.12",
                StaffingDemandCorrectionMigration + "|10.0.12",
            ],
            await ReadMigrationHistoryAsync(database.Path));
        Assert.Equal(
            "Synthetischer Bestand",
            Assert.Single(
                reloaded.WorkLocations,
                location => location.Id == InitialWorkLocationCatalog.Cafeteria.Id).Name.Value);
        Assert.Equal(4, reloaded.ShiftTypes.Count);
        Assert.Equal("D", reloaded.SplitShiftPattern.DisplayCode);
        Assert.Equal("Spr", reloaded.ReliefShiftPattern.DisplayCode);
    }

    [Fact]
    public async Task UpdatesWhenStoreIsRecreatedSurviveRoundTripAndRebuildSplitShiftValues()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore firstStore = new(database.Path);
        await firstStore.InitializeAsync(TestContext.Current.CancellationToken);

        IWorkLocationUpdateStore workLocationStore = firstStore;
        WorkLocation currentLocation = Assert.IsType<WorkLocation>(
            await workLocationStore.FindAsync(
                InitialWorkLocationCatalog.Cafeteria.Id,
                TestContext.Current.CancellationToken));
        WorkLocation replacementLocation = Assert.IsType<WorkLocation>(
            currentLocation.WithDetails("Kiosk", "amber").Value);
        Assert.Equal(
            CatalogEntryUpdateStoreResult.Updated,
            await workLocationStore.UpdateAsync(
                currentLocation,
                replacementLocation,
                TestContext.Current.CancellationToken));

        IShiftTypeStandardTimeUpdateStore shiftTypeStore = firstStore;
        ShiftTypeStandardTimeUpdateData currentShift = Assert.IsType<ShiftTypeStandardTimeUpdateData>(
            await shiftTypeStore.FindAsync(
                InitialShiftTypeCatalog.EarlyShift.Id,
                TestContext.Current.CancellationToken));
        ShiftStandardTime replacementTime = Assert.IsType<ShiftStandardTime>(
            ShiftStandardTime.Create(new TimeOnly(7, 0), new TimeOnly(14, 0)).Value);
        ShiftType replacementShift = Assert.IsType<ShiftType>(
            currentShift.Current.WithStandardTime(replacementTime).Value);
        Assert.Equal(
            CatalogEntryUpdateStoreResult.Updated,
            await shiftTypeStore.UpdateAsync(
                currentShift,
                replacementShift,
                TestContext.Current.CancellationToken));

        SqliteServiceCatalogStore restartedStore = new(database.Path);
        await restartedStore.InitializeAsync(TestContext.Current.CancellationToken);
        ServiceCatalogData reloaded = await restartedStore.LoadAsync(
            TestContext.Current.CancellationToken);

        WorkLocation reloadedLocation = Assert.Single(
            reloaded.WorkLocations,
            location => location.Id == currentLocation.Id);
        Assert.Equal("Kiosk", reloadedLocation.Name.Value);
        Assert.Equal("amber", reloadedLocation.Color.Code);
        ShiftType reloadedShift = Assert.Single(
            reloaded.ShiftTypes,
            shiftType => shiftType.Id == currentShift.Current.Id);
        Assert.Equal(new TimeOnly(7, 0), reloadedShift.StandardTime.Start);
        Assert.Equal(new TimeOnly(14, 0), reloadedShift.StandardTime.End);
        Assert.Equal(150, reloaded.SplitShiftPattern.StandardBreakMinutes);
        Assert.Equal(600, reloaded.SplitShiftPattern.StandardWorkMinutes);
    }

    [Fact]
    public async Task ActualSqliteDatabaseRejectsInvalidRelationshipAndDuplicatePatternKind()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore store = new(database.Path);
        await store.InitializeAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO ShiftTypes "
            + "(Id, Name, WorkLocationId, DisplayKind, Abbreviation, "
            + "StandardStartMinutes, StandardEndMinutes) VALUES "
            + "('00000000-0000-0000-0000-000000000001', 'Synthetic', "
            + "'00000000-0000-0000-0000-000000000002', 0, 'X', 60, 120);"));

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO ShiftPatterns "
            + "(Id, Kind, DisplayCode, DisplayColorCode, AllowedDay, SwitchRule, "
            + "HasInterruption, FirstShiftTypeId, SecondShiftTypeId) "
            + "SELECT '00000000-0000-0000-0000-000000000003', Kind, 'X', NULL, NULL, NULL, "
            + "HasInterruption, FirstShiftTypeId, SecondShiftTypeId "
            + "FROM ShiftPatterns WHERE Kind = 0;"));
    }

    [Fact]
    public async Task FailedWriteLeavesOriginalWorkLocationUnchanged()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore store = new(database.Path);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER RejectWorkLocationUpdate BEFORE UPDATE ON WorkLocations "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic write failure'); END;");

        IWorkLocationUpdateStore updateStore = store;
        WorkLocation current = Assert.IsType<WorkLocation>(
            await updateStore.FindAsync(
                InitialWorkLocationCatalog.Restaurant.Id,
                TestContext.Current.CancellationToken));
        WorkLocation replacement = Assert.IsType<WorkLocation>(
            current.WithDetails("Speisesaal", "crimson").Value);

        await Assert.ThrowsAsync<SqliteException>(() => updateStore.UpdateAsync(
            current,
            replacement,
            TestContext.Current.CancellationToken));

        WorkLocation reloaded = Assert.IsType<WorkLocation>(
            await updateStore.FindAsync(current.Id, TestContext.Current.CancellationToken));
        Assert.Equal(current.Name, reloaded.Name);
        Assert.Equal(current.Color, reloaded.Color);
    }

    [Fact]
    public async Task UpdateWhenStoredValueChangedReturnsConflictWithoutOverwritingNewerValue()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore store = new(database.Path);
        await store.InitializeAsync(TestContext.Current.CancellationToken);

        IWorkLocationUpdateStore updateStore = store;
        WorkLocation staleValue = Assert.IsType<WorkLocation>(
            await updateStore.FindAsync(
                InitialWorkLocationCatalog.Cafeteria.Id,
                TestContext.Current.CancellationToken));
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE WorkLocations SET Name = 'Neuere Fassung' "
            + $"WHERE lower(Id) = lower('{staleValue.Id.Value:D}');");
        WorkLocation replacement = Assert.IsType<WorkLocation>(
            staleValue.WithDetails("Veraltete Fassung", "green").Value);

        CatalogEntryUpdateStoreResult result = await updateStore.UpdateAsync(
            staleValue,
            replacement,
            TestContext.Current.CancellationToken);

        Assert.Equal(CatalogEntryUpdateStoreResult.Conflict, result);
        WorkLocation reloaded = Assert.IsType<WorkLocation>(
            await updateStore.FindAsync(staleValue.Id, TestContext.Current.CancellationToken));
        Assert.Equal("Neuere Fassung", reloaded.Name.Value);
        Assert.Equal(staleValue.Color, reloaded.Color);
    }

    private static async Task<long> ExecuteScalarAsync(string databasePath, string commandText)
    {
        await using SqliteConnection connection = CreateConnection(databasePath);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;

        object? value = await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
        return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static Task<IReadOnlyList<string>> ReadMigrationHistoryAsync(string databasePath)
    {
        return ExecuteStringColumnAsync(
            databasePath,
            "SELECT MigrationId || '|' || ProductVersion "
            + "FROM __EFMigrationsHistory ORDER BY MigrationId;");
    }

    private static Task<IReadOnlyList<string>> ReadServiceCatalogSchemaAsync(string databasePath)
    {
        return ExecuteStringColumnAsync(
            databasePath,
            "SELECT type || '|' || name || '|' || coalesce(sql, '') "
            + "FROM sqlite_schema WHERE tbl_name IN "
            + "('WorkLocations', 'ShiftTypes', 'ShiftPatterns') ORDER BY type, name;");
    }

    private static async Task MigrateToSystem04Async(string databasePath)
    {
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(databasePath).Create();
        IMigrator migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(
            InitialServiceCatalogMigration,
            TestContext.Current.CancellationToken);
    }

    private static async Task<IReadOnlyList<string>> ExecuteStringColumnAsync(
        string databasePath,
        string commandText)
    {
        await using SqliteConnection connection = CreateConnection(databasePath);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(
            TestContext.Current.CancellationToken);
        List<string> values = [];

        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private static async Task ExecuteNonQueryAsync(string databasePath, string commandText)
    {
        await using SqliteConnection connection = CreateConnection(databasePath);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }

    private static SqliteConnection CreateConnection(string databasePath)
    {
        return new SqliteConnection(
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWrite,
                ForeignKeys = true,
                Pooling = false,
            }.ToString());
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
            Path = System.IO.Path.Combine(_directory, "catalog.db");
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }
    }
}
