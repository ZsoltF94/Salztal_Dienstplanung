using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Application.Availabilities;

public interface IRemoveAvailabilityEntryStore
{
    public Task<AvailabilityEntryRemoveStoreResult> RemoveAsync(
        AvailabilityEntry expectedEntry,
        long expectedChangeVersion,
        CancellationToken cancellationToken);
}
