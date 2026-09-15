namespace Salztal.Dienstplanung.Application.Availabilities;

public enum AvailabilityEntryWriteStoreStatus
{
    Succeeded,
    Conflict,
}

public sealed class AvailabilityEntryWriteStoreResult
{
    private AvailabilityEntryWriteStoreResult(
        AvailabilityEntryWriteStoreStatus status,
        long? changeVersion)
    {
        Status = status;
        ChangeVersion = changeVersion;
    }

    public static AvailabilityEntryWriteStoreResult Conflict { get; } = new(
        AvailabilityEntryWriteStoreStatus.Conflict,
        null);

    public AvailabilityEntryWriteStoreStatus Status { get; }

    public long? ChangeVersion { get; }

    public static AvailabilityEntryWriteStoreResult Succeeded(long changeVersion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(changeVersion);
        return new AvailabilityEntryWriteStoreResult(
            AvailabilityEntryWriteStoreStatus.Succeeded,
            changeVersion);
    }
}

public enum AvailabilityEntryRemoveStoreResult
{
    Succeeded,
    Conflict,
}
