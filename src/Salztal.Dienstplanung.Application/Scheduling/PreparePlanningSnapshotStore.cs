namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record PreparePlanningSnapshotChange(
    PlanningInputSnapshot Snapshot,
    int ExpectedDraftVersion,
    Guid? ExpectedPreviousSnapshotId);

public enum PreparePlanningSnapshotStoreStatus
{
    Succeeded,
    Conflict,
}

public sealed record PreparePlanningSnapshotStoreResult(
    PreparePlanningSnapshotStoreStatus Status,
    PlanningInputSnapshot? Snapshot = null)
{
    public static PreparePlanningSnapshotStoreResult Success(
        PlanningInputSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new PreparePlanningSnapshotStoreResult(
            PreparePlanningSnapshotStoreStatus.Succeeded,
            snapshot);
    }

    public static PreparePlanningSnapshotStoreResult Conflict()
    {
        return new PreparePlanningSnapshotStoreResult(
            PreparePlanningSnapshotStoreStatus.Conflict);
    }
}

public interface IPreparePlanningSnapshotStore
{
    public Task<PreparePlanningSnapshotStoreResult> SaveAsync(
        PreparePlanningSnapshotChange change,
        CancellationToken cancellationToken);
}
