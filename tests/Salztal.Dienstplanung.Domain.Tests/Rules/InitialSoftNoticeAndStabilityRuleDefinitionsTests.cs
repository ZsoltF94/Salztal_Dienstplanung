using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class InitialSoftNoticeAndStabilityRuleDefinitionsTests
{
    private const RuleDayMarkerKinds RegularDayOffMarkers =
        RuleDayMarkerKinds.GuaranteedDayOff | RuleDayMarkerKinds.GeneratedDayOff;

    private static readonly string[] ExpectedSoftRuleIds =
    [
        "NORMAL_WEEKLY_MINIMUM",
        "WEEKLY_CONSECUTIVE_DAYS_OFF",
        "RED_X_ADJACENT_DAY_OFF",
        "PRE_VACATION_WEEKEND_FREE",
        "SPLIT_SHIFT_WEEKLY_MAXIMUM",
        "THREE_WEEK_FREE_WEEKEND",
        "MINIMIZE_SPLIT_SHIFTS",
        "AH_WEEKLY_TARGET",
    ];

    private static readonly RulePriority[] ExpectedSoftRulePriorities =
    [
        RulePriority.High,
        RulePriority.High,
        RulePriority.High,
        RulePriority.High,
        RulePriority.High,
        RulePriority.Medium,
        RulePriority.Medium,
        RulePriority.Medium,
    ];

    public static IEnumerable<object[]> AllDefinitions()
    {
        return InitialSoftRuleDefinitions.All
            .Concat(InitialNoticeRuleDefinitions.All)
            .Concat(InitialStabilityRuleDefinitions.All)
            .Select(definition => new object[] { definition });
    }

    [Theory]
    [MemberData(nameof(AllDefinitions))]
    public void InitialDefinitionWhenReadIsValidAndFullyTyped(RuleDefinition definition)
    {
        Assert.NotNull(definition.Id);
        Assert.NotNull(definition.DescriptionKey);
        Assert.NotNull(definition.Parameters);
        Assert.True(Enum.IsDefined(definition.Family));
        Assert.True(Enum.IsDefined(definition.Scope));
        Assert.True(Enum.IsDefined(definition.AutomaticEffect));
        Assert.True(Enum.IsDefined(definition.ManualEffect));
    }

    [Fact]
    public void SoftRulesWhenReadMatchApprovedIdentifiersAndPriorities()
    {
        Assert.Equal(
            ExpectedSoftRuleIds,
            InitialSoftRuleDefinitions.All.Select(rule => rule.Id.Value));

        Assert.Equal(
            ExpectedSoftRulePriorities,
            InitialSoftRuleDefinitions.All.Select(rule => rule.Priority!.Value));

        Assert.All(
            InitialSoftRuleDefinitions.All,
            definition =>
            {
                Assert.Equal(RuleFamily.Soft, definition.Family);
                Assert.Equal(
                    RuleAutomaticEffect.OptimizationObjective,
                    definition.AutomaticEffect);
                Assert.Equal(
                    RuleManualEffect.WarnAndRequireConfirmation,
                    definition.ManualEffect);
            });
    }

    [Fact]
    public void LowPriorityWhenNoInitialRuleUsesItRemainsValidInRuleLanguage()
    {
        RuleDefinitionValidationResult result = RuleDefinition.Create(
            "FUTURE_LOW_PRIORITY_RULE",
            RuleFamily.Soft,
            RuleScope.PlanningPeriod,
            RuleAutomaticEffect.OptimizationObjective,
            RuleManualEffect.WarnAndRequireConfirmation,
            RulePriority.Low,
            NoRuleParameters.Instance,
            "rules.future.low_priority");

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(
            InitialSoftRuleDefinitions.All,
            definition => definition.Priority == RulePriority.Low);
    }

    [Fact]
    public void FreeDayRulesWhenReadDistinguishRegularDaysOffFromWorkSequenceBreakers()
    {
        WeeklyConsecutiveDaysOffParameters consecutiveDaysOff =
            Assert.IsType<WeeklyConsecutiveDaysOffParameters>(
                InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff.Parameters);
        GuaranteedDayOffAdjacencyParameters adjacency =
            Assert.IsType<GuaranteedDayOffAdjacencyParameters>(
                InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff.Parameters);
        MaximumConsecutiveWorkdaysParameters workSequence =
            Assert.IsType<MaximumConsecutiveWorkdaysParameters>(
                InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays.Parameters);

        Assert.Equal(2, consecutiveDaysOff.MinimumConsecutiveDays);
        Assert.Equal(RegularDayOffMarkers, consecutiveDaysOff.QualifyingRegularDayOffMarkers);
        Assert.Equal(2, adjacency.MinimumBlockLength);
        Assert.Equal(RuleDayMarkerKinds.GuaranteedDayOff, adjacency.AnchorMarker);
        Assert.Equal(RegularDayOffMarkers, adjacency.AdjacentQualifyingMarkers);
        Assert.Equal(
            RuleDayMarkerKinds.Vacation
                | RuleDayMarkerKinds.Sickness
                | RegularDayOffMarkers,
            workSequence.WorkSequenceBreakingMarkers);
        Assert.Equal(
            RuleDayMarkerKinds.None,
            consecutiveDaysOff.QualifyingRegularDayOffMarkers
                & (RuleDayMarkerKinds.Vacation | RuleDayMarkerKinds.Sickness));
    }

    [Fact]
    public void WeeklyAndVacationRulesWhenReadContainApprovedThresholdsAndBoundaries()
    {
        EffectiveWeeklyTargetMinimumParameters weeklyMinimum =
            Assert.IsType<EffectiveWeeklyTargetMinimumParameters>(
                InitialSoftRuleDefinitions.NormalWeeklyMinimum.Parameters);
        VacationBoundaryWeekendParameters vacationWeekend =
            Assert.IsType<VacationBoundaryWeekendParameters>(
                InitialSoftRuleDefinitions.PreVacationWeekendFree.Parameters);
        SplitShiftWeeklyMaximumParameters splitMaximum =
            Assert.IsType<SplitShiftWeeklyMaximumParameters>(
                InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum.Parameters);

        Assert.Equal(EmployeeTypePlanningRole.Normal, weeklyMinimum.Role);
        Assert.Equal(3 * 60, weeklyMinimum.MinimumMinutesBelowTarget);
        Assert.Equal(VacationBoundaryKind.Start, vacationWeekend.Boundary);
        Assert.Equal(DayOfWeek.Monday, vacationWeekend.BoundaryDay);
        Assert.Equal(DayOfWeek.Saturday, vacationWeekend.WeekendFirstDay);
        Assert.Equal(DayOfWeek.Sunday, vacationWeekend.WeekendSecondDay);
        Assert.Equal(1, splitMaximum.MaximumAssignments);
    }

    [Fact]
    public void MediumRulesWhenReadContainApprovedThreeWeekAndAuxiliaryTargets()
    {
        PlanningPeriodFreeWeekendParameters freeWeekend =
            Assert.IsType<PlanningPeriodFreeWeekendParameters>(
                InitialSoftRuleDefinitions.ThreeWeekFreeWeekend.Parameters);
        ShiftPatternMinimizationParameters splitMinimization =
            Assert.IsType<ShiftPatternMinimizationParameters>(
                InitialSoftRuleDefinitions.MinimizeSplitShifts.Parameters);
        WeeklyMinutesTargetParameters auxiliaryTarget =
            Assert.IsType<WeeklyMinutesTargetParameters>(
                InitialSoftRuleDefinitions.AuxiliaryWeeklyTarget.Parameters);

        Assert.Equal(3, freeWeekend.PeriodWeeks);
        Assert.Equal(1, freeWeekend.MinimumFreeWeekends);
        Assert.Equal(DayOfWeek.Saturday, freeWeekend.WeekendFirstDay);
        Assert.Equal(DayOfWeek.Sunday, freeWeekend.WeekendSecondDay);
        Assert.Equal(RegularDayOffMarkers, freeWeekend.QualifyingRegularDayOffMarkers);
        Assert.Equal(ShiftPatternRuleKind.SplitShift, splitMinimization.PatternKind);
        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, auxiliaryTarget.Role);
        Assert.Equal(10 * 60, auxiliaryTarget.TargetMinutes);
    }

    [Fact]
    public void NoticesWhenReadContainApprovedExclusiveAndInclusiveThresholds()
    {
        WeeklyMinutesBelowNoticeParameters lowNotice =
            Assert.IsType<WeeklyMinutesBelowNoticeParameters>(
                InitialNoticeRuleDefinitions.AuxiliaryWeeklyLow.Parameters);
        WeeklyMinutesRangeNoticeParameters highNotice =
            Assert.IsType<WeeklyMinutesRangeNoticeParameters>(
                InitialNoticeRuleDefinitions.AuxiliaryWeeklyHigh.Parameters);

        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, lowNotice.Role);
        Assert.Equal(6 * 60, lowNotice.ExclusiveUpperMinutes);
        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, highNotice.Role);
        Assert.Equal(10 * 60, highNotice.ExclusiveLowerMinutes);
        Assert.Equal(12 * 60, highNotice.InclusiveUpperMinutes);
        Assert.All(
            InitialNoticeRuleDefinitions.All,
            definition =>
            {
                Assert.Equal(RuleFamily.Notice, definition.Family);
                Assert.Equal(RuleAutomaticEffect.ReportNotice, definition.AutomaticEffect);
                Assert.Equal(RuleManualEffect.ReportNotice, definition.ManualEffect);
                Assert.Null(definition.Priority);
            });
    }

    [Fact]
    public void StabilityWhenReadRemainsAfterAllPriorityLevels()
    {
        RuleDefinition definition =
            InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution;
        CurrentPeriodFairDistributionParameters parameters =
            Assert.IsType<CurrentPeriodFairDistributionParameters>(definition.Parameters);

        Assert.Equal(RuleFamily.Stability, definition.Family);
        Assert.Equal(RuleAutomaticEffect.StabilityTieBreaker, definition.AutomaticEffect);
        Assert.Equal(RuleManualEffect.NotApplicable, definition.ManualEffect);
        Assert.Null(definition.Priority);
        Assert.Equal(3, parameters.PeriodWeeks);
        Assert.Equal(
            FairDistributionSubjects.UnfavorableShifts
                | FairDistributionSubjects.SplitShifts
                | FairDistributionSubjects.Weekends,
            parameters.Subjects);
    }
}
