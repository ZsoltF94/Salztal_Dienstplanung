using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandDayWorkLocationSummary
{
    internal StaffingDemandDayWorkLocationSummary(
        DateOnly date,
        WorkLocationId workLocationId,
        long requiredWorkMinutes)
    {
        Date = date;
        WorkLocationId = workLocationId;
        RequiredWorkMinutes = requiredWorkMinutes;
    }

    public DateOnly Date { get; }

    public WorkLocationId WorkLocationId { get; }

    public long RequiredWorkMinutes { get; }
}
