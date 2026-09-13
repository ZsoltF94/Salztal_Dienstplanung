using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public interface IShiftTypeStandardTimeUpdateStore
{
    public Task<ShiftTypeStandardTimeUpdateData?> FindAsync(
        ShiftTypeId id,
        CancellationToken cancellationToken);

    public Task<CatalogEntryUpdateStoreResult> UpdateAsync(
        ShiftTypeStandardTimeUpdateData expected,
        ShiftType replacement,
        CancellationToken cancellationToken);
}
