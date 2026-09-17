using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record DiscardAutomaticScheduleChange(
    ScheduleDraft UpdatedDraft,
    int ExpectedDraftVersion,
    Guid? ExpectedPreparedSnapshotId);

public enum DiscardAutomaticScheduleStoreStatus
{
    Succeeded,
    Conflict,
}

public sealed record DiscardAutomaticScheduleStoreResult(
    DiscardAutomaticScheduleStoreStatus Status,
    ScheduleDraft? Draft = null)
{
    public static DiscardAutomaticScheduleStoreResult Success(ScheduleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new DiscardAutomaticScheduleStoreResult(
            DiscardAutomaticScheduleStoreStatus.Succeeded,
            draft);
    }

    public static DiscardAutomaticScheduleStoreResult Conflict() =>
        new(DiscardAutomaticScheduleStoreStatus.Conflict);
}

public interface IDiscardAutomaticScheduleStore
{
    public Task<DiscardAutomaticScheduleStoreResult> DiscardAsync(
        DiscardAutomaticScheduleChange change,
        CancellationToken cancellationToken);
}
