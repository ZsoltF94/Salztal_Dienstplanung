using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialNoticeRuleDefinitions
{
    private static readonly ReadOnlyCollection<RuleDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                Create(
                    "AH_WEEKLY_LOW_NOTICE",
                    new WeeklyMinutesBelowNoticeParameters(
                        EmployeeTypePlanningRole.Auxiliary,
                        6 * 60),
                    "rules.notice.auxiliary_weekly_low"),
                Create(
                    "AH_WEEKLY_HIGH_NOTICE",
                    new WeeklyMinutesRangeNoticeParameters(
                        EmployeeTypePlanningRole.Auxiliary,
                        10 * 60,
                        12 * 60),
                    "rules.notice.auxiliary_weekly_high"),
            });

    public static RuleDefinition AuxiliaryWeeklyLow => Definitions[0];

    public static RuleDefinition AuxiliaryWeeklyHigh => Definitions[1];

    public static IReadOnlyList<RuleDefinition> All => Definitions;

    private static RuleDefinition Create(
        string id,
        RuleParameters parameters,
        string descriptionKey)
    {
        return InitialRuleDefinitionFactory.Create(
            id,
            RuleFamily.Notice,
            RuleScope.EmployeeWeek,
            RuleAutomaticEffect.ReportNotice,
            RuleManualEffect.ReportNotice,
            null,
            parameters,
            descriptionKey);
    }
}
