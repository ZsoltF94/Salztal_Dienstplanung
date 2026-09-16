using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialSoftRuleDefinitions
{
    private const RuleDayMarkerKinds RegularDayOffMarkers =
        RuleDayMarkerKinds.GuaranteedDayOff | RuleDayMarkerKinds.GeneratedDayOff;

    private static readonly ReadOnlyCollection<RuleDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                Create(
                    "NORMAL_WEEKLY_MINIMUM",
                    RuleScope.EmployeeWeek,
                    RulePriority.High,
                    new EffectiveWeeklyTargetMinimumParameters(
                        EmployeeTypePlanningRole.Normal,
                        3 * 60),
                    "rules.soft.normal_weekly_minimum"),
                Create(
                    "WEEKLY_CONSECUTIVE_DAYS_OFF",
                    RuleScope.EmployeeWeek,
                    RulePriority.High,
                    new WeeklyConsecutiveDaysOffParameters(2, RegularDayOffMarkers),
                    "rules.soft.weekly_consecutive_days_off"),
                Create(
                    "RED_X_ADJACENT_DAY_OFF",
                    RuleScope.EmployeeWeek,
                    RulePriority.High,
                    new GuaranteedDayOffAdjacencyParameters(
                        2,
                        RuleDayMarkerKinds.GuaranteedDayOff,
                        RegularDayOffMarkers),
                    "rules.soft.guaranteed_day_off_adjacent_day_off"),
                Create(
                    "PRE_VACATION_WEEKEND_FREE",
                    RuleScope.EmployeeWeekend,
                    RulePriority.High,
                    new VacationBoundaryWeekendParameters(
                        VacationBoundaryKind.Start,
                        DayOfWeek.Monday,
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday),
                    "rules.soft.pre_vacation_weekend_free"),
                Create(
                    "SPLIT_SHIFT_WEEKLY_MAXIMUM",
                    RuleScope.EmployeeWeek,
                    RulePriority.High,
                    new SplitShiftWeeklyMaximumParameters(1),
                    "rules.soft.split_shift_weekly_maximum"),
                Create(
                    "THREE_WEEK_FREE_WEEKEND",
                    RuleScope.PlanningPeriod,
                    RulePriority.Medium,
                    new PlanningPeriodFreeWeekendParameters(
                        3,
                        1,
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday,
                        RegularDayOffMarkers),
                    "rules.soft.three_week_free_weekend"),
                Create(
                    "MINIMIZE_SPLIT_SHIFTS",
                    RuleScope.PlanningPeriod,
                    RulePriority.Medium,
                    new ShiftPatternMinimizationParameters(
                        ShiftPatternRuleKind.SplitShift),
                    "rules.soft.minimize_split_shifts"),
                Create(
                    "AH_WEEKLY_TARGET",
                    RuleScope.EmployeeWeek,
                    RulePriority.Medium,
                    new WeeklyMinutesTargetParameters(
                        EmployeeTypePlanningRole.Auxiliary,
                        10 * 60),
                    "rules.soft.auxiliary_weekly_target"),
            });

    public static RuleDefinition NormalWeeklyMinimum => Definitions[0];

    public static RuleDefinition WeeklyConsecutiveDaysOff => Definitions[1];

    public static RuleDefinition GuaranteedDayOffAdjacentDayOff => Definitions[2];

    public static RuleDefinition PreVacationWeekendFree => Definitions[3];

    public static RuleDefinition SplitShiftWeeklyMaximum => Definitions[4];

    public static RuleDefinition ThreeWeekFreeWeekend => Definitions[5];

    public static RuleDefinition MinimizeSplitShifts => Definitions[6];

    public static RuleDefinition AuxiliaryWeeklyTarget => Definitions[7];

    public static IReadOnlyList<RuleDefinition> All => Definitions;

    private static RuleDefinition Create(
        string id,
        RuleScope scope,
        RulePriority priority,
        RuleParameters parameters,
        string descriptionKey)
    {
        return InitialRuleDefinitionFactory.Create(
            id,
            RuleFamily.Soft,
            scope,
            RuleAutomaticEffect.OptimizationObjective,
            RuleManualEffect.WarnAndRequireConfirmation,
            priority,
            parameters,
            descriptionKey);
    }
}
