namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed record ServiceManagementWeekReadiness(
    DateOnly WeekMonday,
    ServiceManagementWeekReadinessStatus Status,
    int WorkMinutes)
{
    public bool IsFullyUnavailable =>
        Status == ServiceManagementWeekReadinessStatus.ExemptFullyUnavailable;

    public bool HasRequiredAssignment =>
        Status == ServiceManagementWeekReadinessStatus.Ready;
}
