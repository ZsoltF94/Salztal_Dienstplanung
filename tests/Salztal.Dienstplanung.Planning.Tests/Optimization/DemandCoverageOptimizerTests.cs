using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.Optimization;

public sealed class DemandCoverageOptimizerTests
{
    [Fact]
    public void MinimizesUncoveredMinutesBeforeFullyUncoveredSlotCount()
    {
        DateOnly date = CandidateScenarioFactory.Saturday;
        PlanningInputSnapshot snapshot = CreateNormalSnapshot(
        [
            Slot(date, 1, CandidateScenarioFactory.LateShiftId, 420),
            Slot(date, 2, CandidateScenarioFactory.LateShiftId, 180),
            Slot(date, 3, CandidateScenarioFactory.LateShiftId, 180),
        ]);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        PlanningAssignmentCandidate selected = Assert.Single(result.SelectedCandidates);
        Assert.Equal(420, selected.WorkMinutes);
        Assert.Equal(360, result.UncoveredEmployeeMinutes);
        Assert.Equal(2, result.FullyUncoveredDemandSlotCount);
    }

    [Fact]
    public void EqualUncoveredMinutesThenMinimizesFullyUncoveredSlots()
    {
        DateOnly date = CandidateScenarioFactory.Saturday;
        PlanningInputSnapshot snapshot = CreateNormalSnapshot(
        [
            Slot(date, 1, CandidateScenarioFactory.EarlyShiftId, 180, startHour: 6),
            Slot(date, 2, CandidateScenarioFactory.LateShiftId, 180, startHour: 16),
            Slot(
                date,
                3,
                CandidateScenarioFactory.CafeteriaBShiftId,
                360,
                startHour: 10,
                locationId: CandidateScenarioFactory.CafeteriaId),
        ]);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        PlanningAssignmentCandidate selected = Assert.Single(result.SelectedCandidates);
        Assert.Equal(PlanningCandidateKind.SplitShiftPattern, selected.Kind);
        Assert.Equal(360, result.UncoveredEmployeeMinutes);
        Assert.Equal(1, result.FullyUncoveredDemandSlotCount);
    }

    [Fact]
    public void ReliefOnlyAddsCoverageAfterRegularOptimumAndKeepsEarlyGapOpen()
    {
        DateOnly date = CandidateScenarioFactory.Saturday;
        ScheduleDemandSlotSnapshot late = Slot(
            date,
            1,
            CandidateScenarioFactory.LateShiftId,
            180,
            startHour: 16,
            startMinute: 30);
        ScheduleDemandSlotSnapshot cafeteria = Slot(
            date,
            2,
            CandidateScenarioFactory.CafeteriaBShiftId,
            240,
            startHour: 13,
            startMinute: 30,
            locationId: CandidateScenarioFactory.CafeteriaId);
        PlanningInputSnapshot snapshot = CreateNormalSnapshot([late, cafeteria]);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        PlanningAssignmentCandidate selected = Assert.Single(result.SelectedCandidates);
        Assert.Equal(PlanningCandidateKind.ReliefShiftPattern, selected.Kind);
        AutomaticScheduleOpenDemand open = Assert.Single(result.OpenDemands);
        Assert.Equal(late.SourceId, open.SourceId);
        Assert.Equal(new TimeOnly(16, 30), open.UncoveredStart);
        Assert.Equal(new TimeOnly(17, 30), open.UncoveredEnd);
        Assert.Equal(60, open.UncoveredMinutes);
        Assert.Equal(
            AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift,
            open.Kind);
    }

    [Fact]
    public void ReliefCannotReplaceBetterRegularRestaurantCoverage()
    {
        DateOnly date = CandidateScenarioFactory.Saturday;
        ScheduleDemandSlotSnapshot late = Slot(
            date,
            1,
            CandidateScenarioFactory.LateShiftId,
            300,
            startHour: 14,
            startMinute: 30);
        ScheduleDemandSlotSnapshot cafeteria = Slot(
            date,
            2,
            CandidateScenarioFactory.CafeteriaBShiftId,
            240,
            startHour: 13,
            startMinute: 30,
            locationId: CandidateScenarioFactory.CafeteriaId);
        PlanningInputSnapshot snapshot = CreateNormalSnapshot([late, cafeteria]);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        PlanningAssignmentCandidate selected = Assert.Single(result.SelectedCandidates);
        Assert.Equal(PlanningCandidateKind.NormalDemand, selected.Kind);
        Assert.Equal(PlanningDemandKey.From(late), selected.Coverages[0].Demand);
        AutomaticScheduleOpenDemand open = Assert.Single(result.OpenDemands);
        Assert.Equal(cafeteria.SourceId, open.SourceId);
        Assert.Equal(240, open.UncoveredMinutes);
    }

    [Fact]
    public void NoEligibleCandidateLeavesDemandFullyOpen()
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.IneligibleEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        PlanningInputSnapshot snapshot = Clone(
            CandidateScenarioFactory.Create(),
            [employee],
            [type],
            [Slot(CandidateScenarioFactory.Saturday, 1, CandidateScenarioFactory.LateShiftId, 180)],
            []);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        Assert.Empty(result.SelectedCandidates);
        AutomaticScheduleOpenDemand open = Assert.Single(result.OpenDemands);
        Assert.Equal(180, open.UncoveredMinutes);
        Assert.Equal(AutomaticScheduleOpenDemandKind.FullyUncovered, open.Kind);
    }

    [Fact]
    public void CpSatCoverageVectorMatchesIndependentExhaustiveReference()
    {
        DateOnly date = CandidateScenarioFactory.Saturday;
        PlanningInputSnapshot snapshot = CreateNormalSnapshot(
        [
            Slot(date, 1, CandidateScenarioFactory.EarlyShiftId, 180, startHour: 6),
            Slot(date, 2, CandidateScenarioFactory.LateShiftId, 180, startHour: 16),
            Slot(
                date,
                3,
                CandidateScenarioFactory.CafeteriaBShiftId,
                360,
                startHour: 10,
                locationId: CandidateScenarioFactory.CafeteriaId),
        ]);
        PlanningCandidateSet candidateSet = PlanningCandidateBuilder.Build(snapshot);
        DemandCoverageReferenceResult reference =
            ExhaustiveDemandCoverageReferenceSolver.Solve(snapshot, candidateSet);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            candidateSet);

        Assert.Equal(reference.UncoveredMinutes, result.UncoveredEmployeeMinutes);
        Assert.Equal(
            reference.FullyUncoveredSlots,
            result.FullyUncoveredDemandSlotCount);
    }

    private static PlanningInputSnapshot CreateNormalSnapshot(
        IEnumerable<ScheduleDemandSlotSnapshot> slots)
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        return Clone(
            CandidateScenarioFactory.Create(enableAuxiliaryReliefShift: true),
            [employee],
            [type],
            slots,
            []);
    }

    internal static PlanningInputSnapshot Clone(
        PlanningInputSnapshot source,
        IEnumerable<PlanningEmployeeSnapshot> employees,
        IEnumerable<PlanningEmployeeTypeSnapshot> types,
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        IEnumerable<PlanningAvailabilityEntrySnapshot> availabilityEntries) => new(
            source.Id,
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            employees,
            types,
            source.ServiceCatalog,
            availabilityEntries,
            slots,
            [],
            source.RuleCatalog,
            source.RunOptions,
            source.History);

    internal static ScheduleDemandSlotSnapshot Slot(
        DateOnly date,
        int number,
        Guid shiftTypeId,
        int minutes,
        int startHour = 6,
        int startMinute = 0,
        Guid? locationId = null)
    {
        TimeOnly start = new(startHour, startMinute);
        return new ScheduleDemandSlotSnapshot(
            Guid.Parse($"93000000-0000-0000-0000-{number:D12}"),
            ScheduleDemandSourceKindSnapshot.Standard,
            date,
            locationId ?? CandidateScenarioFactory.RestaurantId,
            "Synthetischer Ort",
            shiftTypeId,
            "Synthetischer Dienst",
            1,
            start,
            start.AddMinutes(minutes),
            minutes);
    }
}
