namespace Salztal.Dienstplanung.Planning.Optimization;

internal sealed record AutomaticScheduleOptimizationRun(
    DemandCoverageOptimizationResult Result,
    bool IsProvenOptimal,
    TimeSpan OptimizationDuration);
