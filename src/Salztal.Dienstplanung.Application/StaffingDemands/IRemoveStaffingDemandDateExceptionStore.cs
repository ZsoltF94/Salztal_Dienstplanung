using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public interface IRemoveStaffingDemandDateExceptionStore
{
    public Task<StaffingDemandWriteStoreResult> RemoveAsync(
        StaffingDemandDateException expected,
        CancellationToken cancellationToken);
}
