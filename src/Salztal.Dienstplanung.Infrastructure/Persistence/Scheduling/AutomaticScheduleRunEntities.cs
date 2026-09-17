using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;

internal sealed class AutomaticScheduleRunEntity
{
    public Guid DraftId { get; set; }

    public Guid SnapshotId { get; set; }

    public required string SolverName { get; set; }

    public required string SolverVersion { get; set; }

    public int ResultStatus { get; set; }

    public long TimeLimitTicks { get; set; }

    public long ModelBuildDurationTicks { get; set; }

    public long OptimizationDurationTicks { get; set; }

    public long LegacyPhaseDurationTicks { get; set; }

    public long ResultMappingDurationTicks { get; set; }

    public long TotalDurationTicks { get; set; }

    public required string SettingsPayload { get; set; }

    public required string ObjectivePayload { get; set; }
}

internal sealed record StoredAutomaticScheduleSetting(string Key, string Value);

internal sealed record StoredAutomaticScheduleRuleViolation(
    string RuleId,
    RuleFamily RuleFamily,
    RulePriority? Priority,
    string CaseKey,
    long Magnitude);

internal sealed record StoredAutomaticScheduleObjective(
    int UncoveredEmployeeMinutes,
    int FullyUncoveredDemandSlotCount,
    StoredAutomaticScheduleRuleViolation[] HighPriorityViolations,
    StoredAutomaticScheduleRuleViolation[] MediumPriorityViolations,
    StoredAutomaticScheduleRuleViolation[] LowPriorityViolations,
    StoredAutomaticScheduleRuleViolation[] StabilityViolations,
    string[] TechnicalTieBreakerKeys,
    int ReliefShiftAssignmentCount = 0,
    int SplitShiftAssignmentCount = 0,
    StoredAutomaticScheduleAuxiliaryMinimumCase[]? AuxiliaryMinimumCases = null,
    StoredAutomaticScheduleRelativeWeeklyTargetCase[]? RelativeWeeklyTargetCases = null);

internal sealed record StoredAutomaticScheduleAuxiliaryMinimumCase(
    Guid EmployeeId,
    DateOnly WeekMonday,
    int AssignedMinutes,
    bool HasEligibleDemand);

internal sealed record StoredAutomaticScheduleRelativeWeeklyTargetCase(
    Guid EmployeeId,
    DateOnly WeekMonday,
    int AssignedMinutes,
    int TargetMinutes);
