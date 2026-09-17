using System.Collections.ObjectModel;
using Google.OrTools.Sat;
using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.ModelBuilding;

internal sealed class StructuralPlanningModel
{
    public StructuralPlanningModel(
        CpModel model,
        PlanningCandidateSet candidateSet,
        IDictionary<string, BoolVar> candidateVariables)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(candidateSet);
        ArgumentNullException.ThrowIfNull(candidateVariables);
        Model = model;
        CandidateSet = candidateSet;
        CandidateVariables = new ReadOnlyDictionary<string, BoolVar>(
            new Dictionary<string, BoolVar>(
                candidateVariables,
                StringComparer.Ordinal));
    }

    public CpModel Model { get; }

    public PlanningCandidateSet CandidateSet { get; }

    public ReadOnlyDictionary<string, BoolVar> CandidateVariables { get; }
}
