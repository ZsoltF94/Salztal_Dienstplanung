namespace Salztal.Dienstplanung.Application.StaffingDemands;

public interface IStaffingDemandReader
{
    public Task<StaffingDemandReadData> LoadAsync(CancellationToken cancellationToken);
}
