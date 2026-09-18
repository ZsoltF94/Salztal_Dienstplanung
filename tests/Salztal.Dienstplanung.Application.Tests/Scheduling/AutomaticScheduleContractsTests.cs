using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class AutomaticScheduleContractsTests
{
    [Theory]
    [InlineData(AutomaticSchedulePlanningStatus.Optimal)]
    [InlineData(AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal)]
    public void SuccessfulStatusCreatesPreviewOnly(
        AutomaticSchedulePlanningStatus status)
    {
        AutomaticScheduleProposal proposal = CreateProposal();

        AutomaticSchedulePlanningResult result =
            AutomaticSchedulePlanningResult.Success(status, proposal);

        Assert.Equal(status, result.Status);
        Assert.Same(proposal, result.Preview?.Proposal);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(AutomaticSchedulePlanningStatus.Cancelled)]
    [InlineData(AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult)]
    [InlineData(AutomaticSchedulePlanningStatus.BlockedByInput)]
    [InlineData(AutomaticSchedulePlanningStatus.UnsupportedRule)]
    [InlineData(AutomaticSchedulePlanningStatus.TechnicalFailure)]
    public void NonSuccessfulStatusCannotContainPreview(
        AutomaticSchedulePlanningStatus status)
    {
        AutomaticScheduleError[] errors =
        [
            new AutomaticScheduleError(AutomaticScheduleErrorCode.TechnicalFailure),
        ];

        AutomaticSchedulePlanningResult result =
            AutomaticSchedulePlanningResult.Failure(status, errors);

        errors[0] = new AutomaticScheduleError(AutomaticScheduleErrorCode.ConcurrentRun);
        Assert.Equal(status, result.Status);
        Assert.Null(result.Preview);
        Assert.Equal(AutomaticScheduleErrorCode.TechnicalFailure, result.Errors[0].Code);
    }

    [Fact]
    public void SuccessRejectsFailureStatusAndFailureRejectsSuccessStatus()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AutomaticSchedulePlanningResult.Success(
                AutomaticSchedulePlanningStatus.Cancelled,
                CreateProposal()));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.Optimal));
    }

    [Fact]
    public void PlanningErrorPreservesStableDiagnosticParameters()
    {
        DateOnly date = new(2026, 9, 14);
        Guid entityId = Guid.Parse("90000000-0000-4000-8000-000000000001");
        AutomaticScheduleError error = new(
            AutomaticScheduleErrorCode.RuleNotTranslated,
            "TEST_RULE",
            entityId,
            "correlation-1",
            7,
            date,
            "MissingFromSnapshot");

        Assert.Equal("TEST_RULE", error.RuleId);
        Assert.Equal(entityId, error.TechnicalEntityId);
        Assert.Equal("correlation-1", error.CorrelationId);
        Assert.Equal(7, error.CatalogVersion);
        Assert.Equal(date, error.Date);
        Assert.Equal("MissingFromSnapshot", error.Parameter);
    }

    [Fact]
    public void ProposalAndMetadataCopyMutableInputCollections()
    {
        AutomaticScheduleDayOffProposal[] dayOffs =
        [
            new AutomaticScheduleDayOffProposal(
                Guid.Parse("10000000-0000-4000-8000-000000000001"),
                new DateOnly(2026, 9, 14)),
        ];
        AutomaticScheduleOpenDemand[] openDemands =
        [
            new AutomaticScheduleOpenDemand(
                Guid.Parse("20000000-0000-4000-8000-000000000001"),
                new DateOnly(2026, 9, 14),
                Guid.Parse("30000000-0000-4000-8000-000000000001"),
                Guid.Parse("40000000-0000-4000-8000-000000000001"),
                1,
                new TimeOnly(6, 30),
                new TimeOnly(13, 30),
                420,
                AutomaticScheduleOpenDemandKind.FullyUncovered),
        ];
        AutomaticScheduleSetting[] settings =
        [
            new AutomaticScheduleSetting("workers", "1"),
        ];
        AutomaticScheduleRunMetadata metadata = CreateMetadata(settings);
        AutomaticScheduleProposal proposal = CreateProposal(
            dayOffs,
            openDemands,
            metadata);

        dayOffs[0] = new AutomaticScheduleDayOffProposal(Guid.NewGuid(), DateOnly.MaxValue);
        openDemands[0] = new AutomaticScheduleOpenDemand(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 15),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            new TimeOnly(16, 30),
            new TimeOnly(19, 30),
            180,
            AutomaticScheduleOpenDemandKind.FullyUncovered);
        settings[0] = new AutomaticScheduleSetting("changed", "true");

        Assert.Equal(new DateOnly(2026, 9, 14), proposal.GeneratedDayOffs[0].Date);
        Assert.Equal(420, proposal.OpenDemands[0].UncoveredMinutes);
        Assert.Equal("workers", proposal.Metadata.Settings[0].Key);
    }

    [Fact]
    public void PlanningRequestRequiresSnapshotAndPositiveTimeLimit()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AutomaticSchedulePlanningRequest(null!, TimeSpan.FromMinutes(2)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutomaticSchedulePlanningRequest(
                CreatePlanningInputSnapshot(),
                TimeSpan.Zero));
    }

    [Fact]
    public void OpenDemandRequiresConsistentTechnicalIdentityAndDuration()
    {
        Assert.Throws<ArgumentException>(() => new AutomaticScheduleOpenDemand(
            Guid.Empty,
            new DateOnly(2026, 9, 14),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            new TimeOnly(6, 30),
            new TimeOnly(13, 30),
            420,
            AutomaticScheduleOpenDemandKind.FullyUncovered));
        Assert.Throws<ArgumentException>(() => new AutomaticScheduleOpenDemand(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 14),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            new TimeOnly(6, 30),
            new TimeOnly(13, 30),
            419,
            AutomaticScheduleOpenDemandKind.FullyUncovered));
    }

    [Fact]
    public void PhaseContractsCopyValuesAndRequireStableUniqueOrder()
    {
        AutomaticSchedulePhaseValue[] sourceValues =
        [
            new AutomaticSchedulePhaseValue("covered_minutes", 420),
        ];
        AutomaticSchedulePhaseSnapshot model = new(
            AutomaticSchedulePhaseKind.ModelBuilding,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(2),
            sourceValues);
        AutomaticSchedulePhaseSnapshot coverage = new(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(3));
        AutomaticScheduleRunMetadata metadata = new(
            "Synthetic solver",
            "1.0",
            AutomaticSchedulePlanningStatus.Optimal,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMilliseconds(2),
            TimeSpan.FromMilliseconds(3),
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(5),
            [],
            [model, coverage]);

        sourceValues[0] = new AutomaticSchedulePhaseValue("changed", 1);

        Assert.Equal("covered_minutes", metadata.Phases[0].Values[0].Key);
        Assert.Throws<ArgumentException>(() => new AutomaticScheduleRunMetadata(
            "Synthetic solver",
            "1.0",
            AutomaticSchedulePlanningStatus.Optimal,
            TimeSpan.FromMinutes(2),
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero,
            [],
            [coverage, model]));
        Assert.Throws<ArgumentException>(() =>
            AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                phases: [model, model]));
    }

    [Fact]
    public void RunMetadataDistinguishesCompleteIncompleteAndMissingPhaseTraces()
    {
        AutomaticSchedulePhaseSnapshot input = new(
            AutomaticSchedulePhaseKind.InputValidation,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(1));
        AutomaticSchedulePhaseSnapshot model = new(
            AutomaticSchedulePhaseKind.ModelBuilding,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(2));

        AutomaticScheduleRunMetadata complete = CreateMetadata([], [input, model]);
        AutomaticScheduleRunMetadata missingInput = CreateMetadata([], [model]);
        AutomaticScheduleRunMetadata notRecorded = CreateMetadata([]);

        Assert.Equal(
            AutomaticSchedulePhaseTraceCompleteness.Complete,
            complete.PhaseTraceCompleteness);
        Assert.Equal(
            AutomaticSchedulePhaseTraceCompleteness.InputValidationNotRecorded,
            missingInput.PhaseTraceCompleteness);
        Assert.Equal(
            AutomaticSchedulePhaseTraceCompleteness.NotRecorded,
            notRecorded.PhaseTraceCompleteness);
    }

    [Fact]
    public void PrependingInputPhaseKeepsSuccessfulResultAndMetadataCanonical()
    {
        AutomaticSchedulePhaseSnapshot input = new(
            AutomaticSchedulePhaseKind.InputValidation,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(1));
        AutomaticSchedulePhaseSnapshot model = new(
            AutomaticSchedulePhaseKind.ModelBuilding,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(2));
        AutomaticScheduleProposal original = CreateProposal(
            metadata: CreateMetadata([], [model]));
        AutomaticSchedulePlanningResult result =
            AutomaticSchedulePlanningResult.Success(
                AutomaticSchedulePlanningStatus.Optimal,
                original);

        AutomaticSchedulePlanningResult updated = result.PrependPhase(input);

        AutomaticScheduleProposal proposal = Assert.IsType<AutomaticScheduleProposal>(
            updated.Preview?.Proposal);
        Assert.NotSame(original, proposal);
        Assert.Same(original.ObjectiveVector, proposal.ObjectiveVector);
        Assert.Equal(
            [
                AutomaticSchedulePhaseKind.InputValidation,
                AutomaticSchedulePhaseKind.ModelBuilding,
            ],
            proposal.Metadata.Phases.Select(value => value.Kind));
        Assert.Equal(proposal.Metadata.Phases, updated.Phases);
        Assert.Equal(
            AutomaticSchedulePhaseTraceCompleteness.Complete,
            proposal.Metadata.PhaseTraceCompleteness);
    }

    [Fact]
    public void PhaseTerminationPreservesStructuredTimeoutDetailsAndPartialValues()
    {
        AutomaticSchedulePhaseTerminationSnapshot termination = new(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection,
            AutomaticScheduleOptimizationTargetKind.RegularTouchedDemandSlots,
            TimeSpan.FromSeconds(120),
            TimeSpan.FromMilliseconds(120_018));
        AutomaticSchedulePhaseSnapshot phase = new(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.FromMilliseconds(119_997),
            [new AutomaticSchedulePhaseValue("covered_minutes", 420)],
            termination);

        Assert.Equal(
            AutomaticSchedulePhaseDetailAvailability.Complete,
            phase.DetailAvailability);
        AutomaticSchedulePhaseTerminationSnapshot actualTermination = Assert.IsType<
            AutomaticSchedulePhaseTerminationSnapshot>(phase.Termination);
        Assert.Same(termination, actualTermination);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection,
            actualTermination.Reason);
        Assert.Equal(
            AutomaticScheduleOptimizationTargetKind.RegularTouchedDemandSlots,
            actualTermination.ActiveTarget);
        Assert.Equal(TimeSpan.FromSeconds(120), actualTermination.TimeLimit);
        Assert.Equal(TimeSpan.FromMilliseconds(120_018), actualTermination.BudgetElapsed);
        Assert.Equal(420, Assert.Single(phase.Values).Value);
    }

    [Fact]
    public void TerminalPhasesDistinguishMissingCancellationTimeoutAndFailureDetails()
    {
        AutomaticSchedulePhaseSnapshot legacy = new(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.FromSeconds(120));
        AutomaticSchedulePhaseSnapshot cancellation = new(
            AutomaticSchedulePhaseKind.ModelBuilding,
            AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.FromMilliseconds(5),
            termination: new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.CancellationRequested));
        AutomaticSchedulePhaseSnapshot timeoutWithoutSelection = new(
            AutomaticSchedulePhaseKind.HardRules,
            AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.FromSeconds(2),
            termination: new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection,
                AutomaticScheduleOptimizationTargetKind.HardRuleFeasibility,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(2_004)));
        AutomaticSchedulePhaseSnapshot failure = new(
            AutomaticSchedulePhaseKind.ResultMapping,
            AutomaticSchedulePhaseStatus.Failed,
            TimeSpan.FromMilliseconds(3),
            termination: new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.TechnicalFailure));

        Assert.Equal(
            AutomaticSchedulePhaseDetailAvailability.TerminationDetailsNotRecorded,
            legacy.DetailAvailability);
        Assert.Null(legacy.Termination);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.CancellationRequested,
            cancellation.Termination?.Reason);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection,
            timeoutWithoutSelection.Termination?.Reason);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.TechnicalFailure,
            failure.Termination?.Reason);
    }

    [Fact]
    public void PhaseTerminationRejectsIncompleteOrContradictoryDetails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutomaticSchedulePhaseTerminationSnapshot(
                (AutomaticSchedulePhaseTerminationReason)999));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.CancellationRequested,
                (AutomaticScheduleOptimizationTargetKind)999));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.CancellationRequested,
                timeLimit: TimeSpan.Zero,
                budgetElapsed: TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.CancellationRequested,
                timeLimit: TimeSpan.FromSeconds(2),
                budgetElapsed: TimeSpan.FromTicks(-1)));
        Assert.Throws<ArgumentException>(() =>
            new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection));
        Assert.Throws<ArgumentException>(() =>
            new AutomaticSchedulePhaseTerminationSnapshot(
                AutomaticSchedulePhaseTerminationReason.CancellationRequested,
                timeLimit: TimeSpan.FromSeconds(2)));

        AutomaticSchedulePhaseTerminationSnapshot timeout = new(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection,
            AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(2));
        AutomaticSchedulePhaseTerminationSnapshot failure = new(
            AutomaticSchedulePhaseTerminationReason.TechnicalFailure);
        AutomaticSchedulePhaseTerminationSnapshot wrongTarget = new(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection,
            AutomaticScheduleOptimizationTargetKind.ReliefCoveredMinutes,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(2));

        Assert.Throws<ArgumentException>(() => new AutomaticSchedulePhaseSnapshot(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.Zero,
            termination: timeout));
        Assert.Throws<ArgumentException>(() => new AutomaticSchedulePhaseSnapshot(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Failed,
            TimeSpan.Zero,
            termination: timeout));
        Assert.Throws<ArgumentException>(() => new AutomaticSchedulePhaseSnapshot(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.Zero,
            termination: failure));
        Assert.Throws<ArgumentException>(() => new AutomaticSchedulePhaseSnapshot(
            AutomaticSchedulePhaseKind.RegularCoverage,
            AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.Zero,
            termination: wrongTarget));
    }

    [Fact]
    public void RunRecordCopiesCompleteObjectiveVectorIntoImmutableSnapshot()
    {
        RuleDefinition highRule = InitialSoftRuleDefinitions.NormalWeeklyMinimum;
        RuleDefinition stabilityRule =
            InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution;
        Guid auxiliaryEmployee = Guid.Parse("70000000-0000-4000-8000-000000000001");
        Guid normalEmployee = Guid.Parse("80000000-0000-4000-8000-000000000001");
        DateOnly weekMonday = new(2026, 9, 14);
        ScheduleObjectiveVector objective = new(
            180,
            2,
            new RuleViolationSet(
                [new RuleViolationCase(
                    highRule,
                    "case-high",
                    new RuleViolationMagnitude(3))]),
            1,
            2,
            new AuxiliaryWeeklyMinimumObjective(
                [new AuxiliaryWeeklyMinimumCase(
                    auxiliaryEmployee,
                    weekMonday,
                    179,
                    true)]),
            new RelativeWeeklyTargetObjective(
                [new RelativeWeeklyTargetCase(
                    normalEmployee,
                    weekMonday,
                    1140,
                    1200)]),
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            new RuleViolationSet(
                [new RuleViolationCase(
                    stabilityRule,
                    "case-stability",
                    new RuleViolationMagnitude(4))]),
            ["assignment-b", "assignment-a"]);

        AutomaticScheduleRunRecord run = new(
            Guid.NewGuid(),
            CreateMetadata([]),
            AutomaticScheduleObjectiveSnapshot.Create(objective));

        Assert.Equal(180, run.Objective.UncoveredEmployeeMinutes);
        Assert.Equal(2, run.Objective.FullyUncoveredDemandSlotCount);
        Assert.Equal(1, run.Objective.ReliefShiftAssignmentCount);
        Assert.Equal(2, run.Objective.SplitShiftAssignmentCount);
        Assert.Equal(179, Assert.Single(
            run.Objective.AuxiliaryMinimumCases).AssignedMinutes);
        Assert.Equal(1200, Assert.Single(
            run.Objective.RelativeWeeklyTargetCases).TargetMinutes);
        Assert.Equal("NORMAL_WEEKLY_MINIMUM", Assert.Single(
            run.Objective.HighPriorityViolations).RuleId);
        Assert.Equal(3, run.Objective.HighPriorityViolations[0].Magnitude);
        Assert.Equal(RuleFamily.Stability, Assert.Single(
            run.Objective.StabilityViolations).RuleFamily);
        Assert.Equal(
            ["assignment-a", "assignment-b"],
            run.Objective.TechnicalTieBreakerKeys);
    }

    [Fact]
    public void ObjectiveSnapshotRejectsViolationInWrongStage()
    {
        AutomaticScheduleRuleViolation medium = new(
            "M-01",
            RuleFamily.Soft,
            RulePriority.Medium,
            "case-medium",
            1);

        Assert.Throws<ArgumentException>(() =>
            new AutomaticScheduleObjectiveSnapshot(
                0,
                0,
                [medium],
                [],
                [],
                [],
                []));
    }

    private static AutomaticScheduleProposal CreateProposal(
        IEnumerable<AutomaticScheduleDayOffProposal>? dayOffs = null,
        IEnumerable<AutomaticScheduleOpenDemand>? openDemands = null,
        AutomaticScheduleRunMetadata? metadata = null)
    {
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
        ScheduleRuleEvaluationSet evaluations = new(
            catalog,
            catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.Satisfied,
                NoRuleResultParameters.Instance)));
        ScheduleObjectiveVector vector = new(
            0,
            0,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            ["assignment-a"]);

        return new AutomaticScheduleProposal(
            Guid.Parse("50000000-0000-4000-8000-000000000001"),
            Guid.Parse("60000000-0000-4000-8000-000000000001"),
            1,
            [],
            dayOffs ?? [],
            openDemands ?? [],
            vector,
            evaluations,
            metadata ?? CreateMetadata([]));
    }

    private static AutomaticScheduleRunMetadata CreateMetadata(
        IEnumerable<AutomaticScheduleSetting> settings,
        IEnumerable<AutomaticSchedulePhaseSnapshot>? phases = null)
    {
        return new AutomaticScheduleRunMetadata(
            "Synthetic solver",
            "1.0",
            AutomaticSchedulePlanningStatus.Optimal,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(5),
            TimeSpan.FromMilliseconds(4),
            TimeSpan.FromMilliseconds(10),
            settings,
            phases);
    }

    private static PlanningInputSnapshot CreatePlanningInputSnapshot()
    {
        return new PlanningInputSnapshot(
            Guid.Parse("70000000-0000-4000-8000-000000000001"),
            Guid.Parse("80000000-0000-4000-8000-000000000001"),
            1,
            new DateOnly(2026, 9, 14),
            new DateOnly(2026, 10, 4),
            [],
            [],
            new PlanningServiceCatalogSnapshot(
                [],
                [],
                new PlanningSplitShiftPatternSnapshot(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    180,
                    600),
                new PlanningReliefShiftPatternSnapshot(
                    Guid.NewGuid(),
                    DayOfWeek.Saturday,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    ReliefShiftSwitchRule.EndOfFirstActualDemand,
                    false)),
            [],
            [],
            [],
            new RuleCatalogSnapshot(1, []),
            PlanningRunOptions.Default,
            new PlanningHistorySnapshot(PlanningHistoryCompleteness.Missing, []));
    }
}
