using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Tests.Employees;

public sealed class EmployeeTypePersistenceTests
{
    private const string System05Migration =
        "20260914185047_AddStandardDemandCorrectionSequence";

    [Fact]
    public async Task ExistingSystem05DatabaseWhenUpgradedKeepsSyntheticDataAndAddsTypesOnce()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateAsync(database.Path, System05Migration);
        Guid employeeId = new("46f60bb5-4813-4aaa-bd00-bac3ca686a49");
        Guid demandId = new("583c9f8f-c815-42c5-a785-ab0741075df2");
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE EmployeeTypes SET Name = 'Synthetischer Bestand', "
            + "WeeklyWorkTargetMinutes = 1560 "
            + $"WHERE Id = '{DatabaseId(InitialEmployeeTypeCatalog.Type25.Id.Value)}';"
            + "INSERT INTO Employees (Id, FirstName, LastName, EmployeeTypeId, IsActive) "
            + $"VALUES ('{DatabaseId(employeeId)}', 'Erika', 'Beispiel', "
            + $"'{DatabaseId(InitialEmployeeTypeCatalog.Type25.Id.Value)}', 1);"
            + "INSERT INTO StandardStaffingDemandRevisions "
            + "(Id, DayOfWeek, WorkLocationId, ShiftTypeId, EffectiveFromMonday, Kind, "
            + "ActualStartMinutes, ActualEndMinutes, RequiredEmployeeCount, CorrectionSequence) "
            + $"VALUES ('{DatabaseId(demandId)}', 1, "
            + $"'{DatabaseId(InitialWorkLocationCatalog.Restaurant.Id.Value)}', "
            + $"'{DatabaseId(InitialShiftTypeCatalog.EarlyShift.Id.Value)}', "
            + "'2026-09-14', 0, 480, 720, 1, 1);");

        SqliteServiceCatalogStore catalogStore = new(database.Path);
        await catalogStore.InitializeAsync(TestContext.Current.CancellationToken);
        await catalogStore.InitializeAsync(TestContext.Current.CancellationToken);
        EmployeeReadData reloaded = await new SqliteEmployeeStore(database.Path).LoadAsync(
            TestContext.Current.CancellationToken);

        Employee employee = Assert.Single(reloaded.Employees);
        Assert.Equal(employeeId, employee.Id.Value);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Id, employee.EmployeeTypeId);
        EmployeeType customizedType = Assert.Single(
            reloaded.EmployeeTypes,
            employeeType => employeeType.Id == InitialEmployeeTypeCatalog.Type25.Id);
        Assert.Equal("Synthetischer Bestand", customizedType.Name.Value);
        Assert.Equal(1560, customizedType.WeeklyWorkTarget.Minutes);
        Assert.Equal(300, customizedType.AbsencePolicy.DayValue?.Minutes);
        Assert.Equal(11, reloaded.EmployeeTypes.Count);
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            $"SELECT COUNT(*) FROM StandardStaffingDemandRevisions WHERE Id = '{DatabaseId(demandId)}';"));
        Assert.Equal(6L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));

        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(database.Path).Create();
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(
            TestContext.Current.CancellationToken));
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task CreateAsyncWhenStoreRestartsPreservesEveryEditableValue()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        EmployeeType expected = CreateNormalType(
            new Guid("22922046-f078-48c0-ad1a-e65bed4e7c88"),
            "TypNeu",
            "Neuer synthetischer Typ",
            1710,
            true,
            285,
            Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
            Relief(ShiftEligibilityActivation.ExplicitPlanningRunOption));

        EmployeeTypeWriteStoreResult result =
            await ((ICreateEmployeeTypeStore)new SqliteEmployeeStore(database.Path)).CreateAsync(
                expected,
                TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeWriteStoreResult.Succeeded, result);
        EmployeeType actual = Assert.Single(
            (await new SqliteEmployeeStore(database.Path).LoadAsync(
                TestContext.Current.CancellationToken)).EmployeeTypes,
            employeeType => employeeType.Id == expected.Id);
        AssertEmployeeType(expected, actual);
    }

    [Fact]
    public async Task CreateAsyncWhenCodeDiffersOnlyByCaseReturnsDuplicateWithoutPartialRows()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        ICreateEmployeeTypeStore store = new SqliteEmployeeStore(database.Path);
        EmployeeType first = CreateNormalType(
            new Guid("ee58a372-3204-481f-ae4e-02c9c293abf7"),
            "TypFall",
            "Erster Typ",
            1200,
            true,
            240,
            Shift(InitialShiftTypeCatalog.EarlyShift));
        EmployeeType duplicate = CreateNormalType(
            new Guid("8bf32cbd-6e87-485a-8e4b-667276246f46"),
            "typfall",
            "Zweiter Typ",
            1500,
            true,
            300,
            Shift(InitialShiftTypeCatalog.LateShift));

        Assert.Equal(
            EmployeeTypeWriteStoreResult.Succeeded,
            await store.CreateAsync(first, TestContext.Current.CancellationToken));
        Assert.Equal(
            EmployeeTypeWriteStoreResult.DuplicateCode,
            await store.CreateAsync(duplicate, TestContext.Current.CancellationToken));

        EmployeeReadData reloaded = await new SqliteEmployeeStore(database.Path).LoadAsync(
            TestContext.Current.CancellationToken);
        Assert.Contains(reloaded.EmployeeTypes, employeeType => employeeType.Id == first.Id);
        Assert.DoesNotContain(reloaded.EmployeeTypes, employeeType => employeeType.Id == duplicate.Id);
        Assert.Equal(1L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM EmployeeTypes WHERE Code = 'TypFall' COLLATE NOCASE;"));
    }

    [Fact]
    public async Task UpdateAsyncWhenStoreRestartsPreservesReplacementAndImmutableValues()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        EmployeeType original = CreateNormalType(
            new Guid("fc47f082-e66e-468d-b1bc-1694785c4c68"),
            "TypAenderung",
            "Vorher",
            1350,
            false,
            null,
            Shift(InitialShiftTypeCatalog.EarlyShift));
        await ((ICreateEmployeeTypeStore)store).CreateAsync(
            original,
            TestContext.Current.CancellationToken);
        EmployeeType replacement = Assert.IsType<EmployeeType>(
            original.WithDetails(
                "Nachher",
                1785,
                true,
                315,
                [
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Relief(ShiftEligibilityActivation.ExplicitPlanningRunOption),
                ]).Value);

        EmployeeTypeWriteStoreResult result = await ((IUpdateEmployeeTypeStore)store).UpdateAsync(
            original,
            replacement,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeWriteStoreResult.Succeeded, result);
        EmployeeType actual = Assert.Single(
            (await new SqliteEmployeeStore(database.Path).LoadAsync(
                TestContext.Current.CancellationToken)).EmployeeTypes,
            employeeType => employeeType.Id == original.Id);
        AssertEmployeeType(replacement, actual);
        Assert.Equal(original.Id, actual.Id);
        Assert.Equal(original.Code, actual.Code);
        Assert.Equal(EmployeeTypePlanningPolicy.Standard, actual.PlanningPolicy);
    }

    [Fact]
    public async Task UpdateAsyncWhenWriteFailsRollsBackDetailsAndEligibilities()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        EmployeeType original = CreateNormalType(
            new Guid("6fb09717-2b14-45e1-b639-05d581d210e3"),
            "TypRollback",
            "Unveraendert",
            1200,
            true,
            240,
            Shift(InitialShiftTypeCatalog.EarlyShift));
        await ((ICreateEmployeeTypeStore)store).CreateAsync(
            original,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER RejectSyntheticEligibility BEFORE INSERT "
            + "ON EmployeeTypeShiftEligibilities "
            + $"WHEN NEW.EmployeeTypeId = '{DatabaseId(original.Id.Value)}' "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic write failure'); END;");
        EmployeeType replacement = Assert.IsType<EmployeeType>(
            original.WithDetails(
                "Nicht gespeichert",
                1800,
                true,
                360,
                [Shift(InitialShiftTypeCatalog.LateShift)]).Value);

        EmployeeTypeWriteStoreResult result = await ((IUpdateEmployeeTypeStore)store).UpdateAsync(
            original,
            replacement,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeWriteStoreResult.Conflict, result);
        EmployeeType actual = Assert.Single(
            (await new SqliteEmployeeStore(database.Path).LoadAsync(
                TestContext.Current.CancellationToken)).EmployeeTypes,
            employeeType => employeeType.Id == original.Id);
        AssertEmployeeType(original, actual);
    }

    [Fact]
    public async Task UpdateAsyncWhenStoredAggregateChangedReturnsConflictWithoutOverwrite()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        EmployeeType stale = CreateNormalType(
            new Guid("e9f93d93-41ab-47ee-9016-6199aa1e8b94"),
            "TypKonflikt",
            "Alter Stand",
            1200,
            true,
            240,
            Shift(InitialShiftTypeCatalog.EarlyShift));
        await ((ICreateEmployeeTypeStore)store).CreateAsync(
            stale,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            $"UPDATE EmployeeTypes SET Name = 'Neuer Stand' WHERE Id = '{DatabaseId(stale.Id.Value)}';");
        EmployeeType replacement = Assert.IsType<EmployeeType>(
            stale.WithDetails("Veraltete Aenderung", 1500).Value);

        EmployeeTypeWriteStoreResult result = await ((IUpdateEmployeeTypeStore)store).UpdateAsync(
            stale,
            replacement,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeWriteStoreResult.Conflict, result);
        EmployeeType actual = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).EmployeeTypes,
            employeeType => employeeType.Id == stale.Id);
        Assert.Equal("Neuer Stand", actual.Name.Value);
        Assert.Equal(stale.WeeklyWorkTarget, actual.WeeklyWorkTarget);
    }

    [Fact]
    public async Task UpdateAsyncWhenStoredEligibilityChangedReturnsConflictWithoutOverwrite()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        EmployeeType stale = CreateNormalType(
            new Guid("1ce5b15f-1ce1-4af6-873d-b62e901d2570"),
            "TypFreigabekonflikt",
            "Alter Freigabestand",
            1200,
            true,
            240,
            Shift(InitialShiftTypeCatalog.EarlyShift));
        await ((ICreateEmployeeTypeStore)store).CreateAsync(
            stale,
            TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE EmployeeTypeShiftEligibilities SET Mode = 1 "
            + $"WHERE EmployeeTypeId = '{DatabaseId(stale.Id.Value)}';");
        EmployeeType replacement = Assert.IsType<EmployeeType>(
            stale.WithDetails("Veraltete Freigabeaenderung", 1500).Value);

        EmployeeTypeWriteStoreResult result = await ((IUpdateEmployeeTypeStore)store).UpdateAsync(
            stale,
            replacement,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeWriteStoreResult.Conflict, result);
        EmployeeType actual = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).EmployeeTypes,
            employeeType => employeeType.Id == stale.Id);
        Assert.Equal(stale.Name, actual.Name);
        Assert.Equal(ShiftEligibilityMode.ManualSuggestion, Assert.Single(actual.ShiftEligibilities).Mode);
    }

    [Fact]
    public async Task DeleteAsyncWhenReferencedReturnsReferencedAndKeepsCompleteType()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        EmployeeType employeeType = CreateNormalType(
            new Guid("895aa753-bfcc-47b8-ad95-9aa596f8924f"),
            "TypVerwendet",
            "Verwendeter Typ",
            1200,
            true,
            240,
            Shift(InitialShiftTypeCatalog.EarlyShift));
        await ((ICreateEmployeeTypeStore)store).CreateAsync(
            employeeType,
            TestContext.Current.CancellationToken);
        Employee employee = Assert.IsType<Employee>(Employee.Create(
            new Guid("e69371c2-b503-44bd-a0ed-e65cc24a1b55"),
            "Erika",
            "Beispiel",
            employeeType.Id.Value).Value);
        await ((ICreateEmployeeStore)store).CreateAsync(
            employee,
            InitialEmployeeTypeCatalog.Type1.Id,
            TestContext.Current.CancellationToken);

        EmployeeTypeDeleteStoreResult result = await ((IDeleteEmployeeTypeStore)store).DeleteAsync(
            employeeType,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeDeleteStoreResult.Referenced, result);
        EmployeeReadData reloaded = await store.LoadAsync(TestContext.Current.CancellationToken);
        AssertEmployeeType(
            employeeType,
            Assert.Single(reloaded.EmployeeTypes, value => value.Id == employeeType.Id));
        Assert.Equal(employee.Id, Assert.Single(reloaded.Employees).Id);
    }

    [Fact]
    public async Task DeleteAsyncWhenUnusedRemovesTypeAndEligibilitiesAtomically()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);
        SqliteEmployeeStore store = new(database.Path);
        EmployeeType employeeType = CreateNormalType(
            new Guid("31948ffb-2d4b-4cd0-8b21-8bbe84cf387b"),
            "TypLoeschbar",
            "Loeschbarer Typ",
            1200,
            true,
            240,
            Shift(InitialShiftTypeCatalog.EarlyShift),
            Relief(ShiftEligibilityActivation.Always));
        await ((ICreateEmployeeTypeStore)store).CreateAsync(
            employeeType,
            TestContext.Current.CancellationToken);

        EmployeeTypeDeleteStoreResult result = await ((IDeleteEmployeeTypeStore)store).DeleteAsync(
            employeeType,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeDeleteStoreResult.Succeeded, result);
        Assert.DoesNotContain(
            (await new SqliteEmployeeStore(database.Path).LoadAsync(
                TestContext.Current.CancellationToken)).EmployeeTypes,
            value => value.Id == employeeType.Id);
        Assert.Equal(0L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM EmployeeTypeShiftEligibilities "
            + $"WHERE EmployeeTypeId = '{DatabaseId(employeeType.Id.Value)}';"));
    }

    [Theory]
    [InlineData("AllowsVacationAndSickness = 1, AbsenceDayValueMinutes = NULL")]
    [InlineData("PlanningRole = 9")]
    public async Task DatabaseWhenEmployeeTypePolicyIsInvalidRejectsUpdate(string assignment)
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            $"UPDATE EmployeeTypes SET {assignment} "
            + $"WHERE Id = '{DatabaseId(InitialEmployeeTypeCatalog.Type25.Id.Value)}';"));
    }

    private static EmployeeType CreateNormalType(
        Guid id,
        string code,
        string name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        params EmployeeTypeShiftEligibility[] eligibilities)
    {
        return Assert.IsType<EmployeeType>(EmployeeType.Create(
            id,
            code,
            name,
            weeklyWorkTargetMinutes,
            allowsVacationAndSickness,
            absenceDayValueMinutes,
            eligibilities,
            EmployeeTypePlanningPolicy.Standard).Value);
    }

    private static EmployeeTypeShiftEligibility Shift(ShiftType shiftType)
    {
        return Assert.IsType<EmployeeTypeShiftEligibility>(
            EmployeeTypeShiftEligibility.CreateForShiftType(
                shiftType.Id.Value,
                ShiftEligibilityMode.Regular,
                InitialShiftTypeCatalog.All.Select(value => value.Id).ToArray()).Value);
    }

    private static EmployeeTypeShiftEligibility Relief(ShiftEligibilityActivation activation)
    {
        return Assert.IsType<EmployeeTypeShiftEligibility>(
            EmployeeTypeShiftEligibility.CreateForShiftPattern(
                InitialShiftPatternCatalog.ReliefShift.Id.Value,
                ShiftEligibilityMode.Regular,
                activation,
                InitialShiftPatternCatalog.All.Select(value => value.Id).ToArray()).Value);
    }

    private static void AssertEmployeeType(EmployeeType expected, EmployeeType actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Code, actual.Code);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.WeeklyWorkTarget, actual.WeeklyWorkTarget);
        Assert.Equal(expected.AbsencePolicy, actual.AbsencePolicy);
        Assert.Equal(expected.PlanningPolicy, actual.PlanningPolicy);
        Assert.Equal(expected.ShiftEligibilities, actual.ShiftEligibilities);
    }

    private static Task InitializeAsync(string databasePath)
    {
        return new SqliteServiceCatalogStore(databasePath).InitializeAsync(
            TestContext.Current.CancellationToken);
    }

    private static async Task MigrateAsync(string databasePath, string targetMigration)
    {
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(databasePath).Create();
        IMigrator migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(targetMigration, TestContext.Current.CancellationToken);
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
            Path = System.IO.Path.Combine(_directory, "employee-type-persistence.db");
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
