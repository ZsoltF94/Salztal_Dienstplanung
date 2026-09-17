using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Mapping;
using Salztal.Dienstplanung.Planning.ModelBuilding;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Rules.HardRules;
using Salztal.Dienstplanung.Planning.Rules.SoftRules;
using Salztal.Dienstplanung.Planning.Rules.Stability;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.Rules.SoftRules;

public sealed class JointWeeklyObjectiveOptimizationTests
{
    private static readonly Guid AuxiliaryEmployeeB = Guid.Parse(
        "16000000-0000-0000-0000-000000000002");

    [Theory]
    [InlineData(300, true, false)]
    [InlineData(360, false, false)]
    [InlineData(600, false, false)]
    [InlineData(660, false, true)]
    [InlineData(720, false, true)]
    public void WeeklyNoticeThresholdsUseExclusiveLowAndHighBoundaries(
        int minutes,
        bool expectsLowNotice,
        bool expectsHighNotice)
    {
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
        [
            Slot(snapshotMondayOffset: 0, number: 1, minutes),
        ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        PlanningAssignmentCandidate assignment = Assert.Single(
            result.SelectedCandidates);
        Assert.Equal(minutes, assignment.WorkMinutes);
        Assert.Equal(
            expectsLowNotice,
            NoticeWeeks(result, InitialNoticeRuleDefinitions.AuxiliaryWeeklyLow)
                .Any(item => item.WeekMonday == snapshot.PeriodMonday));
        Assert.Equal(
            expectsHighNotice,
            NoticeWeeks(result, InitialNoticeRuleDefinitions.AuxiliaryWeeklyHigh)
                .Any(item => item.WeekMonday == snapshot.PeriodMonday));
    }

    [Fact]
    public void WeeklyMaximumLeavesMoreThanTwelveHoursOpen()
    {
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
        [
            Slot(snapshotMondayOffset: 0, number: 1, minutes: 721),
        ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Empty(result.SelectedCandidates);
        Assert.Equal(721, Assert.Single(result.OpenDemands).UncoveredMinutes);
        Assert.DoesNotContain(result.HardRuleEvaluation.Results, item =>
            item.RuleId == InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum.Id
            && item.Status == RuleEvaluationStatus.Violated);
    }

    [Fact]
    public void NormalWeeklyMinimumRemainsAheadOfAuxiliaryMinimum()
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningEmployeeSnapshot normal = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeSnapshot auxiliary = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.AuxiliaryEmployeeId);
        PlanningEmployeeTypeSnapshot normalType = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == normal.EmployeeTypeId);
        PlanningEmployeeTypeSnapshot auxiliaryType = CandidateScenarioFactory
            .CreateEmployeeTypes().Single(item => item.Id == auxiliary.EmployeeTypeId);
        PlanningInputSnapshot snapshot = DemandCoverageOptimizerTests.Clone(
            source,
            [normal, auxiliary],
            [normalType, auxiliaryType],
            [Slot(0, 1, 180)],
            []);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        PlanningAssignmentCandidate selected = Assert.Single(result.SelectedCandidates);
        Assert.Equal(normal.Id, selected.EmployeeId);
        Assert.Equal(1, result.AuxiliaryWeeklyMinimum.ViolatedWeekCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void MultipleComparableAuxiliaryEmployeesShareRemainingMinutesFairly(int slotCount)
    {
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
            Enumerable.Range(0, slotCount).Select(index =>
                Slot(index, index + 1, 300)),
            twoEmployees: true);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(0, result.AuxiliaryWeeklyMinimum.ViolatedWeekCount);

        int[] totals = result.SelectedCandidates
            .GroupBy(candidate => candidate.EmployeeId)
            .Select(group => group.Sum(candidate => candidate.WorkMinutes))
            .Order()
            .ToArray();
        Assert.Equal(slotCount == 2 ? [300, 300] : [600, 600], totals);
        long[] deviations = result.RelativeWeeklyTarget.ComparableCases
            .Where(item => item.WeekMonday == snapshot.PeriodMonday)
            .Select(item => item.AbsoluteDeviationMinutes)
            .Order()
            .ToArray();
        Assert.Equal(slotCount == 2 ? [300L, 300L] : [0L, 0L], deviations);
    }

    [Fact]
    public void HighPriorityFixedDayOffAdjacencyWinsBeforeAuxiliaryMinimum()
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningAvailabilityEntrySnapshot blockedFirstDay = new(
            AuxiliaryEmployeeB,
            source.PeriodMonday,
            Domain.Availabilities.AvailabilityEntryKind.FixedDayOff,
            1);
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
        [
            Slot(0, 1, 360),
            Slot(1, 2, 360),
        ],
            twoEmployees: true,
            availabilityEntries: [blockedFirstDay]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(1, result.AuxiliaryWeeklyMinimum.ViolatedWeekCount);
        int[] totals = result.SelectedCandidates
            .GroupBy(candidate => candidate.EmployeeId)
            .Select(group => group.Sum(candidate => candidate.WorkMinutes))
            .Order()
            .ToArray();
        Assert.Equal([720], totals);
    }

    [Fact]
    public void NamesAndInputOrderDoNotChangeAuxiliaryTechnicalSelection()
    {
        PlanningInputSnapshot original = CreateAuxiliarySnapshot(
        [
            Slot(0, 1, 300),
            Slot(1, 2, 300),
        ],
            twoEmployees: true);
        PlanningEmployeeSnapshot[] renamedAndReordered = original.Employees
            .Reverse()
            .Select((employee, index) => new PlanningEmployeeSnapshot(
                employee.Id,
                index == 0 ? "Zeta" : "Alpha",
                index == 0 ? "Z" : "A",
                employee.EmployeeTypeId))
            .ToArray();
        PlanningInputSnapshot changed = DemandCoverageOptimizerTests.Clone(
            original,
            renamedAndReordered,
            original.EmployeeTypes,
            original.DemandSlots.Reverse(),
            original.AvailabilityEntries);

        Assert.Equal(
            Optimize(original).SelectedCandidates.Select(item => item.TechnicalKey),
            Optimize(changed).SelectedCandidates.Select(item => item.TechnicalKey));
    }

    [Fact]
    public void IndependentEvaluationIncludesNormalAndAuxiliaryInOneSelection()
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningEmployeeSnapshot normal = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeSnapshot auxiliary = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.AuxiliaryEmployeeId);
        PlanningEmployeeTypeSnapshot normalType = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == normal.EmployeeTypeId);
        PlanningEmployeeTypeSnapshot auxiliaryType = CandidateScenarioFactory
            .CreateEmployeeTypes().Single(item => item.Id == auxiliary.EmployeeTypeId);
        PlanningInputSnapshot snapshot = DemandCoverageOptimizerTests.Clone(
            source,
            [normal, auxiliary],
            [normalType, auxiliaryType],
            [Slot(0, 1, 180)],
            []);
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(snapshot);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidates);
        HardRulePlanningContext context = AutomaticHardRuleModelBuilder.Apply(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);
        PlanningAssignmentCandidate normalCandidate = candidates.Candidates.Single(candidate =>
            candidate.EmployeeId == normal.Id);
        PlanningAssignmentCandidate auxiliaryCandidate = candidates.Candidates.Single(candidate =>
            candidate.EmployeeId == auxiliary.Id);

        JointPlanningSelectionEvaluation evaluation =
            JointPlanningSelectionEvaluator.Evaluate(
                context,
                [auxiliaryCandidate.TechnicalKey]);

        Assert.Contains(evaluation.RelativeWeeklyTarget.Cases, item =>
            item.EmployeeId == normalCandidate.EmployeeId
            && item.AssignedMinutes == 0);
        Assert.Contains(evaluation.RelativeWeeklyTarget.Cases, item =>
            item.EmployeeId == auxiliaryCandidate.EmployeeId
            && item.AssignedMinutes == 180);
    }

    [Fact]
    public void IndependentHardRuleEvaluationDetectsRemovedAuxiliaryMaximum()
    {
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
        [
            Slot(0, 1, 300),
            Slot(1, 2, 300),
            Slot(2, 3, 300),
        ]);
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(snapshot);
        StructuralPlanningModel model = StructuralPlanningModelBuilder.Build(candidates);
        HardRulePlanningContext context = AutomaticHardRuleModelBuilder.Apply(
            snapshot,
            model,
            ReliefShiftEmergencyGate.None);

        AutomaticHardRuleSelectionEvaluation evaluation =
            AutomaticHardRuleSelectionEvaluator.Evaluate(
                context,
                candidates.Candidates.Select(candidate => candidate.TechnicalKey));

        Assert.Contains(evaluation.Results, item =>
            item.RuleId == InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum.Id
            && item.Status == RuleEvaluationStatus.Violated);
    }

    [Fact]
    public void ProposalContainsStructuredEmployeeWeekMinutesWithoutGermanText()
    {
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
        [
            Slot(0, 1, 300),
        ]);
        DemandCoverageOptimizationResult optimization = Optimize(snapshot);

        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(
            new DeterministicAutomaticScheduleAssignmentIdFactory()).Map(
                snapshot,
                optimization,
                Metadata());

        RuleEvaluationResult low = proposal.RuleEvaluations.NoticeResults.Single(result =>
            result.RuleId == InitialNoticeRuleDefinitions.AuxiliaryWeeklyLow.Id);
        WeeklyMinutesNoticeResultParameters parameters =
            Assert.IsType<WeeklyMinutesNoticeResultParameters>(low.Parameters);
        EmployeeWeekMinutesResult firstWeek = parameters.EmployeeWeeks.Single(item =>
            item.WeekMonday == snapshot.PeriodMonday);
        Assert.Equal(CandidateScenarioFactory.AuxiliaryEmployeeId, firstWeek.EmployeeId);
        Assert.Equal(300, firstWeek.Minutes);
        Assert.Equal(0, proposal.ObjectiveVector.AuxiliaryWeeklyMinimum.ViolatedWeekCount);
        Assert.Contains(proposal.ObjectiveVector.RelativeWeeklyTarget.Cases, item =>
            item.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId
            && item.WeekMonday == snapshot.PeriodMonday
            && item.AssignedMinutes == 300
            && item.TargetMinutes == 600);
    }

    [Fact]
    public void JointObjectivesExposeExactIndependentWeeklyValues()
    {
        PlanningInputSnapshot snapshot = CreateAuxiliarySnapshot(
        [
            Slot(0, 1, 300),
            Slot(1, 2, 300),
            Slot(2, 3, 300),
        ],
            twoEmployees: true);
        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(900, result.SelectedCandidates.Sum(item => item.WorkMinutes));
        Assert.Equal(0, result.UncoveredEmployeeMinutes);
        Assert.Equal(0, result.AuxiliaryWeeklyMinimum.ViolatedWeekCount);
        Assert.Equal(
            [300, 600],
            result.RelativeWeeklyTarget.Cases
                .Where(item => item.WeekMonday == snapshot.PeriodMonday)
                .Select(item => item.AssignedMinutes)
                .Order()
                .ToArray());
    }

    [Fact]
    public void JointOptimizationMatchesExhaustiveReferenceAndImprovesOldPhaseFreeze()
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningEmployeeSnapshot normal = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeSnapshot auxiliary = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.AuxiliaryEmployeeId);
        PlanningEmployeeTypeSnapshot[] types = CandidateScenarioFactory.CreateEmployeeTypes()
            .Where(item => item.Id == normal.EmployeeTypeId
                || item.Id == auxiliary.EmployeeTypeId)
            .ToArray();
        ScheduleAssignmentSnapshot[] protectedNormalWork = Enumerable.Range(0, 5)
            .Select(index => ProtectedWork(normal.Id, source.PeriodMonday.AddDays(index), index + 1))
            .ToArray();
        PlanningInputSnapshot snapshot = new(
            source.Id,
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            [normal, auxiliary],
            types,
            source.ServiceCatalog,
            [],
            [
                DemandCoverageOptimizerTests.Slot(
                    source.PeriodMonday,
                    1,
                    CandidateScenarioFactory.LateShiftId,
                    180,
                    startHour: 16),
            ],
            protectedNormalWork,
            source.RuleCatalog,
            source.RunOptions,
            source.History);
        PlanningCandidateSet candidateSet = PlanningCandidateBuilder.Build(snapshot);
        Assert.Contains(candidateSet.Candidates, item => item.EmployeeId == normal.Id);
        Assert.Contains(candidateSet.Candidates, item => item.EmployeeId == auxiliary.Id);
        string[] auxiliaryKeys = candidateSet.Candidates
            .Where(item => item.EmployeeId == auxiliary.Id)
            .Select(item => item.TechnicalKey)
            .ToArray();
        string[] normalKeys = candidateSet.Candidates
            .Where(item => item.EmployeeId == normal.Id)
            .Select(item => item.TechnicalKey)
            .ToArray();
        ScheduleObjectiveVector auxiliaryObjective =
            ExhaustivePlanningSelectionReferenceSolver.CreateObjective(
            snapshot,
            candidateSet,
            auxiliaryKeys);
        ScheduleObjectiveVector normalObjective =
            ExhaustivePlanningSelectionReferenceSolver.CreateObjective(
            snapshot,
            candidateSet,
            normalKeys);
        Assert.Equal(
            normalObjective.HighPriorityViolations.Violations.Select(item =>
                $"{item.RuleId.Value}:{item.Magnitude.Value}"),
            auxiliaryObjective.HighPriorityViolations.Violations.Select(item =>
                $"{item.RuleId.Value}:{item.Magnitude.Value}"));
        ScheduleObjectiveComparison expectedComparison = ScheduleObjectiveComparer.Compare(
            auxiliaryObjective,
            normalObjective);
        Assert.Equal(
            ScheduleObjectiveStage.AuxiliaryWeeklyMinimum,
            expectedComparison.DecisiveStage);
        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, expectedComparison.Outcome);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            candidateSet);
        ReferenceScheduleCandidate<IReadOnlyCollection<string>> reference =
            ExhaustivePlanningSelectionReferenceSolver.SelectBest(snapshot, candidateSet);

        string[] actualKeys = result.SelectedCandidates
            .Select(item => item.TechnicalKey)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(reference.Value, actualKeys);
        Assert.Equal(
            0,
            result.AuxiliaryWeeklyMinimum.ViolatedWeekCount);
        Assert.Equal(0, result.SelectedCandidates
            .Where(item => item.EmployeeId == normal.Id)
            .Sum(item => item.WorkMinutes));
        Assert.Equal(180, result.SelectedCandidates
            .Where(item => item.EmployeeId == auxiliary.Id)
            .Sum(item => item.WorkMinutes));
        Assert.Contains(result.RelativeWeeklyTarget.Cases, item =>
            item.EmployeeId == normal.Id
            && item.WeekMonday == snapshot.PeriodMonday
            && item.AssignedMinutes == 1_020);

        string[] oldFrozenKeys = candidateSet.Candidates
            .Where(item => item.EmployeeId == normal.Id)
            .Select(item => item.TechnicalKey)
            .Order(StringComparer.Ordinal)
            .ToArray();
        ScheduleObjectiveComparison comparison = ScheduleObjectiveComparer.Compare(
            ExhaustivePlanningSelectionReferenceSolver.CreateObjective(
                snapshot,
                candidateSet,
                actualKeys),
            ExhaustivePlanningSelectionReferenceSolver.CreateObjective(
                snapshot,
                candidateSet,
                oldFrozenKeys));

        Assert.Equal(ScheduleObjectiveComparisonOutcome.Better, comparison.Outcome);
        Assert.Equal(ScheduleObjectiveStage.AuxiliaryWeeklyMinimum, comparison.DecisiveStage);
    }

    private static DemandCoverageOptimizationResult Optimize(
        PlanningInputSnapshot snapshot) => DemandCoverageOptimizer.Optimize(
        snapshot,
        PlanningCandidateBuilder.Build(snapshot));

    private static ReadOnlyCollection<EmployeeWeekMinutesResult> NoticeWeeks(
        DemandCoverageOptimizationResult result,
        RuleDefinition definition) =>
        Assert.IsType<WeeklyMinutesNoticeResultParameters>(
            result.NoticeRuleResults.Single(item => item.RuleId == definition.Id).Parameters)
        .EmployeeWeeks;

    private static PlanningInputSnapshot CreateAuxiliarySnapshot(
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        bool twoEmployees = false,
        IEnumerable<PlanningAvailabilityEntrySnapshot>? availabilityEntries = null)
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningEmployeeSnapshot auxiliary = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.AuxiliaryEmployeeId);
        PlanningEmployeeTypeSnapshot auxiliaryType = CandidateScenarioFactory
            .CreateEmployeeTypes().Single(item => item.Id == auxiliary.EmployeeTypeId);
        PlanningEmployeeSnapshot[] employees = twoEmployees
            ?
            [
                auxiliary,
                new PlanningEmployeeSnapshot(
                    AuxiliaryEmployeeB,
                    "Synthetisch",
                    "Aushilfe B",
                    auxiliaryType.Id),
            ]
            : [auxiliary];
        return DemandCoverageOptimizerTests.Clone(
            source,
            employees,
            [auxiliaryType],
            slots,
            availabilityEntries ?? []);
    }

    private static ScheduleDemandSlotSnapshot Slot(
        int snapshotMondayOffset,
        int number,
        int minutes) => DemandCoverageOptimizerTests.Slot(
        CandidateScenarioFactory.Create().PeriodMonday.AddDays(snapshotMondayOffset),
        number,
        CandidateScenarioFactory.LateShiftId,
        minutes,
        startHour: 6);

    private static ScheduleAssignmentSnapshot ProtectedWork(
        Guid employeeId,
        DateOnly date,
        int number) => new(
        Guid.Parse($"a6000000-0000-0000-0000-{number:D12}"),
        employeeId,
        date,
        ScheduleAssignmentKindSnapshot.OfficeTime,
        ScheduleAssignmentOriginSnapshot.ServiceManagement,
        null,
        204,
        true,
        [
            new ScheduleAssignmentSegmentSnapshot(
                Guid.Parse($"96000000-0000-0000-0000-{number:D12}"),
                date,
                CandidateScenarioFactory.RestaurantId,
                CandidateScenarioFactory.EarlyShiftId,
                new TimeOnly(5, 0),
                new TimeOnly(8, 24),
                204),
        ],
        []);

    private static AutomaticScheduleRunMetadata Metadata() => new(
        "CP-SAT Test",
        "synthetic",
        AutomaticSchedulePlanningStatus.Optimal,
        TimeSpan.FromSeconds(1),
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        [new AutomaticScheduleSetting("workers", "1")]);
}
