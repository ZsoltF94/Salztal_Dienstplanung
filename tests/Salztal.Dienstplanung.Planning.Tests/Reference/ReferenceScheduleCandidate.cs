using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

public sealed record ReferenceScheduleCandidate<TValue>(
    string Key,
    bool IsFeasible,
    ScheduleObjectiveVector ObjectiveVector,
    TValue Value);
