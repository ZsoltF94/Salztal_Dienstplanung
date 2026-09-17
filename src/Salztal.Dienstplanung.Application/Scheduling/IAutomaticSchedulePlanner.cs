namespace Salztal.Dienstplanung.Application.Scheduling;

public interface IAutomaticSchedulePlanner
{
    public Task<AutomaticSchedulePlanningResult> PlanAsync(
        AutomaticSchedulePlanningRequest request,
        CancellationToken cancellationToken);
}
