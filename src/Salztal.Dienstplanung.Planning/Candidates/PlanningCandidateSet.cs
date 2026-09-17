using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Planning.Candidates;

internal sealed class PlanningCandidateSet
{
    public PlanningCandidateSet(
        IEnumerable<PlanningAssignmentCandidate> candidates,
        IEnumerable<PlanningRemainingDemand> remainingDemands)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(remainingDemands);

        PlanningAssignmentCandidate[] orderedCandidates = candidates
            .OrderBy(candidate => candidate.TechnicalKey, StringComparer.Ordinal)
            .ToArray();
        if (orderedCandidates.Select(candidate => candidate.TechnicalKey).Distinct(
                StringComparer.Ordinal).Count() != orderedCandidates.Length)
        {
            throw new ArgumentException("Candidate technical keys must be unique.", nameof(candidates));
        }

        Candidates = Array.AsReadOnly(orderedCandidates);
        RemainingDemands = Array.AsReadOnly(remainingDemands
            .OrderBy(demand => demand.Demand)
            .ThenBy(demand => demand.UncoveredStart)
            .ThenBy(demand => demand.UncoveredEnd)
            .ToArray());
        EmployeeDayChoices = CreateEmployeeDayChoices(orderedCandidates);
        CoverageChoices = CreateCoverageChoices(orderedCandidates);
    }

    public ReadOnlyCollection<PlanningAssignmentCandidate> Candidates { get; }

    public ReadOnlyCollection<PlanningRemainingDemand> RemainingDemands { get; }

    public ReadOnlyCollection<EmployeeDayCandidateChoices> EmployeeDayChoices { get; }

    public ReadOnlyCollection<DemandCoverageCandidateChoices> CoverageChoices { get; }

    private static ReadOnlyCollection<EmployeeDayCandidateChoices>
        CreateEmployeeDayChoices(IEnumerable<PlanningAssignmentCandidate> candidates)
    {
        return Array.AsReadOnly(candidates
            .GroupBy(candidate => (candidate.EmployeeId, candidate.Date))
            .OrderBy(group => group.Key.EmployeeId)
            .ThenBy(group => group.Key.Date)
            .Select(group => new EmployeeDayCandidateChoices(
                group.Key.EmployeeId,
                group.Key.Date,
                Array.AsReadOnly(group
                    .Select(candidate => candidate.TechnicalKey)
                    .Order(StringComparer.Ordinal)
                    .ToArray())))
            .ToArray());
    }

    private static ReadOnlyCollection<DemandCoverageCandidateChoices>
        CreateCoverageChoices(IEnumerable<PlanningAssignmentCandidate> candidates)
    {
        List<DemandCoverageCandidateChoices> choices = [];
        foreach (IGrouping<PlanningDemandKey, PlanningCandidateCoverage> demandGroup in
                 candidates.SelectMany(candidate => candidate.Coverages)
                     .GroupBy(coverage => coverage.Demand)
                     .OrderBy(group => group.Key))
        {
            TimeOnly[] boundaries = demandGroup
                .SelectMany(coverage => new[]
                {
                    coverage.CoveredStart,
                    coverage.CoveredEnd,
                })
                .Distinct()
                .Order()
                .ToArray();
            for (int index = 0; index < boundaries.Length - 1; index++)
            {
                TimeOnly start = boundaries[index];
                TimeOnly end = boundaries[index + 1];
                string[] candidateKeys = candidates
                    .Where(candidate => candidate.Coverages.Any(coverage =>
                        coverage.Demand == demandGroup.Key
                        && coverage.CoveredStart <= start
                        && coverage.CoveredEnd >= end))
                    .Select(candidate => candidate.TechnicalKey)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (candidateKeys.Length > 0)
                {
                    choices.Add(new DemandCoverageCandidateChoices(
                        demandGroup.Key,
                        start,
                        end,
                        Array.AsReadOnly(candidateKeys)));
                }
            }
        }

        return Array.AsReadOnly(choices.ToArray());
    }
}
