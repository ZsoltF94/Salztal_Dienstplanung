using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Planning;

public static class AutomaticSchedulePlannerFactory
{
    public static IAutomaticSchedulePlanner Create()
    {
        return new AutomaticSchedulePlanner(new AutomaticSchedulePlanningEngine());
    }
}
