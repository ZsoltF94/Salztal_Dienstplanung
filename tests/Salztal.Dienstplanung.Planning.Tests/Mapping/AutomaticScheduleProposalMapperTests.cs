using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Mapping;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;
using Salztal.Dienstplanung.Planning.Tests.Optimization;

namespace Salztal.Dienstplanung.Planning.Tests.Mapping;

public sealed class AutomaticScheduleProposalMapperTests
{
    [Fact]
    public void MapsCandidateValuesAndDeterministicIdentifierExactly()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));
        AutomaticScheduleProposalMapper mapper = new(
            new DeterministicAutomaticScheduleAssignmentIdFactory());

        AutomaticScheduleProposal first = mapper.Map(snapshot, optimization, Metadata());
        AutomaticScheduleProposal second = mapper.Map(snapshot, optimization, Metadata());

        ScheduleAssignmentSnapshot assignment = Assert.Single(first.Assignments);
        PlanningAssignmentCandidate candidate = Assert.Single(
            optimization.SelectedCandidates);
        Assert.Equal(candidate.EmployeeId, assignment.EmployeeId);
        Assert.Equal(candidate.Date, assignment.Date);
        Assert.Equal(candidate.WorkMinutes, assignment.WorkMinutes);
        Assert.Equal(ScheduleAssignmentOriginSnapshot.AutomaticGeneration, assignment.Origin);
        Assert.False(assignment.IsProtectedFromAutomaticGeneration);
        Assert.Equal(first.Assignments[0].AssignmentId, second.Assignments[0].AssignmentId);
        Assert.Equal(candidate.Segments.Count, assignment.Segments.Count);
        Assert.Equal(candidate.Coverages.Count, assignment.Coverages.Count);
    }

    [Theory]
    [InlineData(Domain.Availabilities.AvailabilityEntryKind.Vacation)]
    [InlineData(Domain.Availabilities.AvailabilityEntryKind.Sickness)]
    [InlineData(Domain.Availabilities.AvailabilityEntryKind.FixedDayOff)]
    public void CreatesBlackDayOffForEveryOtherwiseEmptyEmployeeDay(
        Domain.Availabilities.AvailabilityEntryKind unavailableKind)
    {
        PlanningInputSnapshot source = CreateSnapshot();
        PlanningAvailabilityEntrySnapshot vacation = new(
            CandidateScenarioFactory.NormalEmployeeId,
            source.PeriodMonday,
            unavailableKind,
            1);
        PlanningInputSnapshot snapshot = DemandCoverageOptimizerTests.Clone(
            source,
            source.Employees,
            source.EmployeeTypes,
            source.DemandSlots,
            [vacation]);
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(
            new DeterministicAutomaticScheduleAssignmentIdFactory()).Map(
                snapshot,
                optimization,
                Metadata());

        Assert.Equal(19, proposal.GeneratedDayOffs.Count);
        Assert.DoesNotContain(proposal.GeneratedDayOffs, item =>
            item.Date == vacation.Date);
        Assert.DoesNotContain(proposal.GeneratedDayOffs, item =>
            item.Date == proposal.Assignments[0].Date);
        Assert.Equal(19, proposal.GeneratedDayOffs
            .Select(item => (item.EmployeeId, item.Date)).Distinct().Count());
    }

    [Fact]
    public void FullyUnavailableWeekReceivesNoBlackDayOff()
    {
        PlanningInputSnapshot source = CreateSnapshot();
        PlanningAvailabilityEntrySnapshot[] unavailableWeek = Enumerable.Range(0, 7)
            .Select(index => new PlanningAvailabilityEntrySnapshot(
                CandidateScenarioFactory.NormalEmployeeId,
                source.PeriodMonday.AddDays(index),
                Domain.Availabilities.AvailabilityEntryKind.FixedDayOff,
                index + 1))
            .ToArray();
        PlanningInputSnapshot snapshot = DemandCoverageOptimizerTests.Clone(
            source,
            source.Employees,
            source.EmployeeTypes,
            source.DemandSlots,
            unavailableWeek);
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(
            new DeterministicAutomaticScheduleAssignmentIdFactory()).Map(
                snapshot,
                optimization,
                Metadata());

        Assert.Equal(14, proposal.GeneratedDayOffs.Count);
        Assert.DoesNotContain(proposal.GeneratedDayOffs, dayOff =>
            dayOff.Date < source.PeriodMonday.AddDays(7));
    }

    [Fact]
    public void ObjectiveOpenDemandAndAllRuleEvaluationsAreMapped()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(
            new DeterministicAutomaticScheduleAssignmentIdFactory()).Map(
                snapshot,
                optimization,
                Metadata());

        Assert.Equal(
            optimization.UncoveredEmployeeMinutes,
            proposal.ObjectiveVector.UncoveredEmployeeMinutes);
        Assert.Equal(
            optimization.FullyUncoveredDemandSlotCount,
            proposal.ObjectiveVector.FullyUncoveredDemandSlotCount);
        Assert.Equal(
            optimization.HighPriorityViolations.Violations,
            proposal.ObjectiveVector.HighPriorityViolations.Violations);
        Assert.Equal(
            optimization.MediumPriorityViolations.Violations,
            proposal.ObjectiveVector.MediumPriorityViolations.Violations);
        Assert.Empty(proposal.ObjectiveVector.LowPriorityViolations.Violations);
        Assert.Equal(
            optimization.StabilityViolations.Violations,
            proposal.ObjectiveVector.StabilityViolations.Violations);
        Assert.Equal(optimization.OpenDemands, proposal.OpenDemands);
        Assert.Equal(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value!.Definitions.Count,
            proposal.RuleEvaluations.StructureResults.Count
                + proposal.RuleEvaluations.AutomaticHardResults.Count
                + proposal.RuleEvaluations.SoftResults.Count
                + proposal.RuleEvaluations.NoticeResults.Count
                + proposal.RuleEvaluations.StabilityResults.Count);
        Assert.DoesNotContain(proposal.RuleEvaluations.AutomaticHardResults, result =>
            result.Status == RuleEvaluationStatus.Violated);
        Assert.Equal(
            optimization.HighPriorityRuleEvaluation.Results.Select(result => result.Status),
            proposal.RuleEvaluations.SoftResults
                .Where(result => optimization.HighPriorityRuleEvaluation.Results.Any(
                    expected => expected.RuleId == result.RuleId))
                .Select(result => result.Status));
    }

    [Fact]
    public void MapsSplitShiftAsOneAssignmentWithTwoFullCoverages()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot(
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
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(
            new DeterministicAutomaticScheduleAssignmentIdFactory()).Map(
                snapshot,
                optimization,
                Metadata());

        ScheduleAssignmentSnapshot assignment = Assert.Single(proposal.Assignments);
        Assert.Equal(ScheduleAssignmentKindSnapshot.SplitShiftPattern, assignment.Kind);
        Assert.Equal(CandidateScenarioFactory.SplitPatternId, assignment.PatternId);
        Assert.Equal(2, assignment.Segments.Count);
        Assert.All(assignment.Coverages, coverage =>
            Assert.Equal(ScheduleDemandCoverageKindSnapshot.Full, coverage.Kind));
    }

    [Fact]
    public void MapsReliefSwitchAndPartialCoverageWithoutHidingEarlyGap()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot(
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
        ]);
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));

        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(
            new DeterministicAutomaticScheduleAssignmentIdFactory()).Map(
                snapshot,
                optimization,
                Metadata());

        ScheduleAssignmentSnapshot assignment = Assert.Single(proposal.Assignments);
        Assert.Equal(ScheduleAssignmentKindSnapshot.ReliefShiftPattern, assignment.Kind);
        Assert.Equal(new TimeOnly(17, 30), assignment.Segments[1].ActualStart);
        Assert.Equal(
            ScheduleDemandCoverageKindSnapshot.PartialReliefShift,
            assignment.Coverages[1].Kind);
        AutomaticScheduleOpenDemand open = Assert.Single(proposal.OpenDemands);
        Assert.Equal(new TimeOnly(16, 30), open.UncoveredStart);
        Assert.Equal(new TimeOnly(17, 30), open.UncoveredEnd);
    }

    [Fact]
    public void IndependentValidatorRejectsChangedAssignmentMinutes()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));
        DeterministicAutomaticScheduleAssignmentIdFactory idFactory = new();
        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(idFactory)
            .Map(snapshot, optimization, Metadata());
        ScheduleAssignmentSnapshot original = Assert.Single(proposal.Assignments);
        ScheduleAssignmentSnapshot changed = new(
            original.AssignmentId,
            original.EmployeeId,
            original.Date,
            original.Kind,
            original.Origin,
            original.PatternId,
            original.WorkMinutes + 1,
            original.IsProtectedFromAutomaticGeneration,
            original.Segments,
            original.Coverages);
        AutomaticScheduleProposal mutation = Copy(proposal, assignments: [changed]);

        AutomaticScheduleProposalValidationResult result =
            AutomaticScheduleProposalValidator.Validate(
                snapshot,
                optimization,
                mutation,
                idFactory);

        Assert.Equal(
            AutomaticScheduleProposalValidationCode.AssignmentMismatch,
            result.Code);
    }

    [Fact]
    public void IndependentValidatorRejectsMissingBlackDayOff()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));
        DeterministicAutomaticScheduleAssignmentIdFactory idFactory = new();
        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(idFactory)
            .Map(snapshot, optimization, Metadata());
        AutomaticScheduleProposal mutation = Copy(
            proposal,
            dayOffs: proposal.GeneratedDayOffs.Skip(1));

        AutomaticScheduleProposalValidationResult result =
            AutomaticScheduleProposalValidator.Validate(
                snapshot,
                optimization,
                mutation,
                idFactory);

        Assert.Equal(AutomaticScheduleProposalValidationCode.DayOffMismatch, result.Code);
    }

    [Fact]
    public void IndependentValidatorRejectsHiddenReliefGap()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot(
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
        ]);
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));
        DeterministicAutomaticScheduleAssignmentIdFactory idFactory = new();
        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(idFactory)
            .Map(snapshot, optimization, Metadata());
        AutomaticScheduleProposal mutation = new(
            proposal.SnapshotId,
            proposal.DraftId,
            proposal.ExpectedDraftVersion,
            proposal.Assignments,
            proposal.GeneratedDayOffs,
            [],
            proposal.ObjectiveVector,
            proposal.RuleEvaluations,
            proposal.Metadata);

        AutomaticScheduleProposalValidationResult result =
            AutomaticScheduleProposalValidator.Validate(
                snapshot,
                optimization,
                mutation,
                idFactory);

        Assert.Equal(
            AutomaticScheduleProposalValidationCode.OpenDemandMismatch,
            result.Code);
    }

    [Fact]
    public void IndependentValidatorRejectsRemovedHighPriorityBoundary()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        DemandCoverageOptimizationResult optimization = DemandCoverageOptimizer.Optimize(
            snapshot,
            PlanningCandidateBuilder.Build(snapshot));
        DeterministicAutomaticScheduleAssignmentIdFactory idFactory = new();
        AutomaticScheduleProposal proposal = new AutomaticScheduleProposalMapper(idFactory)
            .Map(snapshot, optimization, Metadata());
        ScheduleObjectiveVector changedObjective = new(
            proposal.ObjectiveVector.UncoveredEmployeeMinutes,
            proposal.ObjectiveVector.FullyUncoveredDemandSlotCount,
            RuleViolationSet.Empty,
            proposal.ObjectiveVector.MediumPriorityViolations,
            proposal.ObjectiveVector.LowPriorityViolations,
            proposal.ObjectiveVector.StabilityViolations,
            proposal.ObjectiveVector.TechnicalTieBreakerKeys);
        AutomaticScheduleProposal mutation = new(
            proposal.SnapshotId,
            proposal.DraftId,
            proposal.ExpectedDraftVersion,
            proposal.Assignments,
            proposal.GeneratedDayOffs,
            proposal.OpenDemands,
            changedObjective,
            proposal.RuleEvaluations,
            proposal.Metadata);

        AutomaticScheduleProposalValidationResult result =
            AutomaticScheduleProposalValidator.Validate(
                snapshot,
                optimization,
                mutation,
                idFactory);

        Assert.Equal(AutomaticScheduleProposalValidationCode.ObjectiveMismatch, result.Code);
    }

    private static PlanningInputSnapshot CreateSnapshot(
        IEnumerable<ScheduleDemandSlotSnapshot>? slots = null)
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees().Single(
            item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        ScheduleDemandSlotSnapshot defaultSlot = DemandCoverageOptimizerTests.Slot(
            CandidateScenarioFactory.Saturday,
            1,
            CandidateScenarioFactory.LateShiftId,
            180,
            startHour: 16,
            startMinute: 30);
        return DemandCoverageOptimizerTests.Clone(
            CandidateScenarioFactory.Create(),
            [employee],
            [type],
            slots ?? [defaultSlot],
            []);
    }

    private static AutomaticScheduleProposal Copy(
        AutomaticScheduleProposal source,
        IEnumerable<ScheduleAssignmentSnapshot>? assignments = null,
        IEnumerable<AutomaticScheduleDayOffProposal>? dayOffs = null) => new(
            source.SnapshotId,
            source.DraftId,
            source.ExpectedDraftVersion,
            assignments ?? source.Assignments,
            dayOffs ?? source.GeneratedDayOffs,
            source.OpenDemands,
            source.ObjectiveVector,
            source.RuleEvaluations,
            source.Metadata);

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
