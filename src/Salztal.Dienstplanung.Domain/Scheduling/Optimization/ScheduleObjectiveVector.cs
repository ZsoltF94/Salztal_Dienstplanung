using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public sealed class ScheduleObjectiveVector
{
    public ScheduleObjectiveVector(
        int uncoveredEmployeeMinutes,
        int fullyUncoveredDemandSlotCount,
        RuleViolationSet highPriorityViolations,
        RuleViolationSet mediumPriorityViolations,
        RuleViolationSet lowPriorityViolations,
        RuleViolationSet stabilityViolations,
        IEnumerable<string> technicalTieBreakerKeys)
        : this(
            uncoveredEmployeeMinutes,
            fullyUncoveredDemandSlotCount,
            highPriorityViolations,
            0,
            0,
            AuxiliaryWeeklyMinimumObjective.Empty,
            RelativeWeeklyTargetObjective.Empty,
            mediumPriorityViolations,
            lowPriorityViolations,
            stabilityViolations,
            technicalTieBreakerKeys)
    {
    }

    public ScheduleObjectiveVector(
        int uncoveredEmployeeMinutes,
        int fullyUncoveredDemandSlotCount,
        RuleViolationSet highPriorityViolations,
        int reliefShiftAssignmentCount,
        int splitShiftAssignmentCount,
        AuxiliaryWeeklyMinimumObjective auxiliaryWeeklyMinimum,
        RelativeWeeklyTargetObjective relativeWeeklyTarget,
        RuleViolationSet mediumPriorityViolations,
        RuleViolationSet lowPriorityViolations,
        RuleViolationSet stabilityViolations,
        IEnumerable<string> technicalTieBreakerKeys)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(uncoveredEmployeeMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(fullyUncoveredDemandSlotCount);
        ArgumentOutOfRangeException.ThrowIfNegative(reliefShiftAssignmentCount);
        ArgumentOutOfRangeException.ThrowIfNegative(splitShiftAssignmentCount);
        ArgumentNullException.ThrowIfNull(highPriorityViolations);
        ArgumentNullException.ThrowIfNull(auxiliaryWeeklyMinimum);
        ArgumentNullException.ThrowIfNull(relativeWeeklyTarget);
        ArgumentNullException.ThrowIfNull(mediumPriorityViolations);
        ArgumentNullException.ThrowIfNull(lowPriorityViolations);
        ArgumentNullException.ThrowIfNull(stabilityViolations);
        ArgumentNullException.ThrowIfNull(technicalTieBreakerKeys);
        ValidateSoftRuleStage(
            highPriorityViolations,
            RulePriority.High,
            nameof(highPriorityViolations));
        ValidateSoftRuleStage(
            mediumPriorityViolations,
            RulePriority.Medium,
            nameof(mediumPriorityViolations));
        ValidateSoftRuleStage(
            lowPriorityViolations,
            RulePriority.Low,
            nameof(lowPriorityViolations));
        if (stabilityViolations.Violations.Any(
                violation => violation.RuleFamily != RuleFamily.Stability))
        {
            throw new ArgumentException(
                "Stability violations must reference stability rules.",
                nameof(stabilityViolations));
        }

        string[] keys = technicalTieBreakerKeys
            .Select(key => key?.Trim() ?? string.Empty)
            .ToArray();
        if (keys.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "Technical tie-breaker keys cannot be empty.",
                nameof(technicalTieBreakerKeys));
        }

        if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
        {
            throw new ArgumentException(
                "Technical tie-breaker keys must be unique.",
                nameof(technicalTieBreakerKeys));
        }

        UncoveredEmployeeMinutes = uncoveredEmployeeMinutes;
        FullyUncoveredDemandSlotCount = fullyUncoveredDemandSlotCount;
        HighPriorityViolations = highPriorityViolations;
        ReliefShiftAssignmentCount = reliefShiftAssignmentCount;
        SplitShiftAssignmentCount = splitShiftAssignmentCount;
        AuxiliaryWeeklyMinimum = auxiliaryWeeklyMinimum;
        RelativeWeeklyTarget = relativeWeeklyTarget;
        MediumPriorityViolations = mediumPriorityViolations;
        LowPriorityViolations = lowPriorityViolations;
        StabilityViolations = stabilityViolations;
        TechnicalTieBreakerKeys = Array.AsReadOnly(
            keys.Order(StringComparer.Ordinal).ToArray());
    }

    public int UncoveredEmployeeMinutes { get; }

    public int FullyUncoveredDemandSlotCount { get; }

    public RuleViolationSet HighPriorityViolations { get; }

    public int ReliefShiftAssignmentCount { get; }

    public int SplitShiftAssignmentCount { get; }

    public AuxiliaryWeeklyMinimumObjective AuxiliaryWeeklyMinimum { get; }

    public RelativeWeeklyTargetObjective RelativeWeeklyTarget { get; }

    public RuleViolationSet MediumPriorityViolations { get; }

    public RuleViolationSet LowPriorityViolations { get; }

    public RuleViolationSet StabilityViolations { get; }

    public ReadOnlyCollection<string> TechnicalTieBreakerKeys { get; }

    private static void ValidateSoftRuleStage(
        RuleViolationSet violations,
        RulePriority expectedPriority,
        string parameterName)
    {
        if (violations.Violations.Any(violation =>
                violation.RuleFamily != RuleFamily.Soft
                || violation.Priority != expectedPriority))
        {
            throw new ArgumentException(
                $"Rule violations must reference {expectedPriority} soft rules.",
                parameterName);
        }
    }
}
