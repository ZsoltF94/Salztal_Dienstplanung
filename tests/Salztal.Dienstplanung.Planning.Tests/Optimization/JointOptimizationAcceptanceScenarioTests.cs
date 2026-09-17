using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.Optimization;

public sealed class JointOptimizationAcceptanceScenarioTests
{
    private static readonly Guid AuxiliaryEmployeeB = Guid.Parse(
        "17000000-0000-0000-0000-000000000002");

    [Fact]
    public void ScarceNormalDemandIsSharedBetweenComparableAuxiliaries()
    {
        PlanningEmployeeTypeSnapshot auxiliaryType = AuxiliaryType();
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [
                AuxiliaryEmployee(CandidateScenarioFactory.AuxiliaryEmployeeId),
                AuxiliaryEmployee(AuxiliaryEmployeeB),
            ],
            [auxiliaryType],
            [Slot(0, 1, 300), Slot(1, 2, 300)]);

        DemandCoverageOptimizationResult result = OptimizeAndMatchReference(snapshot);

        Assert.Equal(0, result.UncoveredEmployeeMinutes);
        Assert.Equal(0, result.AuxiliaryWeeklyMinimum.ViolatedWeekCount);
        Assert.Equal(
            [300, 300],
            result.SelectedCandidates.GroupBy(item => item.EmployeeId)
                .Select(group => group.Sum(item => item.WorkMinutes))
                .Order()
                .ToArray());
        Assert.Equal(0, result.ReliefShiftAssignmentCount);
        Assert.Equal(0, result.SplitShiftAssignmentCount);
    }

    [Fact]
    public void OversizedAuxiliaryDemandRemainsVisiblyUncovered()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [AuxiliaryEmployee(CandidateScenarioFactory.AuxiliaryEmployeeId)],
            [AuxiliaryType()],
            [Slot(0, 1, 721)]);

        DemandCoverageOptimizationResult result = OptimizeAndMatchReference(snapshot);

        Assert.Empty(result.SelectedCandidates);
        Assert.Equal(721, result.UncoveredEmployeeMinutes);
        Assert.Equal(1, result.FullyUncoveredDemandSlotCount);
    }

    [Fact]
    public void SplitShiftIsUsedOnlyWhenItImprovesCoverage()
    {
        PlanningEmployeeSnapshot normal = NormalEmployee();
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [normal],
            [NormalType()],
            [
                DemandCoverageOptimizerTests.Slot(
                    CandidateScenarioFactory.Saturday,
                    1,
                    CandidateScenarioFactory.EarlyShiftId,
                    180,
                    startHour: 6),
                DemandCoverageOptimizerTests.Slot(
                    CandidateScenarioFactory.Saturday,
                    2,
                    CandidateScenarioFactory.LateShiftId,
                    180,
                    startHour: 16),
            ]);

        DemandCoverageOptimizationResult result = OptimizeAndMatchReference(snapshot);

        PlanningAssignmentCandidate assignment = Assert.Single(result.SelectedCandidates);
        Assert.Equal(PlanningCandidateKind.SplitShiftPattern, assignment.Kind);
        Assert.Equal(0, result.UncoveredEmployeeMinutes);
        Assert.Equal(1, result.SplitShiftAssignmentCount);
        Assert.Equal(0, result.ReliefShiftAssignmentCount);
    }

    [Fact]
    public void ReliefShiftKeepsItsEarlyGapVisible()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [NormalEmployee()],
            [NormalType()],
            [
                DemandCoverageOptimizerTests.Slot(
                    CandidateScenarioFactory.Saturday,
                    1,
                    CandidateScenarioFactory.LateShiftId,
                    180,
                    startHour: 16,
                    startMinute: 30),
                DemandCoverageOptimizerTests.Slot(
                    CandidateScenarioFactory.Saturday,
                    2,
                    CandidateScenarioFactory.CafeteriaBShiftId,
                    240,
                    startHour: 13,
                    startMinute: 30,
                    locationId: CandidateScenarioFactory.CafeteriaId),
            ],
            enableRelief: true);

        DemandCoverageOptimizationResult result = OptimizeAndMatchReference(snapshot);

        Assert.Equal(1, result.ReliefShiftAssignmentCount);
        Assert.Equal(0, result.SplitShiftAssignmentCount);
        Assert.Equal(60, result.UncoveredEmployeeMinutes);
        AutomaticScheduleOpenDemand open = Assert.Single(result.OpenDemands);
        Assert.Equal(new TimeOnly(16, 30), open.UncoveredStart);
        Assert.Equal(new TimeOnly(17, 30), open.UncoveredEnd);
    }

    private static DemandCoverageOptimizationResult OptimizeAndMatchReference(
        PlanningInputSnapshot snapshot)
    {
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(snapshot);
        ReferenceScheduleCandidate<IReadOnlyCollection<string>> reference =
            ExhaustivePlanningSelectionReferenceSolver.SelectBest(snapshot, candidates);
        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            candidates);

        string[] selectedKeys = result.SelectedCandidates
            .Select(item => item.TechnicalKey)
            .Order(StringComparer.Ordinal)
            .ToArray();
        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(
            ExhaustivePlanningSelectionReferenceSolver.CreateObjective(
                snapshot,
                candidates,
                selectedKeys),
            reference.ObjectiveVector);
        Assert.True(
            comparison.Outcome == ScheduleObjectiveComparisonOutcome.Equivalent
            || comparison.DecisiveStage == ScheduleObjectiveStage.TechnicalTieBreaker,
            $"The solver and reference differ at {comparison.DecisiveStage}.");
        return result;
    }

    private static PlanningInputSnapshot CreateSnapshot(
        IEnumerable<PlanningEmployeeSnapshot> employees,
        IEnumerable<PlanningEmployeeTypeSnapshot> types,
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        bool enableRelief = false) => DemandCoverageOptimizerTests.Clone(
        CandidateScenarioFactory.Create(enableAuxiliaryReliefShift: enableRelief),
        employees,
        types,
        slots,
        []);

    private static PlanningEmployeeSnapshot AuxiliaryEmployee(Guid id) => new(
        id,
        "Synthetisch",
        "AH",
        CandidateScenarioFactory.AuxiliaryTypeId);

    private static PlanningEmployeeSnapshot NormalEmployee() =>
        CandidateScenarioFactory.CreateEmployees().Single(item =>
            item.Id == CandidateScenarioFactory.NormalEmployeeId);

    private static PlanningEmployeeTypeSnapshot AuxiliaryType() =>
        CandidateScenarioFactory.CreateEmployeeTypes().Single(item =>
            item.Id == CandidateScenarioFactory.AuxiliaryTypeId);

    private static PlanningEmployeeTypeSnapshot NormalType() =>
        CandidateScenarioFactory.CreateEmployeeTypes().Single(item =>
            item.Id == CandidateScenarioFactory.NormalTypeId);

    private static ScheduleDemandSlotSnapshot Slot(
        int dayOffset,
        int number,
        int minutes) => DemandCoverageOptimizerTests.Slot(
        CandidateScenarioFactory.Create().PeriodMonday.AddDays(dayOffset),
        number,
        CandidateScenarioFactory.LateShiftId,
        minutes,
        startHour: 6);
}
