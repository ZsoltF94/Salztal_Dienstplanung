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
        AutomaticSchedulePlanningStatus status = AutomaticSchedulePlanningStatus.Optimal)
    {
        AutomaticScheduleProposal proposal = CreateProposal(status);
        string message = status == AutomaticSchedulePlanningStatus.Optimal
            ? "Der automatische Vorschlag wurde erfolgreich erzeugt."
            : "Ein zulässiger Vorschlag wurde erzeugt.";
        return new AutomaticScheduleGenerationOutcome(
            status == AutomaticSchedulePlanningStatus.Optimal
                ? AutomaticScheduleGenerationStatus.Optimal
                : AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal,
            message,
            new AutomaticSchedulePreview(status, proposal),
            []);
    }

    public static AutomaticScheduleGenerationOutcome CancelledOutcome() => new(
        AutomaticScheduleGenerationStatus.Cancelled,
        "Die automatische Planung wurde abgebrochen. Der Entwurf blieb unverändert.",
        null,
        []);

    public static AutomaticScheduleGenerationOutcome TechnicalFailureOutcome() => new(
        AutomaticScheduleGenerationStatus.TechnicalFailure,
        "Die automatische Planung konnte wegen eines technischen Fehlers nicht abgeschlossen werden. Fehlerkennung: test-4711.",
        null,
        [new AutomaticScheduleError(
            AutomaticScheduleErrorCode.TechnicalFailure,
            CorrelationId: "test-4711")]);

    private static AutomaticScheduleProposal CreateProposal(
        AutomaticSchedulePlanningStatus status)
    {
        RuleCatalog catalog = InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value!;
        ScheduleRuleEvaluationSet evaluations = new(
            catalog,
            catalog.Definitions.Select(definition => RuleEvaluationResult.Create(
                definition.Id,
                RuleEvaluationStatus.NotApplicable,
                NoRuleResultParameters.Instance)));
        ScheduleObjectiveVector objective = new(
            270,
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
            [new AutomaticScheduleSetting("workers", "1")]);
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
                    new TimeOnly(10, 30),
                    240,
                    AutomaticScheduleOpenDemandKind.FullyUncovered),
                new AutomaticScheduleOpenDemand(
                    new Guid("352895eb-12df-4130-a2c9-484ab98d31d8"),
                    PeriodMonday.AddDays(1),
                    new Guid("08ce42ee-5966-428d-aea0-214833928442"),
                    new Guid("8d5e0be0-e6af-4ebf-a387-c66baaec2ff7"),
                    1,
                    new TimeOnly(16, 30),
                    new TimeOnly(17, 0),
                    30,
                    AutomaticScheduleOpenDemandKind.PartiallyUncoveredReliefShift),
            ],
            objective,
            evaluations,
            metadata);
    }
}
