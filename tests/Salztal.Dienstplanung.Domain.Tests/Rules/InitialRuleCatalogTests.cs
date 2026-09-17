using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class InitialRuleCatalogTests
{
    private static readonly ExpectedRule[] ExpectedRules =
    [
        Structure("STRUCTURE_KNOWN_REFERENCES", RuleScope.Plan),
        Structure("STRUCTURE_SINGLE_DAILY_ASSIGNMENT", RuleScope.EmployeeDay),
        Structure("STRUCTURE_NO_TIME_OVERLAP", RuleScope.EmployeeDay),
        Structure("STRUCTURE_BLOCKED_DAY_MARKER", RuleScope.EmployeeDay),
        Structure("STRUCTURE_NORMAL_SLOT_FULL_COVERAGE", RuleScope.DemandSlot),
        Structure("STRUCTURE_APPROVED_PLAN_IMMUTABLE", RuleScope.PlanVersion),
        Hard("ACTIVE_EMPLOYEES_ONLY", RuleScope.Assignment, RuleManualEffect.Block, typeof(NoRuleParameters)),
        Hard("SHIFT_ELIGIBILITY_REQUIRED", RuleScope.Assignment, RuleManualEffect.WarnAndRequireConfirmation, typeof(NoRuleParameters)),
        Hard("EXPLICIT_RUN_OPTION_REQUIRED", RuleScope.PlanningRun, RuleManualEffect.NotApplicable, typeof(ExplicitRunOptionRuleParameters)),
        Hard("TYPE1_MANUAL_ONLY", RuleScope.Assignment, RuleManualEffect.NotApplicable, typeof(PlanningRoleRuleParameters)),
        Hard("TYPE1_WEEKLY_PREREQUISITE", RuleScope.EmployeeWeek, RuleManualEffect.NotApplicable, typeof(WeeklyManualAssignmentRuleParameters)),
        Hard("NORMAL_WEEKLY_MAXIMUM", RuleScope.EmployeeWeek, RuleManualEffect.WarnAndRequireConfirmation, typeof(EffectiveWeeklyTargetMaximumParameters)),
        Hard("AH_WEEKLY_MAXIMUM", RuleScope.EmployeeWeek, RuleManualEffect.WarnAndRequireConfirmation, typeof(WeeklyMinutesMaximumParameters)),
        Hard("MAX_CONSECUTIVE_WORKDAYS", RuleScope.EmployeeWorkSequence, RuleManualEffect.WarnAndRequireConfirmation, typeof(MaximumConsecutiveWorkdaysParameters)),
        Hard("POST_VACATION_WEEKEND_FREE", RuleScope.EmployeeWeekend, RuleManualEffect.WarnAndRequireConfirmation, typeof(VacationBoundaryWeekendParameters)),
        Hard("AUTOMATIC_NO_OVERSTAFFING", RuleScope.DemandSlot, RuleManualEffect.WarnAndRequireConfirmation, typeof(NoRuleParameters)),
        Hard("RELIEF_SHIFT_EMERGENCY_ONLY", RuleScope.ShiftPatternAssignment, RuleManualEffect.Block, typeof(ReliefShiftEmergencyParameters)),
        Soft("NORMAL_WEEKLY_MINIMUM", RuleScope.EmployeeWeek, RulePriority.High, typeof(EffectiveWeeklyTargetMinimumParameters)),
        Soft("WEEKLY_CONSECUTIVE_DAYS_OFF", RuleScope.EmployeeWeek, RulePriority.High, typeof(WeeklyConsecutiveDaysOffParameters)),
        Soft("RED_X_ADJACENT_DAY_OFF", RuleScope.EmployeeWeek, RulePriority.High, typeof(GuaranteedDayOffAdjacencyParameters)),
        Soft("PRE_VACATION_WEEKEND_FREE", RuleScope.EmployeeWeekend, RulePriority.High, typeof(VacationBoundaryWeekendParameters)),
        Soft("SPLIT_SHIFT_WEEKLY_MAXIMUM", RuleScope.EmployeeWeek, RulePriority.High, typeof(SplitShiftWeeklyMaximumParameters)),
        Soft("THREE_WEEK_FREE_WEEKEND", RuleScope.PlanningPeriod, RulePriority.Medium, typeof(PlanningPeriodFreeWeekendParameters)),
        Soft("MINIMIZE_SPLIT_SHIFTS", RuleScope.PlanningPeriod, RulePriority.Medium, typeof(ShiftPatternMinimizationParameters)),
        Soft("AH_WEEKLY_TARGET", RuleScope.EmployeeWeek, RulePriority.Medium, typeof(WeeklyMinutesTargetParameters)),
        Notice("AH_WEEKLY_LOW_NOTICE", typeof(WeeklyMinutesBelowNoticeParameters)),
        Notice("AH_WEEKLY_HIGH_NOTICE", typeof(WeeklyMinutesRangeNoticeParameters)),
        Stability("CURRENT_PERIOD_FAIR_DISTRIBUTION", typeof(CurrentPeriodFairDistributionParameters)),
    ];

    [Fact]
    public void ReadVersionOneReturnsCompleteDeterministicCatalog()
    {
        RuleCatalog catalog = ReadVersionOne();

        Assert.Equal(1, catalog.Version.Value);
        Assert.Equal(28, catalog.Definitions.Count);
        Assert.Equal(
            ExpectedRules.Select(expected => expected.Id),
            catalog.Definitions.Select(definition => definition.Id.Value));
        Assert.Equal(
            28,
            catalog.Definitions.Select(definition => definition.Id).Distinct().Count());
    }

    [Fact]
    public void ReadVersionOneMapsEveryDefinitionToApprovedMatrix()
    {
        RuleCatalog catalog = ReadVersionOne();

        foreach ((ExpectedRule expected, RuleDefinition actual) in
                 ExpectedRules.Zip(catalog.Definitions))
        {
            Assert.Equal(expected.Id, actual.Id.Value);
            Assert.Equal(expected.Family, actual.Family);
            Assert.Equal(expected.Scope, actual.Scope);
            Assert.Equal(expected.AutomaticEffect, actual.AutomaticEffect);
            Assert.Equal(expected.ManualEffect, actual.ManualEffect);
            Assert.Equal(expected.Priority, actual.Priority);
            Assert.IsType(expected.ParameterType, actual.Parameters);
        }
    }

    [Fact]
    public void ReadVersionOneWhenRepeatedReturnsSameImmutableValuesInSameOrder()
    {
        RuleCatalog first = ReadVersionOne();
        RuleCatalog second = ReadVersionOne();

        Assert.Same(first, second);
        Assert.Equal(first.Definitions, second.Definitions);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<RuleDefinition>)first.Definitions).Add(first.Definitions[0]));
    }

    [Fact]
    public void ReadWhenVersionIsUnknownReturnsStructuredUnsupportedVersionError()
    {
        Assert.True(RuleCatalogVersion.TryCreate(3, out RuleCatalogVersion? version));

        RuleCatalogReadResult result = InitialRuleCatalog.Read(version);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(RuleCatalogReadCode.UnsupportedVersion, result.Error!.Code);
        Assert.Equal(version, result.Error.RequestedVersion);
    }

    [Fact]
    public void ReadCurrentVersionReturnsVersionTwoWithoutReinterpretingVersionOne()
    {
        RuleCatalog versionOne = ReadVersionOne();
        RuleCatalogReadResult currentResult = InitialRuleCatalog.Read(
            InitialRuleCatalog.Version);
        RuleCatalog current = Assert.IsType<RuleCatalog>(currentResult.Value);

        Assert.Equal(1, versionOne.Version.Value);
        Assert.Equal(2, current.Version.Value);
        Assert.Equal(28, versionOne.Definitions.Count);
        Assert.Equal(30, current.Definitions.Count);
        Assert.Contains(versionOne.Definitions, rule =>
            rule.Id == InitialSoftRuleDefinitions.AuxiliaryWeeklyTarget.Id);
        Assert.DoesNotContain(current.Definitions, rule =>
            rule.Id == InitialSoftRuleDefinitions.AuxiliaryWeeklyTarget.Id);
        Assert.Contains(current.Definitions, rule =>
            rule.Id == CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum.Id);
        Assert.Contains(current.Definitions, rule =>
            rule.Id == CurrentSoftRuleDefinitions.RelativeWeeklyTarget.Id);
    }

    [Fact]
    public void FindWhenIdentifierIsKnownReturnsExactDefinition()
    {
        RuleCatalog catalog = ReadVersionOne();
        RuleDefinition expected = catalog.Definitions[13];

        RuleDefinitionLookupResult result = catalog.Find(expected.Id);

        Assert.True(result.IsSuccess);
        Assert.Same(expected, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void FindWhenIdentifierIsUnknownReturnsStructuredErrorWithoutFallback()
    {
        RuleCatalog catalog = ReadVersionOne();
        Assert.True(RuleId.TryCreate("UNKNOWN_RULE", out RuleId? unknownRuleId));

        RuleDefinitionLookupResult result = catalog.Find(unknownRuleId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(RuleDefinitionLookupCode.UnknownRuleId, result.Error!.Code);
        Assert.Equal(unknownRuleId, result.Error.RuleId);
    }

    [Fact]
    public void CatalogWhenGroupedMatchesApprovedCompletenessCounts()
    {
        RuleCatalog catalog = ReadVersionOne();

        Assert.Equal(6, catalog.Definitions.Count(rule => rule.Family == RuleFamily.Structure));
        Assert.Equal(11, catalog.Definitions.Count(rule => rule.Family == RuleFamily.AutomaticHard));
        Assert.Equal(8, catalog.Definitions.Count(rule => rule.Family == RuleFamily.Soft));
        Assert.Equal(2, catalog.Definitions.Count(rule => rule.Family == RuleFamily.Notice));
        Assert.Single(catalog.Definitions, rule => rule.Family == RuleFamily.Stability);
        Assert.Equal(5, catalog.Definitions.Count(rule => rule.Priority == RulePriority.High));
        Assert.Equal(3, catalog.Definitions.Count(rule => rule.Priority == RulePriority.Medium));
        Assert.DoesNotContain(catalog.Definitions, rule => rule.Priority == RulePriority.Low);
    }

    private static RuleCatalog ReadVersionOne()
    {
        RuleCatalogReadResult result = InitialRuleCatalog.Read(
            InitialRuleCatalog.VersionOne);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        return Assert.IsType<RuleCatalog>(result.Value);
    }

    private static ExpectedRule Structure(string id, RuleScope scope)
    {
        return new ExpectedRule(
            id,
            RuleFamily.Structure,
            scope,
            RuleAutomaticEffect.BlockInvalidStructure,
            RuleManualEffect.Block,
            null,
            typeof(NoRuleParameters));
    }

    private static ExpectedRule Hard(
        string id,
        RuleScope scope,
        RuleManualEffect manualEffect,
        Type parameterType)
    {
        return new ExpectedRule(
            id,
            RuleFamily.AutomaticHard,
            scope,
            RuleAutomaticEffect.HardConstraint,
            manualEffect,
            null,
            parameterType);
    }

    private static ExpectedRule Soft(
        string id,
        RuleScope scope,
        RulePriority priority,
        Type parameterType)
    {
        return new ExpectedRule(
            id,
            RuleFamily.Soft,
            scope,
            RuleAutomaticEffect.OptimizationObjective,
            RuleManualEffect.WarnAndRequireConfirmation,
            priority,
            parameterType);
    }

    private static ExpectedRule Notice(string id, Type parameterType)
    {
        return new ExpectedRule(
            id,
            RuleFamily.Notice,
            RuleScope.EmployeeWeek,
            RuleAutomaticEffect.ReportNotice,
            RuleManualEffect.ReportNotice,
            null,
            parameterType);
    }

    private static ExpectedRule Stability(string id, Type parameterType)
    {
        return new ExpectedRule(
            id,
            RuleFamily.Stability,
            RuleScope.PlanningPeriod,
            RuleAutomaticEffect.StabilityTieBreaker,
            RuleManualEffect.NotApplicable,
            null,
            parameterType);
    }

    private sealed record ExpectedRule(
        string Id,
        RuleFamily Family,
        RuleScope Scope,
        RuleAutomaticEffect AutomaticEffect,
        RuleManualEffect ManualEffect,
        RulePriority? Priority,
        Type ParameterType);
}
