using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticSchedulePhaseKind
{
    InputValidation,
    ModelBuilding,
    HardRules,
    RegularCoverage,
    ReliefCoverage,
    HighPriorityRules,
    ReliefShiftMinimization,
    SplitShiftMinimization,
    AuxiliaryMinimum,
    RelativeWeeklyTarget,
    MediumPriorityRules,
    LowPriorityRules,
    Stability,
    TechnicalTieBreak,
    ResultMapping,
}

public enum AutomaticSchedulePhaseStatus
{
    Completed,
    NotApplicable,
    Interrupted,
    Failed,
}

public sealed record AutomaticSchedulePhaseValue
{
    public AutomaticSchedulePhaseValue(string key, long value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Key = key.Trim();
        Value = value;
    }

    public string Key { get; }

    public long Value { get; }
}

public sealed class AutomaticSchedulePhaseSnapshot
{
    public AutomaticSchedulePhaseSnapshot(
        AutomaticSchedulePhaseKind kind,
        AutomaticSchedulePhaseStatus status,
        TimeSpan duration,
        IEnumerable<AutomaticSchedulePhaseValue>? values = null,
        AutomaticSchedulePhaseTerminationSnapshot? termination = null)
    {
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);
        AutomaticSchedulePhaseValue[] phaseValues = (values ?? []).ToArray();
        if (phaseValues.Any(value => value is null)
            || phaseValues.Select(value => value.Key)
                .Distinct(StringComparer.Ordinal)
                .Count() != phaseValues.Length)
        {
            throw new ArgumentException(
                "Phase values must be non-null and unique by key.",
                nameof(values));
        }

        ValidateTermination(kind, status, termination);

        Kind = kind;
        Status = status;
        Duration = duration;
        Values = Array.AsReadOnly(phaseValues
            .OrderBy(value => value.Key, StringComparer.Ordinal)
            .ToArray());
        Termination = termination;
        DetailAvailability = status is AutomaticSchedulePhaseStatus.Interrupted
                or AutomaticSchedulePhaseStatus.Failed
            && termination is null
                ? AutomaticSchedulePhaseDetailAvailability.TerminationDetailsNotRecorded
                : AutomaticSchedulePhaseDetailAvailability.Complete;
    }

    public AutomaticSchedulePhaseKind Kind { get; }

    public AutomaticSchedulePhaseStatus Status { get; }

    public TimeSpan Duration { get; }

    public ReadOnlyCollection<AutomaticSchedulePhaseValue> Values { get; }

    public AutomaticSchedulePhaseTerminationSnapshot? Termination { get; }

    public AutomaticSchedulePhaseDetailAvailability DetailAvailability { get; }

    private static void ValidateTermination(
        AutomaticSchedulePhaseKind kind,
        AutomaticSchedulePhaseStatus status,
        AutomaticSchedulePhaseTerminationSnapshot? termination)
    {
        if (termination is null)
        {
            return;
        }

        bool statusMatches = status switch
        {
            AutomaticSchedulePhaseStatus.Interrupted => termination.Reason
                is not AutomaticSchedulePhaseTerminationReason.TechnicalFailure,
            AutomaticSchedulePhaseStatus.Failed => termination.Reason
                == AutomaticSchedulePhaseTerminationReason.TechnicalFailure,
            _ => false,
        };
        if (!statusMatches)
        {
            throw new ArgumentException(
                "Phase termination details do not match the phase status.",
                nameof(termination));
        }

        if (termination.ActiveTarget is not null
            && !IsTargetForPhase(kind, termination.ActiveTarget.Value))
        {
            throw new ArgumentException(
                "The active optimization target does not belong to the phase.",
                nameof(termination));
        }
    }

    private static bool IsTargetForPhase(
        AutomaticSchedulePhaseKind kind,
        AutomaticScheduleOptimizationTargetKind target) => (kind, target) switch
        {
            (AutomaticSchedulePhaseKind.HardRules,
                AutomaticScheduleOptimizationTargetKind.HardRuleFeasibility) => true,
            (AutomaticSchedulePhaseKind.RegularCoverage,
                AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes
                    or AutomaticScheduleOptimizationTargetKind.RegularTouchedDemandSlots) => true,
            (AutomaticSchedulePhaseKind.ReliefCoverage,
                AutomaticScheduleOptimizationTargetKind.ReliefCoveredMinutes
                    or AutomaticScheduleOptimizationTargetKind.ReliefTouchedDemandSlots) => true,
            (AutomaticSchedulePhaseKind.HighPriorityRules,
                AutomaticScheduleOptimizationTargetKind.HighPriorityRules) => true,
            (AutomaticSchedulePhaseKind.ReliefShiftMinimization,
                AutomaticScheduleOptimizationTargetKind.ReliefShiftAssignments) => true,
            (AutomaticSchedulePhaseKind.SplitShiftMinimization,
                AutomaticScheduleOptimizationTargetKind.SplitShiftAssignments) => true,
            (AutomaticSchedulePhaseKind.AuxiliaryMinimum,
                AutomaticScheduleOptimizationTargetKind.AuxiliaryMinimumFulfillment) => true,
            (AutomaticSchedulePhaseKind.RelativeWeeklyTarget,
                AutomaticScheduleOptimizationTargetKind.RelativeWeeklyTargetDeviation) => true,
            (AutomaticSchedulePhaseKind.MediumPriorityRules,
                AutomaticScheduleOptimizationTargetKind.MediumPriorityRules) => true,
            (AutomaticSchedulePhaseKind.LowPriorityRules,
                AutomaticScheduleOptimizationTargetKind.LowPriorityRules) => true,
            (AutomaticSchedulePhaseKind.Stability,
                AutomaticScheduleOptimizationTargetKind.StabilityRules) => true,
            (AutomaticSchedulePhaseKind.TechnicalTieBreak,
                AutomaticScheduleOptimizationTargetKind.TechnicalTieBreak) => true,
            _ => false,
        };
}

internal static class AutomaticSchedulePhaseSequence
{
    internal static ReadOnlyCollection<AutomaticSchedulePhaseSnapshot> Create(
        IEnumerable<AutomaticSchedulePhaseSnapshot> phases)
    {
        ArgumentNullException.ThrowIfNull(phases);
        AutomaticSchedulePhaseSnapshot[] values = phases.ToArray();
        if (values.Any(value => value is null)
            || values.Select(value => value.Kind).Distinct().Count() != values.Length)
        {
            throw new ArgumentException(
                "Planning phases must be non-null and unique.",
                nameof(phases));
        }

        for (int index = 1; index < values.Length; index++)
        {
            if (values[index - 1].Kind >= values[index].Kind)
            {
                throw new ArgumentException(
                    "Planning phases must follow the declared phase order.",
                    nameof(phases));
            }
        }

        int terminalCount = values.Count(value => value.Status
            is AutomaticSchedulePhaseStatus.Interrupted
            or AutomaticSchedulePhaseStatus.Failed);
        if (terminalCount > 1)
        {
            throw new ArgumentException(
                "At most one planning phase can be interrupted or failed.",
                nameof(phases));
        }

        return Array.AsReadOnly(values);
    }
}
