using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;

public sealed class SqliteAvailabilityStore :
    IAvailabilityReader,
    ISetAvailabilityEntryStore,
    IRemoveAvailabilityEntryStore
{
    private readonly ServiceCatalogDbContextFactory _contextFactory;

    public SqliteAvailabilityStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _contextFactory = new ServiceCatalogDbContextFactory(Path.GetFullPath(databasePath));
    }

    public async Task<AvailabilityReadData> LoadAsync(
        DateOnly periodMonday,
        DateOnly periodSunday,
        CancellationToken cancellationToken)
    {
        if (periodSunday < periodMonday)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodSunday),
                "The end of the availability period must not precede its start.");
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<WorkLocationEntity> workLocations = await context.WorkLocations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<ShiftTypeEntity> shiftTypes = await context.ShiftTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<ShiftPatternEntity> shiftPatterns = await context.ShiftPatterns
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<EmployeeTypeEntity> employeeTypes = await context.EmployeeTypes
            .AsNoTracking()
            .OrderBy(employeeType => employeeType.Code)
            .ToListAsync(cancellationToken);
        List<EmployeeTypeShiftEligibilityEntity> shiftEligibilities =
            await context.EmployeeTypeShiftEligibilities
                .AsNoTracking()
                .OrderBy(eligibility => eligibility.Id)
                .ToListAsync(cancellationToken);
        List<EmployeeEntity> employees = await context.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ThenBy(employee => employee.Id)
            .ToListAsync(cancellationToken);
        List<AvailabilityEntryEntity> entries = await context.AvailabilityEntries
            .AsNoTracking()
            .Where(entry => entry.Date >= periodMonday && entry.Date <= periodSunday)
            .OrderBy(entry => entry.EmployeeId)
            .ThenBy(entry => entry.Date)
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        ServiceCatalogData serviceCatalog = SqliteServiceCatalogStore.CreateData(
            workLocations,
            shiftTypes,
            shiftPatterns);
        EmployeeType[] mappedEmployeeTypes = SqliteEmployeeStore.MapEmployeeTypes(
            employeeTypes,
            shiftEligibilities,
            serviceCatalog);
        Employee[] mappedEmployees = employees
            .Select(SqliteEmployeeStore.MapEmployee)
            .ToArray();
        AvailabilityEntryReadItem[] mappedEntries = entries
            .Select(MapEntry)
            .ToArray();

        return new AvailabilityReadData(
            mappedEmployees,
            mappedEmployeeTypes,
            mappedEntries);
    }

    async Task<AvailabilityEntryWriteStoreResult> ISetAvailabilityEntryStore.SaveAsync(
        AvailabilityEntry? expectedEntry,
        long? expectedChangeVersion,
        AvailabilityEntry replacement,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        ValidateExpectedEntry(expectedEntry, expectedChangeVersion, replacement);

        await using ServiceCatalogDbContext context = _contextFactory.Create();

        if (expectedEntry is null)
        {
            context.AvailabilityEntries.Add(CreateEntity(replacement, 1));
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return AvailabilityEntryWriteStoreResult.Succeeded(1);
            }
            catch (DbUpdateException exception) when (IsConstraintViolation(exception))
            {
                return AvailabilityEntryWriteStoreResult.Conflict;
            }
        }

        if (expectedChangeVersion == long.MaxValue)
        {
            return AvailabilityEntryWriteStoreResult.Conflict;
        }

        long replacementVersion = expectedChangeVersion!.Value + 1;
        try
        {
            int affectedRows = await context.AvailabilityEntries
                .Where(entry =>
                    entry.EmployeeId == expectedEntry.EmployeeId.Value
                    && entry.Date == expectedEntry.Date
                    && entry.Kind == (int)expectedEntry.Kind
                    && entry.ChangeVersion == expectedChangeVersion.Value)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(entry => entry.Kind, (int)replacement.Kind)
                        .SetProperty(entry => entry.ChangeVersion, replacementVersion),
                    cancellationToken);

            return affectedRows == 1
                ? AvailabilityEntryWriteStoreResult.Succeeded(replacementVersion)
                : AvailabilityEntryWriteStoreResult.Conflict;
        }
        catch (SqliteException exception) when (IsConstraintViolation(exception))
        {
            return AvailabilityEntryWriteStoreResult.Conflict;
        }
    }

    async Task<AvailabilityEntryRemoveStoreResult> IRemoveAvailabilityEntryStore.RemoveAsync(
        AvailabilityEntry expectedEntry,
        long expectedChangeVersion,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expectedEntry);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedChangeVersion);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        int affectedRows = await context.AvailabilityEntries
            .Where(entry =>
                entry.EmployeeId == expectedEntry.EmployeeId.Value
                && entry.Date == expectedEntry.Date
                && entry.Kind == (int)expectedEntry.Kind
                && entry.ChangeVersion == expectedChangeVersion)
            .ExecuteDeleteAsync(cancellationToken);

        return affectedRows == 1
            ? AvailabilityEntryRemoveStoreResult.Succeeded
            : AvailabilityEntryRemoveStoreResult.Conflict;
    }

    internal static AvailabilityEntryReadItem MapEntry(AvailabilityEntryEntity entity)
    {
        AvailabilityEntryValidationResult result = AvailabilityEntry.Create(
            entity.EmployeeId,
            entity.Date,
            (AvailabilityEntryKind)entity.Kind);
        AvailabilityEntry entry = result.Value
            ?? throw new InvalidDataException(
                $"Stored availability entry for employee '{entity.EmployeeId:D}' on "
                + $"'{entity.Date:yyyy-MM-dd}' is invalid.");

        return entity.ChangeVersion > 0
            ? new AvailabilityEntryReadItem(entry, entity.ChangeVersion)
            : throw new InvalidDataException(
                $"Stored availability entry for employee '{entity.EmployeeId:D}' on "
                + $"'{entity.Date:yyyy-MM-dd}' has an invalid change version.");
    }

    private static AvailabilityEntryEntity CreateEntity(
        AvailabilityEntry entry,
        long changeVersion)
    {
        return new AvailabilityEntryEntity
        {
            EmployeeId = entry.EmployeeId.Value,
            Date = entry.Date,
            Kind = (int)entry.Kind,
            ChangeVersion = changeVersion,
        };
    }

    private static void ValidateExpectedEntry(
        AvailabilityEntry? expectedEntry,
        long? expectedChangeVersion,
        AvailabilityEntry replacement)
    {
        if (expectedEntry is null && expectedChangeVersion is not null)
        {
            throw new ArgumentException(
                "An expected change version requires an expected entry.",
                nameof(expectedChangeVersion));
        }

        if (expectedEntry is not null && expectedChangeVersion is null or <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedChangeVersion),
                "An expected entry requires a positive change version.");
        }

        if (expectedEntry is not null
            && (expectedEntry.EmployeeId != replacement.EmployeeId
                || expectedEntry.Date != replacement.Date))
        {
            throw new ArgumentException(
                "Replacement must keep the employee and date.",
                nameof(replacement));
        }
    }

    private static bool IsConstraintViolation(Exception exception)
    {
        SqliteException? sqliteException = exception as SqliteException
            ?? exception.InnerException as SqliteException;
        return sqliteException?.SqliteErrorCode == 19;
    }
}
