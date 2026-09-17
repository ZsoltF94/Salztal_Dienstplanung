using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class CurrentSoftRuleDefinitionsTests
{
    [Fact]
    public void VersionTwoRulesSeparatePatternsMinimumAndRelativeTarget()
    {
        Assert.Equal(
            [
                "NORMAL_WEEKLY_MINIMUM",
                "WEEKLY_CONSECUTIVE_DAYS_OFF",
                "RED_X_ADJACENT_DAY_OFF",
                "PRE_VACATION_WEEKEND_FREE",
                "SPLIT_SHIFT_WEEKLY_MAXIMUM",
                "THREE_WEEK_FREE_WEEKEND",
                "MINIMIZE_RELIEF_SHIFTS",
                "MINIMIZE_SPLIT_SHIFTS",
                "AH_WEEKLY_MINIMUM",
                "RELATIVE_WEEKLY_TARGET",
            ],
            CurrentSoftRuleDefinitions.All.Select(rule => rule.Id.Value));

        ShiftPatternMinimizationParameters relief = Assert.IsType<
            ShiftPatternMinimizationParameters>(
                CurrentSoftRuleDefinitions.MinimizeReliefShifts.Parameters);
        ShiftPatternMinimizationParameters split = Assert.IsType<
            ShiftPatternMinimizationParameters>(
                CurrentSoftRuleDefinitions.MinimizeSplitShifts.Parameters);
        WeeklyMinutesMinimumParameters minimum = Assert.IsType<
            WeeklyMinutesMinimumParameters>(
                CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum.Parameters);
        RelativeWeeklyTargetParameters relative = Assert.IsType<
            RelativeWeeklyTargetParameters>(
                CurrentSoftRuleDefinitions.RelativeWeeklyTarget.Parameters);

        Assert.Equal(ShiftPatternRuleKind.ReliefShift, relief.PatternKind);
        Assert.Equal(ShiftPatternRuleKind.SplitShift, split.PatternKind);
        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, minimum.Role);
        Assert.Equal(180, minimum.MinimumMinutes);
        Assert.True(minimum.RequiresEligibleDemand);
        Assert.Equal(600, relative.AuxiliaryTargetMinutes);
        Assert.True(relative.UsesEffectiveNormalWeeklyTarget);
        Assert.True(relative.ExcludesZeroMinuteTargets);
    }

    [Fact]
    public void ReplacedVersionOneRulesDoNotEnterVersionTwoTwice()
    {
        Assert.DoesNotContain(CurrentSoftRuleDefinitions.All, rule =>
            rule.Id == InitialSoftRuleDefinitions.AuxiliaryWeeklyTarget.Id);
        Assert.Single(CurrentSoftRuleDefinitions.All, rule =>
            rule.Id == CurrentSoftRuleDefinitions.MinimizeSplitShifts.Id);
        Assert.Equal(
            CurrentSoftRuleDefinitions.All.Count,
            CurrentSoftRuleDefinitions.All.Select(rule => rule.Id).Distinct().Count());
    }
}
