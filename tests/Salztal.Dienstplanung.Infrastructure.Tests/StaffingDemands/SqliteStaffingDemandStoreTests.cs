using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;
using Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

namespace Salztal.Dienstplanung.Infrastructure.Tests.StaffingDemands;

public sealed class SqliteStaffingDemandStoreTests
{
    private const string InitialServiceCatalogMigration =
        "20260913202156_InitialServiceCatalog";
    private const string EmployeeMigration =
        "20260914005039_AddEmployeesAndEmployeeTypes";
    private const string StaffingDemandMigration =
        "20260914144923_AddStaffingDemands";

    [Fact]
    public async Task InitializeAsyncOnEmptyDatabaseCreatesInitialStandardsExactlyOnce()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore store = new(database.Path);

        await store.InitializeAsync(TestContext.Current.CancellationToken);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        StaffingDemandReadData data = await store.LoadAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(23, data.StandardRevisions.Count);
        Assert.Empty(data.DateExceptions);
        Assert.Equal(7L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
        Assert.Equal(23L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM StandardStaffingDemandRevisions;"));

        StandardStaffingDemandRevision cafeteriaMonday = Assert.Single(
            data.StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Monday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id);
        Assert.Equal(new TimeOnly(13, 30), cafeteriaMonday.ActualTime?.Start);
        Assert.Equal(new TimeOnly(20, 30), cafeteriaMonday.ActualTime?.End);
        Assert.Equal(1, cafeteriaMonday.RequiredEmployeeCount?.Value);
        Assert.Equal(420, cafeteriaMonday.RequiredWorkMinutes);

        StandardStaffingDemandRevision cafeteriaSaturdaySecond = Assert.Single(
            data.StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Saturday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftB.Id);
        Assert.Equal(new TimeOnly(13, 30), cafeteriaSaturdaySecond.ActualTime?.Start);
        Assert.Equal(new TimeOnly(17, 30), cafeteriaSaturdaySecond.ActualTime?.End);
        Assert.Equal(240, cafeteriaSaturdaySecond.RequiredWorkMinutes);
    }

    [Fact]
    public async Task InitializeAsyncWhenUpgradingSystem03UsesCurrentShiftTimesAndKeepsExistingData()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateAsync(database.Path, EmployeeMigration);
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE ShiftTypes SET StandardStartMinutes = 840, StandardEndMinutes = 1260 "
            + $"WHERE lower(Id) = lower('{InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value:D}');");
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE WorkLocations SET Name = 'Synthetischer Altbestand' "
            + $"WHERE lower(Id) = lower('{InitialWorkLocationCatalog.Cafeteria.Id.Value:D}');");
        await ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO Employees (Id, FirstName, LastName, EmployeeTypeId, IsActive) "
            + "SELECT '20000000-0000-4000-8000-000000000001', 'Erika', 'Beispiel', Id, 1 "
            + "FROM EmployeeTypes ORDER BY Code LIMIT 1;");

        SqliteStaffingDemandStore store = new(database.Path);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        StaffingDemandReadData data = await store.LoadAsync(
            TestContext.Current.CancellationToken);

        StandardStaffingDemandRevision currentTimeStandard = Assert.Single(
            data.StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Monday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id);
        Assert.Equal(new TimeOnly(14, 0), currentTimeStandard.ActualTime?.Start);
        Assert.Equal(new TimeOnly(21, 0), currentTimeStandard.ActualTime?.End);
        Assert.Contains(
            data.ServiceCatalog.WorkLocations,
            location => location.Name.Value == "Synthetischer Altbestand");
        Assert.Equal(1L, await ExecuteScalarAsync(database.Path, "SELECT COUNT(*) FROM Employees;"));
        Assert.Equal(23L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM StandardStaffingDemandRevisions;"));
    }

    [Fact]
    public async Task InitializeAsyncWhenUpgradingOldestMigrationPreservesCatalogAndCompletesSchema()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateAsync(database.Path, InitialServiceCatalogMigration);
        await ExecuteNonQueryAsync(
            database.Path,
            "UPDATE ShiftTypes SET StandardStartMinutes = 420, StandardEndMinutes = 840 "
            + $"WHERE lower(Id) = lower('{InitialShiftTypeCatalog.EarlyShift.Id.Value:D}');");

        SqliteStaffingDemandStore store = new(database.Path);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        StaffingDemandReadData data = await store.LoadAsync(
            TestContext.Current.CancellationToken);

        StandardStaffingDemandRevision earlyMonday = Assert.Single(
            data.StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Monday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id);
        Assert.Equal(new TimeOnly(7, 0), earlyMonday.ActualTime?.Start);
        Assert.Equal(new TimeOnly(14, 0), earlyMonday.ActualTime?.End);
        Assert.Equal(11L, await ExecuteScalarAsync(database.Path, "SELECT COUNT(*) FROM EmployeeTypes;"));
        Assert.Equal(7L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
    }

    [Fact]
    public async Task InitializeAsyncWhenUpgradingStaffingDemandMigrationPreservesRevisionAsSequenceOne()
    {
        using TemporarySqliteDatabase database = new();
        await MigrateAsync(database.Path, StaffingDemandMigration);
        await InsertSyntheticStandardAsync(
            database.Path,
            "29000000-0000-4000-8000-000000000001",
            "2026-09-14",
            840,
            1200,
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);

        SqliteStaffingDemandStore store = new(database.Path);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        StandardStaffingDemandRevision revision = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken))
                .StandardRevisions);

        Assert.Equal(1, revision.CorrectionSequence);
        Assert.Equal(new TimeOnly(14, 0), revision.ActualTime?.Start);
        Assert.Equal(7L, await ExecuteScalarAsync(
            database.Path,
            "SELECT COUNT(*) FROM __EFMigrationsHistory;"));
    }

    [Fact]
    public async Task AppendAsyncWhenStoreIsRecreatedRoundTripsRevisionValues()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore firstStore = await InitializeAsync(database.Path);
        StaffingDemandReadData before = await firstStore.LoadAsync(
            TestContext.Current.CancellationToken);
        StandardStaffingDemandRevision previous = Assert.Single(
            before.StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Monday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id);
        StandardStaffingDemandRevision replacement = CreateStandardReplacement(
            previous,
            new Guid("30000000-0000-4000-8000-000000000001"),
            new DateOnly(2026, 9, 21),
            new TimeOnly(14, 0),
            new TimeOnly(19, 30),
            2);

        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await firstStore.AppendAsync(
                previous,
                replacement,
                TestContext.Current.CancellationToken));

        StaffingDemandReadData reloaded = await new SqliteStaffingDemandStore(database.Path)
            .LoadAsync(TestContext.Current.CancellationToken);
        StandardStaffingDemandRevision actual = Assert.Single(
            reloaded.StandardRevisions,
            revision => revision.Id == replacement.Id);
        Assert.Equal(replacement.Key, actual.Key);
        Assert.Equal(new DateOnly(2026, 9, 21), actual.EffectiveFromMonday);
        Assert.Equal(new TimeOnly(14, 0), actual.ActualTime?.Start);
        Assert.Equal(new TimeOnly(19, 30), actual.ActualTime?.End);
        Assert.Equal(330, actual.DurationMinutes);
        Assert.Equal(660, actual.RequiredWorkMinutes);
    }

    [Fact]
    public async Task AppendAsyncUsesLatestFullSnapshotAndPersistsRemovalRevision()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore store = await InitializeAsync(database.Path);
        StandardStaffingDemandRevision initial = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Tuesday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id);
        StandardStaffingDemandRevision replacement = CreateStandardReplacement(
            initial,
            new Guid("31000000-0000-4000-8000-000000000001"),
            new DateOnly(2026, 9, 21),
            new TimeOnly(14, 0),
            new TimeOnly(19, 0),
            2);
        StandardStaffingDemandRevision staleCandidate = CreateStandardReplacement(
            initial,
            new Guid("31000000-0000-4000-8000-000000000002"),
            new DateOnly(2026, 9, 28),
            new TimeOnly(14, 30),
            new TimeOnly(19, 30),
            2);
        StandardStaffingDemandRevision removal = Assert.IsType<StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateRemoval(
                new Guid("31000000-0000-4000-8000-000000000003"),
                replacement.Key.DayOfWeek,
                replacement.Key.WorkLocationId.Value,
                replacement.Key.ShiftTypeId.Value,
                new DateOnly(2026, 9, 28)).Value);

        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.AppendAsync(initial, replacement, TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.Conflict,
            await store.AppendAsync(initial, staleCandidate, TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.AppendAsync(replacement, removal, TestContext.Current.CancellationToken));

        StaffingDemandReadData reloaded = await store.LoadAsync(
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain(reloaded.StandardRevisions, revision => revision.Id == staleCandidate.Id);
        StandardStaffingDemandRevision actualRemoval = Assert.Single(
            reloaded.StandardRevisions,
            revision => revision.Id == removal.Id);
        Assert.Equal(StandardStaffingDemandRevisionKind.Remove, actualRemoval.Kind);
        Assert.Null(actualRemoval.ActualTime);
        Assert.Null(actualRemoval.RequiredEmployeeCount);
        Assert.Null(actualRemoval.RequiredWorkMinutes);
    }

    [Fact]
    public async Task AppendAsyncRepeatedlyForSameMondayPersistsNextCorrectionSequence()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore store = await InitializeAsync(database.Path);
        StandardStaffingDemandRevision initial = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).StandardRevisions,
            revision => revision.Key.DayOfWeek == DayOfWeek.Monday
                && revision.Key.ShiftTypeId == InitialShiftTypeCatalog.CafeteriaShiftA.Id);
        DateOnly effectiveMonday = new(2026, 9, 21);
        StandardStaffingDemandRevision firstCorrection = CreateStandardReplacement(
            initial,
            new Guid("32000000-0000-4000-8000-000000000001"),
            effectiveMonday,
            new TimeOnly(14, 0),
            new TimeOnly(20, 0),
            2,
            correctionSequence: 1);
        StandardStaffingDemandRevision secondCorrection = CreateStandardReplacement(
            initial,
            new Guid("32000000-0000-4000-8000-000000000002"),
            effectiveMonday,
            new TimeOnly(14, 30),
            new TimeOnly(19, 30),
            3,
            correctionSequence: 2);

        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.AppendAsync(
                initial,
                firstCorrection,
                TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.AppendAsync(
                firstCorrection,
                secondCorrection,
                TestContext.Current.CancellationToken));

        StaffingDemandReadData reloaded = await store.LoadAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal(
            [1, 2],
            reloaded.StandardRevisions
                .Where(revision => revision.Key == initial.Key)
                .Where(revision => revision.EffectiveFromMonday == effectiveMonday)
                .Select(revision => revision.CorrectionSequence)
                .ToArray());
        StandardStaffingDemandRevisionSet revisionSet = Assert.IsType<
            StandardStaffingDemandRevisionSet>(
            StandardStaffingDemandRevisionSet.Create(reloaded.StandardRevisions).Value);
        Assert.Equal(
            secondCorrection.Id,
            revisionSet.FindEffectiveRevision(initial.Key, effectiveMonday)!.Id);
    }

    [Fact]
    public async Task DateExceptionSaveReplaceAndRemoveRoundTripAllKinds()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore store = await InitializeAsync(database.Path);
        StaffingDemandDateException addition = CreateDateAddition(
            new Guid("40000000-0000-4000-8000-000000000001"),
            new DateOnly(2026, 10, 3),
            new TimeOnly(12, 30),
            new TimeOnly(18, 0),
            2);
        StaffingDemandDateException replacement = CreateDateReplacement(
            new Guid("40000000-0000-4000-8000-000000000002"),
            addition.Key.Date,
            new TimeOnly(13, 0),
            new TimeOnly(17, 0),
            3);
        StaffingDemandDateException removalException =
            Assert.IsType<StaffingDemandDateException>(
                StaffingDemandDateException.CreateRemoval(
                    new Guid("40000000-0000-4000-8000-000000000003"),
                    addition.Key.Date.AddDays(1),
                    InitialWorkLocationCatalog.Cafeteria.Id.Value,
                    InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value).Value);

        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.SaveAsync(null, addition, TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.SaveAsync(addition, replacement, TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.SaveAsync(
                null,
                removalException,
                TestContext.Current.CancellationToken));

        IReadOnlyList<StaffingDemandDateException> reloadedExceptions =
            (await store.LoadAsync(TestContext.Current.CancellationToken)).DateExceptions;
        StaffingDemandDateException reloaded = Assert.Single(
            reloadedExceptions,
            exception => exception.Key == replacement.Key);
        Assert.Equal(replacement.Id, reloaded.Id);
        Assert.Equal(240, reloaded.DurationMinutes);
        Assert.Equal(720, reloaded.RequiredWorkMinutes);
        StaffingDemandDateException reloadedRemoval = Assert.Single(
            reloadedExceptions,
            exception => exception.Key == removalException.Key);
        Assert.Equal(StaffingDemandDateExceptionKind.Remove, reloadedRemoval.Kind);
        Assert.Null(reloadedRemoval.ActualTime);
        Assert.Null(reloadedRemoval.RequiredWorkMinutes);
        Assert.Equal(
            StaffingDemandWriteStoreResult.Succeeded,
            await store.RemoveAsync(reloaded, TestContext.Current.CancellationToken));
        Assert.Equal(
            removalException.Id,
            Assert.Single(
                (await new SqliteStaffingDemandStore(database.Path)
                    .LoadAsync(TestContext.Current.CancellationToken)).DateExceptions).Id);
    }

    [Fact]
    public async Task WritesWithStaleOrMissingExpectedSnapshotsDoNotOverwriteStoredValues()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore store = await InitializeAsync(database.Path);
        StaffingDemandDateException current = CreateDateAddition(
            new Guid("50000000-0000-4000-8000-000000000001"),
            new DateOnly(2026, 10, 4),
            new TimeOnly(13, 30),
            new TimeOnly(20, 30),
            1);
        await store.SaveAsync(null, current, TestContext.Current.CancellationToken);
        StaffingDemandDateException stale = CreateDateAddition(
            new Guid("50000000-0000-4000-8000-000000000002"),
            current.Key.Date,
            new TimeOnly(13, 30),
            new TimeOnly(20, 30),
            1);
        StaffingDemandDateException candidate = CreateDateReplacement(
            new Guid("50000000-0000-4000-8000-000000000003"),
            current.Key.Date,
            new TimeOnly(14, 0),
            new TimeOnly(18, 0),
            2);
        StaffingDemandDateException missing = CreateDateAddition(
            new Guid("50000000-0000-4000-8000-000000000004"),
            current.Key.Date.AddDays(1),
            new TimeOnly(13, 30),
            new TimeOnly(20, 30),
            1);

        Assert.Equal(
            StaffingDemandWriteStoreResult.Conflict,
            await store.SaveAsync(stale, candidate, TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.Conflict,
            await store.RemoveAsync(stale, TestContext.Current.CancellationToken));
        Assert.Equal(
            StaffingDemandWriteStoreResult.NotFound,
            await store.RemoveAsync(missing, TestContext.Current.CancellationToken));
        Assert.Equal(
            current.Id,
            Assert.Single(
                (await store.LoadAsync(TestContext.Current.CancellationToken)).DateExceptions).Id);
    }

    [Fact]
    public async Task FailedDateExceptionReplacementRollsBackRemovedOriginal()
    {
        using TemporarySqliteDatabase database = new();
        SqliteStaffingDemandStore store = await InitializeAsync(database.Path);
        StaffingDemandDateException original = CreateDateAddition(
            new Guid("60000000-0000-4000-8000-000000000001"),
            new DateOnly(2026, 10, 10),
            new TimeOnly(13, 30),
            new TimeOnly(17, 30),
            1);
        StaffingDemandDateException replacement = CreateDateReplacement(
            new Guid("60000000-0000-4000-8000-000000000002"),
            original.Key.Date,
            new TimeOnly(14, 0),
            new TimeOnly(18, 0),
            2);
        await store.SaveAsync(null, original, TestContext.Current.CancellationToken);
        await ExecuteNonQueryAsync(
            database.Path,
            "CREATE TRIGGER RejectSyntheticReplacement BEFORE INSERT "
            + "ON StaffingDemandDateExceptions "
            + $"WHEN lower(NEW.Id) = lower('{replacement.Id.Value:D}') "
            + "BEGIN SELECT RAISE(ABORT, 'synthetic rejection'); END;");

        Assert.Equal(
            StaffingDemandWriteStoreResult.Conflict,
            await store.SaveAsync(original, replacement, TestContext.Current.CancellationToken));

        StaffingDemandDateException remaining = Assert.Single(
            (await store.LoadAsync(TestContext.Current.CancellationToken)).DateExceptions);
        Assert.Equal(original.Id, remaining.Id);
        Assert.Equal(original.RequiredWorkMinutes, remaining.RequiredWorkMinutes);
    }

    [Fact]
    public async Task DatabaseRejectsDuplicateInvalidTimeInvalidMondayAndMissingRelationships()
    {
        using TemporarySqliteDatabase database = new();
        await InitializeAsync(database.Path);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO StandardStaffingDemandRevisions "
            + "(Id, DayOfWeek, WorkLocationId, ShiftTypeId, EffectiveFromMonday, Kind, "
            + "ActualStartMinutes, ActualEndMinutes, RequiredEmployeeCount) "
            + "SELECT '70000000-0000-4000-8000-000000000001', DayOfWeek, WorkLocationId, "
            + "ShiftTypeId, EffectiveFromMonday, Kind, ActualStartMinutes, ActualEndMinutes, "
            + "RequiredEmployeeCount FROM StandardStaffingDemandRevisions LIMIT 1;"));
        await Assert.ThrowsAsync<SqliteException>(() => InsertSyntheticStandardAsync(
            database.Path,
            "70000000-0000-4000-8000-000000000002",
            "2026-09-21",
            811,
            1230,
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteNonQueryAsync(
            database.Path,
            "INSERT INTO StandardStaffingDemandRevisions "
            + "(Id, DayOfWeek, WorkLocationId, ShiftTypeId, EffectiveFromMonday, "
            + "CorrectionSequence, Kind, ActualStartMinutes, ActualEndMinutes, "
            + "RequiredEmployeeCount) VALUES "
            + "('70000000-0000-4000-8000-000000000005', 1, "
            + $"'{InitialWorkLocationCatalog.Cafeteria.Id.Value:D}', "
            + $"'{InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value:D}', "
            + "'2026-09-21', 0, 0, 810, 1230, 1);"));
        await Assert.ThrowsAsync<SqliteException>(() => InsertSyntheticStandardAsync(
            database.Path,
            "70000000-0000-4000-8000-000000000003",
            "2026-09-22",
            810,
            1230,
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value));
        await Assert.ThrowsAsync<SqliteException>(() => InsertSyntheticStandardAsync(
            database.Path,
            "70000000-0000-4000-8000-000000000004",
            "2026-09-21",
            810,
            1230,
            new Guid("70000000-0000-4000-8000-000000000099"),
            InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value));
    }

    private static StandardStaffingDemandRevision CreateStandardReplacement(
        StandardStaffingDemandRevision previous,
        Guid id,
        DateOnly effectiveMonday,
        TimeOnly start,
        TimeOnly end,
        int count,
        int correctionSequence = 1)
    {
        return Assert.IsType<StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateReplacement(
                id,
                previous.Key.DayOfWeek,
                previous.Key.WorkLocationId.Value,
                previous.Key.ShiftTypeId.Value,
                effectiveMonday,
                start,
                end,
                count,
                correctionSequence).Value);
    }

    private static StaffingDemandDateException CreateDateAddition(
        Guid id,
        DateOnly date,
        TimeOnly start,
        TimeOnly end,
        int count)
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateAddition(
                id,
                date,
                InitialWorkLocationCatalog.Cafeteria.Id.Value,
                InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
                start,
                end,
                count).Value);
    }

    private static StaffingDemandDateException CreateDateReplacement(
        Guid id,
        DateOnly date,
        TimeOnly start,
        TimeOnly end,
        int count)
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateReplacement(
                id,
                date,
                InitialWorkLocationCatalog.Cafeteria.Id.Value,
                InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
                start,
                end,
                count).Value);
    }

    private static async Task<SqliteStaffingDemandStore> InitializeAsync(string databasePath)
    {
        SqliteStaffingDemandStore store = new(databasePath);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        return store;
    }

    private static async Task MigrateAsync(string databasePath, string targetMigration)
    {
        await using ServiceCatalogDbContext context =
            new ServiceCatalogDbContextFactory(databasePath).Create();
        IMigrator migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(targetMigration, TestContext.Current.CancellationToken);
    }

    private static Task InsertSyntheticStandardAsync(
        string databasePath,
        string id,
        string effectiveMonday,
        int startMinutes,
        int endMinutes,
        Guid workLocationId,
        Guid shiftTypeId)
    {
        string workLocationDatabaseId = workLocationId.ToString("D").ToUpperInvariant();
        string shiftTypeDatabaseId = shiftTypeId.ToString("D").ToUpperInvariant();

        return ExecuteNonQueryAsync(
            databasePath,
            "INSERT INTO StandardStaffingDemandRevisions "
            + "(Id, DayOfWeek, WorkLocationId, ShiftTypeId, EffectiveFromMonday, Kind, "
            + "ActualStartMinutes, ActualEndMinutes, RequiredEmployeeCount) VALUES "
            + $"('{id}', 1, '{workLocationDatabaseId}', '{shiftTypeDatabaseId}', '{effectiveMonday}', "
            + $"0, {startMinutes}, {endMinutes}, 1);");
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
            Path = System.IO.Path.Combine(_directory, "staffing-demands.db");
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }
    }
}
