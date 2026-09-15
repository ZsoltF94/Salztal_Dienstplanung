using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Application.Availabilities;

public interface ISetAvailabilityEntryStore
{
    public Task<AvailabilityEntryWriteStoreResult> SaveAsync(
        AvailabilityEntry? expectedEntry,
        long? expectedChangeVersion,
        AvailabilityEntry replacement,
        CancellationToken cancellationToken);
}
