using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public interface IStaffingDemandDateExceptionStore
{
    public Task<StaffingDemandWriteStoreResult> SaveAsync(
        StaffingDemandDateException? expectedCurrent,
        StaffingDemandDateException replacement,
        CancellationToken cancellationToken);
}
