using System.Diagnostics;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Rules;
using Salztal.Dienstplanung.Planning.Validation;

namespace Salztal.Dienstplanung.Planning;

internal sealed class AutomaticSchedulePlanner : IAutomaticSchedulePlanner
{
    private readonly PlanningInputValidator inputValidator;
    private readonly IAutomaticSchedulePlanningEngine planningEngine;

    internal AutomaticSchedulePlanner(IAutomaticSchedulePlanningEngine planningEngine)
        : this(
            new PlanningInputValidator(new InitialRuleTranslationRegistry()),
            planningEngine)
    {
    }

    internal AutomaticSchedulePlanner(
        PlanningInputValidator inputValidator,
        IAutomaticSchedulePlanningEngine planningEngine)
    {
        ArgumentNullException.ThrowIfNull(inputValidator);
        ArgumentNullException.ThrowIfNull(planningEngine);
        this.inputValidator = inputValidator;
        this.planningEngine = planningEngine;
    }

    public async Task<AutomaticSchedulePlanningResult> PlanAsync(
        AutomaticSchedulePlanningRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.Cancelled,
                phases:
                [
                    new AutomaticSchedulePhaseSnapshot(
                        AutomaticSchedulePhaseKind.InputValidation,
                        AutomaticSchedulePhaseStatus.Interrupted,
                        TimeSpan.Zero,
                        termination: new AutomaticSchedulePhaseTerminationSnapshot(
                            AutomaticSchedulePhaseTerminationReason.CancellationRequested)),
                ]);
        }

        Stopwatch validationStopwatch = Stopwatch.StartNew();
        try
        {
            PlanningInputValidationResult validation = inputValidator.Validate(
                request.Snapshot);
            AutomaticSchedulePhaseSnapshot validationPhase = new(
                AutomaticSchedulePhaseKind.InputValidation,
                AutomaticSchedulePhaseStatus.Completed,
                validationStopwatch.Elapsed,
                [
                    new AutomaticSchedulePhaseValue(
                        "issue_count",
                        validation.Issues.Count),
                ]);
            if (!validation.IsValid)
            {
                return CreateBlockedResult(validation, validationPhase);
            }

            AutomaticSchedulePlanningResult result = await planningEngine.PlanAsync(
                request,
                cancellationToken);
            return result.PrependPhase(validationPhase);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.Cancelled,
                phases:
                [
                    new AutomaticSchedulePhaseSnapshot(
                        AutomaticSchedulePhaseKind.InputValidation,
                        AutomaticSchedulePhaseStatus.Interrupted,
                        validationStopwatch.Elapsed,
                        termination: new AutomaticSchedulePhaseTerminationSnapshot(
                            AutomaticSchedulePhaseTerminationReason.CancellationRequested)),
                ]);
        }
        catch (Exception exception)
        {
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.TechnicalFailure,
                [new AutomaticScheduleError(
                    AutomaticScheduleErrorCode.TechnicalFailure,
                    CorrelationId: Guid.NewGuid().ToString("N"),
                    Parameter: exception.GetType().FullName,
                    TechnicalDetails:
                        AutomaticScheduleTechnicalFailureDetails.FromException(
                            AutomaticScheduleTechnicalStage.PlanningBoundary,
                            exception))],
                [
                    new AutomaticSchedulePhaseSnapshot(
                        AutomaticSchedulePhaseKind.InputValidation,
                        AutomaticSchedulePhaseStatus.Failed,
                        validationStopwatch.Elapsed,
                        termination: new AutomaticSchedulePhaseTerminationSnapshot(
                            AutomaticSchedulePhaseTerminationReason.TechnicalFailure)),
                ]);
        }
    }

    private static AutomaticSchedulePlanningResult CreateBlockedResult(
        PlanningInputValidationResult validation,
        AutomaticSchedulePhaseSnapshot validationPhase)
    {
        AutomaticScheduleError[] errors = validation.Issues
            .Select(issue => new AutomaticScheduleError(
                MapCode(issue.Code),
                issue.RuleId,
                issue.TechnicalEntityId,
                CatalogVersion: issue.CatalogVersion,
                Date: issue.Date,
                Parameter: issue.Parameter))
            .ToArray();
        bool containsUnsupportedRule = errors.Any(error =>
            error.Code is AutomaticScheduleErrorCode.UnknownCatalogVersion
                or AutomaticScheduleErrorCode.UnknownRule
                or AutomaticScheduleErrorCode.RuleNotTranslated);

        return AutomaticSchedulePlanningResult.Failure(
            containsUnsupportedRule
                ? AutomaticSchedulePlanningStatus.UnsupportedRule
                : AutomaticSchedulePlanningStatus.BlockedByInput,
            errors,
            [validationPhase]);
    }

    private static AutomaticScheduleErrorCode MapCode(
        PlanningInputValidationCode code)
    {
        return code switch
        {
            PlanningInputValidationCode.UnknownCatalogVersion =>
                AutomaticScheduleErrorCode.UnknownCatalogVersion,
            PlanningInputValidationCode.UnknownRule =>
                AutomaticScheduleErrorCode.UnknownRule,
            PlanningInputValidationCode.RuleNotTranslated
                or PlanningInputValidationCode.DuplicateRule
                or PlanningInputValidationCode.RuleDefinitionMismatch =>
                AutomaticScheduleErrorCode.RuleNotTranslated,
            PlanningInputValidationCode.ProtectedAssignmentConflict =>
                AutomaticScheduleErrorCode.ProtectedAssignmentConflict,
            _ => AutomaticScheduleErrorCode.SnapshotOutdated,
        };
    }
}
