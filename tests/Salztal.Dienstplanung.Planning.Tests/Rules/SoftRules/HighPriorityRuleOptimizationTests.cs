using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.Rules.SoftRules;

public sealed class HighPriorityRuleOptimizationTests
{
    private static readonly Guid EmployeeA = Guid.Parse(
        "12000000-0000-0000-0000-000000000001");
    private static readonly Guid EmployeeB = Guid.Parse(
        "12000000-0000-0000-0000-000000000002");
    private static readonly Guid TypeA = Guid.Parse(
        "22000000-0000-0000-0000-000000000001");
    private static readonly Guid TypeB = Guid.Parse(
        "22000000-0000-0000-0000-000000000002");

    [Fact]
    public void NormalWeeklyMinimumChoosesAssignmentThatRemovesAViolationCase()
    {
        DateOnly friday = CandidateScenarioFactory.Saturday.AddDays(-1);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA), Employee(EmployeeB, TypeB)],
            [Type(TypeA, EmployeeTypePlanningRole.Normal, 600), Type(TypeB, EmployeeTypePlanningRole.Normal, 1_200)],
            [Slot(friday, 1, CandidateScenarioFactory.EarlyShiftId, 420)]);

        PlanningCandidateSet candidateSet = PlanningCandidateBuilder.Build(snapshot);
        IReadOnlyList<RuleViolationSet> reference =
            ExhaustiveHighPriorityReferenceSolver.FindParetoFrontier(
                snapshot,
                candidateSet,
                selected => EvaluateNormalMinimumIndependently(snapshot, selected));
        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            candidateSet);

        Assert.Equal(EmployeeA, Assert.Single(result.SelectedCandidates).EmployeeId);
        RuleViolationCase violation = Assert.Single(
            result.HighPriorityViolations.Violations,
            item => item.RuleId == InitialSoftRuleDefinitions.NormalWeeklyMinimum.Id
                && item.CaseKey == $"{EmployeeB:N}:20260921");
        Assert.Equal(1_020, violation.Magnitude.Value);
        Assert.Contains(reference, value => ViolationSetsEqual(
            value,
            result.HighPriorityViolations));
    }

    [Fact]
    public void ConsecutiveDaysOffChoosesEmployeeWhoseOnlyFreePairIsNotBroken()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        DateOnly friday = monday.AddDays(4);
        ScheduleAssignmentSnapshot[] protectedAssignments =
        [
            ProtectedOffice(EmployeeA, monday, 1),
            ProtectedOffice(EmployeeA, monday.AddDays(1), 2),
            ProtectedOffice(EmployeeA, monday.AddDays(2), 3),
            ProtectedOffice(EmployeeA, monday.AddDays(3), 4),
            ProtectedOffice(EmployeeA, monday.AddDays(6), 5),
        ];
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA), Employee(EmployeeB, TypeA)],
            [Type(TypeA, EmployeeTypePlanningRole.Normal, 180)],
            [Slot(friday, 1, CandidateScenarioFactory.EarlyShiftId, 180)],
            protectedAssignments: protectedAssignments);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(EmployeeB, Assert.Single(result.SelectedCandidates).EmployeeId);
        Assert.DoesNotContain(result.HighPriorityViolations.Violations, item =>
            item.RuleId == InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff.Id);
    }

    [Fact]
    public void RedXAdjacencyKeepsTheAdjacentGeneratedDayOff()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA), Employee(EmployeeB, TypeA)],
            [Type(TypeA, EmployeeTypePlanningRole.Normal, 180)],
            [Slot(monday.AddDays(1), 1, CandidateScenarioFactory.EarlyShiftId, 180)],
            [new PlanningAvailabilityEntrySnapshot(
                EmployeeA,
                monday,
                AvailabilityEntryKind.FixedDayOff,
                1)]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(EmployeeB, Assert.Single(result.SelectedCandidates).EmployeeId);
        RuleEvaluationResult redResult = Assert.Single(
            result.HighPriorityRuleEvaluation.Results,
            item => item.RuleId
                == InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff.Id);
        Assert.Equal(RuleEvaluationStatus.Satisfied, redResult.Status);
    }

    [Fact]
    public void PreVacationWeekendKeepsSaturdayFreeForMondayVacationStart()
    {
        DateOnly saturday = CandidateScenarioFactory.Saturday;
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA), Employee(EmployeeB, TypeA)],
            [Type(TypeA, EmployeeTypePlanningRole.Normal, 180)],
            [Slot(saturday, 1, CandidateScenarioFactory.EarlyShiftId, 180)],
            [new PlanningAvailabilityEntrySnapshot(
                EmployeeA,
                saturday.AddDays(2),
                AvailabilityEntryKind.Vacation,
                1)]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(EmployeeB, Assert.Single(result.SelectedCandidates).EmployeeId);
        Assert.DoesNotContain(result.HighPriorityViolations.Violations, item =>
            item.RuleId == InitialSoftRuleDefinitions.PreVacationWeekendFree.Id);
    }

    [Fact]
    public void SplitShiftWeeklyMaximumDistributesTwoSplitShifts()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        ScheduleDemandSlotSnapshot[] slots =
        [
            Slot(monday, 1, CandidateScenarioFactory.EarlyShiftId, 180, 6),
            Slot(monday, 2, CandidateScenarioFactory.LateShiftId, 180, 16),
            Slot(monday.AddDays(1), 3, CandidateScenarioFactory.EarlyShiftId, 180, 6),
            Slot(monday.AddDays(1), 4, CandidateScenarioFactory.LateShiftId, 180, 16),
        ];
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA), Employee(EmployeeB, TypeA)],
            [Type(TypeA, EmployeeTypePlanningRole.Normal, 180)],
            slots);

        PlanningCandidateSet allCandidates = PlanningCandidateBuilder.Build(snapshot);
        PlanningCandidateSet splitCandidates = new(
            allCandidates.Candidates.Where(candidate =>
                candidate.Kind == PlanningCandidateKind.SplitShiftPattern),
            allCandidates.RemainingDemands);
        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            splitCandidates);

        Assert.Equal(2, result.SelectedCandidates.Count);
        Assert.All(result.SelectedCandidates, candidate =>
            Assert.Equal(PlanningCandidateKind.SplitShiftPattern, candidate.Kind));
        Assert.Equal(2, result.SelectedCandidates.Select(item => item.EmployeeId).Distinct().Count());
        Assert.DoesNotContain(result.HighPriorityViolations.Violations, item =>
            item.RuleId == InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum.Id);
    }

    [Fact]
    public void DifferentRuleMagnitudesRemainUnweightedAndEquivalent()
    {
        RuleViolationSet left = Violations(
            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, "normal", 600),
            (InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff, "red", 1));
        RuleViolationSet right = Violations(
            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, "normal-a", 420),
            (InitialSoftRuleDefinitions.NormalWeeklyMinimum, "normal-b", 600));
        ScheduleObjectiveVector leftVector = Vector(left);
        ScheduleObjectiveVector rightVector = Vector(right);

        Assert.Equal(
            ScheduleObjectiveComparisonOutcome.Equivalent,
            ScheduleObjectiveComparer.Compare(leftVector, rightVector).Outcome);
        Assert.Equal(
            ScheduleObjectiveComparisonOutcome.Equivalent,
            ScheduleObjectiveComparer.Compare(rightVector, leftVector).Outcome);
    }

    [Fact]
    public void VacationAndSicknessDoNotFormTheRegularDaysOffPair()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA)],
            [Type(TypeA, EmployeeTypePlanningRole.Auxiliary, 1_200)],
            [],
            [
                new PlanningAvailabilityEntrySnapshot(
                    EmployeeA,
                    monday.AddDays(4),
                    AvailabilityEntryKind.Vacation,
                    1),
                new PlanningAvailabilityEntrySnapshot(
                    EmployeeA,
                    monday.AddDays(5),
                    AvailabilityEntryKind.Sickness,
                    1),
            ],
            [
                ProtectedOffice(EmployeeA, monday, 1),
                ProtectedOffice(EmployeeA, monday.AddDays(1), 2),
                ProtectedOffice(EmployeeA, monday.AddDays(2), 3),
                ProtectedOffice(EmployeeA, monday.AddDays(3), 4),
                ProtectedOffice(EmployeeA, monday.AddDays(6), 5),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Contains(result.HighPriorityViolations.Violations, item =>
            item.RuleId == InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff.Id
            && item.CaseKey == $"{EmployeeA:N}:{monday:yyyyMMdd}");
    }

    [Fact]
    public void UnknownWeekendBeforeFirstMondayVacationIsNotFullyEvaluable()
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA, TypeA)],
            [Type(TypeA, EmployeeTypePlanningRole.Auxiliary, 1_200)],
            [],
            [
                new PlanningAvailabilityEntrySnapshot(
                    EmployeeA,
                    source.PeriodMonday,
                    AvailabilityEntryKind.Vacation,
                    1),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        RuleEvaluationResult evaluation = Assert.Single(
            result.HighPriorityRuleEvaluation.Results,
            item => item.RuleId == InitialSoftRuleDefinitions.PreVacationWeekendFree.Id);
        Assert.Equal(RuleEvaluationStatus.NotFullyEvaluable, evaluation.Status);
    }

    private static DemandCoverageOptimizationResult Optimize(PlanningInputSnapshot snapshot) =>
        DemandCoverageOptimizer.Optimize(snapshot, PlanningCandidateBuilder.Build(snapshot));

    private static PlanningInputSnapshot CreateSnapshot(
        IEnumerable<PlanningEmployeeSnapshot> employees,
        IEnumerable<PlanningEmployeeTypeSnapshot> types,
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        IEnumerable<PlanningAvailabilityEntrySnapshot>? availability = null,
        IEnumerable<ScheduleAssignmentSnapshot>? protectedAssignments = null)
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        return new PlanningInputSnapshot(
            source.Id,
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            employees,
            types,
            source.ServiceCatalog,
            availability ?? [],
            slots,
            protectedAssignments ?? [],
            source.RuleCatalog,
            source.RunOptions,
            source.History);
    }

    private static PlanningEmployeeSnapshot Employee(Guid id, Guid typeId) =>
        new(id, "Test", id == EmployeeA ? "A" : "B", typeId);

    private static PlanningEmployeeTypeSnapshot Type(
        Guid id,
        EmployeeTypePlanningRole role,
        int weeklyTarget) => new(
        id,
        id == TypeA ? "A" : "B",
        "Synthetischer Typ",
        weeklyTarget,
        true,
        240,
        role,
        true,
        false,
        false,
        ManualSuggestionPriority.Standard,
        [
            new PlanningEmployeeTypeEligibilitySnapshot(
                ShiftEligibilityTargetKind.ShiftType,
                CandidateScenarioFactory.EarlyShiftId,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always),
            new PlanningEmployeeTypeEligibilitySnapshot(
                ShiftEligibilityTargetKind.ShiftType,
                CandidateScenarioFactory.LateShiftId,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always),
            new PlanningEmployeeTypeEligibilitySnapshot(
                ShiftEligibilityTargetKind.ShiftPattern,
                CandidateScenarioFactory.SplitPatternId,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always),
        ]);

    private static ScheduleDemandSlotSnapshot Slot(
        DateOnly date,
        int number,
        Guid shiftTypeId,
        int minutes,
        int startHour = 6) => DemandCoverageOptimizerTests.Slot(
        date,
        number,
        shiftTypeId,
        minutes,
        startHour);

    private static ScheduleAssignmentSnapshot ProtectedOffice(
        Guid employeeId,
        DateOnly date,
        int number) => new(
        Guid.Parse($"a2000000-0000-0000-0000-{number:D12}"),
        employeeId,
        date,
        ScheduleAssignmentKindSnapshot.OfficeTime,
        ScheduleAssignmentOriginSnapshot.ServiceManagement,
        null,
        1,
        true,
        [
            new ScheduleAssignmentSegmentSnapshot(
                Guid.Parse($"92000000-0000-0000-0000-{number:D12}"),
                date,
                CandidateScenarioFactory.RestaurantId,
                CandidateScenarioFactory.EarlyShiftId,
                new TimeOnly(5, 0),
                new TimeOnly(5, 1),
                1),
        ],
        []);

    private static RuleViolationSet Violations(
        params (RuleDefinition Rule, string Key, int Magnitude)[] values) => new(
        values.Select(value => new RuleViolationCase(
            value.Rule,
            value.Key,
            new RuleViolationMagnitude(value.Magnitude))));

    private static RuleViolationSet EvaluateNormalMinimumIndependently(
        PlanningInputSnapshot snapshot,
        PlanningAssignmentCandidate[] selected)
    {
        Dictionary<Guid, PlanningEmployeeTypeSnapshot> types = snapshot.EmployeeTypes
            .ToDictionary(type => type.Id);
        List<RuleViolationCase> values = [];
        foreach (PlanningEmployeeSnapshot employee in snapshot.Employees)
        {
            PlanningEmployeeTypeSnapshot type = types[employee.EmployeeTypeId];
            if (type.PlanningRole != EmployeeTypePlanningRole.Normal)
            {
                continue;
            }

            for (int week = 0; week < 3; week++)
            {
                DateOnly monday = snapshot.PeriodMonday.AddDays(week * 7);
                int work = selected.Where(candidate =>
                        candidate.EmployeeId == employee.Id
                        && candidate.Date >= monday
                        && candidate.Date <= monday.AddDays(6))
                    .Sum(candidate => candidate.WorkMinutes);
                int magnitude = Math.Max(0, type.WeeklyWorkTargetMinutes - 180 - work);
                if (magnitude > 0)
                {
                    values.Add(new RuleViolationCase(
                        InitialSoftRuleDefinitions.NormalWeeklyMinimum,
                        $"{employee.Id:N}:{monday:yyyyMMdd}",
                        new RuleViolationMagnitude(magnitude)));
                }
            }
        }

        return new RuleViolationSet(values);
    }

    private static bool ViolationSetsEqual(RuleViolationSet left, RuleViolationSet right) =>
        left.Violations.SequenceEqual(right.Violations.Where(item =>
            item.RuleId == InitialSoftRuleDefinitions.NormalWeeklyMinimum.Id));

    private static ScheduleObjectiveVector Vector(RuleViolationSet high) => new(
        0,
        0,
        high,
        RuleViolationSet.Empty,
        RuleViolationSet.Empty,
        RuleViolationSet.Empty,
        []);
}
