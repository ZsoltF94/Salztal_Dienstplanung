using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum OpenScheduleDraftStoreStatus
{
    Succeeded,
    Overlap,
    Conflict,
}

public sealed record OpenScheduleDraftStoreResult(
    OpenScheduleDraftStoreStatus Status,
    ScheduleDraft? Draft = null,
    SchedulePeriod? OverlappingPeriod = null)
{
    public static OpenScheduleDraftStoreResult Success(ScheduleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new OpenScheduleDraftStoreResult(
            OpenScheduleDraftStoreStatus.Succeeded,
            draft);
    }

    public static OpenScheduleDraftStoreResult Overlap(SchedulePeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return new OpenScheduleDraftStoreResult(
            OpenScheduleDraftStoreStatus.Overlap,
            OverlappingPeriod: period);
    }

    public static OpenScheduleDraftStoreResult Conflict()
    {
        return new OpenScheduleDraftStoreResult(OpenScheduleDraftStoreStatus.Conflict);
    }
}
