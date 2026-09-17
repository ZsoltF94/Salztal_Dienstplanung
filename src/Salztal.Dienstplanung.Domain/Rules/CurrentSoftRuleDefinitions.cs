using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public static class CurrentSoftRuleDefinitions
{
    private static readonly ReadOnlyCollection<RuleDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                InitialSoftRuleDefinitions.NormalWeeklyMinimum,
                InitialSoftRuleDefinitions.WeeklyConsecutiveDaysOff,
                InitialSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff,
                InitialSoftRuleDefinitions.PreVacationWeekendFree,
                InitialSoftRuleDefinitions.SplitShiftWeeklyMaximum,
                InitialSoftRuleDefinitions.ThreeWeekFreeWeekend,
                Create(
                    "MINIMIZE_RELIEF_SHIFTS",
                    RuleScope.PlanningPeriod,
                    new ShiftPatternMinimizationParameters(
                        ShiftPatternRuleKind.ReliefShift),
                    "rules.soft.minimize_relief_shifts"),
                InitialSoftRuleDefinitions.MinimizeSplitShifts,
                Create(
                    "AH_WEEKLY_MINIMUM",
                    RuleScope.EmployeeWeek,
                    new WeeklyMinutesMinimumParameters(
                        EmployeeTypePlanningRole.Auxiliary,
                        3 * 60,
                        requiresEligibleDemand: true),
                    "rules.soft.auxiliary_weekly_minimum"),
                Create(
                    "RELATIVE_WEEKLY_TARGET",
                    RuleScope.EmployeeWeek,
                    new RelativeWeeklyTargetParameters(10 * 60),
                    "rules.soft.relative_weekly_target"),
            });

    public static RuleDefinition NormalWeeklyMinimum => Definitions[0];

    public static RuleDefinition WeeklyConsecutiveDaysOff => Definitions[1];

    public static RuleDefinition GuaranteedDayOffAdjacentDayOff => Definitions[2];

    public static RuleDefinition PreVacationWeekendFree => Definitions[3];

    public static RuleDefinition SplitShiftWeeklyMaximum => Definitions[4];

    public static RuleDefinition ThreeWeekFreeWeekend => Definitions[5];

    public static RuleDefinition MinimizeReliefShifts => Definitions[6];

    public static RuleDefinition MinimizeSplitShifts => Definitions[7];

    public static RuleDefinition AuxiliaryWeeklyMinimum => Definitions[8];

    public static RuleDefinition RelativeWeeklyTarget => Definitions[9];

    public static IReadOnlyList<RuleDefinition> All => Definitions;

    private static RuleDefinition Create(
        string id,
        RuleScope scope,
        RuleParameters parameters,
        string descriptionKey)
    {
        return InitialRuleDefinitionFactory.Create(
            id,
            RuleFamily.Soft,
            scope,
            RuleAutomaticEffect.OptimizationObjective,
            RuleManualEffect.WarnAndRequireConfirmation,
            RulePriority.Medium,
            parameters,
            descriptionKey);
    }
}
