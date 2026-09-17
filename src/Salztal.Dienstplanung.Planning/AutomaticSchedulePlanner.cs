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
                AutomaticSchedulePlanningStatus.Cancelled);
        }

        try
        {
            PlanningInputValidationResult validation = inputValidator.Validate(
                request.Snapshot);
            if (!validation.IsValid)
            {
                return CreateBlockedResult(validation);
            }

            return await planningEngine.PlanAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return AutomaticSchedulePlanningResult.Failure(
                AutomaticSchedulePlanningStatus.Cancelled);
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
                            exception))]);
        }
    }

    private static AutomaticSchedulePlanningResult CreateBlockedResult(
        PlanningInputValidationResult validation)
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
            errors);
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
