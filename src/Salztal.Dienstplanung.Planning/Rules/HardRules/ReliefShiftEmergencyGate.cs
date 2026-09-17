using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.Rules.HardRules;

internal sealed class ReliefShiftEmergencyGate
{
    public ReliefShiftEmergencyGate(
        IEnumerable<PlanningDemandKey> confirmedRestaurantLateDemands)
    {
        ArgumentNullException.ThrowIfNull(confirmedRestaurantLateDemands);
        ConfirmedRestaurantLateDemands = Array.AsReadOnly(
            confirmedRestaurantLateDemands.Distinct().Order().ToArray());
    }

    public ReadOnlyCollection<PlanningDemandKey> ConfirmedRestaurantLateDemands
    {
        get;
    }

    public bool Allows(PlanningAssignmentCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return candidate.Kind != PlanningCandidateKind.ReliefShiftPattern
            || candidate.Coverages.Count == 2
            && ConfirmedRestaurantLateDemands.Contains(candidate.Coverages[1].Demand);
    }

    public static ReliefShiftEmergencyGate None { get; } = new([]);
}
