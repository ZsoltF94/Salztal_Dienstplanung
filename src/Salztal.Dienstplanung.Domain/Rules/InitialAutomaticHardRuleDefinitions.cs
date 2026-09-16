using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialAutomaticHardRuleDefinitions
{
    private static readonly ReadOnlyCollection<RuleDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                Create(
                    "ACTIVE_EMPLOYEES_ONLY",
                    RuleScope.Assignment,
                    RuleManualEffect.Block,
                    NoRuleParameters.Instance,
                    "rules.hard.active_employees_only"),
                Create(
                    "SHIFT_ELIGIBILITY_REQUIRED",
                    RuleScope.Assignment,
                    RuleManualEffect.WarnAndRequireConfirmation,
                    NoRuleParameters.Instance,
                    "rules.hard.shift_eligibility_required"),
                Create(
                    "EXPLICIT_RUN_OPTION_REQUIRED",
                    RuleScope.PlanningRun,
                    RuleManualEffect.NotApplicable,
                    new ExplicitRunOptionRuleParameters(
                        ShiftEligibilityActivation.ExplicitPlanningRunOption),
                    "rules.hard.explicit_run_option_required"),
                Create(
                    "TYPE1_MANUAL_ONLY",
                    RuleScope.Assignment,
                    RuleManualEffect.NotApplicable,
                    new PlanningRoleRuleParameters(
                        EmployeeTypePlanningRole.ServiceManagement),
                    "rules.hard.service_management_manual_only"),
                Create(
                    "TYPE1_WEEKLY_PREREQUISITE",
                    RuleScope.EmployeeWeek,
                    RuleManualEffect.NotApplicable,
                    new WeeklyManualAssignmentRuleParameters(
                        EmployeeTypePlanningRole.ServiceManagement,
                        1,
                        true),
                    "rules.hard.service_management_weekly_prerequisite"),
                Create(
                    "NORMAL_WEEKLY_MAXIMUM",
                    RuleScope.EmployeeWeek,
                    RuleManualEffect.WarnAndRequireConfirmation,
                    new EffectiveWeeklyTargetMaximumParameters(
                        EmployeeTypePlanningRole.Normal,
                        3 * 60),
                    "rules.hard.normal_weekly_maximum"),
                Create(
                    "AH_WEEKLY_MAXIMUM",
                    RuleScope.EmployeeWeek,
                    RuleManualEffect.WarnAndRequireConfirmation,
                    new WeeklyMinutesMaximumParameters(
                        EmployeeTypePlanningRole.Auxiliary,
                        12 * 60),
                    "rules.hard.auxiliary_weekly_maximum"),
                Create(
                    "MAX_CONSECUTIVE_WORKDAYS",
                    RuleScope.EmployeeWorkSequence,
                    RuleManualEffect.WarnAndRequireConfirmation,
                    new MaximumConsecutiveWorkdaysParameters(
                        7,
                        true,
                        RuleDayMarkerKinds.Vacation
                            | RuleDayMarkerKinds.Sickness
                            | RuleDayMarkerKinds.GuaranteedDayOff
                            | RuleDayMarkerKinds.GeneratedDayOff),
                    "rules.hard.maximum_consecutive_workdays"),
                Create(
                    "POST_VACATION_WEEKEND_FREE",
                    RuleScope.EmployeeWeekend,
                    RuleManualEffect.WarnAndRequireConfirmation,
                    new VacationBoundaryWeekendParameters(
                        VacationBoundaryKind.End,
                        DayOfWeek.Friday,
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday),
                    "rules.hard.post_vacation_weekend_free"),
                Create(
                    "AUTOMATIC_NO_OVERSTAFFING",
                    RuleScope.DemandSlot,
                    RuleManualEffect.WarnAndRequireConfirmation,
                    NoRuleParameters.Instance,
                    "rules.hard.automatic_no_overstaffing"),
                Create(
                    "RELIEF_SHIFT_EMERGENCY_ONLY",
                    RuleScope.ShiftPatternAssignment,
                    RuleManualEffect.Block,
                    new ReliefShiftEmergencyParameters(
                        DayOfWeek.Saturday,
                        ReliefShiftDemandCondition.RemainingRestaurantLateShiftUndercoverage,
                        ReliefShiftCoverageStart.ActualWorkLocationSwitch),
                    "rules.hard.relief_shift_emergency_only"),
            });

    public static RuleDefinition ActiveEmployeesOnly => Definitions[0];

    public static RuleDefinition ShiftEligibilityRequired => Definitions[1];

    public static RuleDefinition ExplicitRunOptionRequired => Definitions[2];

    public static RuleDefinition ServiceManagementManualOnly => Definitions[3];

    public static RuleDefinition ServiceManagementWeeklyPrerequisite => Definitions[4];

    public static RuleDefinition NormalWeeklyMaximum => Definitions[5];

    public static RuleDefinition AuxiliaryWeeklyMaximum => Definitions[6];

    public static RuleDefinition MaximumConsecutiveWorkdays => Definitions[7];

    public static RuleDefinition PostVacationWeekendFree => Definitions[8];

    public static RuleDefinition AutomaticNoOverstaffing => Definitions[9];

    public static RuleDefinition ReliefShiftEmergencyOnly => Definitions[10];

    public static IReadOnlyList<RuleDefinition> All => Definitions;

    private static RuleDefinition Create(
        string id,
        RuleScope scope,
        RuleManualEffect manualEffect,
        RuleParameters parameters,
        string descriptionKey)
    {
        return InitialRuleDefinitionFactory.Create(
            id,
            RuleFamily.AutomaticHard,
            scope,
            RuleAutomaticEffect.HardConstraint,
            manualEffect,
            null,
            parameters,
            descriptionKey);
    }
}
