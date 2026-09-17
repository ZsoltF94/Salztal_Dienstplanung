using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Planning;

internal interface IAutomaticSchedulePlanningEngine
{
    public Task<AutomaticSchedulePlanningResult> PlanAsync(
        AutomaticSchedulePlanningRequest request,
        CancellationToken cancellationToken);
}
