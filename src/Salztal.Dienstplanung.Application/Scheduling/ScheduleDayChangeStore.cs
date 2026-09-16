using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ScheduleAvailabilityMutationKind
{
    None,
    Upsert,
    Remove,
}

public enum SchedulePreparationImpact
{
    PotentiallyOutdated,
}

public sealed record ScheduleAvailabilityMutation(
    ScheduleAvailabilityMutationKind Kind,
    AvailabilityEntryReadItem? ExpectedCurrent,
    AvailabilityEntry? Replacement)
{
    public static ScheduleAvailabilityMutation None() => new(
        ScheduleAvailabilityMutationKind.None,
        null,
        null);

    public static ScheduleAvailabilityMutation Upsert(
        AvailabilityEntryReadItem? expectedCurrent,
        AvailabilityEntry replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        return new ScheduleAvailabilityMutation(
            ScheduleAvailabilityMutationKind.Upsert,
            expectedCurrent,
            replacement);
    }

    public static ScheduleAvailabilityMutation Remove(
        AvailabilityEntryReadItem expectedCurrent)
    {
        ArgumentNullException.ThrowIfNull(expectedCurrent);
        return new ScheduleAvailabilityMutation(
            ScheduleAvailabilityMutationKind.Remove,
            expectedCurrent,
            null);
    }
}

public sealed record ScheduleDayChange(
    ScheduleDraft UpdatedDraft,
    int ExpectedDraftVersion,
    ScheduleAvailabilityMutation AvailabilityMutation,
    SchedulePreparationImpact PreparationImpact);

public enum ScheduleDayChangeStoreStatus
{
    Succeeded,
    Conflict,
}

public sealed record ScheduleDayChangeStoreResult(
    ScheduleDayChangeStoreStatus Status,
    ScheduleDraft? Draft = null)
{
    public static ScheduleDayChangeStoreResult Success(ScheduleDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new ScheduleDayChangeStoreResult(
            ScheduleDayChangeStoreStatus.Succeeded,
            draft);
    }

    public static ScheduleDayChangeStoreResult Conflict()
    {
        return new ScheduleDayChangeStoreResult(ScheduleDayChangeStoreStatus.Conflict);
    }
}

public interface IChangeScheduleDayStore
{
    public Task<ScheduleDayChangeStoreResult> ChangeAsync(
        ScheduleDayChange change,
        CancellationToken cancellationToken);
}
