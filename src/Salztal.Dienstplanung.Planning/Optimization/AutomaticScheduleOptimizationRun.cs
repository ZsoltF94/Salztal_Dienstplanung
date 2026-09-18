using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Planning.Optimization;

internal sealed class AutomaticScheduleOptimizationRun
{
    public AutomaticScheduleOptimizationRun(
        DemandCoverageOptimizationResult result,
        bool isProvenOptimal,
        TimeSpan optimizationDuration,
        IEnumerable<AutomaticSchedulePhaseSnapshot>? phases = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentOutOfRangeException.ThrowIfLessThan(optimizationDuration, TimeSpan.Zero);
        Result = result;
        IsProvenOptimal = isProvenOptimal;
        OptimizationDuration = optimizationDuration;
        AutomaticSchedulePhaseSnapshot[] phaseValues = (phases ?? []).ToArray();
        if (phaseValues.Any(value => value is null))
        {
            throw new ArgumentException(
                "Optimization phases cannot contain null values.",
                nameof(phases));
        }

        Phases = Array.AsReadOnly(phaseValues);
    }

    public DemandCoverageOptimizationResult Result { get; }

    public bool IsProvenOptimal { get; }

    public TimeSpan OptimizationDuration { get; }

    public ReadOnlyCollection<AutomaticSchedulePhaseSnapshot> Phases { get; }
}
