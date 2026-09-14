using Microsoft.Data.Sqlite;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Tests.Employees;

public sealed class SqliteEmployeeStoreTests
{
    [Fact]
    public async Task InitializeAsyncWhenRepeatedSeedsExactTypeCatalogWithoutEmployees()
    {
        using TemporarySqliteDatabase database = new();
        SqliteServiceCatalogStore catalogStore = new(database.Path);

        await catalogStore.InitializeAsync(TestContext.Current.CancellationToken);
        await catalogStore.InitializeAsync(TestContext.Current.CancellationToken);
        EmployeeReadData data = await new SqliteEmployeeStore(database.Path).LoadAsync(
            TestContext.Current.CancellationToken);

        Assert.Empty(data.Employees);
        Assert.Equal(8, data.EmployeeTypes.Count);
        Assert.Equal(8L, await ExecuteScalarAsync(database.Path, "SELECT COUNT(*) FROM EmployeeTypes;"));
        Assert.Equal(
            38L,
            await ExecuteScalarAsync(
                database.Path,
                "SELECT COUNT(*) FROM EmployeeTypeShiftEligibilities;"));
        Assert.Equal(4L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));

        foreach (EmployeeType expected in InitialEmployeeTypeCatalog.All)
        {
            EmployeeType actual = Assert.Single(
                data.EmployeeTypes,
                employeeType => employeeType.Id == expected.Id);
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.WeeklyWorkTarget, actual.WeeklyWorkTarget);
            Assert.Equal(expected.PlanningPolicy, actual.PlanningPolicy);
            Assert.Equal(expected.ShiftEligibilities, actual.ShiftEligibilities);
        }
    }

    [Fact]
    public async Task EmployeeWritesWhenStoreIsRecreatedSurviveCompleteRoundTrip()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore firstStore = new(database.Path);
        Employee original = CreateEmployee(
            new Guid("78e39edf-05a6-4e46-b478-af00823e459a"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id);

        Assert.Equal(
            EmployeeWriteStoreResult.Succeeded,
            await ((ICreateEmployeeStore)firstStore).CreateAsync(
                original,
                InitialEmployeeTypeCatalog.Type1.Id,
                TestContext.Current.CancellationToken));
        Employee renamed = Assert.IsType<Employee>(original.WithName("Maria", "Muster").Value);
        Assert.Equal(
            EmployeeWriteStoreResult.Succeeded,
            await ((IUpdateEmployeeNameStore)firstStore).UpdateNameAsync(
                original,
                renamed,
                TestContext.Current.CancellationToken));
        Employee changedType = Assert.IsType<Employee>(
            renamed.WithEmployeeType(InitialEmployeeTypeCatalog.Type30a.Id.Value).Value);
        Assert.Equal(
            EmployeeWriteStoreResult.Succeeded,
            await ((IChangeEmployeeTypeStore)firstStore).ChangeTypeAsync(
                renamed,
                changedType,
                InitialEmployeeTypeCatalog.Type1.Id,
                TestContext.Current.CancellationToken));
        Employee deactivated = changedType.Deactivate();
        Assert.Equal(
            EmployeeWriteStoreResult.Succeeded,
            await ((IDeactivateEmployeeStore)firstStore).DeactivateAsync(
                changedType,
                deactivated,
                TestContext.Current.CancellationToken));

        EmployeeReadData reloaded = await new SqliteEmployeeStore(database.Path).LoadAsync(
            TestContext.Current.CancellationToken);
        Employee actual = Assert.Single(reloaded.Employees);
        Assert.Equal(deactivated.Id, actual.Id);
        Assert.Equal(deactivated.FirstName, actual.FirstName);
        Assert.Equal(deactivated.LastName, actual.LastName);
        Assert.Equal(deactivated.EmployeeTypeId, actual.EmployeeTypeId);
        Assert.False(actual.IsActive);
    }

    [Fact]
    public async Task CreateAsyncWhenSecondActiveType1IsRequestedReturnsConflictWithoutSecondRow()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        ICreateEmployeeStore createStore = store;
        Employee first = CreateEmployee(
            new Guid("d87fb912-1724-4e93-9b64-45fbaf4ce8b2"),
            "Erste",
            "Leitung",
            InitialEmployeeTypeCatalog.Type1.Id);
        Employee second = CreateEmployee(
            new Guid("cd61fe13-ac76-49f7-bf9e-bf2d55bfbcfa"),
            "Zweite",
            "Leitung",
            InitialEmployeeTypeCatalog.Type1.Id);

        Assert.Equal(
            EmployeeWriteStoreResult.Succeeded,
            await createStore.CreateAsync(
                first,
                InitialEmployeeTypeCatalog.Type1.Id,
                TestContext.Current.CancellationToken));
        Assert.Equal(
            EmployeeWriteStoreResult.ActiveType1Conflict,
            await createStore.CreateAsync(
                second,
                InitialEmployeeTypeCatalog.Type1.Id,
                TestContext.Current.CancellationToken));

        EmployeeReadData data = await store.LoadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(first.Id, Assert.Single(data.Employees).Id);
    }

    [Fact]
    public async Task ChangeTypeAsyncWhenActiveType1ExistsReturnsConflictAndKeepsOriginalType()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        ICreateEmployeeStore createStore = store;
        Employee serviceManagement = CreateEmployee(
            new Guid("fc9973dd-20fe-4405-919f-79d53d222383"),
            "Erste",
            "Leitung",
            InitialEmployeeTypeCatalog.Type1.Id);
        Employee restaurantEmployee = CreateEmployee(
            new Guid("cdbf1474-81ee-4ea8-8bce-1a1edfb5c0bf"),
            "Restaurant",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id);
        await createStore.CreateAsync(
            serviceManagement,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        await createStore.CreateAsync(
            restaurantEmployee,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        Employee replacement = Assert.IsType<Employee>(
            restaurantEmployee.WithEmployeeType(InitialEmployeeTypeCatalog.Type1.Id.Value).Value);

        EmployeeWriteStoreResult result = await ((IChangeEmployeeTypeStore)store).ChangeTypeAsync(
            restaurantEmployee,
            replacement,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeWriteStoreResult.ActiveType1Conflict, result);
        EmployeeReadData reloaded = await store.LoadAsync(TestContext.Current.CancellationToken);
        Employee unchanged = Assert.Single(
            reloaded.Employees,
            employee => employee.Id == restaurantEmployee.Id);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Id, unchanged.EmployeeTypeId);
    }

    [Fact]
    public async Task ReactivateAsyncWhenNoType1ConflictPersistsActiveStateAcrossStoreRestart()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee inactive = CreateEmployee(
                new Guid("db5f5ddc-fe2e-4d07-bd51-203bd835858a"),
                "Reaktivierte",
                "Person",
                InitialEmployeeTypeCatalog.Type30.Id)
            .Deactivate();
        await ((ICreateEmployeeStore)store).CreateAsync(
            inactive,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        EmployeeWriteStoreResult result = await ((IReactivateEmployeeStore)store).ReactivateAsync(
            inactive,
            inactive.Reactivate(),
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeWriteStoreResult.Succeeded, result);
        Employee actual = Assert.Single(
            (await new SqliteEmployeeStore(database.Path).LoadAsync(
                TestContext.Current.CancellationToken)).Employees);
        Assert.Equal(inactive.Id, actual.Id);
        Assert.Equal(inactive.FirstName, actual.FirstName);
        Assert.Equal(inactive.LastName, actual.LastName);
        Assert.Equal(inactive.EmployeeTypeId, actual.EmployeeTypeId);
        Assert.True(actual.IsActive);
    }

    [Fact]
    public async Task ReactivateAsyncWhenAnotherActiveType1ExistsReturnsConflictAndRemainsInactive()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        ICreateEmployeeStore createStore = store;
        Employee active = CreateEmployee(
            new Guid("b68f8a50-2826-40ab-8b8f-258057b66a4f"),
            "Aktive",
            "Leitung",
            InitialEmployeeTypeCatalog.Type1.Id);
        Employee inactive = CreateEmployee(
                new Guid("aa26ed40-491e-48a5-8092-f60acc89ee50"),
                "Frühere",
                "Leitung",
                InitialEmployeeTypeCatalog.Type1.Id)
            .Deactivate();
        await createStore.CreateAsync(
            active,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        await createStore.CreateAsync(
            inactive,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        EmployeeWriteStoreResult result = await ((IReactivateEmployeeStore)store).ReactivateAsync(
            inactive,
            inactive.Reactivate(),
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeWriteStoreResult.ActiveType1Conflict, result);
        Employee unchanged = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).Employees,
            employee => employee.Id == inactive.Id);
        Assert.False(unchanged.IsActive);
    }

    [Fact]
    public async Task DeleteAsyncWhenInactiveEmployeeIsUnusedRemovesRowAcrossStoreRestart()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee inactive = CreateEmployee(
                new Guid("55033687-ac74-4de8-b585-91609a0c11d2"),
                "Löschbare",
                "Person",
                InitialEmployeeTypeCatalog.Type35.Id)
            .Deactivate();
        await ((ICreateEmployeeStore)store).CreateAsync(
            inactive,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        EmployeeDeleteStoreResult result = await ((IDeleteEmployeeStore)store).DeleteAsync(
            inactive,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeDeleteStoreResult.Succeeded, result);
        Assert.Empty(
            (await new SqliteEmployeeStore(database.Path).LoadAsync(
                TestContext.Current.CancellationToken)).Employees);
    }

    [Fact]
    public async Task DeleteAsyncWhenInactiveEmployeeIsReferencedReturnsReferencedAndKeepsRow()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee inactive = CreateEmployee(
                new Guid("906594b6-40ee-4752-af40-b7ec4f5a3ccf"),
                "Verwendete",
                "Person",
                InitialEmployeeTypeCatalog.TypeAh1.Id)
            .Deactivate();
        await ((ICreateEmployeeStore)store).CreateAsync(
            inactive,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TABLE SyntheticEmployeeReferences ("
            + "Id INTEGER NOT NULL PRIMARY KEY, "
            + "EmployeeId TEXT NOT NULL, "
            + "FOREIGN KEY (EmployeeId) REFERENCES Employees (Id) ON DELETE RESTRICT);"
            + "INSERT INTO SyntheticEmployeeReferences (Id, EmployeeId) "
            + "SELECT 1, Id FROM Employees;");

        EmployeeDeleteStoreResult result = await ((IDeleteEmployeeStore)store).DeleteAsync(
            inactive,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeDeleteStoreResult.Referenced, result);
        Employee unchanged = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).Employees);
        Assert.Equal(inactive.Id, unchanged.Id);
        Assert.False(unchanged.IsActive);
    }

    [Fact]
    public async Task DeleteAsyncWhenStoredEmployeeChangedReturnsConflictWithoutDeleting()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee stale = CreateEmployee(
                new Guid("4a1e9f7c-35cf-46b2-a508-e52a34088a37"),
                "Alter",
                "Stand",
                InitialEmployeeTypeCatalog.TypeAh2.Id)
            .Deactivate();
        await ((ICreateEmployeeStore)store).CreateAsync(
            stale,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE Employees SET FirstName = 'Neuer' "
            + $"WHERE lower(Id) = lower('{stale.Id.Value:D}');");

        EmployeeDeleteStoreResult result = await ((IDeleteEmployeeStore)store).DeleteAsync(
            stale,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeDeleteStoreResult.Conflict, result);
        Assert.Single((await store.LoadAsync(TestContext.Current.CancellationToken)).Employees);
    }

    [Fact]
    public async Task UpdateNameAsyncWhenStoredEmployeeChangedReturnsConflictWithoutOverwrite()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee stale = CreateEmployee(
            new Guid("0909fb86-f473-42a0-9075-5a2d40d69118"),
            "Alter",
            "Stand",
            InitialEmployeeTypeCatalog.Type30.Id);
        await ((ICreateEmployeeStore)store).CreateAsync(
            stale,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE Employees SET FirstName = 'Neuer' "
            + $"WHERE lower(Id) = lower('{stale.Id.Value:D}');");
        Employee replacement = Assert.IsType<Employee>(stale.WithName("Veraltet", "Stand").Value);

        EmployeeWriteStoreResult result = await ((IUpdateEmployeeNameStore)store).UpdateNameAsync(
            stale,
            replacement,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeWriteStoreResult.Conflict, result);
        Employee actual = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).Employees);
        Assert.Equal("Neuer", actual.FirstName.Value);
    }

    [Fact]
    public async Task FailedUpdateLeavesStoredEmployeeCompletelyUnchanged()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee original = CreateEmployee(
            new Guid("5af3b173-0ae7-47ec-9306-cd2171489600"),
            "Unverändert",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type35.Id);
        await ((ICreateEmployeeStore)store).CreateAsync(
            original,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER RejectEmployeeUpdate BEFORE UPDATE ON Employees "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic write failure'); END;");
        Employee replacement = Assert.IsType<Employee>(
            original.WithEmployeeType(InitialEmployeeTypeCatalog.Type35a.Id.Value).Value);

        Assert.Equal(
            EmployeeWriteStoreResult.Conflict,
            await ((IChangeEmployeeTypeStore)store).ChangeTypeAsync(
                original,
                replacement,
                InitialEmployeeTypeCatalog.Type1.Id,
                TestContext.Current.CancellationToken));

        Employee actual = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).Employees);
        Assert.Equal(original.FirstName, actual.FirstName);
        Assert.Equal(original.LastName, actual.LastName);
        Assert.Equal(original.EmployeeTypeId, actual.EmployeeTypeId);
        Assert.Equal(original.IsActive, actual.IsActive);
    }

    [Theory]
    [InlineData("ShiftTypeId", 0)]
    [InlineData("ShiftPatternId", 1)]
    public async Task DatabaseWhenEligibilityReferencesUnknownCatalogEntryRejectsInsert(
        string targetColumn,
        int targetKind)
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        Guid unknownTargetId = new("eb67d3e3-941b-494e-acd8-47e19ea1cc4b");
        string shiftTypeValue = targetColumn == "ShiftTypeId"
            ? $"'{unknownTargetId:D}'"
            : "NULL";
        string shiftPatternValue = targetColumn == "ShiftPatternId"
            ? $"'{unknownTargetId:D}'"
            : "NULL";

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO EmployeeTypeShiftEligibilities "
            + "(Id, EmployeeTypeId, TargetKind, ShiftTypeId, ShiftPatternId, Mode, Activation) "
            + $"VALUES (999, '{InitialEmployeeTypeCatalog.Type25.Id.Value:D}', {targetKind}, "
            + $"{shiftTypeValue}, {shiftPatternValue}, 0, 0);"));
    }

    [Fact]
    public async Task CreateAsyncWhenEmployeeTypeReferenceIsUnknownReturnsConflictWithoutRow()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee employee = CreateEmployee(
            new Guid("3a57b8d5-27c0-45e2-9dcf-6822cf966350"),
            "Unbekannter",
            "Typ",
            CreateEmployeeTypeId(new Guid("7ed79a32-bac2-4e88-9474-2810d12d5261")));

        EmployeeWriteStoreResult result = await ((ICreateEmployeeStore)store).CreateAsync(
            employee,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeWriteStoreResult.Conflict, result);
        Assert.Empty((await store.LoadAsync(TestContext.Current.CancellationToken)).Employees);
    }

    [Fact]
    public async Task DatabaseWhenUsedEmployeeTypeIsDeletedRejectsDelete()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        Employee employee = CreateEmployee(
            new Guid("64194214-643e-43a8-887c-ab1aad87d16e"),
            "Verwendeter",
            "Typ",
            InitialEmployeeTypeCatalog.TypeAh1.Id);
        await ((ICreateEmployeeStore)store).CreateAsync(
            employee,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "DELETE FROM EmployeeTypes "
            + $"WHERE lower(Id) = lower('{employee.EmployeeTypeId.Value:D}');"));

        Assert.Single((await store.LoadAsync(TestContext.Current.CancellationToken)).Employees);
    }

    private static Task InitializeAsync(string databasePath)
    {
        return new SqliteServiceCatalogStore(databasePath).InitializeAsync(
            TestContext.Current.CancellationToken);
    }

    private static Employee CreateEmployee(
        Guid id,
        string firstName,
        string lastName,
        EmployeeTypeId employeeTypeId)
    {
        return Assert.IsType<Employee>(
            Employee.Create(id, firstName, lastName, employeeTypeId.Value).Value);
    }

    private static EmployeeTypeId CreateEmployeeTypeId(Guid id)
    {
        Assert.True(EmployeeTypeId.TryCreate(id, out EmployeeTypeId? employeeTypeId));
        return employeeTypeId;
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
            Path = System.IO.Path.Combine(_directory, "employees.db");
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }
    }
}
