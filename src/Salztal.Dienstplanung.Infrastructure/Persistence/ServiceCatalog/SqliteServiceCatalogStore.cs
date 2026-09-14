using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

public sealed class SqliteServiceCatalogStore :
    IServiceCatalogReader,
    IWorkLocationUpdateStore,
    IShiftTypeStandardTimeUpdateStore
{
    private readonly string _databasePath;
    private readonly ServiceCatalogDbContextFactory _contextFactory;

    public SqliteServiceCatalogStore(string databasePath)
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
    }

    public async Task<ServiceCatalogData> LoadAsync(CancellationToken cancellationToken)
    {
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<WorkLocationEntity> locationEntities = await context.WorkLocations
            .AsNoTracking()
            .OrderBy(location => location.Name)
            .ToListAsync(cancellationToken);
        List<ShiftTypeEntity> shiftTypeEntities = await context.ShiftTypes
            .AsNoTracking()
            .OrderBy(shiftType => shiftType.Name)
            .ToListAsync(cancellationToken);
        List<ShiftPatternEntity> patternEntities = await context.ShiftPatterns
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return CreateData(locationEntities, shiftTypeEntities, patternEntities);
    }

    internal static ServiceCatalogData CreateData(
        IEnumerable<WorkLocationEntity> locationEntities,
        IEnumerable<ShiftTypeEntity> shiftTypeEntities,
        IEnumerable<ShiftPatternEntity> patternEntities)
    {
        WorkLocation[] locations = locationEntities.Select(MapWorkLocation).ToArray();
        Dictionary<Guid, ShiftType> shiftTypes = shiftTypeEntities
            .Select(MapShiftType)
            .ToDictionary(shiftType => shiftType.Id.Value);

        SplitShiftPattern splitShiftPattern = MapSplitShiftPattern(patternEntities, shiftTypes);
        ReliefShiftPattern reliefShiftPattern = MapReliefShiftPattern(patternEntities, shiftTypes);

        return new ServiceCatalogData(
            locations,
            shiftTypes.Values.OrderBy(shiftType => shiftType.Name.Value),
            splitShiftPattern,
            reliefShiftPattern);
    }

    async Task<WorkLocation?> IWorkLocationUpdateStore.FindAsync(
        WorkLocationId id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        WorkLocationEntity? entity = await context.WorkLocations
            .AsNoTracking()
            .SingleOrDefaultAsync(location => location.Id == id.Value, cancellationToken);

        return entity is null ? null : MapWorkLocation(entity);
    }

    async Task<CatalogEntryUpdateStoreResult> IWorkLocationUpdateStore.UpdateAsync(
        WorkLocation expected,
        WorkLocation replacement,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(replacement);

        if (expected.Id != replacement.Id)
        {
            throw new ArgumentException("Replacement must keep the work-location identifier.");
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        int affectedRows = await context.WorkLocations
            .Where(location =>
                location.Id == expected.Id.Value
                && location.Name == expected.Name.Value
                && location.ColorCode == expected.Color.Code)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(location => location.Name, replacement.Name.Value)
                    .SetProperty(location => location.ColorCode, replacement.Color.Code),
                cancellationToken);

        return affectedRows == 1
            ? CatalogEntryUpdateStoreResult.Updated
            : CatalogEntryUpdateStoreResult.Conflict;
    }

    async Task<ShiftTypeStandardTimeUpdateData?> IShiftTypeStandardTimeUpdateStore.FindAsync(
        ShiftTypeId id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        List<ShiftTypeEntity> entities = await context.ShiftTypes
            .AsNoTracking()
            .Where(shiftType =>
                shiftType.Id == id.Value
                || shiftType.Id == InitialShiftTypeCatalog.EarlyShift.Id.Value
                || shiftType.Id == InitialShiftTypeCatalog.LateShift.Id.Value)
            .ToListAsync(cancellationToken);

        ShiftTypeEntity? currentEntity = entities.SingleOrDefault(entity => entity.Id == id.Value);

        if (currentEntity is null)
        {
            return null;
        }

        Dictionary<Guid, ShiftType> shiftTypes = entities
            .Select(MapShiftType)
            .ToDictionary(shiftType => shiftType.Id.Value);
        ShiftPatternEntity splitEntity = await context.ShiftPatterns
            .AsNoTracking()
            .SingleAsync(
                pattern => pattern.Kind == StoredShiftPatternKind.SplitShift,
                cancellationToken);
        SplitShiftPattern splitPattern = MapSplitShiftPattern([splitEntity], shiftTypes);

        return new ShiftTypeStandardTimeUpdateData(
            shiftTypes[id.Value],
            GetRequiredShiftType(shiftTypes, InitialShiftTypeCatalog.EarlyShift.Id.Value),
            GetRequiredShiftType(shiftTypes, InitialShiftTypeCatalog.LateShift.Id.Value),
            splitPattern);
    }

    async Task<CatalogEntryUpdateStoreResult> IShiftTypeStandardTimeUpdateStore.UpdateAsync(
        ShiftTypeStandardTimeUpdateData expected,
        ShiftType replacement,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(replacement);
        ValidateShiftTypeReplacement(expected.Current, replacement);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<ShiftTypeEntity> entities = await context.ShiftTypes
            .Where(shiftType =>
                shiftType.Id == expected.Current.Id.Value
                || shiftType.Id == expected.EarlyShift.Id.Value
                || shiftType.Id == expected.LateShift.Id.Value)
            .ToListAsync(cancellationToken);
        ShiftPatternEntity? splitEntity = await context.ShiftPatterns.SingleOrDefaultAsync(
            pattern => pattern.Kind == StoredShiftPatternKind.SplitShift,
            cancellationToken);

        if (!MatchesExpectedContext(entities, splitEntity, expected))
        {
            return CatalogEntryUpdateStoreResult.Conflict;
        }

        ShiftTypeEntity currentEntity = entities.Single(
            entity => entity.Id == expected.Current.Id.Value);
        currentEntity.StandardStartMinutes = ToMinutes(replacement.StandardTime.Start);
        currentEntity.StandardEndMinutes = ToMinutes(replacement.StandardTime.End);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CatalogEntryUpdateStoreResult.Updated;
    }

    private static bool MatchesExpectedContext(
        List<ShiftTypeEntity> entities,
        ShiftPatternEntity? splitEntity,
        ShiftTypeStandardTimeUpdateData expected)
    {
        if (splitEntity is null || entities.Count is < 2 or > 3)
        {
            return false;
        }

        ShiftType[] expectedShiftTypes =
        [
            expected.Current,
            expected.EarlyShift,
            expected.LateShift,
        ];

        bool allShiftTypesMatch = expectedShiftTypes
            .DistinctBy(shiftType => shiftType.Id)
            .All(expectedShiftType => entities.Any(
                entity => Matches(entity, expectedShiftType)));

        return allShiftTypesMatch
            && splitEntity.Id == expected.SplitShiftPattern.Id.Value
            && splitEntity.DisplayCode == expected.SplitShiftPattern.DisplayCode
            && splitEntity.FirstShiftTypeId == expected.SplitShiftPattern.FirstShiftTypeId.Value
            && splitEntity.SecondShiftTypeId == expected.SplitShiftPattern.SecondShiftTypeId.Value;
    }

    private static bool Matches(ShiftTypeEntity entity, ShiftType expected)
    {
        return entity.Id == expected.Id.Value
            && entity.Name == expected.Name.Value
            && entity.WorkLocationId == expected.WorkLocationId.Value
            && entity.DisplayKind == expected.Display.Kind
            && entity.Abbreviation == expected.Display.Abbreviation
            && entity.StandardStartMinutes == ToMinutes(expected.StandardTime.Start)
            && entity.StandardEndMinutes == ToMinutes(expected.StandardTime.End);
    }

    private static void ValidateShiftTypeReplacement(ShiftType expected, ShiftType replacement)
    {
        if (expected.Id != replacement.Id
            || expected.Name != replacement.Name
            || expected.WorkLocationId != replacement.WorkLocationId
            || expected.Display != replacement.Display)
        {
            throw new ArgumentException("Replacement may only change the standard time.");
        }
    }

    private static WorkLocation MapWorkLocation(WorkLocationEntity entity)
    {
        return WorkLocation.Create(entity.Id, entity.Name, entity.ColorCode).Value
            ?? throw InvalidStoredData("work location", entity.Id);
    }

    private static ShiftType MapShiftType(ShiftTypeEntity entity)
    {
        ShiftStandardTime standardTime = ShiftStandardTime.Create(
            FromMinutes(entity.StandardStartMinutes),
            FromMinutes(entity.StandardEndMinutes)).Value
            ?? throw InvalidStoredData("shift standard time", entity.Id);

        WorkLocationId workLocationId = CreateWorkLocationId(entity.WorkLocationId);

        return ShiftType.Create(
            entity.Id,
            entity.Name,
            workLocationId,
            entity.DisplayKind,
            entity.Abbreviation,
            standardTime).Value
            ?? throw InvalidStoredData("shift type", entity.Id);
    }

    private static SplitShiftPattern MapSplitShiftPattern(
        IEnumerable<ShiftPatternEntity> entities,
        IReadOnlyDictionary<Guid, ShiftType> shiftTypes)
    {
        ShiftPatternEntity entity = entities.Single(
            pattern => pattern.Kind == StoredShiftPatternKind.SplitShift);

        if (entity.DisplayCode != "D"
            || entity.DisplayColorCode is not null
            || entity.AllowedDay is not null
            || entity.SwitchRule is not null
            || !entity.HasInterruption)
        {
            throw InvalidStoredData("split-shift pattern", entity.Id);
        }

        return SplitShiftPattern.Create(
            entity.Id,
            GetRequiredShiftType(shiftTypes, entity.FirstShiftTypeId),
            GetRequiredShiftType(shiftTypes, entity.SecondShiftTypeId)).Value
            ?? throw InvalidStoredData("split-shift pattern", entity.Id);
    }

    private static ReliefShiftPattern MapReliefShiftPattern(
        IEnumerable<ShiftPatternEntity> entities,
        IReadOnlyDictionary<Guid, ShiftType> shiftTypes)
    {
        ShiftPatternEntity entity = entities.Single(
            pattern => pattern.Kind == StoredShiftPatternKind.ReliefShift);

        if (entity.AllowedDay is null)
        {
            throw InvalidStoredData("relief-shift pattern", entity.Id);
        }

        ReliefShiftPattern pattern = ReliefShiftPattern.Create(
            entity.Id,
            entity.AllowedDay.Value,
            GetRequiredShiftType(shiftTypes, entity.FirstShiftTypeId),
            GetRequiredShiftType(shiftTypes, entity.SecondShiftTypeId)).Value
            ?? throw InvalidStoredData("relief-shift pattern", entity.Id);

        if (entity.DisplayCode != pattern.DisplayCode
            || entity.DisplayColorCode != pattern.DisplayColorCode
            || entity.SwitchRule != pattern.SwitchRule
            || entity.HasInterruption != pattern.HasInterruption)
        {
            throw InvalidStoredData("relief-shift pattern", entity.Id);
        }

        return pattern;
    }

    private static ShiftType GetRequiredShiftType(
        IReadOnlyDictionary<Guid, ShiftType> shiftTypes,
        Guid id)
    {
        return shiftTypes.TryGetValue(id, out ShiftType? shiftType)
            ? shiftType
            : throw InvalidStoredData("shift-type relationship", id);
    }

    private static WorkLocationId CreateWorkLocationId(Guid id)
    {
        return WorkLocationId.TryCreate(id, out WorkLocationId? workLocationId)
            ? workLocationId
            : throw InvalidStoredData("work-location relationship", id);
    }

    private static int ToMinutes(TimeOnly value)
    {
        return (value.Hour * 60) + value.Minute;
    }

    private static TimeOnly FromMinutes(int minutes)
    {
        if (minutes is < 0 or >= 1440)
        {
            throw new InvalidDataException($"Stored minute value '{minutes}' is invalid.");
        }

        return new TimeOnly(minutes / 60, minutes % 60);
    }

    private static InvalidDataException InvalidStoredData(string kind, Guid id)
    {
        return new InvalidDataException($"Stored {kind} '{id:D}' is invalid.");
    }
}
