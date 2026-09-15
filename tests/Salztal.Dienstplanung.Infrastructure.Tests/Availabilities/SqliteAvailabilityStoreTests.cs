using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Tests.Availabilities;

public sealed class SqliteAvailabilityStoreTests
{
    private const string EmployeeTypeManagementMigration =
        "20260915160041_AddEmployeeTypeManagement";

    private static readonly Guid EmployeeIdentifier =
        new("d01d799c-b3df-4b8f-b55e-80026b478816");

    private static readonly DateOnly EntryDate = new(2026, 12, 23);

    [Fact]
    public async Task InitializeOnEmptyDatabaseCreatesAvailabilityTableAndCurrentModel()
    {
        using TemporarySqliteDatabase database = new();

        await InitializeAsync(database.Path);

        Assert.Equal(6L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM sqlite_schema "
            + "WHERE type = 'table' AND name = 'AvailabilityEntries';"));
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(database.Path).Create();
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(
            TestContext.Current.CancellationToken));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task ExistingEmployeeTypeManagementDatabaseWhenUpgradedKeepsSyntheticEmployee()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateAsync(database.Path, EmployeeTypeManagementMigration);
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO Employees (Id, FirstName, LastName, EmployeeTypeId, IsActive) "
            + $"VALUES ('{DatabaseId(EmployeeIdentifier)}', 'Erika', 'Muster', "
            + $"'{DatabaseId(InitialEmployeeTypeCatalog.Type25.Id.Value)}', 1);");

        await InitializeAsync(database.Path);

        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            $"SELECT COUNT(*) FROM Employees WHERE Id = '{DatabaseId(EmployeeIdentifier)}';"));
        Assert.Equal(0L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM AvailabilityEntries;"));
        Assert.Equal(6L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
    }

    [Fact]
    public async Task SaveWhenStoreRestartsPreservesEntryAndChangeVersion()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);
        AvailabilityEntry entry = CreateEntry(EntryDate, AvailabilityEntryKind.Vacation);
        ISetAvailabilityEntryStore store = new SqliteAvailabilityStore(database.Path);

        AvailabilityEntryWriteStoreResult result = await store.SaveAsync(
            null,
            null,
            entry,
            TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryWriteStoreStatus.Succeeded, result.Status);
        Assert.Equal(1, result.ChangeVersion);
        AvailabilityReadData reloaded = await ((IAvailabilityReader)
            new SqliteAvailabilityStore(database.Path)).LoadAsync(
                new DateOnly(2026, 12, 21),
                new DateOnly(2026, 12, 27),
                TestContext.Current.CancellationToken);
        AvailabilityEntryReadItem item = Assert.Single(reloaded.Entries);
        Assert.Equal(entry.EmployeeId, item.Entry.EmployeeId);
        Assert.Equal(entry.Date, item.Entry.Date);
        Assert.Equal(entry.Kind, item.Entry.Kind);
        Assert.Equal(1, item.ChangeVersion);
        Assert.Single(reloaded.Employees);
        Assert.Equal(11, reloaded.EmployeeTypes.Count);
    }

    [Fact]
    public async Task SaveReplacementIncrementsVersionAndStaleWriteCannotOverwriteIt()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);
        ISetAvailabilityEntryStore store = new SqliteAvailabilityStore(database.Path);
        AvailabilityEntry vacation = CreateEntry(EntryDate, AvailabilityEntryKind.Vacation);
        AvailabilityEntry sickness = CreateEntry(EntryDate, AvailabilityEntryKind.Sickness);
        AvailabilityEntry fixedDayOff = CreateEntry(
            EntryDate,
            AvailabilityEntryKind.FixedDayOff);
        await store.SaveAsync(null, null, vacation, TestContext.Current.CancellationToken);

        AvailabilityEntryWriteStoreResult replacement = await store.SaveAsync(
            vacation,
            1,
            sickness,
            TestContext.Current.CancellationToken);
        AvailabilityEntryWriteStoreResult stale = await store.SaveAsync(
            vacation,
            1,
            fixedDayOff,
            TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryWriteStoreStatus.Succeeded, replacement.Status);
        Assert.Equal(2, replacement.ChangeVersion);
        Assert.Same(AvailabilityEntryWriteStoreResult.Conflict, stale);
        AvailabilityEntryReadItem item = Assert.Single(
            (await LoadWeekAsync(database.Path)).Entries);
        Assert.Equal(AvailabilityEntryKind.Sickness, item.Entry.Kind);
        Assert.Equal(2, item.ChangeVersion);
    }

    [Fact]
    public async Task RemoveOnlyDeletesMatchingCurrentVersion()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);
        AvailabilityEntry entry = CreateEntry(EntryDate, AvailabilityEntryKind.FixedDayOff);
        SqliteAvailabilityStore store = new(database.Path);
        await ((ISetAvailabilityEntryStore)store).SaveAsync(
            null,
            null,
            entry,
            TestContext.Current.CancellationToken);

        AvailabilityEntryRemoveStoreResult stale = await ((IRemoveAvailabilityEntryStore)store)
            .RemoveAsync(entry, 2, TestContext.Current.CancellationToken);
        AvailabilityEntryRemoveStoreResult removed = await ((IRemoveAvailabilityEntryStore)store)
            .RemoveAsync(entry, 1, TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryRemoveStoreResult.Conflict, stale);
        Assert.Equal(AvailabilityEntryRemoveStoreResult.Succeeded, removed);
        Assert.Empty((await LoadWeekAsync(database.Path)).Entries);
    }

    [Fact]
    public async Task DatabaseRejectsSecondCurrentEntryForEmployeeAndDate()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);
        string employeeId = DatabaseId(EmployeeIdentifier);

        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO AvailabilityEntries (EmployeeId, Date, Kind, ChangeVersion) "
            + $"VALUES ('{employeeId}', '2026-12-23', 0, 1);");

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO AvailabilityEntries (EmployeeId, Date, Kind, ChangeVersion) "
            + $"VALUES ('{employeeId}', '2026-12-23', 1, 1);"));
    }

    [Fact]
    public async Task DatabaseForeignKeyProtectsUnknownAndReferencedEmployee()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO AvailabilityEntries (EmployeeId, Date, Kind, ChangeVersion) "
            + "VALUES ('00000000-0000-0000-0000-000000000001', '2026-12-23', 0, 1);"));
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO AvailabilityEntries (EmployeeId, Date, Kind, ChangeVersion) "
            + $"VALUES ('{DatabaseId(EmployeeIdentifier)}', '2026-12-23', 0, 1);");
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            $"DELETE FROM Employees WHERE Id = '{DatabaseId(EmployeeIdentifier)}';"));
    }

    [Theory]
    [InlineData("Kind", "9")]
    [InlineData("ChangeVersion", "0")]
    [InlineData("ChangeVersion", "-1")]
    public async Task DatabaseRejectsInvalidKindAndChangeVersion(
        string column,
        string value)
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO AvailabilityEntries (EmployeeId, Date, Kind, ChangeVersion) "
            + $"VALUES ('{DatabaseId(EmployeeIdentifier)}', '2026-12-23', "
            + $"{(column == "Kind" ? value : "0")}, "
            + $"{(column == "ChangeVersion" ? value : "1")});"));
    }

    [Fact]
    public async Task LoadReturnsOnlyRequestedDatesButKeepsCompleteCatalogContext()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);
        ISetAvailabilityEntryStore store = new SqliteAvailabilityStore(database.Path);
        AvailabilityEntry inside = CreateEntry(
            new DateOnly(2026, 12, 23),
            AvailabilityEntryKind.Vacation);
        AvailabilityEntry outside = CreateEntry(
            new DateOnly(2026, 12, 30),
            AvailabilityEntryKind.Sickness);
        await store.SaveAsync(null, null, inside, TestContext.Current.CancellationToken);
        await store.SaveAsync(null, null, outside, TestContext.Current.CancellationToken);

        AvailabilityReadData result = await LoadWeekAsync(database.Path);

        Assert.Equal(inside.Date, Assert.Single(result.Entries).Entry.Date);
        Assert.Single(result.Employees);
        Assert.Equal(11, result.EmployeeTypes.Count);
    }

    [Fact]
    public async Task FailedReplacementLeavesPreviousEntryUnchanged()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeWithEmployeeAsync(database.Path);
        ISetAvailabilityEntryStore store = new SqliteAvailabilityStore(database.Path);
        AvailabilityEntry current = CreateEntry(EntryDate, AvailabilityEntryKind.Vacation);
        await store.SaveAsync(null, null, current, TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER RejectAvailabilityUpdate BEFORE UPDATE ON AvailabilityEntries "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic write failure'); END;");

        AvailabilityEntryWriteStoreResult result = await store.SaveAsync(
            current,
            1,
            CreateEntry(EntryDate, AvailabilityEntryKind.Sickness),
            TestContext.Current.CancellationToken);

        Assert.Same(AvailabilityEntryWriteStoreResult.Conflict, result);
        AvailabilityEntryReadItem reloaded = Assert.Single(
            (await LoadWeekAsync(database.Path)).Entries);
        Assert.Equal(AvailabilityEntryKind.Vacation, reloaded.Entry.Kind);
        Assert.Equal(1, reloaded.ChangeVersion);
    }

    private static async Task InitializeWithEmployeeAsync(string databasePath)
    {
        await InitializeAsync(databasePath);
        Employee employee = Assert.IsType<Employee>(Employee.Create(
            EmployeeIdentifier,
            "Erika",
            "Muster",
            InitialEmployeeTypeCatalog.Type25.Id.Value).Value);
        await ((ICreateEmployeeStore)new SqliteEmployeeStore(databasePath)).CreateAsync(
            employee,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
    }

    private static Task InitializeAsync(string databasePath)
    {
        return new SqliteServiceCatalogStore(databasePath).InitializeAsync(
            TestContext.Current.CancellationToken);
    }

    private static async Task MigrateAsync(string databasePath, string migration)
    {
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(databasePath).Create();
        await context.GetService<IMigrator>().MigrateAsync(
            migration,
            TestContext.Current.CancellationToken);
    }

    private static Task<AvailabilityReadData> LoadWeekAsync(string databasePath)
    {
        return ((IAvailabilityReader)new SqliteAvailabilityStore(databasePath)).LoadAsync(
            new DateOnly(2026, 12, 21),
            new DateOnly(2026, 12, 27),
            TestContext.Current.CancellationToken);
    }

    private static AvailabilityEntry CreateEntry(
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(EmployeeIdentifier, date, kind).Value);
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
        return new SqliteConnection(
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWrite,
                ForeignKeys = true,
                Pooling = false,
            }.ToString());
    }

    private static string DatabaseId(Guid id)
    {
        return id.ToString("D").ToUpperInvariant();
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
            Path = System.IO.Path.Combine(_directory, "availability.db");
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
