using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public interface IScheduleWorkspaceReader
{
    public Task<ScheduleWorkspaceReadData> LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken);
}
