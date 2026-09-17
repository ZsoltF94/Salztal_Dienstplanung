using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Rules.HardRules;

namespace Salztal.Dienstplanung.Planning.Tests.Reference;

internal sealed record DemandCoverageReferenceResult(
    int UncoveredMinutes,
    int FullyUncoveredSlots);

internal static class ExhaustiveDemandCoverageReferenceSolver
{
    public static DemandCoverageReferenceResult Solve(
        PlanningInputSnapshot snapshot,
        PlanningCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(candidateSet);
        if (candidateSet.Candidates.Count > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateSet),
                "The exhaustive reference solver supports at most 20 candidates.");
        }

        PlanningAssignmentCandidate[][] structuralSelections = Enumerate(candidateSet)
            .Where(HasValidStructure)
            .ToArray();
        StructuralPlanningModel referenceModel = StructuralPlanningModelBuilder.Build(
            candidateSet);
        HardRulePlanningContext regularContext = new(
            snapshot,
            referenceModel,
            ReliefShiftEmergencyGate.None);
        PlanningAssignmentCandidate[][] regularSelections = structuralSelections
            .Where(selection => selection.All(candidate =>
                candidate.Kind != PlanningCandidateKind.ReliefShiftPattern))
            .Where(selection => AutomaticHardRuleSelectionEvaluator.Evaluate(
                regularContext,
                selection.Select(candidate => candidate.TechnicalKey)).IsValid)
            .ToArray();
        int maximumRegularMinutes = regularSelections.Max(selection =>
            RegularCoverages(selection).Sum(coverage => coverage.CoveredMinutes));
        int maximumRegularTouched = regularSelections
            .Where(selection => RegularCoverages(selection)
                .Sum(coverage => coverage.CoveredMinutes) == maximumRegularMinutes)
            .Max(selection => CountTouchedFullDemands(candidateSet, RegularCoverages(selection)));

        ReliefShiftEmergencyGate finalGate = new(candidateSet.Candidates
            .Where(candidate => candidate.Kind == PlanningCandidateKind.ReliefShiftPattern)
            .Select(candidate => candidate.Coverages[1].Demand));
        HardRulePlanningContext finalContext = new(
            snapshot,
            referenceModel,
            finalGate);
        return structuralSelections
            .Where(selection => RegularCoverages(selection)
                .Sum(coverage => coverage.CoveredMinutes) == maximumRegularMinutes)
            .Where(selection => CountTouchedFullDemands(
                candidateSet,
                RegularCoverages(selection)) == maximumRegularTouched)
            .Where(selection => AutomaticHardRuleSelectionEvaluator.Evaluate(
                finalContext,
                selection.Select(candidate => candidate.TechnicalKey)).IsValid)
            .Select(selection => EvaluateOpenDemand(candidateSet, selection))
            .OrderBy(result => result.UncoveredMinutes)
            .ThenBy(result => result.FullyUncoveredSlots)
            .First();
    }

    private static IEnumerable<PlanningAssignmentCandidate[]> Enumerate(
        PlanningCandidateSet candidateSet)
    {
        PlanningAssignmentCandidate[] candidates = candidateSet.Candidates.ToArray();
        int combinations = 1 << candidates.Length;
        for (int mask = 0; mask < combinations; mask++)
        {
            yield return candidates
                .Where((_, index) => (mask & (1 << index)) != 0)
                .ToArray();
        }
    }

    private static bool HasValidStructure(
        PlanningAssignmentCandidate[] selected)
    {
        if (selected.GroupBy(candidate => (candidate.EmployeeId, candidate.Date))
            .Any(group => group.Count() > 1))
        {
            return false;
        }

        return !selected.SelectMany(candidate => candidate.Coverages)
            .GroupBy(coverage => coverage.Demand)
            .Any(group => group.Any(first => group.Any(second =>
                !ReferenceEquals(first, second)
                && first.CoveredStart < second.CoveredEnd
                && second.CoveredStart < first.CoveredEnd)));
    }

    private static IEnumerable<PlanningCandidateCoverage> RegularCoverages(
        IEnumerable<PlanningAssignmentCandidate> selected) => selected.SelectMany(candidate =>
        candidate.Kind == PlanningCandidateKind.ReliefShiftPattern
            ? candidate.Coverages.Take(1)
            : candidate.Coverages);

    private static int CountTouchedFullDemands(
        PlanningCandidateSet candidateSet,
        IEnumerable<PlanningCandidateCoverage> coverages)
    {
        HashSet<PlanningDemandKey> fullyUncovered = candidateSet.RemainingDemands
            .Where(demand => demand.Kind == PlanningRemainingDemandKind.FullyUncovered)
            .Select(demand => demand.Demand)
            .ToHashSet();
        return coverages.Select(coverage => coverage.Demand)
            .Distinct()
            .Count(fullyUncovered.Contains);
    }

    private static DemandCoverageReferenceResult EvaluateOpenDemand(
        PlanningCandidateSet candidateSet,
        IEnumerable<PlanningAssignmentCandidate> selected)
    {
        Dictionary<PlanningDemandKey, PlanningCandidateCoverage> coverages = selected
            .SelectMany(candidate => candidate.Coverages)
            .ToDictionary(coverage => coverage.Demand);
        int uncoveredMinutes = 0;
        int fullyUncoveredSlots = 0;
        foreach (PlanningRemainingDemand remaining in candidateSet.RemainingDemands)
        {
            if (!coverages.TryGetValue(
                    remaining.Demand,
                    out PlanningCandidateCoverage? coverage))
            {
                uncoveredMinutes += remaining.UncoveredMinutes;
                fullyUncoveredSlots += remaining.Kind
                    == PlanningRemainingDemandKind.FullyUncovered ? 1 : 0;
                continue;
            }

            int coveredMinutes = Math.Min(
                remaining.UncoveredMinutes,
                coverage.CoveredMinutes);
            uncoveredMinutes += remaining.UncoveredMinutes - coveredMinutes;
        }

        return new DemandCoverageReferenceResult(
            uncoveredMinutes,
            fullyUncoveredSlots);
    }
}
