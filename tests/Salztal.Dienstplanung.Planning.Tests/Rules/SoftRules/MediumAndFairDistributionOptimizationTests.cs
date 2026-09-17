using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Optimization;

namespace Salztal.Dienstplanung.Planning.Tests.Rules.SoftRules;

public sealed class MediumAndFairDistributionOptimizationTests
{
    private static readonly Guid EmployeeA = Guid.Parse(
        "13000000-0000-0000-0000-000000000001");
    private static readonly Guid EmployeeB = Guid.Parse(
        "13000000-0000-0000-0000-000000000002");
    private static readonly Guid TypeId = Guid.Parse(
        "23000000-0000-0000-0000-000000000001");

    [Fact]
    public void ThreeWeekFreeWeekendKeepsTheLastCompleteWeekendFree()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            [Slot(monday.AddDays(19), 1, CandidateScenarioFactory.EarlyShiftId)],
            protectedAssignments:
            [
                ProtectedOffice(EmployeeA, monday.AddDays(5), 1),
                ProtectedOffice(EmployeeA, monday.AddDays(12), 2),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(EmployeeB, Assert.Single(result.SelectedCandidates).EmployeeId);
        Assert.DoesNotContain(result.MediumPriorityViolations.Violations, item =>
            item.RuleId == InitialSoftRuleDefinitions.ThreeWeekFreeWeekend.Id);
    }

    [Fact]
    public void MinimizeSplitShiftsChoosesTwoNormalAssignmentsWhenCoverageIsEqual()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            [
                Slot(monday, 1, CandidateScenarioFactory.EarlyShiftId),
                Slot(monday, 2, CandidateScenarioFactory.LateShiftId, startHour: 16),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(2, result.SelectedCandidates.Count);
        Assert.DoesNotContain(result.SelectedCandidates, candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
        Assert.DoesNotContain(result.MediumPriorityViolations.Violations, item =>
            item.RuleId == InitialSoftRuleDefinitions.MinimizeSplitShifts.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FairlyDistributesEarlyAndLateShiftsBetweenEquallyEligibleEmployees(
        bool early)
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        Guid shiftId = early
            ? CandidateScenarioFactory.EarlyShiftId
            : CandidateScenarioFactory.LateShiftId;
        int startHour = early ? 6 : 16;
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            [
                Slot(monday, 1, shiftId, startHour),
                Slot(monday.AddDays(1), 2, shiftId, startHour),
                Slot(monday.AddDays(7), 3, shiftId, startHour),
                Slot(monday.AddDays(14), 4, shiftId, startHour),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        int[] counts = result.SelectedCandidates.GroupBy(candidate => candidate.EmployeeId)
            .Select(group => group.Count())
            .Order()
            .ToArray();
        Assert.Equal([2, 2], counts);
    }

    [Fact]
    public void FairlyDistributesUnavoidableSplitShiftsAcrossTheCurrentPeriod()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            SplitSlots(monday, 1)
                .Concat(SplitSlots(monday.AddDays(7), 3))
                .Concat(SplitSlots(monday.AddDays(8), 5))
                .Concat(SplitSlots(monday.AddDays(14), 7)));
        PlanningCandidateSet all = PlanningCandidateBuilder.Build(snapshot);
        PlanningCandidateSet splitOnly = new(
            all.Candidates.Where(candidate =>
                candidate.Kind == PlanningCandidateKind.SplitShiftPattern),
            all.RemainingDemands);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            splitOnly);

        int[] counts = result.SelectedCandidates.GroupBy(candidate => candidate.EmployeeId)
            .Select(group => group.Count())
            .Order()
            .ToArray();
        Assert.Equal([2, 2], counts);
    }

    [Fact]
    public void FairlyDistributesWeekendAssignmentsAcrossTheCurrentPeriod()
    {
        DateOnly firstSaturday = CandidateScenarioFactory.Saturday;
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            [
                Slot(firstSaturday, 1, CandidateScenarioFactory.EarlyShiftId),
                Slot(firstSaturday.AddDays(1), 2, CandidateScenarioFactory.EarlyShiftId),
                Slot(firstSaturday.AddDays(7), 3, CandidateScenarioFactory.EarlyShiftId),
                Slot(firstSaturday.AddDays(14), 4, CandidateScenarioFactory.EarlyShiftId),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        int[] counts = result.SelectedCandidates.GroupBy(candidate => candidate.EmployeeId)
            .Select(group => group.Count())
            .Order()
            .ToArray();
        Assert.Equal([2, 2], counts);
    }

    [Fact]
    public void FairlyDistributesAllowedReliefShiftsAcrossTheCurrentPeriod()
    {
        DateOnly firstSaturday = CandidateScenarioFactory.Saturday;
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            Enumerable.Range(0, 3).SelectMany(index => ReliefSlots(
                firstSaturday.AddDays(index * 7),
                index * 2 + 1)),
            enableRelief: true);
        PlanningCandidateSet all = PlanningCandidateBuilder.Build(snapshot);
        PlanningCandidateSet reliefOnly = new(
            all.Candidates.Where(candidate =>
                candidate.Kind == PlanningCandidateKind.ReliefShiftPattern
                || candidate.Kind == PlanningCandidateKind.NormalDemand
                && candidate.Coverages[0].Demand.ShiftTypeId
                    == CandidateScenarioFactory.CafeteriaBShiftId),
            all.RemainingDemands);

        DemandCoverageOptimizationResult result = DemandCoverageOptimizer.Optimize(
            snapshot,
            reliefOnly);

        int[] counts = result.SelectedCandidates.GroupBy(candidate => candidate.EmployeeId)
            .Select(group => group.Count())
            .Order()
            .ToArray();
        Assert.Equal([1, 2], counts);
    }

    [Fact]
    public void DoesNotCompareEmployeesWithDifferentCandidateOpportunities()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningInputSnapshot snapshot = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            [
                Slot(monday, 1, CandidateScenarioFactory.EarlyShiftId),
                Slot(monday.AddDays(1), 2, CandidateScenarioFactory.EarlyShiftId),
            ],
            [
                new PlanningAvailabilityEntrySnapshot(
                    EmployeeB,
                    monday,
                    AvailabilityEntryKind.FixedDayOff,
                    1),
            ]);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Empty(result.StabilityViolations.Violations);
    }

    [Fact]
    public void ReorderedInputsProduceTheSameTechnicalSelection()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        ScheduleDemandSlotSnapshot[] slots =
        [
            Slot(monday, 1, CandidateScenarioFactory.EarlyShiftId),
            Slot(monday.AddDays(7), 2, CandidateScenarioFactory.EarlyShiftId),
        ];
        PlanningInputSnapshot ordered = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            slots);
        PlanningInputSnapshot reversed = CreateSnapshot(
            [Employee(EmployeeB), Employee(EmployeeA)],
            slots.Reverse());

        string[] first = Optimize(ordered).SelectedCandidates
            .Select(candidate => candidate.TechnicalKey)
            .ToArray();
        string[] second = Optimize(reversed).SelectedCandidates
            .Select(candidate => candidate.TechnicalKey)
            .ToArray();

        Assert.Equal(first, second);
    }

    [Fact]
    public void JointOptimizationMayAssignAuxiliaryWhenEarlierObjectivesRemainEqual()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        PlanningEmployeeSnapshot auxiliary = new(
            EmployeeB,
            "Synthetisch",
            "AH",
            CandidateScenarioFactory.AuxiliaryTypeId);
        PlanningEmployeeTypeSnapshot auxiliaryType = CandidateScenarioFactory
            .CreateEmployeeTypes()
            .Single(type => type.Id == CandidateScenarioFactory.AuxiliaryTypeId);
        PlanningInputSnapshot source = CandidateScenarioFactory.Create();
        PlanningInputSnapshot snapshot = new(
            source.Id,
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            [Employee(EmployeeA), auxiliary],
            [CreateType(), auxiliaryType],
            source.ServiceCatalog,
            [],
            [Slot(monday, 1, CandidateScenarioFactory.LateShiftId, 16)],
            [],
            source.RuleCatalog,
            source.RunOptions,
            source.History);

        DemandCoverageOptimizationResult result = Optimize(snapshot);

        Assert.Equal(
            auxiliary.Id,
            Assert.Single(result.SelectedCandidates).EmployeeId);
    }

    [Fact]
    public void NamesAndTypeCodesDoNotChangeTheTechnicalSelection()
    {
        DateOnly monday = CandidateScenarioFactory.Saturday.AddDays(-5);
        ScheduleDemandSlotSnapshot[] slots =
        [
            Slot(monday, 1, CandidateScenarioFactory.EarlyShiftId),
            Slot(monday.AddDays(7), 2, CandidateScenarioFactory.EarlyShiftId),
        ];
        PlanningInputSnapshot original = CreateSnapshot(
            [Employee(EmployeeA), Employee(EmployeeB)],
            slots);
        PlanningInputSnapshot renamed = CreateSnapshot(
            [
                new PlanningEmployeeSnapshot(EmployeeA, "Zeta", "Z", TypeId),
                new PlanningEmployeeSnapshot(EmployeeB, "Alpha", "A", TypeId),
            ],
            slots,
            employeeType: CreateType("ZZZ"));

        Assert.Equal(
            Optimize(original).SelectedCandidates.Select(item => item.TechnicalKey),
            Optimize(renamed).SelectedCandidates.Select(item => item.TechnicalKey));
    }

    private static DemandCoverageOptimizationResult Optimize(PlanningInputSnapshot snapshot) =>
        DemandCoverageOptimizer.Optimize(snapshot, PlanningCandidateBuilder.Build(snapshot));

    private static PlanningInputSnapshot CreateSnapshot(
        IEnumerable<PlanningEmployeeSnapshot> employees,
        IEnumerable<ScheduleDemandSlotSnapshot> slots,
        IEnumerable<PlanningAvailabilityEntrySnapshot>? availability = null,
        IEnumerable<ScheduleAssignmentSnapshot>? protectedAssignments = null,
        bool enableRelief = false,
        PlanningEmployeeTypeSnapshot? employeeType = null)
    {
        PlanningInputSnapshot source = CandidateScenarioFactory.Create(
            enableAuxiliaryReliefShift: enableRelief);
        return new PlanningInputSnapshot(
            source.Id,
            source.DraftId,
            source.DraftVersion,
            source.PeriodMonday,
            source.PeriodSunday,
            employees,
            [employeeType ?? CreateType()],
            source.ServiceCatalog,
            availability ?? [],
            slots,
            protectedAssignments ?? [],
            source.RuleCatalog,
            source.RunOptions,
            source.History);
    }

    private static PlanningEmployeeSnapshot Employee(Guid id) => new(
        id,
        "Synthetisch",
        id == EmployeeA ? "A" : "B",
        TypeId);

    private static PlanningEmployeeTypeSnapshot CreateType(string code = "N") => new(
        TypeId,
        code,
        "Synthetischer Normaltyp",
        180,
        true,
        180,
        EmployeeTypePlanningRole.Normal,
        true,
        false,
        false,
        ManualSuggestionPriority.Standard,
        [
            Eligibility(ShiftEligibilityTargetKind.ShiftType, CandidateScenarioFactory.EarlyShiftId),
            Eligibility(ShiftEligibilityTargetKind.ShiftType, CandidateScenarioFactory.LateShiftId),
            Eligibility(ShiftEligibilityTargetKind.ShiftType, CandidateScenarioFactory.CafeteriaBShiftId),
            Eligibility(ShiftEligibilityTargetKind.ShiftPattern, CandidateScenarioFactory.SplitPatternId),
            Eligibility(
                ShiftEligibilityTargetKind.ShiftPattern,
                CandidateScenarioFactory.ReliefPatternId,
                ShiftEligibilityActivation.ExplicitPlanningRunOption),
        ]);

    private static PlanningEmployeeTypeEligibilitySnapshot Eligibility(
        ShiftEligibilityTargetKind kind,
        Guid id,
        ShiftEligibilityActivation activation = ShiftEligibilityActivation.Always) => new(
        kind,
        id,
        ShiftEligibilityMode.Regular,
        activation);

    private static ScheduleDemandSlotSnapshot Slot(
        DateOnly date,
        int number,
        Guid shiftId,
        int startHour = 6) => DemandCoverageOptimizerTests.Slot(
        date,
        number,
        shiftId,
        180,
        startHour);

    private static IEnumerable<ScheduleDemandSlotSnapshot> SplitSlots(
        DateOnly date,
        int firstNumber) =>
    [
        Slot(date, firstNumber, CandidateScenarioFactory.EarlyShiftId),
        Slot(date, firstNumber + 1, CandidateScenarioFactory.LateShiftId, 16),
    ];

    private static IEnumerable<ScheduleDemandSlotSnapshot> ReliefSlots(
        DateOnly date,
        int firstNumber) =>
    [
        DemandCoverageOptimizerTests.Slot(
            date,
            firstNumber,
            CandidateScenarioFactory.CafeteriaBShiftId,
            240,
            13,
            30,
            CandidateScenarioFactory.CafeteriaId),
        DemandCoverageOptimizerTests.Slot(
            date,
            firstNumber + 1,
            CandidateScenarioFactory.LateShiftId,
            180,
            16,
            30),
    ];

    private static ScheduleAssignmentSnapshot ProtectedOffice(
        Guid employeeId,
        DateOnly date,
        int number) => new(
        Guid.Parse($"a3000000-0000-0000-0000-{number:D12}"),
        employeeId,
        date,
        ScheduleAssignmentKindSnapshot.OfficeTime,
        ScheduleAssignmentOriginSnapshot.ServiceManagement,
        null,
        1,
        true,
        [
            new ScheduleAssignmentSegmentSnapshot(
                Guid.Parse($"94000000-0000-0000-0000-{number:D12}"),
                date,
                CandidateScenarioFactory.RestaurantId,
                CandidateScenarioFactory.EarlyShiftId,
                new TimeOnly(5, 0),
                new TimeOnly(5, 1),
                1),
        ],
        []);
}
