using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record AcceptAutomaticScheduleProposalChange(
    ScheduleDraft UpdatedDraft,
    int ExpectedDraftVersion,
    AutomaticScheduleRunRecord Run)
{
    public Guid ExpectedSnapshotId => Run.SnapshotId;

    public AutomaticScheduleRunMetadata Metadata => Run.Metadata;
}

public enum AcceptAutomaticScheduleProposalStoreStatus
{
    Succeeded,
    Conflict,
}

public sealed record AcceptAutomaticScheduleProposalStoreResult(
    AcceptAutomaticScheduleProposalStoreStatus Status,
    ScheduleDraft? Draft = null)
{
    public static AcceptAutomaticScheduleProposalStoreResult Success(
        ScheduleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new AcceptAutomaticScheduleProposalStoreResult(
            AcceptAutomaticScheduleProposalStoreStatus.Succeeded,
            draft);
    }

    public static AcceptAutomaticScheduleProposalStoreResult Conflict() =>
        new(AcceptAutomaticScheduleProposalStoreStatus.Conflict);
}

public interface IAcceptAutomaticScheduleProposalStore
{
    public Task<AcceptAutomaticScheduleProposalStoreResult> AcceptAsync(
        AcceptAutomaticScheduleProposalChange change,
        CancellationToken cancellationToken);
}
