using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Reference;

namespace Salztal.Dienstplanung.Planning.Tests.Candidates;

public sealed class PlanningCandidateBuilderTests
{
    [Fact]
    public void BuildsAllAndOnlyEligibleNormalSplitAndReliefCandidates()
    {
        PlanningCandidateSet result = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create());

        Assert.Equal(13, result.Candidates.Count);
        Assert.Equal(
            7,
            result.Candidates.Count(candidate =>
                candidate.Kind == PlanningCandidateKind.NormalDemand));
        Assert.Equal(
            4,
            result.Candidates.Count(candidate =>
                candidate.Kind == PlanningCandidateKind.SplitShiftPattern));
        Assert.Equal(
            2,
            result.Candidates.Count(candidate =>
                candidate.Kind == PlanningCandidateKind.ReliefShiftPattern));
        Assert.DoesNotContain(result.Candidates, candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.IneligibleEmployeeId
            || candidate.EmployeeId == CandidateScenarioFactory.ServiceEmployeeId);

        PlanningAssignmentCandidate split = result.Candidates.First(candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId
            && candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
        Assert.Equal(600, split.WorkMinutes);
        Assert.Equal(2, split.Segments.Count);
        Assert.Equal(2, split.Coverages.Count);

        PlanningAssignmentCandidate relief = result.Candidates.First(candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        Assert.Equal(360, relief.WorkMinutes);
        Assert.Equal(new TimeOnly(17, 30), relief.Segments[1].ActualStart);
        Assert.Equal(new TimeOnly(17, 30), relief.Coverages[1].CoveredStart);
        Assert.Equal(120, relief.Coverages[1].CoveredMinutes);
    }

    [Fact]
    public void ExplicitReliefOptionOnlyAddsEligibleAuxiliaryReliefCandidates()
    {
        PlanningCandidateSet disabled = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(enableAuxiliaryReliefShift: false));
        PlanningCandidateSet enabled = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(enableAuxiliaryReliefShift: true));

        Assert.DoesNotContain(disabled.Candidates, candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId
            && candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        Assert.Equal(
            2,
            enabled.Candidates.Count(candidate =>
                candidate.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId
                && candidate.Kind == PlanningCandidateKind.ReliefShiftPattern));
        Assert.Equal(disabled.Candidates.Count + 2, enabled.Candidates.Count);
    }

    [Fact]
    public void NormalCandidatesCoverExactlyOneExistingSlotAtItsActualTime()
    {
        ScheduleDemandSlotSnapshot[] slots = CandidateScenarioFactory.CreateDemandSlots();
        PlanningCandidateSet result = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(demandSlots: slots));
        Dictionary<PlanningDemandKey, ScheduleDemandSlotSnapshot> slotsByKey = slots
            .ToDictionary(PlanningDemandKey.From);

        foreach (PlanningAssignmentCandidate candidate in result.Candidates.Where(candidate =>
                     candidate.Kind == PlanningCandidateKind.NormalDemand))
        {
            PlanningCandidateSegment segment = Assert.Single(candidate.Segments);
            PlanningCandidateCoverage coverage = Assert.Single(candidate.Coverages);
            ScheduleDemandSlotSnapshot slot = slotsByKey[coverage.Demand];
            Assert.Equal(segment.AnchorDemand, coverage.Demand);
            Assert.Equal(slot.ActualStart, segment.ActualStart);
            Assert.Equal(slot.ActualEnd, segment.ActualEnd);
            Assert.Equal(slot.ActualStart, coverage.CoveredStart);
            Assert.Equal(slot.ActualEnd, coverage.CoveredEnd);
            Assert.Equal(slot.DurationMinutes, candidate.WorkMinutes);
        }

        string[] sameSourceKeys = result.Candidates
            .Where(candidate => candidate.Kind == PlanningCandidateKind.NormalDemand)
            .Where(candidate => candidate.EmployeeId == CandidateScenarioFactory.NormalEmployeeId)
            .Where(candidate => candidate.Coverages[0].Demand.SourceId
                == slots[1].SourceId)
            .Select(candidate => candidate.TechnicalKey)
            .ToArray();
        Assert.Equal(2, sameSourceKeys.Length);
        Assert.Equal(2, sameSourceKeys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ManualSuggestionEligibilityDoesNotCreateAutomaticNormalCandidate()
    {
        PlanningEmployeeTypeSnapshot normalType = CandidateScenarioFactory
            .CreateEmployeeTypes()
            .Single(type => type.Id == CandidateScenarioFactory.NormalTypeId);
        PlanningEmployeeTypeEligibilitySnapshot[] eligibilities = normalType.Eligibilities
            .Select(eligibility => eligibility.TargetId == CandidateScenarioFactory.EarlyShiftId
                ? eligibility with { Mode = ShiftEligibilityMode.ManualSuggestion }
                : eligibility)
            .ToArray();

        PlanningCandidateSet result = BuildForNormal(
            CloneType(normalType, eligibilities));

        Assert.DoesNotContain(result.Candidates, candidate =>
            candidate.Kind == PlanningCandidateKind.NormalDemand
            && candidate.Coverages[0].Demand.ShiftTypeId
                == CandidateScenarioFactory.EarlyShiftId);
        Assert.Contains(result.Candidates, candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
    }

    [Fact]
    public void SplitCandidateRequiresPatternPermissionSameDayAndRealBreak()
    {
        PlanningEmployeeTypeSnapshot normalType = CandidateScenarioFactory
            .CreateEmployeeTypes()
            .Single(type => type.Id == CandidateScenarioFactory.NormalTypeId);
        PlanningEmployeeTypeSnapshot withoutPattern = CloneType(
            normalType,
            normalType.Eligibilities.Where(eligibility =>
                eligibility.TargetId != CandidateScenarioFactory.SplitPatternId));
        PlanningCandidateSet missingPermission = BuildForNormal(withoutPattern);

        ScheduleDemandSlotSnapshot[] overlappingSlots = CandidateScenarioFactory
            .CreateDemandSlots()
            .Select(slot => slot.ShiftTypeId == CandidateScenarioFactory.LateShiftId
                ? slot with
                {
                    ActualStart = new TimeOnly(12, 30),
                    ActualEnd = new TimeOnly(15, 30),
                    DurationMinutes = 180,
                }
                : slot)
            .ToArray();
        PlanningCandidateSet overlapping = BuildForNormal(
            normalType,
            overlappingSlots);

        ScheduleDemandSlotSnapshot[] differentDays = CandidateScenarioFactory
            .CreateDemandSlots()
            .Select(slot => slot.ShiftTypeId == CandidateScenarioFactory.LateShiftId
                ? slot with { Date = slot.Date.AddDays(1) }
                : slot)
            .ToArray();
        PlanningCandidateSet differentDay = BuildForNormal(normalType, differentDays);

        Assert.DoesNotContain(missingPermission.Candidates, candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
        Assert.DoesNotContain(overlapping.Candidates, candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
        Assert.DoesNotContain(differentDay.Candidates, candidate =>
            candidate.Kind == PlanningCandidateKind.SplitShiftPattern);
    }

    [Fact]
    public void ReliefCandidateRequiresBothShiftsPatternSaturdayAndInternalSwitch()
    {
        PlanningEmployeeTypeSnapshot normalType = CandidateScenarioFactory
            .CreateEmployeeTypes()
            .Single(type => type.Id == CandidateScenarioFactory.NormalTypeId);
        Guid[] eachRequiredEligibility =
        [
            CandidateScenarioFactory.CafeteriaBShiftId,
            CandidateScenarioFactory.LateShiftId,
            CandidateScenarioFactory.ReliefPatternId,
        ];

        foreach (Guid missingEligibility in eachRequiredEligibility)
        {
            PlanningEmployeeTypeSnapshot incomplete = CloneType(
                normalType,
                normalType.Eligibilities.Where(eligibility =>
                    eligibility.TargetId != missingEligibility));
            PlanningCandidateSet result = BuildForNormal(incomplete);

            Assert.DoesNotContain(result.Candidates, candidate =>
                candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        }

        ScheduleDemandSlotSnapshot[] fridaySlots = CandidateScenarioFactory
            .CreateDemandSlots()
            .Select(slot => slot with { Date = slot.Date.AddDays(-1) })
            .ToArray();
        ScheduleDemandSlotSnapshot[] switchOutsideSlots = CandidateScenarioFactory
            .CreateDemandSlots()
            .Select(slot => slot.ShiftTypeId == CandidateScenarioFactory.LateShiftId
                ? slot with
                {
                    ActualStart = new TimeOnly(18, 0),
                    ActualEnd = new TimeOnly(20, 0),
                    DurationMinutes = 120,
                }
                : slot)
            .ToArray();

        Assert.DoesNotContain(BuildForNormal(normalType, fridaySlots).Candidates, candidate =>
            candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
        Assert.DoesNotContain(
            BuildForNormal(normalType, switchOutsideSlots).Candidates,
            candidate => candidate.Kind == PlanningCandidateKind.ReliefShiftPattern);
    }

    [Fact]
    public void BlockedDayProducesNoCandidateForEmployee()
    {
        PlanningAvailabilityEntrySnapshot blocked = new(
            CandidateScenarioFactory.NormalEmployeeId,
            CandidateScenarioFactory.Saturday,
            AvailabilityEntryKind.FixedDayOff,
            1);

        PlanningCandidateSet result = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(availabilityEntries: [blocked]));

        Assert.DoesNotContain(result.Candidates, candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.NormalEmployeeId);
        Assert.Contains(result.Candidates, candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.AuxiliaryEmployeeId);
    }

    [Fact]
    public void ProtectedFullCoverageIsConsumedButOfficeTimeIsDemandNeutral()
    {
        ScheduleDemandSlotSnapshot[] slots = CandidateScenarioFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot protectedNormal =
            CandidateScenarioFactory.CreateProtectedNormal(slots[0]);
        ScheduleAssignmentSnapshot officeTime =
            CandidateScenarioFactory.CreateProtectedNormal(
                slots[1],
                assignmentNumber: 2,
                officeTime: true);

        PlanningCandidateSet result = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(
                demandSlots: slots,
                protectedAssignments: [protectedNormal, officeTime]));

        PlanningDemandKey earlyDemand = PlanningDemandKey.From(slots[0]);
        PlanningDemandKey officeAnchorDemand = PlanningDemandKey.From(slots[1]);
        Assert.DoesNotContain(result.RemainingDemands, demand =>
            demand.Demand == earlyDemand);
        Assert.Contains(result.RemainingDemands, demand =>
            demand.Demand == officeAnchorDemand
            && demand.Kind == PlanningRemainingDemandKind.FullyUncovered);
        Assert.DoesNotContain(result.Candidates, candidate =>
            candidate.Coverages.Any(coverage => coverage.Demand == earlyDemand));
        Assert.Contains(result.Candidates, candidate =>
            candidate.Coverages.Any(coverage => coverage.Demand == officeAnchorDemand));
    }

    [Fact]
    public void ProtectedReliefCoverageLeavesOnlyEarlierRestaurantMinutesVisible()
    {
        ScheduleDemandSlotSnapshot[] slots = CandidateScenarioFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot protectedRelief =
            CandidateScenarioFactory.CreateProtectedRelief(slots[3], slots[1]);

        PlanningCandidateSet result = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(
                demandSlots: slots,
                protectedAssignments: [protectedRelief]));

        PlanningDemandKey cafeteriaDemand = PlanningDemandKey.From(slots[3]);
        PlanningDemandKey firstLateDemand = PlanningDemandKey.From(slots[1]);
        PlanningRemainingDemand remainingLate = Assert.Single(
            result.RemainingDemands,
            demand => demand.Demand == firstLateDemand);
        Assert.DoesNotContain(result.RemainingDemands, demand =>
            demand.Demand == cafeteriaDemand);
        Assert.Equal(new TimeOnly(16, 30), remainingLate.UncoveredStart);
        Assert.Equal(new TimeOnly(17, 30), remainingLate.UncoveredEnd);
        Assert.Equal(60, remainingLate.UncoveredMinutes);
        Assert.Equal(
            PlanningRemainingDemandKind.PartiallyUncoveredByProtectedReliefShift,
            remainingLate.Kind);
        Assert.DoesNotContain(result.Candidates, candidate => candidate.Coverages.Any(
            coverage => coverage.Demand is var demand
                && (demand == cafeteriaDemand || demand == firstLateDemand)));
    }

    [Fact]
    public void StableKeysAndOrderingUseOnlyTechnicalValues()
    {
        PlanningInputSnapshot original = CandidateScenarioFactory.Create(
            enableAuxiliaryReliefShift: true);
        PlanningInputSnapshot reordered = CandidateScenarioFactory.Create(
            employees: original.Employees.Reverse(),
            employeeTypes: original.EmployeeTypes.Reverse(),
            demandSlots: original.DemandSlots.Reverse(),
            enableAuxiliaryReliefShift: true);

        PlanningCandidateSet first = PlanningCandidateBuilder.Build(original);
        PlanningCandidateSet second = PlanningCandidateBuilder.Build(reordered);

        Assert.Equal(
            first.Candidates.Select(candidate => candidate.TechnicalKey),
            second.Candidates.Select(candidate => candidate.TechnicalKey));
        Assert.Equal(
            first.RemainingDemands,
            second.RemainingDemands);
        Assert.All(first.Candidates, candidate =>
        {
            Assert.DoesNotContain("Nora", candidate.TechnicalKey, StringComparison.Ordinal);
            Assert.DoesNotContain("AH", candidate.TechnicalKey, StringComparison.Ordinal);
            Assert.Contains(
                candidate.EmployeeId.ToString("N"),
                candidate.TechnicalKey,
                StringComparison.Ordinal);
        });
        Assert.Equal(
            first.Candidates.Count,
            first.Candidates.Select(candidate => candidate.TechnicalKey)
                .Distinct(StringComparer.Ordinal)
                .Count());
        PlanningAssignmentCandidate normalEarly = first.Candidates.Single(candidate =>
            candidate.EmployeeId == CandidateScenarioFactory.NormalEmployeeId
            && candidate.Kind == PlanningCandidateKind.NormalDemand
            && candidate.Coverages[0].Demand.ShiftTypeId
                == CandidateScenarioFactory.EarlyShiftId);
        Assert.Equal(
            "candidate_normal_11000000000000000000000000000001_none_"
            + "91000000000000000000000000000001_20260926_"
            + "31000000000000000000000000000001_"
            + "41000000000000000000000000000001_1",
            normalEarly.TechnicalKey);
    }

    [Fact]
    public void ChoiceSetsMakeDoubleAssignmentOverlapAndOverstaffingExplicitlyImpossible()
    {
        PlanningCandidateSet result = PlanningCandidateBuilder.Build(
            CandidateScenarioFactory.Create(enableAuxiliaryReliefShift: true));

        EmployeeDayCandidateChoices normalDay = Assert.Single(
            result.EmployeeDayChoices,
            choice => choice.EmployeeId == CandidateScenarioFactory.NormalEmployeeId);
        Assert.Equal(8, normalDay.CandidateKeys.Count);

        ScheduleDemandSlotSnapshot lateSlot = CandidateScenarioFactory.CreateDemandSlots()[1];
        PlanningDemandKey lateDemand = PlanningDemandKey.From(lateSlot);
        DemandCoverageCandidateChoices[] lateChoices = result.CoverageChoices
            .Where(choice => choice.Demand == lateDemand)
            .ToArray();
        Assert.Equal(2, lateChoices.Length);
        Assert.Equal(new TimeOnly(16, 30), lateChoices[0].Start);
        Assert.Equal(new TimeOnly(17, 30), lateChoices[0].End);
        Assert.Equal(new TimeOnly(17, 30), lateChoices[1].Start);
        Assert.Equal(new TimeOnly(19, 30), lateChoices[1].End);
        Assert.True(lateChoices[1].CandidateKeys.Count > lateChoices[0].CandidateKeys.Count);
    }

    [Fact]
    public void ExhaustiveReferenceSelectionRespectsDailyAndCoverageBoundaries()
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

        CandidateSelectionReferenceResult result =
            ExhaustiveCandidateSelectionReferenceSolver.FindMaximumCoverage(candidates);

        Assert.Equal(960, result.CoveredMinutes);
        Assert.Equal(2, result.CandidateKeys.Count);
    }

    [Fact]
    public void GeneratedInputPermutationsPreserveCandidateInvariants()
    {
        PlanningInputSnapshot baseline = CandidateScenarioFactory.Create(
            enableAuxiliaryReliefShift: true);
        string[] expectedKeys = PlanningCandidateBuilder.Build(baseline).Candidates
            .Select(candidate => candidate.TechnicalKey)
            .ToArray();

        for (int seed = 0; seed < 20; seed++)
        {
            Random random = new(seed);
            PlanningInputSnapshot permuted = CandidateScenarioFactory.Create(
                employees: baseline.Employees.OrderBy(_ => random.Next()),
                employeeTypes: baseline.EmployeeTypes.OrderBy(_ => random.Next()),
                demandSlots: baseline.DemandSlots.OrderBy(_ => random.Next()),
                enableAuxiliaryReliefShift: true);

            PlanningCandidateSet result = PlanningCandidateBuilder.Build(permuted);

            Assert.Equal(expectedKeys, result.Candidates.Select(candidate =>
                candidate.TechnicalKey));
            Assert.All(result.Candidates, candidate =>
            {
                Assert.NotEmpty(candidate.Segments);
                Assert.All(candidate.Segments, segment =>
                    Assert.Equal(candidate.Date, segment.AnchorDemand.Date));
                Assert.All(candidate.Coverages, coverage =>
                    Assert.Equal(candidate.Date, coverage.Demand.Date));
            });
        }
    }

    private static PlanningCandidateSet BuildForNormal(
        PlanningEmployeeTypeSnapshot normalType,
        IEnumerable<ScheduleDemandSlotSnapshot>? slots = null)
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees()
            .Single(item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        return PlanningCandidateBuilder.Build(CandidateScenarioFactory.Create(
            employees: [employee],
            employeeTypes: [normalType],
            demandSlots: slots ?? CandidateScenarioFactory.CreateDemandSlots()));
    }

    private static PlanningEmployeeTypeSnapshot CloneType(
        PlanningEmployeeTypeSnapshot source,
        IEnumerable<PlanningEmployeeTypeEligibilitySnapshot> eligibilities) => new(
            source.Id,
            source.Code,
            source.Name,
            source.WeeklyWorkTargetMinutes,
            source.AllowsVacationAndSickness,
            source.AbsenceDayValueMinutes,
            source.PlanningRole,
            source.AllowsAutomaticAssignment,
            source.RequiresWeeklyManualAssignment,
            source.PreservesManualAssignmentsOnGeneration,
            source.ManualSuggestionPriority,
            eligibilities);
}
