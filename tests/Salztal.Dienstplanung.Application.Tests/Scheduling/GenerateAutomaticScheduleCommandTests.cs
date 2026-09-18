using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

[Collection(nameof(AutomaticScheduleGenerationExecutionGroup))]
public sealed class GenerateAutomaticScheduleCommandTests
{
    [Theory]
    [InlineData(AutomaticSchedulePlanningStatus.Optimal)]
    [InlineData(AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal)]
    public async Task SuccessfulPlanningResultIsHeldOnlyAsTransientPreview(
        AutomaticSchedulePlanningStatus status)
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(request => Task.FromResult(
            Success(request.Snapshot, status)));
        GenerateAutomaticScheduleCommand command = new(context.Data, planner);
        int savesBeforeGeneration = context.Data.SaveCallCount;

        AutomaticScheduleGenerationResult result = await command.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);

        Assert.Equal(Map(status), result.Status);
        Assert.Equal(1, planner.CallCount);
        Assert.Same(result.Preview, command.CurrentPreview);
        Assert.Same(context.Data.PreparedSnapshot, planner.Request?.Snapshot);
        Assert.Equal(
            AutomaticSchedulePlanningRequest.ProductiveTimeLimit,
            planner.Request?.TimeLimit);
        Assert.Equal(savesBeforeGeneration, context.Data.SaveCallCount);
        Assert.Equal(context.Draft.Version.Value, result.Preview?.Proposal.ExpectedDraftVersion);
        Assert.Equal(context.Data.PreparedSnapshot?.Id, result.Preview?.Proposal.SnapshotId);
        Assert.Equal(status, result.Preview?.Proposal.Metadata.ResultStatus);
        Assert.Empty(result.PlanningErrors);
        Assert.Equal(result.Status, result.Report.Status);
        Assert.Equal(
            context.Data.PreparedSnapshot?.Id,
            result.Report.PlanningReport?.SnapshotId);
        Assert.Empty(result.Report.Errors);
        Assert.Equal(15, result.Report.PhaseReports.Count);
        Assert.NotNull(result.Report.TechnicalDetails);
        Assert.NotNull(result.Report.Objective);
        AutomaticScheduleGenerationPhaseReportSnapshot reliefCoverage = Assert.Single(
            result.Report.PhaseReports,
            value => value.Kind == AutomaticSchedulePhaseKind.ReliefCoverage);
        Assert.Equal(
            result.Report.Objective.UncoveredEmployeeMinutes,
            Assert.Single(
                reliefCoverage.Metrics,
                value => value.Key == "uncovered_minutes").AchievedValue);
        Assert.Equal(
            result.Report.Objective.FullyUncoveredDemandSlotCount,
            Assert.Single(
                reliefCoverage.Metrics,
                value => value.Key == "fully_uncovered_demand_count").AchievedValue);
    }

    [Fact]
    public async Task OptimalRunExplainsProvenOptimizationPhase()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(request => Task.FromResult(
            Success(request.Snapshot, AutomaticSchedulePlanningStatus.Optimal)));

        AutomaticScheduleGenerationResult result = await
            new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        AutomaticScheduleGenerationPhaseReportSnapshot regular = Assert.Single(
            result.Report.PhaseReports,
            phase => phase.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Equal(
            AutomaticScheduleGenerationPhaseReportStatus.OptimalProven,
            regular.Status);
        Assert.Equal(
            "Die Optimalität dieses Teilziels wurde nachgewiesen.",
            regular.Explanation);
        Assert.Null(regular.MetricContext);
    }

    [Fact]
    public async Task FeasibleTimeoutExplainsRetainedIntermediateAndFollowingPhases()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        AutomaticSchedulePhaseSnapshot[] phases =
        [
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.InputValidation,
                AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(1)),
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.RegularCoverage,
                AutomaticSchedulePhaseStatus.Interrupted,
                TimeSpan.FromSeconds(120),
                [
                    new AutomaticSchedulePhaseValue("required_minutes", 600),
                    new AutomaticSchedulePhaseValue("covered_minutes", 480),
                    new AutomaticSchedulePhaseValue("uncovered_minutes", 120),
                ],
                new AutomaticSchedulePhaseTerminationSnapshot(
                    AutomaticSchedulePhaseTerminationReason
                        .TimeLimitWithFeasibleSelection,
                    AutomaticScheduleOptimizationTargetKind
                        .RegularTouchedDemandSlots,
                    TimeSpan.FromSeconds(120),
                    TimeSpan.FromMilliseconds(120_018))),
        ];
        RecordingPlanner planner = new(request => Task.FromResult(Success(
            request.Snapshot,
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
            phases: phases)));

        AutomaticScheduleGenerationResult result = await
            new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        AutomaticScheduleGenerationPhaseReportSnapshot regular = Assert.Single(
            result.Report.PhaseReports,
            phase => phase.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Equal(
            AutomaticScheduleGenerationPhaseReportStatus.FeasibleNotProvenOptimal,
            regular.Status);
        Assert.Contains("Zeitgrenze von 120,000 s", regular.Explanation);
        Assert.Contains(
            "vollständig berührte reguläre Bedarfsplätze",
            regular.Explanation);
        Assert.Contains("nach 120,018 s", regular.Explanation);
        Assert.Contains("zulässige Zwischenstand wurde behalten", regular.Explanation);
        Assert.Contains("Optimalität ist nicht nachgewiesen", regular.Explanation);
        Assert.Equal(
            "Die Werte beschreiben den behaltenen zulässigen Zwischenstand. Sie sind nicht als Optimum bewiesen.",
            regular.MetricContext);
        Assert.All(
            result.Report.PhaseReports.Where(phase =>
                phase.Kind > AutomaticSchedulePhaseKind.RegularCoverage),
            phase =>
            {
                Assert.Equal(
                    AutomaticScheduleGenerationPhaseReportStatus.NotReached,
                    phase.Status);
                Assert.Contains("Nicht begonnen", phase.Explanation);
                Assert.Contains("Reguläre Bedarfsdeckung", phase.Explanation);
                Assert.Contains("Zeitgrenze", phase.Explanation);
            });
    }

    public static TheoryData<
        AutomaticSchedulePhaseTerminationReason,
        AutomaticSchedulePlanningStatus,
        string> TerminationExplanations => new()
        {
            {
                AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection,
                AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult,
                "keine zulässige Auswahl"
            },
            {
                AutomaticSchedulePhaseTerminationReason.CancellationRequested,
                AutomaticSchedulePlanningStatus.Cancelled,
                "auf Anforderung"
            },
            {
                AutomaticSchedulePhaseTerminationReason.TechnicalFailure,
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                "technischen Fehlers"
            },
        };

    [Theory]
    [MemberData(nameof(TerminationExplanations))]
    public async Task FailedRunExplainsConfirmedTerminationAndFollowingPhases(
        AutomaticSchedulePhaseTerminationReason reason,
        AutomaticSchedulePlanningStatus planningStatus,
        string expectedText)
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        AutomaticSchedulePhaseTerminationSnapshot termination = reason switch
        {
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection =>
                new AutomaticSchedulePhaseTerminationSnapshot(
                    reason,
                    AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes,
                    TimeSpan.FromSeconds(120),
                    TimeSpan.FromMilliseconds(120_006)),
            _ => new AutomaticSchedulePhaseTerminationSnapshot(
                reason,
                AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes),
        };
        AutomaticSchedulePhaseSnapshot terminalPhase = new(
            AutomaticSchedulePhaseKind.RegularCoverage,
            reason == AutomaticSchedulePhaseTerminationReason.TechnicalFailure
                ? AutomaticSchedulePhaseStatus.Failed
                : AutomaticSchedulePhaseStatus.Interrupted,
            TimeSpan.FromMilliseconds(8),
            termination: termination);
        RecordingPlanner planner = new(_ => Task.FromResult(
            AutomaticSchedulePlanningResult.Failure(
                planningStatus,
                phases:
                [
                    new AutomaticSchedulePhaseSnapshot(
                        AutomaticSchedulePhaseKind.InputValidation,
                        AutomaticSchedulePhaseStatus.Completed,
                        TimeSpan.FromMilliseconds(1)),
                    terminalPhase,
                ])));

        AutomaticScheduleGenerationResult result = await
            new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        AutomaticScheduleGenerationPhaseReportSnapshot regular = Assert.Single(
            result.Report.PhaseReports,
            phase => phase.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Contains(expectedText, regular.Explanation);
        AutomaticScheduleGenerationPhaseReportSnapshot following = Assert.Single(
            result.Report.PhaseReports,
            phase => phase.Kind == AutomaticSchedulePhaseKind.ReliefCoverage);
        Assert.Contains("Nicht begonnen", following.Explanation);
        Assert.Contains(expectedText, following.Explanation);
    }

    [Fact]
    public async Task LegacyInterruptionMarksMissingDetailsWithoutInventingReason()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        AutomaticSchedulePhaseSnapshot[] phases =
        [
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.InputValidation,
                AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(1)),
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.RegularCoverage,
                AutomaticSchedulePhaseStatus.Interrupted,
                TimeSpan.FromSeconds(120),
                [new AutomaticSchedulePhaseValue("covered_minutes", 480)]),
        ];
        RecordingPlanner planner = new(request => Task.FromResult(Success(
            request.Snapshot,
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
            phases: phases)));

        AutomaticScheduleGenerationResult result = await
            new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        AutomaticScheduleGenerationPhaseReportSnapshot regular = Assert.Single(
            result.Report.PhaseReports,
            phase => phase.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Contains("älteren Lauf nicht gespeichert", regular.Explanation);
        Assert.DoesNotContain("Zeitgrenze", regular.Explanation);
        Assert.Equal(
            "Die aufgezeichneten Werte sind Teilstände und kein nachgewiesenes Optimum.",
            regular.MetricContext);
        AutomaticScheduleGenerationPhaseReportSnapshot following = Assert.Single(
            result.Report.PhaseReports,
            phase => phase.Kind == AutomaticSchedulePhaseKind.ReliefCoverage);
        Assert.Contains("Nicht begonnen", following.Explanation);
        Assert.Contains("älteren Lauf nicht gespeichert", following.Explanation);
        Assert.DoesNotContain("Zeitgrenze", following.Explanation);
    }

    [Fact]
    public async Task DiscardAndNewCommandRemovePreviewWithoutWriting()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(request => Task.FromResult(
            Success(request.Snapshot, AutomaticSchedulePlanningStatus.Optimal)));
        GenerateAutomaticScheduleCommand command = new(context.Data, planner);
        await command.ExecuteAsync(context.Request, TestContext.Current.CancellationToken);
        int savesBeforeDiscard = context.Data.SaveCallCount;

        Assert.True(command.DiscardPreview());
        Assert.Null(command.CurrentPreview);
        Assert.False(command.DiscardPreview());
        Assert.Null(new GenerateAutomaticScheduleCommand(context.Data, planner).CurrentPreview);
        Assert.Equal(savesBeforeDiscard, context.Data.SaveCallCount);
    }

    [Fact]
    public async Task NewFailedRunClearsEarlierTransientPreview()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        int call = 0;
        RecordingPlanner planner = new(request => Task.FromResult(
            Interlocked.Increment(ref call) == 1
                ? Success(request.Snapshot, AutomaticSchedulePlanningStatus.Optimal)
                : AutomaticSchedulePlanningResult.Failure(
                    AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult)));
        GenerateAutomaticScheduleCommand command = new(context.Data, planner);
        await command.ExecuteAsync(context.Request, TestContext.Current.CancellationToken);
        Assert.NotNull(command.CurrentPreview);

        AutomaticScheduleGenerationResult failed = await command.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            AutomaticScheduleGenerationStatus.TimedOutWithoutFeasibleResult,
            failed.Status);
        Assert.Null(command.CurrentPreview);
        Assert.Equal(2, planner.CallCount);
        Assert.Equal(1, context.Data.SaveCallCount);
    }

    [Fact]
    public async Task MissingPreparationNeverCallsPlannerOrStore()
    {
        ScheduleDraft draft = CreateReadyDraft();
        PlanningDataDouble data = new(
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(draft),
            CreateHistory());
        RecordingPlanner planner = new(_ => throw new InvalidOperationException());
        GenerateAutomaticScheduleCommand command = new(data, planner);

        AutomaticScheduleGenerationResult result = await command.ExecuteAsync(
            Request(draft, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.PreparationMissing, result.Status);
        Assert.Contains("vorbereitet", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, planner.CallCount);
        Assert.Equal(0, data.SaveCallCount);
        Assert.Null(command.CurrentPreview);
    }

    [Fact]
    public async Task ChangedInputMarksPreparationOutdatedBeforePlanning()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        context.Data.Workspace = ScheduleWorkspaceTestContext.CreateReadDataForDraft(
            context.Draft,
            entries:
            [
                ScheduleWorkspaceTestContext.CreateEntry(
                    ScheduleWorkspaceTestContext.StandardEmployeeId,
                    context.Draft.Period.StartMonday,
                    AvailabilityEntryKind.FixedDayOff,
                    1),
            ]);
        RecordingPlanner planner = new(_ => throw new InvalidOperationException());

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.PreparationOutdated, result.Status);
        Assert.Contains("aktualisieren", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, planner.CallCount);
        Assert.Equal(1, context.Data.SaveCallCount);
    }

    [Fact]
    public async Task VersionOnePreparationIsOutdatedAndDoesNotStartPlanner()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        PlanningInputSnapshot current = Assert.IsType<PlanningInputSnapshot>(
            context.Data.PreparedSnapshot);
        RuleCatalogSnapshot versionOne = await GetRuleCatalogQuery.ExecuteAsync(
            InitialRuleCatalog.VersionOne.Value,
            TestContext.Current.CancellationToken);
        context.Data.UsePreparedSnapshot(new PlanningInputSnapshot(
            current.Id,
            current.DraftId,
            current.DraftVersion,
            current.PeriodMonday,
            current.PeriodSunday,
            current.Employees,
            current.EmployeeTypes,
            current.ServiceCatalog,
            current.AvailabilityEntries,
            current.DemandSlots,
            current.ServiceManagementAssignments,
            versionOne,
            current.RunOptions,
            current.History));
        RecordingPlanner planner = new(_ => throw new InvalidOperationException());

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.PreparationOutdated, result.Status);
        Assert.Equal(0, planner.CallCount);
    }

    [Fact]
    public async Task ChangedProtectedTyp1AssignmentBlocksPlanning()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        DateOnly assignmentDate = context.Draft.Assignments[0].Date;
        context.Data.Workspace = ScheduleWorkspaceTestContext.CreateReadDataForDraft(
            context.Draft,
            entries:
            [
                ScheduleWorkspaceTestContext.CreateEntry(
                    ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
                    assignmentDate,
                    AvailabilityEntryKind.Vacation,
                    2),
            ]);
        RecordingPlanner planner = new(_ => throw new InvalidOperationException());

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.BlockedByInput, result.Status);
        Assert.Contains("Typ1", result.Message, StringComparison.Ordinal);
        Assert.Equal(0, planner.CallCount);
        Assert.Equal(1, context.Data.SaveCallCount);
    }

    [Fact]
    public async Task ChangedExpectedSnapshotIsReportedAsConflict()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(_ => throw new InvalidOperationException());

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request with { ExpectedSnapshotId = Guid.NewGuid() },
                TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.Conflict, result.Status);
        Assert.Contains("erneut", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, planner.CallCount);
    }

    public static TheoryData<
        AutomaticSchedulePlanningStatus,
        AutomaticScheduleGenerationStatus> FailureStatuses => new()
        {
            {
                AutomaticSchedulePlanningStatus.Cancelled,
                AutomaticScheduleGenerationStatus.Cancelled
            },
            {
                AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult,
                AutomaticScheduleGenerationStatus.TimedOutWithoutFeasibleResult
            },
            {
                AutomaticSchedulePlanningStatus.BlockedByInput,
                AutomaticScheduleGenerationStatus.BlockedByInput
            },
            {
                AutomaticSchedulePlanningStatus.UnsupportedRule,
                AutomaticScheduleGenerationStatus.UnsupportedRule
            },
            {
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                AutomaticScheduleGenerationStatus.TechnicalFailure
            },
        };

    [Theory]
    [MemberData(nameof(FailureStatuses))]
    public async Task EveryPlanningFailureStatusHasNoPreviewAndWritesNothing(
        AutomaticSchedulePlanningStatus planningStatus,
        AutomaticScheduleGenerationStatus expectedStatus)
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        AutomaticScheduleError[] errors = planningStatus is
            AutomaticSchedulePlanningStatus.BlockedByInput
            or AutomaticSchedulePlanningStatus.UnsupportedRule
            or AutomaticSchedulePlanningStatus.TechnicalFailure
                ? [new AutomaticScheduleError(
                    AutomaticScheduleErrorCode.TechnicalFailure,
                    CorrelationId: "synthetic-correlation")]
                : [];
        AutomaticSchedulePhaseSnapshot phase = new(
            AutomaticSchedulePhaseKind.InputValidation,
            AutomaticSchedulePhaseStatus.Completed,
            TimeSpan.FromMilliseconds(1));
        RecordingPlanner planner = new(_ => Task.FromResult(
            AutomaticSchedulePlanningResult.Failure(
                planningStatus,
                errors,
                [phase])));
        GenerateAutomaticScheduleCommand command = new(context.Data, planner);
        int savesBeforeGeneration = context.Data.SaveCallCount;

        AutomaticScheduleGenerationResult result = await command.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Null(result.Preview);
        Assert.Null(command.CurrentPreview);
        Assert.Equal(savesBeforeGeneration, context.Data.SaveCallCount);
        Assert.Equal(expectedStatus, result.Report.Status);
        Assert.Null(result.Report.PlanningReport);
        Assert.Same(phase, Assert.Single(result.Report.PlanningPhases));
        Assert.Equal(errors, result.Report.Errors);
        Assert.Equal(
            AutomaticSchedulePhaseKind.InputValidation,
            result.Report.LastReachedPhase);
        Assert.Equal(15, result.Report.PhaseReports.Count);
        Assert.Equal(
            AutomaticScheduleGenerationPhaseReportStatus.Completed,
            result.Report.PhaseReports[0].Status);
        Assert.All(
            result.Report.PhaseReports.Skip(1),
            value => Assert.Equal(
                AutomaticScheduleGenerationPhaseReportStatus.NotReached,
                value.Status));
        Assert.Null(result.Report.TechnicalDetails);
        Assert.Null(result.Report.Objective);
    }

    [Fact]
    public async Task CancellationBeforeStartDoesNotReadPlanOrCallPlanner()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(_ => throw new InvalidOperationException());
        int readsBefore = context.Data.LoadCallCount;

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                new CancellationToken(true));

        Assert.Equal(AutomaticScheduleGenerationStatus.Cancelled, result.Status);
        Assert.Equal(readsBefore, context.Data.LoadCallCount);
        Assert.Equal(0, planner.CallCount);
    }

    [Fact]
    public async Task CancellationDuringPlanningClearsPreviewAndAllowsRestart()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int call = 0;
        RecordingPlanner planner = new(async request =>
        {
            if (Interlocked.Increment(ref call) == 1)
            {
                entered.SetResult();
                await Task.Delay(Timeout.Infinite, request.CancellationToken);
            }

            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult);
        });
        GenerateAutomaticScheduleCommand command = new(context.Data, planner);
        using CancellationTokenSource cancellation = new();
        Task<AutomaticScheduleGenerationResult> running = command.ExecuteAsync(
            context.Request,
            cancellation.Token);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);

        cancellation.Cancel();
        AutomaticScheduleGenerationResult cancelled = await running;
        AutomaticScheduleGenerationResult restarted = await command.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.Cancelled, cancelled.Status);
        Assert.Equal(
            AutomaticScheduleGenerationStatus.TimedOutWithoutFeasibleResult,
            restarted.Status);
        Assert.Equal(2, planner.CallCount);
        Assert.Null(command.CurrentPreview);
    }

    [Fact]
    public async Task ConcurrentStartIsRejectedBeforeSecondPlannerCall()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingPlanner firstPlanner = new(async request =>
        {
            entered.SetResult();
            await release.Task.WaitAsync(request.CancellationToken);
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.TimedOutWithoutFeasibleResult);
        });
        RecordingPlanner secondPlanner = new(_ => throw new InvalidOperationException());
        GenerateAutomaticScheduleCommand first = new(context.Data, firstPlanner);
        GenerateAutomaticScheduleCommand second = new(context.Data, secondPlanner);
        Task<AutomaticScheduleGenerationResult> running = first.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);

        AutomaticScheduleGenerationResult concurrent = await second.ExecuteAsync(
            context.Request,
            TestContext.Current.CancellationToken);
        release.SetResult();
        await running;

        Assert.Equal(AutomaticScheduleGenerationStatus.ConcurrentRun, concurrent.Status);
        Assert.Equal(AutomaticScheduleErrorCode.ConcurrentRun,
            Assert.Single(concurrent.PlanningErrors).Code);
        Assert.Equal(0, secondPlanner.CallCount);
    }

    [Fact]
    public async Task UnexpectedExceptionIsSanitizedAndDoesNotRetry()
    {
        const string SensitiveMessage = "Mitarbeiter Erika Mustermann kompletter Dienstplan";
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(_ => throw new InvalidDataException(SensitiveMessage));
        RecordingTechnicalErrorStore technicalErrors = new();

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(
                context.Data,
                planner,
                technicalErrors).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        AutomaticScheduleError error = Assert.Single(result.PlanningErrors);
        AutomaticScheduleTechnicalFailureDetails details = Assert.IsType<
            AutomaticScheduleTechnicalFailureDetails>(error.TechnicalDetails);
        Assert.Equal(AutomaticScheduleGenerationStatus.TechnicalFailure, result.Status);
        Assert.Equal(typeof(InvalidDataException).FullName, error.Parameter);
        Assert.Equal(AutomaticScheduleTechnicalStage.PlanningBoundary, details.Stage);
        Assert.Equal(typeof(InvalidDataException).FullName, details.ExceptionType);
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
        Assert.DoesNotContain(SensitiveMessage, result.Message);
        Assert.DoesNotContain(SensitiveMessage, string.Join('|', result.PlanningErrors));
        Assert.Same(error, Assert.Single(technicalErrors.Errors));
        Assert.Equal(1, planner.CallCount);
        Assert.Null(result.Preview);
    }

    [Fact]
    public async Task PlannerTechnicalFailureIsPersistedWithUnchangedCorrelationIdentifier()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        const string CorrelationId = "diagnostic-correlation";
        InvalidOperationException exception = new("Sensitive plan content");
        AutomaticScheduleError planningError = new(
            AutomaticScheduleErrorCode.TechnicalFailure,
            CorrelationId: CorrelationId,
            Parameter: exception.GetType().FullName,
            TechnicalDetails:
                AutomaticScheduleTechnicalFailureDetails.FromException(
                    AutomaticScheduleTechnicalStage.Solving,
                    exception));
        RecordingPlanner planner = new(_ => Task.FromResult(
            AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                [planningError])));
        RecordingTechnicalErrorStore technicalErrors = new();

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(
                context.Data,
                planner,
                technicalErrors).ExecuteAsync(
                    context.Request,
                    TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.TechnicalFailure, result.Status);
        Assert.Equal(CorrelationId, Assert.Single(result.PlanningErrors).CorrelationId);
        Assert.Same(planningError, Assert.Single(technicalErrors.Errors));
        Assert.Equal("GenerateAutomaticSchedule", Assert.Single(technicalErrors.Operations));
    }

    [Fact]
    public async Task InconsistentSuccessfulProposalIsRejected()
    {
        GenerationTestContext context = await GenerationTestContext.CreateAsync();
        RecordingPlanner planner = new(request => Task.FromResult(
            Success(
                request.Snapshot,
                AutomaticSchedulePlanningStatus.Optimal,
                snapshotId: Guid.NewGuid())));

        AutomaticScheduleGenerationResult result =
            await new GenerateAutomaticScheduleCommand(context.Data, planner).ExecuteAsync(
                context.Request,
                TestContext.Current.CancellationToken);

        Assert.Equal(AutomaticScheduleGenerationStatus.TechnicalFailure, result.Status);
        Assert.Equal(
            AutomaticScheduleErrorCode.ResultValidationFailed,
            Assert.Single(result.PlanningErrors).Code);
        Assert.Null(result.Preview);
    }

    private static AutomaticSchedulePlanningResult Success(
        PlanningInputSnapshot snapshot,
        AutomaticSchedulePlanningStatus status,
        Guid? snapshotId = null,
        AutomaticSchedulePhaseSnapshot[]? phases = null)
    {
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
        ScheduleRuleEvaluationSet evaluations = new(
            catalog,
            catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.Satisfied,
                NoRuleResultParameters.Instance)));
        HashSet<(Guid SourceId, DateOnly Date, Guid WorkLocationId, Guid ShiftTypeId,
            int Ordinal)> covered = snapshot.ServiceManagementAssignments
            .SelectMany(assignment => assignment.Coverages)
            .Select(coverage => (
                coverage.DemandSourceId,
                coverage.Date,
                coverage.WorkLocationId,
                coverage.ShiftTypeId,
                coverage.Ordinal))
            .ToHashSet();
        AutomaticScheduleOpenDemand[] openDemands = snapshot.DemandSlots
            .Where(demand => !covered.Contains((
                demand.SourceId,
                demand.Date,
                demand.WorkLocationId,
                demand.ShiftTypeId,
                demand.Ordinal)))
            .Select(demand => new AutomaticScheduleOpenDemand(
                demand.SourceId,
                demand.Date,
                demand.WorkLocationId,
                demand.ShiftTypeId,
                demand.Ordinal,
                demand.ActualStart,
                demand.ActualEnd,
                demand.DurationMinutes,
                AutomaticScheduleOpenDemandKind.FullyUncovered))
            .ToArray();
        ScheduleObjectiveVector vector = new(
            openDemands.Sum(value => value.UncoveredMinutes),
            openDemands.Count(value =>
                value.Kind == AutomaticScheduleOpenDemandKind.FullyUncovered),
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            ["synthetic-assignment"]);
        AutomaticScheduleRunMetadata metadata = new(
            "Synthetic solver",
            "1.0",
            status,
            AutomaticSchedulePlanningRequest.ProductiveTimeLimit,
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(15),
            [new AutomaticScheduleSetting("workers", "1")],
            phases ?? CreateGenerationPhases(vector));
        AutomaticScheduleProposal proposal = new(
            snapshotId ?? snapshot.Id,
            snapshot.DraftId,
            snapshot.DraftVersion,
            [],
            [],
            openDemands,
            vector,
            evaluations,
            metadata);
        return AutomaticSchedulePlanningResult.Success(status, proposal);
    }

    private static AutomaticSchedulePhaseSnapshot[] CreateGenerationPhases(
        ScheduleObjectiveVector objective)
    {
        return Enum.GetValues<AutomaticSchedulePhaseKind>()
            .Select(kind => new AutomaticSchedulePhaseSnapshot(
                kind,
                kind == AutomaticSchedulePhaseKind.LowPriorityRules
                    ? AutomaticSchedulePhaseStatus.NotApplicable
                    : AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(1),
                kind switch
                {
                    AutomaticSchedulePhaseKind.InputValidation =>
                    [new AutomaticSchedulePhaseValue("issue_count", 0)],
                    AutomaticSchedulePhaseKind.RegularCoverage =>
                    [
                        new AutomaticSchedulePhaseValue(
                            "required_minutes",
                            objective.UncoveredEmployeeMinutes + 120L),
                        new AutomaticSchedulePhaseValue("covered_minutes", 60),
                        new AutomaticSchedulePhaseValue(
                            "uncovered_minutes",
                            objective.UncoveredEmployeeMinutes + 60L),
                        new AutomaticSchedulePhaseValue(
                            "covered_full_demand_count",
                            0),
                        new AutomaticSchedulePhaseValue(
                            "fully_uncovered_demand_count",
                            objective.FullyUncoveredDemandSlotCount + 1L),
                    ],
                    AutomaticSchedulePhaseKind.ReliefCoverage =>
                    [
                        new AutomaticSchedulePhaseValue(
                            "required_minutes",
                            objective.UncoveredEmployeeMinutes + 120L),
                        new AutomaticSchedulePhaseValue("initial_covered_minutes", 60),
                        new AutomaticSchedulePhaseValue("covered_minutes", 120),
                        new AutomaticSchedulePhaseValue(
                            "additional_covered_minutes",
                            60),
                        new AutomaticSchedulePhaseValue(
                            "initial_uncovered_minutes",
                            objective.UncoveredEmployeeMinutes + 60L),
                        new AutomaticSchedulePhaseValue(
                            "uncovered_minutes",
                            objective.UncoveredEmployeeMinutes),
                        new AutomaticSchedulePhaseValue(
                            "covered_full_demand_count",
                            1),
                        new AutomaticSchedulePhaseValue(
                            "initial_fully_uncovered_demand_count",
                            objective.FullyUncoveredDemandSlotCount + 1L),
                        new AutomaticSchedulePhaseValue(
                            "fully_uncovered_demand_count",
                            objective.FullyUncoveredDemandSlotCount),
                    ],
                    AutomaticSchedulePhaseKind.HighPriorityRules =>
                    [new AutomaticSchedulePhaseValue(
                        "violation_count",
                        objective.HighPriorityViolations.Count)],
                    AutomaticSchedulePhaseKind.ReliefShiftMinimization =>
                    [
                        new AutomaticSchedulePhaseValue(
                            "initial_assignment_count",
                            objective.ReliefShiftAssignmentCount),
                        new AutomaticSchedulePhaseValue(
                            "assignment_count",
                            objective.ReliefShiftAssignmentCount),
                    ],
                    AutomaticSchedulePhaseKind.SplitShiftMinimization =>
                    [
                        new AutomaticSchedulePhaseValue(
                            "initial_assignment_count",
                            objective.SplitShiftAssignmentCount),
                        new AutomaticSchedulePhaseValue(
                            "assignment_count",
                            objective.SplitShiftAssignmentCount),
                    ],
                    AutomaticSchedulePhaseKind.AuxiliaryMinimum =>
                    [
                        new AutomaticSchedulePhaseValue(
                            "violation_count",
                            objective.AuxiliaryWeeklyMinimum.ViolatedWeekCount),
                        new AutomaticSchedulePhaseValue(
                            "missing_minutes",
                            objective.AuxiliaryWeeklyMinimum.MissingMinutes),
                    ],
                    AutomaticSchedulePhaseKind.MediumPriorityRules =>
                    [new AutomaticSchedulePhaseValue(
                        "violation_count",
                        objective.MediumPriorityViolations.Count)],
                    AutomaticSchedulePhaseKind.Stability =>
                    [new AutomaticSchedulePhaseValue(
                        "total_spread",
                        objective.StabilityViolations.MagnitudeByRule.Values.Sum())],
                    _ => [],
                }))
            .ToArray();
    }

    private static AutomaticScheduleGenerationStatus Map(
        AutomaticSchedulePlanningStatus status) => status switch
        {
            AutomaticSchedulePlanningStatus.Optimal =>
                AutomaticScheduleGenerationStatus.Optimal,
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal =>
                AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal,
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };

    private static GenerateAutomaticScheduleRequest Request(
        ScheduleDraft draft,
        Guid snapshotId) => new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            snapshotId);

    private static ScheduleDraft CreateReadyDraft()
    {
        ScheduleDraft empty = ScheduleWorkspaceTestContext.CreateDraft();
        List<ScheduleAssignment> assignments = [];
        for (int week = 0; week < 3; week++)
        {
            DateOnly monday = empty.Period.StartMonday.AddDays(week * 7);
            DemandSlot slot = Assert.Single(
                empty.DemandSlots.Slots,
                item => item.Id.Date == monday
                    && item.Id.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id
                    && item.Id.Ordinal == 1);
            assignments.Add(Assert.IsType<ScheduleAssignment>(
                ScheduleAssignment.CreateNormal(
                    Guid.Parse($"61000000-0000-4000-8000-{week + 1:D12}"),
                    ScheduleWorkspaceTestContext.ServiceManagementEmployee.Id,
                    slot,
                    AssignmentOrigin.ServiceManagement).Value));
        }

        return ScheduleWorkspaceTestContext.CreateDraft(assignments: assignments);
    }

    private static PlanningHistoryDayReadItem[] CreateHistory()
    {
        DateOnly first = ScheduleWorkspaceTestContext.PeriodMonday.AddDays(-7);
        return Enumerable.Range(0, 7)
            .Select(index => new PlanningHistoryDayReadItem(
                first.AddDays(index),
                [
                    new PlanningHistoryAssignmentReadItem(
                        ScheduleWorkspaceTestContext.StandardEmployeeId,
                        300),
                ]))
            .ToArray();
    }

    private sealed class GenerationTestContext(
        ScheduleDraft draft,
        PlanningDataDouble data)
    {
        public ScheduleDraft Draft { get; } = draft;

        public PlanningDataDouble Data { get; } = data;

        public GenerateAutomaticScheduleRequest Request =>
            GenerateAutomaticScheduleCommandTests.Request(
                Draft,
                Data.PreparedSnapshot!.Id);

        public static async Task<GenerationTestContext> CreateAsync()
        {
            ScheduleDraft draft = CreateReadyDraft();
            PlanningDataDouble data = new(
                ScheduleWorkspaceTestContext.CreateReadDataForDraft(draft),
                CreateHistory());
            PreparePlanningInputResult result = await new PreparePlanningInputCommand(
                data,
                data).ExecuteAsync(
                    new PreparePlanningInputRequest(
                        draft.Id.Value,
                        draft.Version.Value,
                        draft.Period.StartMonday,
                        PlanningPreparationExpectation.NotPrepared,
                        null,
                        PlanningRunOptions.Default),
                    TestContext.Current.CancellationToken);
            Assert.Equal(PreparePlanningInputStatus.Succeeded, result.Status);
            return new GenerationTestContext(draft, data);
        }
    }

    private sealed class PlanningDataDouble(
        ScheduleWorkspaceReadData workspace,
        IEnumerable<PlanningHistoryDayReadItem> history)
        : IPlanningInputReader, IPreparePlanningSnapshotStore
    {
        private readonly PlanningHistoryDayReadItem[] history = history.ToArray();

        public ScheduleWorkspaceReadData Workspace { get; set; } = workspace;

        public PlanningInputSnapshot? PreparedSnapshot { get; private set; }

        public int LoadCallCount { get; private set; }

        public int SaveCallCount { get; private set; }

        public void UsePreparedSnapshot(PlanningInputSnapshot snapshot)
        {
            PreparedSnapshot = snapshot;
        }

        public Task<PlanningInputReadData> LoadAsync(
            SchedulePeriod period,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(new PlanningInputReadData(
                Workspace,
                history,
                PreparedSnapshot));
        }

        public Task<PreparePlanningSnapshotStoreResult> SaveAsync(
            PreparePlanningSnapshotChange change,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveCallCount++;
            PreparedSnapshot = change.Snapshot;
            return Task.FromResult(
                PreparePlanningSnapshotStoreResult.Success(change.Snapshot));
        }
    }

    private sealed class RecordingPlanner(
        Func<PlannerInvocation, Task<AutomaticSchedulePlanningResult>> implementation)
        : IAutomaticSchedulePlanner
    {
        public int CallCount { get; private set; }

        public AutomaticSchedulePlanningRequest? Request { get; private set; }

        public Task<AutomaticSchedulePlanningResult> PlanAsync(
            AutomaticSchedulePlanningRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            return implementation(new PlannerInvocation(request, cancellationToken));
        }
    }

    private sealed record PlannerInvocation(
        AutomaticSchedulePlanningRequest Request,
        CancellationToken CancellationToken)
    {
        public PlanningInputSnapshot Snapshot => Request.Snapshot;
    }

    private sealed class RecordingTechnicalErrorStore
        : IAutomaticScheduleTechnicalErrorStore
    {
        public List<string> Operations { get; } = [];

        public List<AutomaticScheduleError> Errors { get; } = [];

        public bool TryWrite(
            string operation,
            AutomaticScheduleError technicalError)
        {
            Operations.Add(operation);
            Errors.Add(technicalError);
            return true;
        }
    }
}

[CollectionDefinition(
    nameof(AutomaticScheduleGenerationExecutionGroup),
    DisableParallelization = true)]
public sealed class AutomaticScheduleGenerationExecutionGroup;
