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

public sealed class AutomaticScheduleGenerationResult
{
    private AutomaticScheduleGenerationResult(
        AutomaticScheduleGenerationStatus status,
        string message,
        AutomaticSchedulePreview? preview,
        ReadOnlyCollection<AutomaticScheduleError> planningErrors)
    {
        Status = status;
        Message = message;
        Preview = preview;
        PlanningErrors = planningErrors;
    }

    public AutomaticScheduleGenerationStatus Status { get; }

    public string Message { get; }

    public AutomaticSchedulePreview? Preview { get; }

    public ReadOnlyCollection<AutomaticScheduleError> PlanningErrors { get; }

    internal static AutomaticScheduleGenerationResult Success(
        AutomaticSchedulePlanningResult planningResult,
        string message)
    {
        ArgumentNullException.ThrowIfNull(planningResult);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
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
            Array.AsReadOnly(Array.Empty<AutomaticScheduleError>()));
    }

    internal static AutomaticScheduleGenerationResult Failure(
        AutomaticScheduleGenerationStatus status,
        string message,
        IEnumerable<AutomaticScheduleError>? planningErrors = null)
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
            Array.AsReadOnly(errors));
    }
}
