using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public interface IWorkLocationUpdateStore
{
    public Task<WorkLocation?> FindAsync(
        WorkLocationId id,
        CancellationToken cancellationToken);

    public Task<CatalogEntryUpdateStoreResult> UpdateAsync(
        WorkLocation expected,
        WorkLocation replacement,
        CancellationToken cancellationToken);
}
