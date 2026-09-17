using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record AutomaticScheduleRuleViolation
{
    public AutomaticScheduleRuleViolation(
        string ruleId,
        RuleFamily ruleFamily,
        RulePriority? priority,
        string caseKey,
        long magnitude)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(caseKey);
        if (!Enum.IsDefined(ruleFamily)
            || (priority is not null && !Enum.IsDefined(priority.Value)))
        {
            throw new ArgumentOutOfRangeException(nameof(ruleFamily));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(magnitude);
        RuleId = ruleId.Trim();
        RuleFamily = ruleFamily;
        Priority = priority;
        CaseKey = caseKey.Trim();
        Magnitude = magnitude;
    }

    public string RuleId { get; }

    public RuleFamily RuleFamily { get; }

    public RulePriority? Priority { get; }

    public string CaseKey { get; }

    public long Magnitude { get; }
}

public sealed record AutomaticScheduleAuxiliaryMinimumCase(
    Guid EmployeeId,
    DateOnly WeekMonday,
    int AssignedMinutes,
    bool HasEligibleDemand);

public sealed record AutomaticScheduleRelativeWeeklyTargetCase(
    Guid EmployeeId,
    DateOnly WeekMonday,
    int AssignedMinutes,
    int TargetMinutes);

public sealed class AutomaticScheduleObjectiveSnapshot
{
    public AutomaticScheduleObjectiveSnapshot(
        int uncoveredEmployeeMinutes,
        int fullyUncoveredDemandSlotCount,
        IEnumerable<AutomaticScheduleRuleViolation> highPriorityViolations,
        IEnumerable<AutomaticScheduleRuleViolation> mediumPriorityViolations,
        IEnumerable<AutomaticScheduleRuleViolation> lowPriorityViolations,
        IEnumerable<AutomaticScheduleRuleViolation> stabilityViolations,
        IEnumerable<string> technicalTieBreakerKeys)
        : this(
            uncoveredEmployeeMinutes,
            fullyUncoveredDemandSlotCount,
            highPriorityViolations,
            0,
            0,
            [],
            [],
            mediumPriorityViolations,
            lowPriorityViolations,
            stabilityViolations,
            technicalTieBreakerKeys)
    {
    }

    public AutomaticScheduleObjectiveSnapshot(
        int uncoveredEmployeeMinutes,
        int fullyUncoveredDemandSlotCount,
        IEnumerable<AutomaticScheduleRuleViolation> highPriorityViolations,
        int reliefShiftAssignmentCount,
        int splitShiftAssignmentCount,
        IEnumerable<AutomaticScheduleAuxiliaryMinimumCase> auxiliaryMinimumCases,
        IEnumerable<AutomaticScheduleRelativeWeeklyTargetCase> relativeWeeklyTargetCases,
        IEnumerable<AutomaticScheduleRuleViolation> mediumPriorityViolations,
        IEnumerable<AutomaticScheduleRuleViolation> lowPriorityViolations,
        IEnumerable<AutomaticScheduleRuleViolation> stabilityViolations,
        IEnumerable<string> technicalTieBreakerKeys)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(uncoveredEmployeeMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(fullyUncoveredDemandSlotCount);
        ArgumentOutOfRangeException.ThrowIfNegative(reliefShiftAssignmentCount);
        ArgumentOutOfRangeException.ThrowIfNegative(splitShiftAssignmentCount);
        HighPriorityViolations = CreateViolations(
            highPriorityViolations,
            RuleFamily.Soft,
            RulePriority.High,
            nameof(highPriorityViolations));
        MediumPriorityViolations = CreateViolations(
            mediumPriorityViolations,
            RuleFamily.Soft,
            RulePriority.Medium,
            nameof(mediumPriorityViolations));
        LowPriorityViolations = CreateViolations(
            lowPriorityViolations,
            RuleFamily.Soft,
            RulePriority.Low,
            nameof(lowPriorityViolations));
        StabilityViolations = CreateViolations(
            stabilityViolations,
            RuleFamily.Stability,
            null,
            nameof(stabilityViolations));
        AuxiliaryMinimumCases = CreateAuxiliaryMinimumCases(auxiliaryMinimumCases);
        RelativeWeeklyTargetCases = CreateRelativeWeeklyTargetCases(
            relativeWeeklyTargetCases);
        ArgumentNullException.ThrowIfNull(technicalTieBreakerKeys);
        string[] keys = technicalTieBreakerKeys
            .Select(key => key?.Trim() ?? string.Empty)
            .ToArray();
        if (keys.Any(string.IsNullOrWhiteSpace)
            || keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
        {
            throw new ArgumentException(
                "Technical tie-breaker keys must be non-empty and unique.",
                nameof(technicalTieBreakerKeys));
        }

        UncoveredEmployeeMinutes = uncoveredEmployeeMinutes;
        FullyUncoveredDemandSlotCount = fullyUncoveredDemandSlotCount;
        ReliefShiftAssignmentCount = reliefShiftAssignmentCount;
        SplitShiftAssignmentCount = splitShiftAssignmentCount;
        TechnicalTieBreakerKeys = Array.AsReadOnly(
            keys.Order(StringComparer.Ordinal).ToArray());
    }

    public int UncoveredEmployeeMinutes { get; }

    public int FullyUncoveredDemandSlotCount { get; }

    public ReadOnlyCollection<AutomaticScheduleRuleViolation>
        HighPriorityViolations
    { get; }

    public int ReliefShiftAssignmentCount { get; }

    public int SplitShiftAssignmentCount { get; }

    public ReadOnlyCollection<AutomaticScheduleAuxiliaryMinimumCase>
        AuxiliaryMinimumCases
    { get; }

    public ReadOnlyCollection<AutomaticScheduleRelativeWeeklyTargetCase>
        RelativeWeeklyTargetCases
    { get; }

    public ReadOnlyCollection<AutomaticScheduleRuleViolation>
        MediumPriorityViolations
    { get; }

    public ReadOnlyCollection<AutomaticScheduleRuleViolation>
        LowPriorityViolations
    { get; }

    public ReadOnlyCollection<AutomaticScheduleRuleViolation>
        StabilityViolations
    { get; }

    public ReadOnlyCollection<string> TechnicalTieBreakerKeys { get; }

    public static AutomaticScheduleObjectiveSnapshot Create(
        ScheduleObjectiveVector objective)
    {
        ArgumentNullException.ThrowIfNull(objective);
        return new AutomaticScheduleObjectiveSnapshot(
            objective.UncoveredEmployeeMinutes,
            objective.FullyUncoveredDemandSlotCount,
            objective.HighPriorityViolations.Violations.Select(CreateViolation),
            objective.ReliefShiftAssignmentCount,
            objective.SplitShiftAssignmentCount,
            objective.AuxiliaryWeeklyMinimum.Cases.Select(value =>
                new AutomaticScheduleAuxiliaryMinimumCase(
                    value.EmployeeId,
                    value.WeekMonday,
                    value.AssignedMinutes,
                    value.HasEligibleDemand)),
            objective.RelativeWeeklyTarget.Cases.Select(value =>
                new AutomaticScheduleRelativeWeeklyTargetCase(
                    value.EmployeeId,
                    value.WeekMonday,
                    value.AssignedMinutes,
                    value.TargetMinutes)),
            objective.MediumPriorityViolations.Violations.Select(CreateViolation),
            objective.LowPriorityViolations.Violations.Select(CreateViolation),
            objective.StabilityViolations.Violations.Select(CreateViolation),
            objective.TechnicalTieBreakerKeys);
    }

    private static ReadOnlyCollection<AutomaticScheduleAuxiliaryMinimumCase>
        CreateAuxiliaryMinimumCases(
            IEnumerable<AutomaticScheduleAuxiliaryMinimumCase> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);
        AutomaticScheduleAuxiliaryMinimumCase[] values = cases.ToArray();
        if (values.Any(value => value is null))
        {
            throw new ArgumentException(
                "Objective cases cannot contain null values.",
                nameof(cases));
        }

        AuxiliaryWeeklyMinimumObjective validated = new(values.Select(value =>
            new AuxiliaryWeeklyMinimumCase(
                value.EmployeeId,
                value.WeekMonday,
                value.AssignedMinutes,
                value.HasEligibleDemand)));
        Dictionary<(Guid EmployeeId, DateOnly WeekMonday),
            AutomaticScheduleAuxiliaryMinimumCase> byIdentity = values.ToDictionary(
                value => (value.EmployeeId, value.WeekMonday));
        return Array.AsReadOnly(validated.Cases
            .Select(value => byIdentity[(value.EmployeeId, value.WeekMonday)])
            .ToArray());
    }

    private static ReadOnlyCollection<AutomaticScheduleRelativeWeeklyTargetCase>
        CreateRelativeWeeklyTargetCases(
            IEnumerable<AutomaticScheduleRelativeWeeklyTargetCase> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);
        AutomaticScheduleRelativeWeeklyTargetCase[] values = cases.ToArray();
        if (values.Any(value => value is null))
        {
            throw new ArgumentException(
                "Objective cases cannot contain null values.",
                nameof(cases));
        }

        RelativeWeeklyTargetObjective validated = new(values.Select(value =>
            new RelativeWeeklyTargetCase(
                value.EmployeeId,
                value.WeekMonday,
                value.AssignedMinutes,
                value.TargetMinutes)));
        Dictionary<(Guid EmployeeId, DateOnly WeekMonday),
            AutomaticScheduleRelativeWeeklyTargetCase> byIdentity = values.ToDictionary(
                value => (value.EmployeeId, value.WeekMonday));
        return Array.AsReadOnly(validated.Cases
            .Select(value => byIdentity[(value.EmployeeId, value.WeekMonday)])
            .ToArray());
    }

    private static ReadOnlyCollection<AutomaticScheduleRuleViolation>
        CreateViolations(
            IEnumerable<AutomaticScheduleRuleViolation> violations,
            RuleFamily expectedFamily,
            RulePriority? expectedPriority,
            string parameterName)
    {
        ArgumentNullException.ThrowIfNull(violations);
        AutomaticScheduleRuleViolation[] values = violations.ToArray();
        if (values.Any(value => value is null)
            || values.Any(value =>
                value.RuleFamily != expectedFamily
                || value.Priority != expectedPriority)
            || values.GroupBy(value => (value.RuleId, value.CaseKey)).Any(
                group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Objective violations do not match their stage.",
                parameterName);
        }

        return Array.AsReadOnly(values
            .OrderBy(value => value.RuleId, StringComparer.Ordinal)
            .ThenBy(value => value.CaseKey, StringComparer.Ordinal)
            .ToArray());
    }

    private static AutomaticScheduleRuleViolation CreateViolation(
        RuleViolationCase violation) => new(
            violation.RuleId.Value,
            violation.RuleFamily,
            violation.Priority,
            violation.CaseKey,
            violation.Magnitude.Value);
}

public sealed record AutomaticScheduleRunRecord
{
    public AutomaticScheduleRunRecord(
        Guid snapshotId,
        AutomaticScheduleRunMetadata metadata,
        AutomaticScheduleObjectiveSnapshot objective)
    {
        if (snapshotId == Guid.Empty)
        {
            throw new ArgumentException(
                "Snapshot identifier is required.",
                nameof(snapshotId));
        }

        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(objective);
        SnapshotId = snapshotId;
        Metadata = metadata;
        Objective = objective;
    }

    public Guid SnapshotId { get; }

    public AutomaticScheduleRunMetadata Metadata { get; }

    public AutomaticScheduleObjectiveSnapshot Objective { get; }
}
