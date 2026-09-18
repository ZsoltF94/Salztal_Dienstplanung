using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Desktop.Features.Scheduling;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

internal sealed class RecordingAutomaticScheduleGenerationActions
    : IAutomaticScheduleGenerationActions
{
    private readonly TaskCompletionSource<AutomaticScheduleGenerationOutcome>
        _generationCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public AutomaticScheduleAcceptanceOutcome AcceptanceOutcome { get; set; } = new(
        AutomaticScheduleAcceptanceStatus.Succeeded,
        "Der automatische Vorschlag wurde vollständig in den Entwurf übernommen.");

    public int GenerateCallCount { get; private set; }

    public int AcceptCallCount { get; private set; }

    public int DiscardCallCount { get; private set; }

    public GenerateAutomaticScheduleRequest? GenerationRequest { get; private set; }

    public AcceptAutomaticScheduleProposalRequest? AcceptanceRequest { get; private set; }

    public Task<AutomaticScheduleGenerationOutcome> GenerateAsync(
        GenerateAutomaticScheduleRequest request,
        CancellationToken cancellationToken)
    {
        GenerateCallCount++;
        GenerationRequest = request;
        cancellationToken.Register(() => _generationCompletion.TrySetResult(
            AutomaticScheduleGenerationTestData.CancelledOutcome()));
        return _generationCompletion.Task;
    }

    public Task<AutomaticScheduleAcceptanceOutcome> AcceptAsync(
        AcceptAutomaticScheduleProposalRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AcceptCallCount++;
        AcceptanceRequest = request;
        return Task.FromResult(AcceptanceOutcome);
    }

    public bool DiscardPreview()
    {
        DiscardCallCount++;
        return true;
    }

    public void CompleteGeneration(AutomaticScheduleGenerationOutcome outcome)
    {
        _generationCompletion.TrySetResult(outcome);
    }
}

internal sealed class RecordingAutomaticScheduleReportPresenter
    : IAutomaticScheduleReportPresenter
{
    public AutomaticScheduleReportSource? Source { get; private set; }

    public string? UnavailableMessage { get; private set; }

    public int ShowCallCount { get; private set; }

    public void SetSource(
        AutomaticScheduleReportSource? source,
        string unavailableMessage)
    {
        Source = source;
        UnavailableMessage = unavailableMessage;
    }

    public void ShowCurrent()
    {
        ShowCallCount++;
    }
}

internal static class AutomaticScheduleGenerationTestData
{
    public static readonly Guid DraftId = new("38443985-7d74-41a8-b5c3-6867dcb70934");
    public static readonly Guid SnapshotId = new("bd4af505-694b-40fd-aed3-c4018956d08e");
    public static readonly DateOnly PeriodMonday = new(2026, 9, 21);

    public static AutomaticScheduleGenerationContext ReadyContext() => new(
        DraftId,
        3,
        PeriodMonday,
        SnapshotId,
        SchedulePreparationStatus.Prepared,
        true,
        null);

    public static AutomaticScheduleGenerationOutcome SuccessOutcome(
        AutomaticSchedulePlanningStatus status = AutomaticSchedulePlanningStatus.Optimal,
        IEnumerable<AutomaticSchedulePhaseSnapshot>? phases = null)
    {
        AutomaticScheduleProposal proposal = CreateProposal(status, phases);
        AutomaticSchedulePlanningReport planningReport =
            PlanningDemandReportTestData.Create();
        AutomaticScheduleGenerationStatus generationStatus =
            status == AutomaticSchedulePlanningStatus.Optimal
                ? AutomaticScheduleGenerationStatus.Optimal
                : AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal;
        string message = status == AutomaticSchedulePlanningStatus.Optimal
            ? "Der automatische Vorschlag wurde erfolgreich erzeugt."
            : "Ein zulässiger Vorschlag wurde erzeugt.";
        return new AutomaticScheduleGenerationOutcome(
            generationStatus,
            message,
            new AutomaticSchedulePreview(status, proposal),
            [],
            new AutomaticScheduleGenerationReport(
                generationStatus,
                planningReport,
                proposal.Metadata.Phases,
                [],
                proposal.Metadata,
                AutomaticScheduleObjectiveSnapshot.Create(
                    proposal.ObjectiveVector)));
    }

    public static AutomaticScheduleGenerationOutcome TimeLimitedFeasibleOutcome() =>
        SuccessOutcome(
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
            CreateRegularCoverageInterruption(includeTermination: true));

    public static AutomaticScheduleGenerationOutcome LegacyInterruptedOutcome() =>
        SuccessOutcome(
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal,
            CreateRegularCoverageInterruption(includeTermination: false));

    public static AutomaticScheduleGenerationOutcome CancelledOutcome() => new(
        AutomaticScheduleGenerationStatus.Cancelled,
        "Die automatische Planung wurde abgebrochen. Der Entwurf blieb unverändert.",
        null,
        [],
        new AutomaticScheduleGenerationReport(
            AutomaticScheduleGenerationStatus.Cancelled,
            null,
            [],
            []));

    public static AutomaticScheduleGenerationOutcome TechnicalFailureOutcome() => new(
        AutomaticScheduleGenerationStatus.TechnicalFailure,
        "Die automatische Planung konnte wegen eines technischen Fehlers nicht abgeschlossen werden. Fehlerkennung: test-4711.",
        null,
        [new AutomaticScheduleError(
            AutomaticScheduleErrorCode.TechnicalFailure,
            CorrelationId: "test-4711")],
        new AutomaticScheduleGenerationReport(
            AutomaticScheduleGenerationStatus.TechnicalFailure,
            null,
            [new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.ModelBuilding,
                AutomaticSchedulePhaseStatus.Failed,
                TimeSpan.FromMilliseconds(5))],
            [new AutomaticScheduleError(
                AutomaticScheduleErrorCode.TechnicalFailure,
                CorrelationId: "test-4711")]));

    private static AutomaticScheduleProposal CreateProposal(
        AutomaticSchedulePlanningStatus status,
        IEnumerable<AutomaticSchedulePhaseSnapshot>? phases)
    {
        RuleCatalog catalog = InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value!;
        ScheduleRuleEvaluationSet evaluations = new(
            catalog,
            catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.NotApplicable,
                NoRuleResultParameters.Instance)));
        ScheduleObjectiveVector objective = new(
            240,
            1,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            RuleViolationSet.Empty,
            []);
        AutomaticScheduleRunMetadata metadata = new(
            "Synthetischer Testsolver",
            "1.0",
            status,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromSeconds(4),
            [new AutomaticScheduleSetting("workers", "1")],
            phases ?? CreateGenerationPhases(objective, status));
        return new AutomaticScheduleProposal(
            SnapshotId,
            DraftId,
            3,
            [],
            [new AutomaticScheduleDayOffProposal(
                new Guid("12b1d9a8-ec8c-4b5c-960d-d28c9ac1ad43"),
                PeriodMonday)],
            [
                new AutomaticScheduleOpenDemand(
                    new Guid("2d45a0c4-2c52-4c85-9e4f-333c087fd92b"),
                    PeriodMonday,
                    new Guid("08ce42ee-5966-428d-aea0-214833928442"),
                    new Guid("2a43f085-f20c-4fb5-821e-d23606b3730c"),
                    1,
                    new TimeOnly(6, 30),
                    new TimeOnly(9, 30),
                    180,
                    AutomaticScheduleOpenDemandKind.FullyUncovered),
                new AutomaticScheduleOpenDemand(
                    new Guid("352895eb-12df-4130-a2c9-484ab98d31d8"),
                    PeriodMonday.AddDays(1),
                    new Guid("08ce42ee-5966-428d-aea0-214833928442"),
                    new Guid("8d5e0be0-e6af-4ebf-a387-c66baaec2ff7"),
                    1,
                    new TimeOnly(16, 30),
                    new TimeOnly(17, 30),
                    60,
                    AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift),
            ],
            objective,
            evaluations,
            metadata);
    }

    private static AutomaticSchedulePhaseSnapshot[] CreateRegularCoverageInterruption(
        bool includeTermination)
    {
        return
        [
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.InputValidation,
                AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(3),
                [new AutomaticSchedulePhaseValue("issue_count", 0)]),
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.ModelBuilding,
                AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(40),
                [
                    new AutomaticSchedulePhaseValue("candidate_count", 42),
                    new AutomaticSchedulePhaseValue("remaining_demand_count", 8),
                ]),
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.HardRules,
                AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(80)),
            new AutomaticSchedulePhaseSnapshot(
                AutomaticSchedulePhaseKind.RegularCoverage,
                AutomaticSchedulePhaseStatus.Interrupted,
                TimeSpan.FromSeconds(120),
                [
                    new AutomaticSchedulePhaseValue("required_minutes", 360),
                    new AutomaticSchedulePhaseValue("covered_minutes", 120),
                    new AutomaticSchedulePhaseValue("uncovered_minutes", 240),
                    new AutomaticSchedulePhaseValue("covered_full_demand_count", 1),
                    new AutomaticSchedulePhaseValue(
                        "fully_uncovered_demand_count",
                        1),
                ],
                includeTermination
                    ? new AutomaticSchedulePhaseTerminationSnapshot(
                        AutomaticSchedulePhaseTerminationReason
                            .TimeLimitWithFeasibleSelection,
                        AutomaticScheduleOptimizationTargetKind
                            .RegularTouchedDemandSlots,
                        TimeSpan.FromSeconds(120),
                        TimeSpan.FromMilliseconds(120_018))
                    : null),
        ];
    }

    private static AutomaticSchedulePhaseSnapshot[] CreateGenerationPhases(
        ScheduleObjectiveVector objective,
        AutomaticSchedulePlanningStatus status)
    {
        return Enum.GetValues<AutomaticSchedulePhaseKind>()
            .Select(kind => new AutomaticSchedulePhaseSnapshot(
                kind,
                kind == AutomaticSchedulePhaseKind.LowPriorityRules
                    ? AutomaticSchedulePhaseStatus.NotApplicable
                    : kind == AutomaticSchedulePhaseKind.TechnicalTieBreak
                        && status
                            == AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal
                            ? AutomaticSchedulePhaseStatus.Interrupted
                            : AutomaticSchedulePhaseStatus.Completed,
                TimeSpan.FromMilliseconds(1),
                kind switch
                {
                    AutomaticSchedulePhaseKind.HighPriorityRules =>
                    [new AutomaticSchedulePhaseValue("violation_count", 0)],
                    AutomaticSchedulePhaseKind.ReliefShiftMinimization
                        or AutomaticSchedulePhaseKind.SplitShiftMinimization =>
                    [
                        new AutomaticSchedulePhaseValue(
                            "initial_assignment_count",
                            2),
                        new AutomaticSchedulePhaseValue("assignment_count", 0),
                    ],
                    AutomaticSchedulePhaseKind.AuxiliaryMinimum =>
                    [
                        new AutomaticSchedulePhaseValue("violation_count", 0),
                        new AutomaticSchedulePhaseValue("missing_minutes", 0),
                    ],
                    AutomaticSchedulePhaseKind.MediumPriorityRules =>
                    [new AutomaticSchedulePhaseValue("violation_count", 0)],
                    AutomaticSchedulePhaseKind.Stability =>
                    [new AutomaticSchedulePhaseValue(
                        "total_spread",
                        objective.StabilityViolations.MagnitudeByRule.Values.Sum())],
                    _ => [],
                }))
            .ToArray();
    }
}
