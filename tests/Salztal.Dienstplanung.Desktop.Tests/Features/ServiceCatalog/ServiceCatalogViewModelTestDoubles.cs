using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.ServiceCatalog;

internal sealed class FakeServiceCatalogReader : IServiceCatalogReader
{
    public Func<CancellationToken, Task<ServiceCatalogData>> LoadHandler { get; set; } =
        _ => Task.FromResult(CreateInitialData());

    public int LoadCallCount { get; private set; }

    public Task<ServiceCatalogData> LoadAsync(CancellationToken cancellationToken)
    {
        LoadCallCount++;
        return LoadHandler(cancellationToken);
    }

    public static ServiceCatalogData CreateInitialData(
        IEnumerable<WorkLocation>? workLocations = null)
    {
        return new ServiceCatalogData(
            workLocations ?? InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            Domain.ShiftPatterns.InitialShiftPatternCatalog.SplitShift,
            Domain.ShiftPatterns.InitialShiftPatternCatalog.ReliefShift);
    }
}

internal sealed class FakeWorkLocationUpdateStore : IWorkLocationUpdateStore
{
    public WorkLocation? Current { get; set; } = InitialWorkLocationCatalog.Cafeteria;

    public CatalogEntryUpdateStoreResult UpdateResult { get; set; } =
        CatalogEntryUpdateStoreResult.Updated;

    public Func<WorkLocation, WorkLocation, CancellationToken, Task<CatalogEntryUpdateStoreResult>>?
        UpdateHandler
    { get; set; }

    public Exception? FindException { get; set; }

    public int FindCallCount { get; private set; }

    public WorkLocation? Replacement { get; private set; }

    public Task<WorkLocation?> FindAsync(
        WorkLocationId id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FindCallCount++;

        if (FindException is not null)
        {
            return Task.FromException<WorkLocation?>(FindException);
        }

        return Task.FromResult(Current?.Id == id ? Current : null);
    }

    public Task<CatalogEntryUpdateStoreResult> UpdateAsync(
        WorkLocation expected,
        WorkLocation replacement,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Replacement = replacement;

        return UpdateHandler is null
            ? Task.FromResult(UpdateResult)
            : UpdateHandler(expected, replacement, cancellationToken);
    }
}

internal sealed class FakeShiftTypeStandardTimeUpdateStore
    : IShiftTypeStandardTimeUpdateStore
{
    public CatalogEntryUpdateStoreResult UpdateResult { get; set; } =
        CatalogEntryUpdateStoreResult.Updated;

    public Func<
        ShiftTypeStandardTimeUpdateData,
        ShiftType,
        CancellationToken,
        Task<CatalogEntryUpdateStoreResult>>? UpdateHandler
    { get; set; }

    public Exception? FindException { get; set; }

    public int FindCallCount { get; private set; }

    public ShiftType? Replacement { get; private set; }

    public Task<ShiftTypeStandardTimeUpdateData?> FindAsync(
        ShiftTypeId id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FindCallCount++;

        if (FindException is not null)
        {
            return Task.FromException<ShiftTypeStandardTimeUpdateData?>(FindException);
        }

        ShiftType? current = InitialShiftTypeCatalog.All.FirstOrDefault(
            shiftType => shiftType.Id == id);
        ShiftTypeStandardTimeUpdateData? result = current is null
            ? null
            : new ShiftTypeStandardTimeUpdateData(
                current,
                InitialShiftTypeCatalog.EarlyShift,
                InitialShiftTypeCatalog.LateShift,
                InitialShiftPatternCatalog.SplitShift);

        return Task.FromResult(result);
    }

    public Task<CatalogEntryUpdateStoreResult> UpdateAsync(
        ShiftTypeStandardTimeUpdateData expected,
        ShiftType replacement,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Replacement = replacement;

        return UpdateHandler is null
            ? Task.FromResult(UpdateResult)
            : UpdateHandler(expected, replacement, cancellationToken);
    }
}

internal sealed class CollectingUnexpectedErrorReporter : IUnexpectedErrorReporter
{
    public List<(Exception Exception, string Operation)> Reports { get; } = [];

    public void Report(Exception exception, string operation)
    {
        Reports.Add((exception, operation));
    }
}
