using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

public sealed record AutomaticScheduleObjectiveScenario(
    string Name,
    IReadOnlyList<ReferenceScheduleCandidate<string>> Candidates,
    string ExpectedBestKey);

internal static class AutomaticScheduleObjectiveScenarios
{
    internal static IReadOnlyList<AutomaticScheduleObjectiveScenario> All { get; } =
    [
        new AutomaticScheduleObjectiveScenario(
            "Uncovered minutes dominate every soft improvement",
            [
                Candidate(
                    "less-open-time",
                    Vector(
                        uncoveredMinutes: 60,
                        high: Violations(
                            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 600)))),
                Candidate("more-open-time", Vector(uncoveredMinutes: 61)),
            ],
            "less-open-time"),
        new AutomaticScheduleObjectiveScenario(
            "High priority dominates many medium improvements",
            [
                Candidate(
                    "high-satisfied",
                    Vector(
                        medium: Violations(
                            (InitialSoftRuleDefinitions.MinimizeSplitShifts, 20),
                            (InitialSoftRuleDefinitions.ThreeWeekFreeWeekend, 20)))),
                Candidate(
                    "medium-satisfied",
                    Vector(
                        high: Violations(
                            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 1)))),
            ],
            "high-satisfied"),
        new AutomaticScheduleObjectiveScenario(
            "Different high rule magnitudes have no hidden order",
            [
                Candidate(
                    "later-stage-better",
                    Vector(
                        high: Violations(
                            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 60),
                            (InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum, 2)))),
                Candidate(
                    "later-stage-worse",
                    Vector(
                        high: Violations(
                            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, 120),
                            (InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum, 1)),
                        medium: Violations(
                            (InitialSoftRuleDefinitions.MinimizeSplitShifts, 1)))),
            ],
            "later-stage-better"),
        new AutomaticScheduleObjectiveScenario(
            "Infeasible candidates never win",
            [
                Candidate("feasible", Vector(uncoveredMinutes: 420)),
                Candidate("infeasible", Vector(), isFeasible: false),
            ],
            "feasible"),
    ];

    private static ReferenceScheduleCandidate<string> Candidate(
        string key,
        ScheduleObjectiveVector vector,
        bool isFeasible = true)
    {
        return new ReferenceScheduleCandidate<string>(key, isFeasible, vector, key);
    }

    private static ScheduleObjectiveVector Vector(
        int uncoveredMinutes = 0,
        RuleViolationSet? high = null,
        RuleViolationSet? medium = null)
    {
        return new ScheduleObjectiveVector(
            uncoveredMinutes,
            0,
            high ?? RuleViolationSet.Empty,
            medium ?? RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            ["technical-key"]);
    }

    private static RuleViolationSet Violations(
        params (RuleDefinition Rule, long Magnitude)[] values)
    {
        return new RuleViolationSet(
            values.Select((value, index) => new RuleViolationCase(
                value.Rule,
                $"case-{index + 1}",
                new RuleViolationMagnitude(value.Magnitude))));
    }
}
