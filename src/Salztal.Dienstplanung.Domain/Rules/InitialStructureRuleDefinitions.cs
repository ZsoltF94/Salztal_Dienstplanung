using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Rules;

public static class InitialStructureRuleDefinitions
{
    private static readonly ReadOnlyCollection<RuleDefinition> Definitions =
        Array.AsReadOnly(
            new[]
            {
                Create(
                    "STRUCTURE_KNOWN_REFERENCES",
                    RuleScope.Plan,
                    "rules.structure.known_references"),
                Create(
                    "STRUCTURE_SINGLE_DAILY_ASSIGNMENT",
                    RuleScope.EmployeeDay,
                    "rules.structure.single_daily_assignment"),
                Create(
                    "STRUCTURE_NO_TIME_OVERLAP",
                    RuleScope.EmployeeDay,
                    "rules.structure.no_time_overlap"),
                Create(
                    "STRUCTURE_BLOCKED_DAY_MARKER",
                    RuleScope.EmployeeDay,
                    "rules.structure.blocked_day_marker"),
                Create(
                    "STRUCTURE_NORMAL_SLOT_FULL_COVERAGE",
                    RuleScope.DemandSlot,
                    "rules.structure.normal_slot_full_coverage"),
                Create(
                    "STRUCTURE_APPROVED_PLAN_IMMUTABLE",
                    RuleScope.PlanVersion,
                    "rules.structure.approved_plan_immutable"),
            });

    public static RuleDefinition KnownReferences => Definitions[0];

    public static RuleDefinition SingleDailyAssignment => Definitions[1];

    public static RuleDefinition NoTimeOverlap => Definitions[2];

    public static RuleDefinition BlockedDayMarker => Definitions[3];

    public static RuleDefinition NormalSlotFullCoverage => Definitions[4];

    public static RuleDefinition ApprovedPlanImmutable => Definitions[5];

    public static IReadOnlyList<RuleDefinition> All => Definitions;

    private static RuleDefinition Create(
        string id,
        RuleScope scope,
        string descriptionKey)
    {
        return InitialRuleDefinitionFactory.Create(
            id,
            RuleFamily.Structure,
            scope,
            RuleAutomaticEffect.BlockInvalidStructure,
            RuleManualEffect.Block,
            null,
            NoRuleParameters.Instance,
            descriptionKey);
    }
}
