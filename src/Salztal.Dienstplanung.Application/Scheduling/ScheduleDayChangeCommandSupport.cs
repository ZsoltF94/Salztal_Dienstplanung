using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class ScheduleDayChangeCommandSupport
{
    public static async Task<ScheduleDayChangeResult> StoreAsync(
        IChangeScheduleDayStore store,
        ScheduleDraft current,
        ScheduleDraft updated,
        ScheduleAvailabilityMutation availabilityMutation,
        Guid? assignmentId,
        Guid employeeId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        ScheduleDayChangeStoreResult storeResult = await store.ChangeAsync(
            new ScheduleDayChange(
                updated,
                current.Version.Value,
                availabilityMutation,
                SchedulePreparationImpact.PotentiallyOutdated),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return storeResult.Status switch
        {
            ScheduleDayChangeStoreStatus.Succeeded =>
                ScheduleDayChangeResult.Success(
                    new ScheduleDayChangeSnapshot(
                        storeResult.Draft!.Id.Value,
                        storeResult.Draft.Version.Value,
                        assignmentId,
                        employeeId,
                        date)),
            ScheduleDayChangeStoreStatus.Conflict => Conflict(),
            _ => throw new InvalidOperationException(
                $"Unsupported schedule day change store result: {storeResult.Status}"),
        };
    }

    public static ScheduleDayChangeResult Conflict()
    {
        return ScheduleDayChangeResult.Failure(
            ScheduleDayChangeStatus.Conflict,
            ScheduleDayChangeErrorCode.Conflict,
            "Der Planungsstand wurde zwischenzeitlich geändert. Bitte laden Sie den Zeitraum erneut.");
    }

    public static ScheduleDayChangeResult StoredDataInvalid()
    {
        return ScheduleDayChangeResult.Failure(
            ScheduleDayChangeStatus.StoredDataInvalid,
            ScheduleDayChangeErrorCode.StoredDataInvalid,
            "Aus den aktuellen Daten kann kein widerspruchsfreier Entwurf gespeichert werden.");
    }
}
