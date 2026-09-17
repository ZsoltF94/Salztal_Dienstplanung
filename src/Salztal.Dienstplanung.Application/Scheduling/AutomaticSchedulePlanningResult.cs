using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticSchedulePlanningStatus
{
    Optimal,
    FeasibleNotProvenOptimal,
    Cancelled,
    TimedOutWithoutFeasibleResult,
    BlockedByInput,
    UnsupportedRule,
    TechnicalFailure,
}

public enum AutomaticScheduleErrorCode
{
    SnapshotMissing,
    SnapshotOutdated,
    UnknownCatalogVersion,
    UnknownRule,
    RuleNotTranslated,
    ProtectedAssignmentConflict,
    ResultValidationFailed,
    ConcurrentRun,
    UnexpectedSolverStatus,
    TechnicalFailure,
}

public sealed record AutomaticScheduleError(
    AutomaticScheduleErrorCode Code,
    string? RuleId = null,
    Guid? TechnicalEntityId = null,
    string? CorrelationId = null,
    int? CatalogVersion = null,
    DateOnly? Date = null,
    string? Parameter = null,
    AutomaticScheduleTechnicalFailureDetails? TechnicalDetails = null);

public sealed record AutomaticSchedulePreview
{
    public AutomaticSchedulePreview(
        AutomaticSchedulePlanningStatus status,
        AutomaticScheduleProposal proposal)
    {
        if (status is not AutomaticSchedulePlanningStatus.Optimal
            and not AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentNullException.ThrowIfNull(proposal);
        Status = status;
        Proposal = proposal;
    }

    public AutomaticSchedulePlanningStatus Status { get; }

    public AutomaticScheduleProposal Proposal { get; }
}

public sealed class AutomaticSchedulePlanningResult
{
    private AutomaticSchedulePlanningResult(
        AutomaticSchedulePlanningStatus status,
        AutomaticSchedulePreview? preview,
        ReadOnlyCollection<AutomaticScheduleError> errors)
    {
        Status = status;
        Preview = preview;
        Errors = errors;
    }

    public AutomaticSchedulePlanningStatus Status { get; }

    public AutomaticSchedulePreview? Preview { get; }

    public ReadOnlyCollection<AutomaticScheduleError> Errors { get; }

    public static AutomaticSchedulePlanningResult Success(
        AutomaticSchedulePlanningStatus status,
        AutomaticScheduleProposal proposal)
    {
        return new AutomaticSchedulePlanningResult(
            status,
            new AutomaticSchedulePreview(status, proposal),
            Array.AsReadOnly(Array.Empty<AutomaticScheduleError>()));
    }

    public static AutomaticSchedulePlanningResult Failure(
        AutomaticSchedulePlanningStatus status,
        IEnumerable<AutomaticScheduleError>? errors = null)
    {
        if (status is AutomaticSchedulePlanningStatus.Optimal
            or AutomaticSchedulePlanningStatus.FeasibleNotProvenOptimal)
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        AutomaticScheduleError[] errorValues = (errors ?? []).ToArray();
        if (errorValues.Any(error => error is null))
        {
            throw new ArgumentException(
                "Planning errors cannot contain null values.",
                nameof(errors));
        }

        return new AutomaticSchedulePlanningResult(
            status,
            null,
            Array.AsReadOnly(errorValues));
    }
}
