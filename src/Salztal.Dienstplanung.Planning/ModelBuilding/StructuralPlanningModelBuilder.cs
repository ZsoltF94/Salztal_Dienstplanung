using Google.OrTools.Sat;
using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.ModelBuilding;

internal static class StructuralPlanningModelBuilder
{
    public static StructuralPlanningModel Build(PlanningCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(candidateSet);

        CpModel model = new();
        Dictionary<string, BoolVar> variables = candidateSet.Candidates.ToDictionary(
            candidate => candidate.TechnicalKey,
            candidate => model.NewBoolVar(candidate.TechnicalKey),
            StringComparer.Ordinal);

        foreach (EmployeeDayCandidateChoices choice in candidateSet.EmployeeDayChoices)
        {
            model.Add(LinearExpr.Sum(choice.CandidateKeys.Select(key =>
                variables[key])) <= 1);
        }

        foreach (DemandCoverageCandidateChoices choice in candidateSet.CoverageChoices)
        {
            model.Add(LinearExpr.Sum(choice.CandidateKeys.Select(key =>
                variables[key])) <= 1);
        }

        return new StructuralPlanningModel(model, candidateSet, variables);
    }
}
