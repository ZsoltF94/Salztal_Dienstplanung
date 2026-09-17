using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Optimization;

public static class ScheduleObjectiveComparer
{
    public static ScheduleObjectiveComparison Compare(
        ScheduleObjectiveVector left,
        ScheduleObjectiveVector right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        ScheduleObjectiveComparison? comparison = CompareNumber(
            left.UncoveredEmployeeMinutes,
            right.UncoveredEmployeeMinutes,
            ScheduleObjectiveStage.UncoveredEmployeeMinutes);
        comparison ??= CompareNumber(
            left.FullyUncoveredDemandSlotCount,
            right.FullyUncoveredDemandSlotCount,
            ScheduleObjectiveStage.FullyUncoveredDemandSlots);
        comparison ??= CompareRuleStage(
            left.HighPriorityViolations,
            right.HighPriorityViolations,
            ScheduleObjectiveStage.HighPriorityRules);
        comparison ??= CompareNumber(
            left.ReliefShiftAssignmentCount,
            right.ReliefShiftAssignmentCount,
            ScheduleObjectiveStage.ReliefShiftAssignments);
        comparison ??= CompareNumber(
            left.SplitShiftAssignmentCount,
            right.SplitShiftAssignmentCount,
            ScheduleObjectiveStage.SplitShiftAssignments);
        comparison ??= CompareAuxiliaryMinimum(
            left.AuxiliaryWeeklyMinimum,
            right.AuxiliaryWeeklyMinimum);
        comparison ??= CompareRelativeWeeklyTarget(
            left.RelativeWeeklyTarget,
            right.RelativeWeeklyTarget);
        comparison ??= CompareRuleStage(
            left.MediumPriorityViolations,
            right.MediumPriorityViolations,
            ScheduleObjectiveStage.MediumPriorityRules);
        comparison ??= CompareRuleStage(
            left.LowPriorityViolations,
            right.LowPriorityViolations,
            ScheduleObjectiveStage.LowPriorityRules);
        comparison ??= CompareRuleStage(
            left.StabilityViolations,
            right.StabilityViolations,
            ScheduleObjectiveStage.Stability);
        comparison ??= CompareTechnicalKeys(
            left.TechnicalTieBreakerKeys,
            right.TechnicalTieBreakerKeys);

        return comparison ?? ScheduleObjectiveComparison.Equivalent;
    }

    private static ScheduleObjectiveComparison? CompareAuxiliaryMinimum(
        AuxiliaryWeeklyMinimumObjective left,
        AuxiliaryWeeklyMinimumObjective right)
    {
        ScheduleObjectiveComparison? comparison = CompareNumber(
            left.ViolatedWeekCount,
            right.ViolatedWeekCount,
            ScheduleObjectiveStage.AuxiliaryWeeklyMinimum);
        return comparison ?? CompareNumber(
            left.MissingMinutes,
            right.MissingMinutes,
            ScheduleObjectiveStage.AuxiliaryWeeklyMinimum);
    }

    private static ScheduleObjectiveComparison? CompareRelativeWeeklyTarget(
        RelativeWeeklyTargetObjective left,
        RelativeWeeklyTargetObjective right)
    {
        int comparison = left.CompareTo(right);
        return comparison == 0
            ? null
            : CreateComparison(
                comparison < 0,
                ScheduleObjectiveStage.RelativeWeeklyTarget);
    }

    private static ScheduleObjectiveComparison? CompareNumber(
        long left,
        long right,
        ScheduleObjectiveStage stage)
    {
        return left == right ? null : CreateComparison(left < right, stage);
    }

    private static ScheduleObjectiveComparison? CompareRuleStage(
        RuleViolationSet left,
        RuleViolationSet right,
        ScheduleObjectiveStage stage)
    {
        ScheduleObjectiveComparison? caseCountComparison = CompareNumber(
            left.Count,
            right.Count,
            stage);
        if (caseCountComparison is not null)
        {
            return caseCountComparison;
        }

        HashSet<RuleId> ruleIds = left.MagnitudeByRule.Keys
            .Concat(right.MagnitudeByRule.Keys)
            .ToHashSet();
        bool leftHasSmallerMagnitude = false;
        bool leftHasLargerMagnitude = false;

        foreach (RuleId ruleId in ruleIds)
        {
            long leftMagnitude = left.MagnitudeByRule.GetValueOrDefault(ruleId);
            long rightMagnitude = right.MagnitudeByRule.GetValueOrDefault(ruleId);
            leftHasSmallerMagnitude |= leftMagnitude < rightMagnitude;
            leftHasLargerMagnitude |= leftMagnitude > rightMagnitude;
        }

        if (leftHasSmallerMagnitude == leftHasLargerMagnitude)
        {
            return null;
        }

        return CreateComparison(leftHasSmallerMagnitude, stage);
    }

    private static ScheduleObjectiveComparison? CompareTechnicalKeys(
        ReadOnlyCollection<string> left,
        ReadOnlyCollection<string> right)
    {
        int sharedCount = Math.Min(left.Count, right.Count);
        for (int index = 0; index < sharedCount; index++)
        {
            int keyComparison = StringComparer.Ordinal.Compare(left[index], right[index]);
            if (keyComparison != 0)
            {
                return CreateComparison(
                    keyComparison < 0,
                    ScheduleObjectiveStage.TechnicalTieBreaker);
            }
        }

        return left.Count == right.Count
            ? null
            : CreateComparison(
                left.Count < right.Count,
                ScheduleObjectiveStage.TechnicalTieBreaker);
    }

    private static ScheduleObjectiveComparison CreateComparison(
        bool leftIsBetter,
        ScheduleObjectiveStage stage)
    {
        return new ScheduleObjectiveComparison(
            leftIsBetter
                ? ScheduleObjectiveComparisonOutcome.Better
                : ScheduleObjectiveComparisonOutcome.Worse,
            stage);
    }
}
