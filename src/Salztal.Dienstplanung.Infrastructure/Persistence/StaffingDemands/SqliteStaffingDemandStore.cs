using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

public sealed class SqliteStaffingDemandStore :
    IStaffingDemandReader,
    IStandardStaffingDemandRevisionStore,
    IStaffingDemandDateExceptionStore,
    IRemoveStaffingDemandDateExceptionStore
{
    private readonly string _databasePath;
    private readonly ServiceCatalogDbContextFactory _contextFactory;

    public SqliteStaffingDemandStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        _databasePath = Path.GetFullPath(databasePath);
        _contextFactory = new ServiceCatalogDbContextFactory(_databasePath);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(_databasePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await context.Database.MigrateAsync(cancellationToken);
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        if (!await context.StandardStaffingDemandRevisions.AnyAsync(cancellationToken))
        {
            ServiceCatalogData catalog = await LoadServiceCatalogAsync(context, cancellationToken);
            StandardStaffingDemandRevisionSet initialStandards =
                InitialStaffingDemandCatalog.CreateFromCurrentShiftTypes(catalog.ShiftTypes);

            await context.StandardStaffingDemandRevisions.AddRangeAsync(
                initialStandards.Revisions.Select(CreateEntity),
                cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<StaffingDemandReadData> LoadAsync(CancellationToken cancellationToken)
    {
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        ServiceCatalogData catalog = await LoadServiceCatalogAsync(context, cancellationToken);
        List<StandardStaffingDemandRevisionEntity> standardEntities = await context
            .StandardStaffingDemandRevisions
            .AsNoTracking()
            .OrderBy(entity => entity.EffectiveFromMonday)
            .ThenBy(entity => entity.DayOfWeek)
            .ThenBy(entity => entity.WorkLocationId)
            .ThenBy(entity => entity.ShiftTypeId)
            .ThenBy(entity => entity.CorrectionSequence)
            .ToListAsync(cancellationToken);
        List<StaffingDemandDateExceptionEntity> exceptionEntities = await context
            .StaffingDemandDateExceptions
            .AsNoTracking()
            .OrderBy(entity => entity.Date)
            .ThenBy(entity => entity.WorkLocationId)
            .ThenBy(entity => entity.ShiftTypeId)
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new StaffingDemandReadData(
            standardEntities.Select(MapStandardRevision),
            exceptionEntities.Select(MapDateException),
            catalog);
    }

    public async Task<StaffingDemandWriteStoreResult> AppendAsync(
        StandardStaffingDemandRevision? expectedCurrent,
        StandardStaffingDemandRevision revision,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(revision);

        if (expectedCurrent is not null && expectedCurrent.Key != revision.Key)
        {
            throw new ArgumentException("Expected and replacement standard keys must match.");
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        StandardStaffingDemandRevisionEntity? currentEntity = await context
            .StandardStaffingDemandRevisions
            .AsNoTracking()
            .Where(entity => entity.DayOfWeek == revision.Key.DayOfWeek)
            .Where(entity => entity.WorkLocationId == revision.Key.WorkLocationId.Value)
            .Where(entity => entity.ShiftTypeId == revision.Key.ShiftTypeId.Value)
            .Where(entity => entity.EffectiveFromMonday <= revision.EffectiveFromMonday)
            .OrderByDescending(entity => entity.EffectiveFromMonday)
            .ThenByDescending(entity => entity.CorrectionSequence)
            .FirstOrDefaultAsync(cancellationToken);

        StaffingDemandWriteStoreResult? expectationFailure = MatchExpectation(
            currentEntity,
            expectedCurrent,
            Matches);
        if (expectationFailure is not null)
        {
            return expectationFailure.Value;
        }

        int? latestCorrectionSequence = await context.StandardStaffingDemandRevisions
            .Where(entity => entity.DayOfWeek == revision.Key.DayOfWeek)
            .Where(entity => entity.WorkLocationId == revision.Key.WorkLocationId.Value)
            .Where(entity => entity.ShiftTypeId == revision.Key.ShiftTypeId.Value)
            .Where(entity => entity.EffectiveFromMonday == revision.EffectiveFromMonday)
            .Select(entity => (int?)entity.CorrectionSequence)
            .MaxAsync(cancellationToken);
        if (latestCorrectionSequence == int.MaxValue
            || revision.CorrectionSequence != (latestCorrectionSequence ?? 0) + 1)
        {
            return StaffingDemandWriteStoreResult.Conflict;
        }

        List<StandardStaffingDemandRevisionEntity> historyEntities = await context
            .StandardStaffingDemandRevisions
            .AsNoTracking()
            .Where(entity => entity.DayOfWeek == revision.Key.DayOfWeek)
            .Where(entity => entity.WorkLocationId == revision.Key.WorkLocationId.Value)
            .Where(entity => entity.ShiftTypeId == revision.Key.ShiftTypeId.Value)
            .ToListAsync(cancellationToken);
        StandardStaffingDemandRevisionSetValidationResult historyResult =
            StandardStaffingDemandRevisionSet.Create(
                historyEntities.Select(MapStandardRevision).Append(revision));
        if (!historyResult.IsSuccess)
        {
            return StaffingDemandWriteStoreResult.Conflict;
        }

        try
        {
            await context.StandardStaffingDemandRevisions.AddAsync(
                CreateEntity(revision),
                cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return StaffingDemandWriteStoreResult.Succeeded;
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            return StaffingDemandWriteStoreResult.Conflict;
        }
    }

    public async Task<StaffingDemandWriteStoreResult> SaveAsync(
        StaffingDemandDateException? expectedCurrent,
        StaffingDemandDateException replacement,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        if (expectedCurrent is not null && expectedCurrent.Key != replacement.Key)
        {
            throw new ArgumentException("Expected and replacement date-exception keys must match.");
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        StaffingDemandDateExceptionEntity? currentEntity = await context
            .StaffingDemandDateExceptions
            .SingleOrDefaultAsync(
                entity => entity.Date == replacement.Key.Date
                    && entity.WorkLocationId == replacement.Key.WorkLocationId.Value
                    && entity.ShiftTypeId == replacement.Key.ShiftTypeId.Value,
                cancellationToken);

        StaffingDemandWriteStoreResult? expectationFailure = MatchExpectation(
            currentEntity,
            expectedCurrent,
            Matches);
        if (expectationFailure is not null)
        {
            return expectationFailure.Value;
        }

        try
        {
            if (currentEntity is not null)
            {
                context.StaffingDemandDateExceptions.Remove(currentEntity);
                await context.SaveChangesAsync(cancellationToken);
            }

            await context.StaffingDemandDateExceptions.AddAsync(
                CreateEntity(replacement),
                cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return StaffingDemandWriteStoreResult.Succeeded;
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            return StaffingDemandWriteStoreResult.Conflict;
        }
    }

    public async Task<StaffingDemandWriteStoreResult> RemoveAsync(
        StaffingDemandDateException expected,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expected);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        StaffingDemandDateExceptionEntity? currentEntity = await context
            .StaffingDemandDateExceptions
            .SingleOrDefaultAsync(
                entity => entity.Date == expected.Key.Date
                    && entity.WorkLocationId == expected.Key.WorkLocationId.Value
                    && entity.ShiftTypeId == expected.Key.ShiftTypeId.Value,
                cancellationToken);

        if (currentEntity is null)
        {
            return StaffingDemandWriteStoreResult.NotFound;
        }

        if (!Matches(currentEntity, expected))
        {
            return StaffingDemandWriteStoreResult.Conflict;
        }

        context.StaffingDemandDateExceptions.Remove(currentEntity);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return StaffingDemandWriteStoreResult.Succeeded;
    }

    private static async Task<ServiceCatalogData> LoadServiceCatalogAsync(
        ServiceCatalogDbContext context,
        CancellationToken cancellationToken)
    {
        List<WorkLocationEntity> locations = await context.WorkLocations
            .AsNoTracking()
            .OrderBy(entity => entity.Name)
            .ToListAsync(cancellationToken);
        List<ShiftTypeEntity> shiftTypes = await context.ShiftTypes
            .AsNoTracking()
            .OrderBy(entity => entity.Name)
            .ToListAsync(cancellationToken);
        List<ShiftPatternEntity> patterns = await context.ShiftPatterns
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return SqliteServiceCatalogStore.CreateData(locations, shiftTypes, patterns);
    }

    private static StaffingDemandWriteStoreResult? MatchExpectation<TEntity, TDomain>(
        TEntity? current,
        TDomain? expected,
        Func<TEntity, TDomain, bool> matches)
        where TEntity : class
        where TDomain : class
    {
        if (current is null)
        {
            return expected is null ? null : StaffingDemandWriteStoreResult.NotFound;
        }

        if (expected is null || !matches(current, expected))
        {
            return StaffingDemandWriteStoreResult.Conflict;
        }

        return null;
    }

    private static StandardStaffingDemandRevisionEntity CreateEntity(
        StandardStaffingDemandRevision revision)
    {
        return new StandardStaffingDemandRevisionEntity
        {
            Id = revision.Id.Value,
            DayOfWeek = revision.Key.DayOfWeek,
            WorkLocationId = revision.Key.WorkLocationId.Value,
            ShiftTypeId = revision.Key.ShiftTypeId.Value,
            EffectiveFromMonday = revision.EffectiveFromMonday,
            CorrectionSequence = revision.CorrectionSequence,
            Kind = revision.Kind,
            ActualStartMinutes = revision.ActualTime is null
                ? null
                : ToMinutes(revision.ActualTime.Start),
            ActualEndMinutes = revision.ActualTime is null
                ? null
                : ToMinutes(revision.ActualTime.End),
            RequiredEmployeeCount = revision.RequiredEmployeeCount?.Value,
        };
    }

    private static StaffingDemandDateExceptionEntity CreateEntity(
        StaffingDemandDateException exception)
    {
        return new StaffingDemandDateExceptionEntity
        {
            Id = exception.Id.Value,
            Date = exception.Key.Date,
            WorkLocationId = exception.Key.WorkLocationId.Value,
            ShiftTypeId = exception.Key.ShiftTypeId.Value,
            Kind = exception.Kind,
            ActualStartMinutes = exception.ActualTime is null
                ? null
                : ToMinutes(exception.ActualTime.Start),
            ActualEndMinutes = exception.ActualTime is null
                ? null
                : ToMinutes(exception.ActualTime.End),
            RequiredEmployeeCount = exception.RequiredEmployeeCount?.Value,
        };
    }

    private static StandardStaffingDemandRevision MapStandardRevision(
        StandardStaffingDemandRevisionEntity entity)
    {
        StandardStaffingDemandRevisionValidationResult result =
            entity.Kind switch
            {
                StandardStaffingDemandRevisionKind.Add =>
                    StandardStaffingDemandRevision.CreateAddition(
                        entity.Id,
                        entity.DayOfWeek,
                        entity.WorkLocationId,
                        entity.ShiftTypeId,
                        entity.EffectiveFromMonday,
                        FromRequiredMinutes(entity.ActualStartMinutes),
                        FromRequiredMinutes(entity.ActualEndMinutes),
                        GetRequiredEmployeeCount(entity.RequiredEmployeeCount),
                        entity.CorrectionSequence),
                StandardStaffingDemandRevisionKind.Replace =>
                    StandardStaffingDemandRevision.CreateReplacement(
                        entity.Id,
                        entity.DayOfWeek,
                        entity.WorkLocationId,
                        entity.ShiftTypeId,
                        entity.EffectiveFromMonday,
                        FromRequiredMinutes(entity.ActualStartMinutes),
                        FromRequiredMinutes(entity.ActualEndMinutes),
                        GetRequiredEmployeeCount(entity.RequiredEmployeeCount),
                        entity.CorrectionSequence),
                StandardStaffingDemandRevisionKind.Remove =>
                    StandardStaffingDemandRevision.CreateRemoval(
                        entity.Id,
                        entity.DayOfWeek,
                        entity.WorkLocationId,
                        entity.ShiftTypeId,
                        entity.EffectiveFromMonday,
                        entity.CorrectionSequence),
                _ => throw InvalidStoredData("standard staffing-demand kind", entity.Id),
            };

        return result.Value
            ?? throw InvalidStoredData("standard staffing-demand revision", entity.Id);
    }

    private static StaffingDemandDateException MapDateException(
        StaffingDemandDateExceptionEntity entity)
    {
        StaffingDemandDateExceptionValidationResult result =
            entity.Kind switch
            {
                StaffingDemandDateExceptionKind.Add =>
                    StaffingDemandDateException.CreateAddition(
                        entity.Id,
                        entity.Date,
                        entity.WorkLocationId,
                        entity.ShiftTypeId,
                        FromRequiredMinutes(entity.ActualStartMinutes),
                        FromRequiredMinutes(entity.ActualEndMinutes),
                        GetRequiredEmployeeCount(entity.RequiredEmployeeCount)),
                StaffingDemandDateExceptionKind.Replace =>
                    StaffingDemandDateException.CreateReplacement(
                        entity.Id,
                        entity.Date,
                        entity.WorkLocationId,
                        entity.ShiftTypeId,
                        FromRequiredMinutes(entity.ActualStartMinutes),
                        FromRequiredMinutes(entity.ActualEndMinutes),
                        GetRequiredEmployeeCount(entity.RequiredEmployeeCount)),
                StaffingDemandDateExceptionKind.Remove =>
                    StaffingDemandDateException.CreateRemoval(
                        entity.Id,
                        entity.Date,
                        entity.WorkLocationId,
                        entity.ShiftTypeId),
                _ => throw InvalidStoredData("staffing-demand date-exception kind", entity.Id),
            };

        return result.Value
            ?? throw InvalidStoredData("staffing-demand date exception", entity.Id);
    }

    private static bool Matches(
        StandardStaffingDemandRevisionEntity entity,
        StandardStaffingDemandRevision expected)
    {
        return entity.Id == expected.Id.Value
            && entity.DayOfWeek == expected.Key.DayOfWeek
            && entity.WorkLocationId == expected.Key.WorkLocationId.Value
            && entity.ShiftTypeId == expected.Key.ShiftTypeId.Value
            && entity.EffectiveFromMonday == expected.EffectiveFromMonday
            && entity.CorrectionSequence == expected.CorrectionSequence
            && entity.Kind == expected.Kind
            && entity.ActualStartMinutes == ToNullableMinutes(expected.ActualTime?.Start)
            && entity.ActualEndMinutes == ToNullableMinutes(expected.ActualTime?.End)
            && entity.RequiredEmployeeCount == expected.RequiredEmployeeCount?.Value;
    }

    private static bool Matches(
        StaffingDemandDateExceptionEntity entity,
        StaffingDemandDateException expected)
    {
        return entity.Id == expected.Id.Value
            && entity.Date == expected.Key.Date
            && entity.WorkLocationId == expected.Key.WorkLocationId.Value
            && entity.ShiftTypeId == expected.Key.ShiftTypeId.Value
            && entity.Kind == expected.Kind
            && entity.ActualStartMinutes == ToNullableMinutes(expected.ActualTime?.Start)
            && entity.ActualEndMinutes == ToNullableMinutes(expected.ActualTime?.End)
            && entity.RequiredEmployeeCount == expected.RequiredEmployeeCount?.Value;
    }

    private static int GetRequiredEmployeeCount(int? count)
    {
        return count ?? throw new InvalidDataException(
            "Stored staffing-demand employee count is missing.");
    }

    private static int? ToNullableMinutes(TimeOnly? value)
    {
        return value is null ? null : ToMinutes(value.Value);
    }

    private static int ToMinutes(TimeOnly value)
    {
        return (value.Hour * 60) + value.Minute;
    }

    private static TimeOnly FromRequiredMinutes(int? minutes)
    {
        return minutes is null
            ? throw new InvalidDataException("Stored staffing-demand time is missing.")
            : FromMinutes(minutes.Value);
    }

    private static TimeOnly FromMinutes(int minutes)
    {
        if (minutes is < 0 or >= 1440)
        {
            throw new InvalidDataException(
                $"Stored staffing-demand minute value '{minutes}' is invalid.");
        }

        return new TimeOnly(minutes / 60, minutes % 60);
    }

    private static bool IsConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteErrorCode: 19,
        };
    }

    private static InvalidDataException InvalidStoredData(string kind, Guid id)
    {
        return new InvalidDataException($"Stored {kind} '{id:D}' is invalid.");
    }
}
