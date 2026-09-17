using Google.OrTools.Sat;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.ModelBuilding;

public sealed class StructuralPlanningModelBuilderTests
{
    [Fact]
    public void StableCandidateKeysAreUsedAsVariableNames()
    {
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create());

        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidates);

        Assert.Equal(candidates.Candidates.Count, model.CandidateVariables.Count);
        Assert.Equal(
            candidates.Candidates.Select(candidate => candidate.TechnicalKey),
            model.CandidateVariables.Keys);
        Assert.All(model.CandidateVariables, pair =>
            Assert.Equal(pair.Key, pair.Value.Name()));
    }

    [Fact]
    public void ModelRejectsTwoAssignmentsForSameEmployeeAndDay()
    {
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create());
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidates);
        PlanningAssignmentCandidate[] sameEmployeeDay = candidates.Candidates
            .Where(candidate =>
                candidate.EmployeeId == CandidateScenarioFactory.NormalEmployeeId)
            .Take(2)
            .ToArray();
        model.Model.Add(model.CandidateVariables[sameEmployeeDay[0].TechnicalKey] == 1);
        model.Model.Add(model.CandidateVariables[sameEmployeeDay[1].TechnicalKey] == 1);

        CpSolverStatus status = CreateSolver().Solve(model.Model);

        Assert.Equal(CpSolverStatus.Infeasible, status);
    }

    [Fact]
    public void ModelRejectsOverstaffingAcrossDifferentEmployees()
    {
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create());
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidates);
        ScheduleDemandSlotSnapshot lateSlot = CandidateScenarioFactory.CreateDemandSlots()[1];
        PlanningDemandKey demand = PlanningDemandKey.From(lateSlot);
        PlanningAssignmentCandidate normal = candidates.Candidates.Single(candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.NormalEmployeeId
            && candidate.Kind == PlanningCandidateKind.NormalDemand
            && candidate.Coverages[0].Demand == demand);
        PlanningAssignmentCandidate auxiliary = candidates.Candidates.Single(candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId
            && candidate.Kind == PlanningCandidateKind.NormalDemand
            && candidate.Coverages[0].Demand == demand);
        model.Model.Add(model.CandidateVariables[normal.TechnicalKey] == 1);
        model.Model.Add(model.CandidateVariables[auxiliary.TechnicalKey] == 1);

        CpSolverStatus status = CreateSolver().Solve(model.Model);

        Assert.Equal(CpSolverStatus.Infeasible, status);
    }

    [Fact]
    public void StructuralModelCoverageMatchesIndependentExhaustiveReference()
    {
        PlanningEmployeeSnapshot[] employees = CandidateScenarioFactory.CreateEmployees()
            .Where(employee =>
                employee.Id == CandidateScenarioFactory.NormalEmployeeId
                || employee.Id == CandidateScenarioFactory.AuxiliaryEmployeeId)
            .ToArray();
        PlanningEmployeeTypeSnapshot[] types = CandidateScenarioFactory.CreateEmployeeTypes()
            .Where(type => employees.Any(employee => employee.EmployeeTypeId == type.Id))
            .ToArray();
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(
                employees: employees,
                employeeTypes: types,
                enableAuxiliaryReliefShift: false));
        CandidateSelectionReferenceResult reference =
            ExhaustiveCandidateSelectionReferenceSolver.FindMaximumCoverage(candidates);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidates);
        model.Model.Maximize(LinearExpr.Sum(candidates.Candidates.Select(candidate =>
            model.CandidateVariables[candidate.TechnicalKey]
            * candidate.Coverages.Sum(coverage => coverage.CoveredMinutes))));

        CpSolver solver = CreateSolver();
        CpSolverStatus status = solver.Solve(model.Model);

        Assert.Equal(CpSolverStatus.Optimal, status);
        Assert.Equal(reference.CoveredMinutes, (int)solver.ObjectiveValue);
    }

    private static CpSolver CreateSolver() => new()
    {
        StringParameters = "num_search_workers:1 random_seed:0",
    };
}
