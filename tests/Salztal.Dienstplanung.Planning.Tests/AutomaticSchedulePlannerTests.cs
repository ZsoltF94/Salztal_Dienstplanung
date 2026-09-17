using Salztal.Dienstplanung.Application.Rules;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Tests.Validation;

namespace Salztal.Dienstplanung.Planning.Tests;

public sealed class AutomaticSchedulePlannerTests
{
    public static TheoryData<PlanningInputSnapshot, AutomaticSchedulePlanningStatus>
        BlockedSnapshots => new()
        {
            {
                PlanningInputTestFactory.Create(
                    ruleCatalog: new RuleCatalogSnapshot(
                        99,
                        PlanningInputTestFactory.CreateRuleCatalog().Definitions)),
                AutomaticSchedulePlanningStatus.UnsupportedRule
            },
            {
                PlanningInputTestFactory.Create(
                    periodSunday: PlanningInputTestFactory.PeriodMonday.AddDays(19)),
                AutomaticSchedulePlanningStatus.BlockedByInput
            },
            {
                CreateWithUnprotectedAssignment(),
                AutomaticSchedulePlanningStatus.BlockedByInput
            },
            {
                PlanningInputTestFactory.Create(assignments: []),
                AutomaticSchedulePlanningStatus.BlockedByInput
            },
        };

    [Theory]
    [MemberData(nameof(BlockedSnapshots))]
    public async Task BlockedInputNeverReachesPlanningEngine(
        PlanningInputSnapshot snapshot,
        AutomaticSchedulePlanningStatus expectedStatus)
    {
        RecordingPlanningEngine engine = new();
        AutomaticSchedulePlanner planner = new(engine);

        AutomaticSchedulePlanningResult result = await planner.PlanAsync(
            new AutomaticSchedulePlanningRequest(snapshot, TimeSpan.FromSeconds(1)),
            CancellationToken.None);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(0, engine.CallCount);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidInputReachesPlanningEngineOnce()
    {
        RecordingPlanningEngine engine = new();
        AutomaticSchedulePlanner planner = new(engine);

        AutomaticSchedulePlanningResult result = await planner.PlanAsync(
            new AutomaticSchedulePlanningRequest(
                PlanningInputTestFactory.Create(),
                TimeSpan.FromSeconds(1)),
            CancellationToken.None);

        Assert.Equal(AutomaticSchedulePlanningStatus.TechnicalFailure, result.Status);
        Assert.Equal(1, engine.CallCount);
    }

    [Fact]
    public async Task PreCancelledRequestNeverReachesValidationOrEngine()
    {
        RecordingPlanningEngine engine = new();
        AutomaticSchedulePlanner planner = new(engine);

        AutomaticSchedulePlanningResult result = await planner.PlanAsync(
            new AutomaticSchedulePlanningRequest(
                PlanningInputTestFactory.Create(snapshotId: Guid.Empty),
                TimeSpan.FromSeconds(1)),
            new CancellationToken(true));

        Assert.Equal(AutomaticSchedulePlanningStatus.Cancelled, result.Status);
        Assert.Equal(0, engine.CallCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RuleErrorsKeepStableCatalogAndRuleParameters()
    {
        RuleCatalogSnapshot initial = PlanningInputTestFactory.CreateRuleCatalog();
        RuleDefinitionSnapshot unknown = new(
            "UNKNOWN_RULE",
            initial.Definitions[0].Family,
            initial.Definitions[0].Scope,
            initial.Definitions[0].AutomaticEffect,
            initial.Definitions[0].ManualEffect,
            initial.Definitions[0].Priority,
            initial.Definitions[0].Parameters,
            initial.Definitions[0].DescriptionKey);
        RuleCatalogSnapshot invalidCatalog = new(
            77,
            initial.Definitions.Skip(1).Append(unknown));
        RecordingPlanningEngine engine = new();
        AutomaticSchedulePlanner planner = new(engine);

        AutomaticSchedulePlanningResult result = await planner.PlanAsync(
            new AutomaticSchedulePlanningRequest(
                PlanningInputTestFactory.Create(ruleCatalog: invalidCatalog),
                TimeSpan.FromSeconds(1)),
            CancellationToken.None);

        Assert.Equal(AutomaticSchedulePlanningStatus.UnsupportedRule, result.Status);
        Assert.Equal(0, engine.CallCount);
        Assert.Equal(
            AutomaticScheduleErrorCode.UnknownCatalogVersion,
            result.Errors[0].Code);
        Assert.Contains(result.Errors, error =>
            error.Code == AutomaticScheduleErrorCode.UnknownRule
            && error.RuleId == "UNKNOWN_RULE"
            && error.CatalogVersion == 77);
        Assert.Contains(result.Errors, error =>
            error.Code == AutomaticScheduleErrorCode.RuleNotTranslated
            && error.RuleId == initial.Definitions[0].Id
            && error.CatalogVersion == 77
            && error.Parameter == "MissingFromSnapshot");
    }

    [Fact]
    public async Task UnexpectedEngineExceptionBecomesPrivacySafeTechnicalFailure()
    {
        const string SensitiveMessage = "Mitarbeiter Erika Mustermann kompletter Dienstplan";
        AutomaticSchedulePlanner planner = new(new ThrowingPlanningEngine(
            SensitiveMessage));

        AutomaticSchedulePlanningResult result = await planner.PlanAsync(
            new AutomaticSchedulePlanningRequest(
                PlanningInputTestFactory.Create(),
                TimeSpan.FromSeconds(1)),
            CancellationToken.None);

        AutomaticScheduleError error = Assert.Single(result.Errors);
        Assert.Equal(AutomaticSchedulePlanningStatus.TechnicalFailure, result.Status);
        Assert.Equal(AutomaticScheduleErrorCode.TechnicalFailure, error.Code);
        Assert.Equal(typeof(InvalidDataException).FullName, error.Parameter);
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
        Assert.Equal(
            AutomaticScheduleTechnicalStage.PlanningBoundary,
            error.TechnicalDetails?.Stage);
        Assert.DoesNotContain(SensitiveMessage, string.Join('|', result.Errors));
    }

    private static PlanningInputSnapshot CreateWithUnprotectedAssignment()
    {
        ScheduleDemandSlotSnapshot[] slots = PlanningInputTestFactory.CreateDemandSlots();
        ScheduleAssignmentSnapshot[] assignments =
        [
            PlanningInputTestFactory.CreateAssignment(slots[0], isProtected: false),
            .. PlanningInputTestFactory.CreateAssignments(slots).Skip(1),
        ];
        return PlanningInputTestFactory.Create(
            demandSlots: slots,
            assignments: assignments);
    }

    private sealed class RecordingPlanningEngine : IAutomaticSchedulePlanningEngine
    {
        public int CallCount { get; private set; }

        public Task<AutomaticSchedulePlanningResult> PlanAsync(
            AutomaticSchedulePlanningRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(
                AutomaticSchedulePlanningResult.Failure(
                    AutomaticSchedulePlanningStatus.TechnicalFailure));
        }
    }

    private sealed class ThrowingPlanningEngine(string message)
        : IAutomaticSchedulePlanningEngine
    {
        public Task<AutomaticSchedulePlanningResult> PlanAsync(
            AutomaticSchedulePlanningRequest request,
            CancellationToken cancellationToken) =>
            throw new InvalidDataException(message);
    }
}
