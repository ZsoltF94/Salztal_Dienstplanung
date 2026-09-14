using Salztal.Dienstplanung.Domain.StaffingDemands;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public interface IStandardStaffingDemandRevisionStore
{
    public Task<StaffingDemandWriteStoreResult> AppendAsync(
        StandardStaffingDemandRevision? expectedCurrent,
        StandardStaffingDemandRevision revision,
        CancellationToken cancellationToken);
}
