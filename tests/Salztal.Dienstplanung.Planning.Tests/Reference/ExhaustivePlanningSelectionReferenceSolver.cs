using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Rules.HardRules;
using Salztal.Dienstplanung.Planning.Rules.SoftRules;
using Salztal.Dienstplanung.Planning.Rules.Stability;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

internal static class ExhaustivePlanningSelectionReferenceSolver
{
    public static ReferenceScheduleCandidate<IReadOnlyCollection<string>> SelectBest(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet) => Assert.IsType<
            ReferenceScheduleCandidate<IReadOnlyCollection<string>>>(
                ExhaustiveScheduleReferenceSolver.SelectBest(
                    EnumerateSelections(snapshot, candidateSet)));

    public static ScheduleObjectiveVector CreateObjective(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        IReadOnlyCollection<string> selectedKeys) =>
        TryCreateObjective(snapshot, candidateSet, selectedKeys)
        ?? throw new InvalidOperationException("The reference selection is infeasible.");

    private static IEnumerable<ReferenceScheduleCandidate<IReadOnlyCollection<string>>>
        EnumerateSelections(
            PlanningInputSnapshot snapshot,
            PlanningCandidateSet candidateSet)
    {
        PlanningAssignmentCandidate[] candidates = candidateSet.Candidates.ToArray();
        if (candidates.Length >= 31)
        {
            throw new ArgumentException(
                "The exhaustive reference solver accepts at most 30 candidates.",
                nameof(candidateSet));
        }

        ScheduleObjectiveVector empty = CreateObjective(snapshot, candidateSet, []);
        for (int mask = 0; mask < 1 << candidates.Length; mask++)
        {
            string[] keys = candidates
                .Where((_, index) => (mask & 1 << index) != 0)
                .Select(item => item.TechnicalKey)
                .Order(StringComparer.Ordinal)
                .ToArray();
            ScheduleObjectiveVector? vector = TryCreateObjective(
                snapshot,
                candidateSet,
                keys);
            yield return new ReferenceScheduleCandidate<IReadOnlyCollection<string>>(
                string.Join('|', keys),
                vector is not null,
                vector ?? empty,
                keys);
        }
    }

    private static ScheduleObjectiveVector? TryCreateObjective(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet,
        IReadOnlyCollection<string> selectedKeys)
    {
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidateSet);
        ReliefShiftEmergencyGate reliefGate = new(candidateSet.Candidates
            .Where(item => item.Kind == PlanningCandidateKind.ReliefShiftPattern)
            .Select(item => item.Coverages[1].Demand));
        HardRulePlanningContext context = AutomaticHardRuleModelBuilder.Apply(
            snapshot,
            model,
            reliefGate);
        PlanningAssignmentCandidate[] selected = candidateSet.Candidates
            .Where(item => selectedKeys.Contains(item.TechnicalKey, StringComparer.Ordinal))
            .ToArray();
        if (selected.GroupBy(item => (item.EmployeeId, item.Date))
                .Any(group => group.Count() > 1)
            || selected.SelectMany(item => item.Coverages)
                .GroupBy(item => item.Demand)
                .Any(group => group.Count() > 1))
        {
            return null;
        }

        AutomaticHardRuleSelectionEvaluation hard =
            AutomaticHardRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        if (!hard.IsValid)
        {
            return null;
        }

        Dictionary<PlanningDemandKey, PlanningCandidateCoverage> coverages = selected
            .SelectMany(item => item.Coverages)
            .ToDictionary(item => item.Demand);
        HighPriorityRuleSelectionEvaluation high =
            HighPriorityRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        MediumPriorityRuleSelectionEvaluation medium =
            MediumPriorityRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        FairDistributionRuleSelectionEvaluation fair =
            FairDistributionRuleSelectionEvaluator.Evaluate(context, selectedKeys);
        JointPlanningSelectionEvaluation joint =
            JointPlanningSelectionEvaluator.Evaluate(context, selectedKeys);
        return new ScheduleObjectiveVector(
            candidateSet.RemainingDemands.Sum(item =>
                coverages.TryGetValue(
                    item.Demand,
                    out PlanningCandidateCoverage? coverage)
                    ? item.UncoveredMinutes - coverage.CoveredMinutes
                    : item.UncoveredMinutes),
            candidateSet.RemainingDemands.Count(item =>
                item.Kind == PlanningRemainingDemandKind.FullyUncovered
                && !coverages.ContainsKey(item.Demand)),
            high.Violations,
            selected.Count(item => item.Kind == PlanningCandidateKind.ReliefShiftPattern),
            selected.Count(item => item.Kind == PlanningCandidateKind.SplitShiftPattern),
            joint.AuxiliaryMinimum,
            joint.RelativeWeeklyTarget,
            medium.Violations,
            RuleViolationSet.Empty,
            fair.Violations,
            selectedKeys);
    }
}
