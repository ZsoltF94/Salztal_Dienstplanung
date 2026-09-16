using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class InitialStructureAndHardRuleDefinitionsTests
{
    private static readonly string[] ExpectedStructureRuleIds =
    [
        "STRUCTURE_KNOWN_REFERENCES",
        "STRUCTURE_SINGLE_DAILY_ASSIGNMENT",
        "STRUCTURE_NO_TIME_OVERLAP",
        "STRUCTURE_BLOCKED_DAY_MARKER",
        "STRUCTURE_NORMAL_SLOT_FULL_COVERAGE",
        "STRUCTURE_APPROVED_PLAN_IMMUTABLE",
    ];

    private static readonly string[] ExpectedAutomaticHardRuleIds =
    [
        "ACTIVE_EMPLOYEES_ONLY",
        "SHIFT_ELIGIBILITY_REQUIRED",
        "EXPLICIT_RUN_OPTION_REQUIRED",
        "TYPE1_MANUAL_ONLY",
        "TYPE1_WEEKLY_PREREQUISITE",
        "NORMAL_WEEKLY_MAXIMUM",
        "AH_WEEKLY_MAXIMUM",
        "MAX_CONSECUTIVE_WORKDAYS",
        "POST_VACATION_WEEKEND_FREE",
        "AUTOMATIC_NO_OVERSTAFFING",
        "RELIEF_SHIFT_EMERGENCY_ONLY",
    ];

    public static IEnumerable<object[]> AllDefinitions()
    {
        return InitialStructureRuleDefinitions.All
            .Concat(InitialAutomaticHardRuleDefinitions.All)
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
    public void StructureRulesWhenReadMatchApprovedIdentifiersAndBlockingEffect()
    {
        Assert.Equal(
            ExpectedStructureRuleIds,
            InitialStructureRuleDefinitions.All.Select(rule => rule.Id.Value));

        Assert.All(
            InitialStructureRuleDefinitions.All,
            definition =>
            {
                Assert.Equal(RuleFamily.Structure, definition.Family);
                Assert.Equal(
                    RuleAutomaticEffect.BlockInvalidStructure,
                    definition.AutomaticEffect);
                Assert.Equal(RuleManualEffect.Block, definition.ManualEffect);
                Assert.Null(definition.Priority);
                Assert.IsType<NoRuleParameters>(definition.Parameters);
            });
    }

    [Fact]
    public void AutomaticHardRulesWhenReadMatchApprovedIdentifiersAndEffects()
    {
        Assert.Equal(
            ExpectedAutomaticHardRuleIds,
            InitialAutomaticHardRuleDefinitions.All.Select(rule => rule.Id.Value));

        Assert.All(
            InitialAutomaticHardRuleDefinitions.All,
            definition =>
            {
                Assert.Equal(RuleFamily.AutomaticHard, definition.Family);
                Assert.Equal(RuleAutomaticEffect.HardConstraint, definition.AutomaticEffect);
                Assert.Null(definition.Priority);
            });

        Assert.Equal(
            new[]
            {
                RuleManualEffect.Block,
                RuleManualEffect.WarnAndRequireConfirmation,
                RuleManualEffect.NotApplicable,
                RuleManualEffect.NotApplicable,
                RuleManualEffect.NotApplicable,
                RuleManualEffect.WarnAndRequireConfirmation,
                RuleManualEffect.WarnAndRequireConfirmation,
                RuleManualEffect.WarnAndRequireConfirmation,
                RuleManualEffect.WarnAndRequireConfirmation,
                RuleManualEffect.WarnAndRequireConfirmation,
                RuleManualEffect.Block,
            },
            InitialAutomaticHardRuleDefinitions.All.Select(rule => rule.ManualEffect));
    }

    [Fact]
    public void AutomaticHardRulesWhenReadContainApprovedTypedThresholds()
    {
        PlanningRoleRuleParameters manualOnly = Assert.IsType<PlanningRoleRuleParameters>(
            InitialAutomaticHardRuleDefinitions.ServiceManagementManualOnly.Parameters);
        WeeklyManualAssignmentRuleParameters prerequisite =
            Assert.IsType<WeeklyManualAssignmentRuleParameters>(
                InitialAutomaticHardRuleDefinitions
                    .ServiceManagementWeeklyPrerequisite
                    .Parameters);
        EffectiveWeeklyTargetMaximumParameters normalMaximum =
            Assert.IsType<EffectiveWeeklyTargetMaximumParameters>(
                InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum.Parameters);
        WeeklyMinutesMaximumParameters auxiliaryMaximum =
            Assert.IsType<WeeklyMinutesMaximumParameters>(
                InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum.Parameters);
        MaximumConsecutiveWorkdaysParameters consecutiveWorkdays =
            Assert.IsType<MaximumConsecutiveWorkdaysParameters>(
                InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays.Parameters);

        Assert.Equal(EmployeeTypePlanningRole.ServiceManagement, manualOnly.Role);
        Assert.Equal(EmployeeTypePlanningRole.ServiceManagement, prerequisite.Role);
        Assert.Equal(1, prerequisite.MinimumAssignments);
        Assert.True(prerequisite.SkipWhenFullyAbsent);
        Assert.Equal(EmployeeTypePlanningRole.Normal, normalMaximum.Role);
        Assert.Equal(3 * 60, normalMaximum.MaximumMinutesAboveTarget);
        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, auxiliaryMaximum.Role);
        Assert.Equal(12 * 60, auxiliaryMaximum.MaximumMinutes);
        Assert.Equal(7, consecutiveWorkdays.MaximumDays);
        Assert.True(consecutiveWorkdays.RequiresPrecedingHistory);
        Assert.Equal(
            RuleDayMarkerKinds.Vacation
                | RuleDayMarkerKinds.Sickness
                | RuleDayMarkerKinds.GuaranteedDayOff
                | RuleDayMarkerKinds.GeneratedDayOff,
            consecutiveWorkdays.WorkSequenceBreakingMarkers);
    }

    [Fact]
    public void AutomaticHardRulesWhenReadContainApprovedRunVacationAndReliefConditions()
    {
        ExplicitRunOptionRuleParameters runOption =
            Assert.IsType<ExplicitRunOptionRuleParameters>(
                InitialAutomaticHardRuleDefinitions.ExplicitRunOptionRequired.Parameters);
        VacationBoundaryWeekendParameters vacationWeekend =
            Assert.IsType<VacationBoundaryWeekendParameters>(
                InitialAutomaticHardRuleDefinitions.PostVacationWeekendFree.Parameters);
        ReliefShiftEmergencyParameters reliefShift =
            Assert.IsType<ReliefShiftEmergencyParameters>(
                InitialAutomaticHardRuleDefinitions.ReliefShiftEmergencyOnly.Parameters);

        Assert.Equal(
            ShiftEligibilityActivation.ExplicitPlanningRunOption,
            runOption.Activation);
        Assert.Equal(VacationBoundaryKind.End, vacationWeekend.Boundary);
        Assert.Equal(DayOfWeek.Friday, vacationWeekend.BoundaryDay);
        Assert.Equal(DayOfWeek.Saturday, vacationWeekend.WeekendFirstDay);
        Assert.Equal(DayOfWeek.Sunday, vacationWeekend.WeekendSecondDay);
        Assert.Equal(DayOfWeek.Saturday, reliefShift.AllowedDay);
        Assert.Equal(
            ReliefShiftDemandCondition.RemainingRestaurantLateShiftUndercoverage,
            reliefShift.DemandCondition);
        Assert.Equal(
            ReliefShiftCoverageStart.ActualWorkLocationSwitch,
            reliefShift.CoverageStart);
    }

    [Fact]
    public void ApprovedParametersWhenInspectedUseNoDisplayNameOrVisibleTypeCode()
    {
        RuleParameters[] parameters = InitialAutomaticHardRuleDefinitions.All
            .Select(definition => definition.Parameters)
            .ToArray();

        Assert.All(
            parameters,
            parameter => Assert.DoesNotContain(
                parameter.GetType().GetProperties(),
                property => property.PropertyType == typeof(string)
                    || property.Name.Contains("Code", StringComparison.Ordinal)
                    || property.Name.Contains("Name", StringComparison.Ordinal)
                    || property.Name.Contains("Display", StringComparison.Ordinal)));
    }
}
