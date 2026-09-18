using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record GenerateAutomaticScheduleRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday,
    Guid ExpectedSnapshotId);

public enum AutomaticScheduleGenerationStatus
{
    Optimal,
    FeasibleNotProvenOptimal,
    ValidationFailed,
    DraftNotFound,
    PreparationMissing,
    PreparationOutdated,
    Conflict,
    Cancelled,
    TimedOutWithoutFeasibleResult,
    BlockedByInput,
    UnsupportedRule,
    ConcurrentRun,
    TechnicalFailure,
}

public sealed class AutomaticScheduleGenerationReport
{
    internal AutomaticScheduleGenerationReport(
        AutomaticScheduleGenerationStatus status,
        AutomaticSchedulePlanningReport? planningReport,
        IEnumerable<AutomaticSchedulePhaseSnapshot> planningPhases,
        IEnumerable<AutomaticScheduleError> errors,
        AutomaticScheduleRunMetadata? technicalDetails = null,
        AutomaticScheduleObjectiveSnapshot? objective = null)
    {
        PlanningPhases = AutomaticSchedulePhaseSequence.Create(planningPhases);
        AutomaticScheduleError[] errorValues = errors.ToArray();
        if (errorValues.Any(value => value is null))
        {
            throw new ArgumentException(
                "Generation report errors cannot contain null values.",
                nameof(errors));
        }

        bool succeeded = status is AutomaticScheduleGenerationStatus.Optimal
            or AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal;
        if (succeeded != (planningReport is not null))
        {
            throw new ArgumentException(
                "Only a successful generation report can contain planning values.",
                nameof(planningReport));
        }

        if (succeeded != (technicalDetails is not null)
            || succeeded != (objective is not null))
        {
            throw new ArgumentException(
                "Only a successful generation report can contain technical and objective values.");
        }

        if (succeeded)
        {
            ValidateSuccessfulReport(planningReport!, objective!, PlanningPhases);
        }

        Status = status;
        PlanningReport = planningReport;
        TechnicalDetails = technicalDetails;
        Objective = objective;
        PhaseReports = AutomaticScheduleGenerationPhaseReportCalculator.Create(
            status,
            PlanningPhases,
            objective,
            planningReport);
        LastReachedPhase = PlanningPhases.LastOrDefault()?.Kind;
        MeasuredPhaseDuration = PlanningPhases.Aggregate(
            TimeSpan.Zero,
            (total, phase) => total + phase.Duration);
        Errors = Array.AsReadOnly(errorValues);
    }

    public AutomaticScheduleGenerationStatus Status { get; }

    public AutomaticSchedulePlanningReport? PlanningReport { get; }

    public AutomaticScheduleRunMetadata? TechnicalDetails { get; }

    public AutomaticScheduleObjectiveSnapshot? Objective { get; }

    public ReadOnlyCollection<AutomaticSchedulePhaseSnapshot> PlanningPhases { get; }

    public ReadOnlyCollection<AutomaticScheduleGenerationPhaseReportSnapshot>
        PhaseReports
    { get; }

    public AutomaticSchedulePhaseKind? LastReachedPhase { get; }

    public TimeSpan MeasuredPhaseDuration { get; }

    public ReadOnlyCollection<AutomaticScheduleError> Errors { get; }

    private static void ValidateSuccessfulReport(
        AutomaticSchedulePlanningReport planningReport,
        AutomaticScheduleObjectiveSnapshot objective,
        IEnumerable<AutomaticSchedulePhaseSnapshot> phases)
    {
        if (planningReport.DemandTotal.OpenMinutes
                != objective.UncoveredEmployeeMinutes
            || planningReport.DemandTotal.UncoveredPersonCount
                != objective.FullyUncoveredDemandSlotCount)
        {
            throw new ArgumentException(
                "Generation objectives must match the planning report.");
        }

        ValidatePhaseValue(
            phases,
            AutomaticSchedulePhaseKind.HighPriorityRules,
            "violation_count",
            objective.HighPriorityViolations.Count);
        ValidatePhaseValue(
            phases,
            AutomaticSchedulePhaseKind.ReliefShiftMinimization,
            "assignment_count",
            objective.ReliefShiftAssignmentCount);
        ValidatePhaseValue(
            phases,
            AutomaticSchedulePhaseKind.SplitShiftMinimization,
            "assignment_count",
            objective.SplitShiftAssignmentCount);
        ValidatePhaseValue(
            phases,
            AutomaticSchedulePhaseKind.MediumPriorityRules,
            "violation_count",
            objective.MediumPriorityViolations.Count);
        ValidatePhaseValue(
            phases,
            AutomaticSchedulePhaseKind.Stability,
            "total_spread",
            objective.StabilityViolations.Sum(value => value.Magnitude));
    }

    private static void ValidatePhaseValue(
        IEnumerable<AutomaticSchedulePhaseSnapshot> phases,
        AutomaticSchedulePhaseKind kind,
        string key,
        long expected)
    {
        AutomaticSchedulePhaseValue? value = phases
            .SingleOrDefault(phase => phase.Kind == kind)?
            .Values
            .SingleOrDefault(item => item.Key == key);
        if (value is not null && value.Value != expected)
        {
            throw new ArgumentException(
                $"Generation phase {kind} does not match the final objective.");
        }
    }
}

public sealed class AutomaticScheduleGenerationResult
{
    private AutomaticScheduleGenerationResult(
        AutomaticScheduleGenerationStatus status,
        string message,
        AutomaticSchedulePreview? preview,
        ReadOnlyCollection<AutomaticScheduleError> planningErrors,
        AutomaticScheduleGenerationReport report)
    {
        Status = status;
        Message = message;
        Preview = preview;
        PlanningErrors = planningErrors;
        Report = report;
    }

    public AutomaticScheduleGenerationStatus Status { get; }

    public string Message { get; }

    public AutomaticSchedulePreview? Preview { get; }

    public ReadOnlyCollection<AutomaticScheduleError> PlanningErrors { get; }

    public AutomaticScheduleGenerationReport Report { get; }

    internal static AutomaticScheduleGenerationResult Success(
        AutomaticSchedulePlanningResult planningResult,
        AutomaticSchedulePlanningReport planningReport,
        string message)
    {
        ArgumentNullException.ThrowIfNull(planningResult);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(planningReport);
        AutomaticSchedulePreview preview = planningResult.Preview
            ?? throw new ArgumentException(
                "A successful planning result requires a preview.",
                nameof(planningResult));
        AutomaticScheduleGenerationStatus status = planningResult.Status switch
        {
            AutomaticSchedulePlanningStatus.Optimal =>
                AutomaticScheduleGenerationStatus.Optimal,
            AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal =>
                AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal,
            _ => throw new ArgumentOutOfRangeException(nameof(planningResult)),
        };
        return new AutomaticScheduleGenerationResult(
            status,
            message.Trim(),
            preview,
            Array.AsReadOnly(Array.Empty<AutomaticScheduleError>()),
            new AutomaticScheduleGenerationReport(
                status,
                planningReport,
                planningResult.Phases,
                [],
                preview.Proposal.Metadata,
                AutomaticScheduleObjectiveSnapshot.Create(
                    preview.Proposal.ObjectiveVector)));
    }

    internal static AutomaticScheduleGenerationResult Failure(
        AutomaticScheduleGenerationStatus status,
        string message,
        IEnumerable<AutomaticScheduleError>? planningErrors = null,
        IEnumerable<AutomaticSchedulePhaseSnapshot>? planningPhases = null)
    {
        if (status is AutomaticScheduleGenerationStatus.Optimal
            or AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        AutomaticScheduleError[] errors = (planningErrors ?? []).ToArray();
        if (errors.Any(error => error is null))
        {
            throw new ArgumentException(
                "Planning errors cannot contain null values.",
                nameof(planningErrors));
        }

        return new AutomaticScheduleGenerationResult(
            status,
            message.Trim(),
            null,
            Array.AsReadOnly(errors),
            new AutomaticScheduleGenerationReport(
                status,
                null,
                planningPhases ?? [],
                errors));
    }
}
