using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandWorkLocationWeekSummary
{
    internal StaffingDemandWorkLocationWeekSummary(
        WorkLocationId workLocationId,
        long requiredWorkMinutes)
    {
        WorkLocationId = workLocationId;
        RequiredWorkMinutes = requiredWorkMinutes;
    }

    public WorkLocationId WorkLocationId { get; }

    public long RequiredWorkMinutes { get; }
}
