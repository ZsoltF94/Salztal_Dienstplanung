using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public interface IOpenScheduleDraftStore
{
    public Task<OpenScheduleDraftStoreResult> CreateAsync(
        ScheduleDraft draft,
        CancellationToken cancellationToken);
}
