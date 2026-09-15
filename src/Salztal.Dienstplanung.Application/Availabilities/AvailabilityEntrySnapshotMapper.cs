using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Application.Availabilities;

internal static class AvailabilityEntrySnapshotMapper
{
    public static AvailabilityPeriodEntrySnapshot Create(
        AvailabilityEntry entry,
        long changeVersion)
    {
        return new AvailabilityPeriodEntrySnapshot(
            entry.Date,
            ToApplicationKind(entry.Kind),
            changeVersion);
    }

    public static AvailabilityEntryKind ToDomainKind(AvailabilityDayEntryKind kind)
    {
        return kind switch
        {
            AvailabilityDayEntryKind.Vacation => AvailabilityEntryKind.Vacation,
            AvailabilityDayEntryKind.Sickness => AvailabilityEntryKind.Sickness,
            AvailabilityDayEntryKind.FixedDayOff => AvailabilityEntryKind.FixedDayOff,
            _ => throw new InvalidOperationException(
                $"Unsupported availability entry kind: {kind}"),
        };
    }

    private static AvailabilityDayEntryKind ToApplicationKind(AvailabilityEntryKind kind)
    {
        return kind switch
        {
            AvailabilityEntryKind.Vacation => AvailabilityDayEntryKind.Vacation,
            AvailabilityEntryKind.Sickness => AvailabilityDayEntryKind.Sickness,
            AvailabilityEntryKind.FixedDayOff => AvailabilityDayEntryKind.FixedDayOff,
            _ => throw new InvalidOperationException(
                $"Unsupported availability entry kind: {kind}"),
        };
    }
}
