using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Application.Rules;

public static class GetRuleCatalogQuery
{
    public static Task<RuleCatalogSnapshot> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(InitialRuleCatalog.Version.Value, cancellationToken);
    }

    public static Task<RuleCatalogSnapshot> ExecuteAsync(
        int version,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!RuleCatalogVersion.TryCreate(version, out RuleCatalogVersion? requestedVersion))
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        RuleCatalogReadResult readResult = InitialRuleCatalog.Read(
            requestedVersion);
        RuleCatalog catalog = readResult.Value
            ?? throw new InvalidOperationException(
                $"Rule catalog version {version} cannot be read: {readResult.Error?.Code}.");

        RuleDefinitionSnapshot[] definitions = catalog.Definitions
            .Select(CreateDefinitionSnapshot)
            .ToArray();

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            new RuleCatalogSnapshot(catalog.Version.Value, definitions));
    }

    private static RuleDefinitionSnapshot CreateDefinitionSnapshot(
        RuleDefinition definition)
    {
        RuleParameters parameters = EnsureSupportedParameters(definition.Parameters);

        return new RuleDefinitionSnapshot(
            definition.Id.Value,
            definition.Family,
            definition.Scope,
            definition.AutomaticEffect,
            definition.ManualEffect,
            definition.Priority,
            parameters,
            definition.DescriptionKey.Value);
    }

    private static RuleParameters EnsureSupportedParameters(RuleParameters parameters)
    {
        return parameters switch
        {
            NoRuleParameters => parameters,
            PlanningRoleRuleParameters => parameters,
            ExplicitRunOptionRuleParameters => parameters,
            WeeklyManualAssignmentRuleParameters => parameters,
            EffectiveWeeklyTargetMaximumParameters => parameters,
            WeeklyMinutesMaximumParameters => parameters,
            MaximumConsecutiveWorkdaysParameters => parameters,
            VacationBoundaryWeekendParameters => parameters,
            ReliefShiftEmergencyParameters => parameters,
            EffectiveWeeklyTargetMinimumParameters => parameters,
            WeeklyConsecutiveDaysOffParameters => parameters,
            GuaranteedDayOffAdjacencyParameters => parameters,
            SplitShiftWeeklyMaximumParameters => parameters,
            PlanningPeriodFreeWeekendParameters => parameters,
            ShiftPatternMinimizationParameters => parameters,
            WeeklyMinutesTargetParameters => parameters,
            WeeklyMinutesMinimumParameters => parameters,
            RelativeWeeklyTargetParameters => parameters,
            WeeklyMinutesBelowNoticeParameters => parameters,
            WeeklyMinutesRangeNoticeParameters => parameters,
            CurrentPeriodFairDistributionParameters => parameters,
            _ => throw new InvalidOperationException(
                $"Unsupported rule parameters {parameters.GetType().FullName}."),
        };
    }
}
