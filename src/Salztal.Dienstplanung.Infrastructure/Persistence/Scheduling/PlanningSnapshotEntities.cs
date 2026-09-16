using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;

internal sealed class PlanningSnapshotEntity
{
    public Guid Id { get; set; }

    public Guid DraftId { get; set; }

    public int DraftVersion { get; set; }

    public DateOnly PeriodMonday { get; set; }

    public DateOnly PeriodSunday { get; set; }

    public int RuleCatalogVersion { get; set; }

    public bool EnableAuxiliaryReliefShift { get; set; }

    public int HistoryCompleteness { get; set; }
}

internal enum PlanningSnapshotComponentKind
{
    Employee,
    EmployeeType,
    WorkLocation,
    ShiftType,
    SplitShiftPattern,
    ReliefShiftPattern,
    AvailabilityEntry,
    DemandSlot,
    ServiceManagementAssignment,
    RuleDefinition,
    HistoryDay,
}

internal sealed class PlanningSnapshotComponentEntity
{
    public Guid SnapshotId { get; set; }

    public int Kind { get; set; }

    public int Sequence { get; set; }

    public required string Payload { get; set; }
}

internal sealed record StoredRuleDefinition(
    string Id,
    int Family,
    int Scope,
    int AutomaticEffect,
    int ManualEffect,
    int? Priority,
    string ParameterType,
    string ParameterPayload,
    string DescriptionKey);

internal sealed record StoredPlanningEmployeeType(
    Guid Id,
    string Code,
    string Name,
    int WeeklyWorkTargetMinutes,
    bool AllowsVacationAndSickness,
    int? AbsenceDayValueMinutes,
    EmployeeTypePlanningRole PlanningRole,
    bool AllowsAutomaticAssignment,
    bool RequiresWeeklyManualAssignment,
    bool PreservesManualAssignmentsOnGeneration,
    ManualSuggestionPriority ManualSuggestionPriority,
    PlanningEmployeeTypeEligibilitySnapshot[] Eligibilities);

internal sealed record StoredScheduleAssignment(
    Guid AssignmentId,
    Guid EmployeeId,
    DateOnly Date,
    ScheduleAssignmentKindSnapshot Kind,
    ScheduleAssignmentOriginSnapshot Origin,
    Guid? PatternId,
    int WorkMinutes,
    bool IsProtectedFromAutomaticGeneration,
    ScheduleAssignmentSegmentSnapshot[] Segments,
    ScheduleDemandCoverageSnapshot[] Coverages);

internal sealed record StoredPlanningHistoryDay(
    DateOnly Date,
    PlanningHistoryDayStatus Status,
    PlanningHistoryAssignmentSnapshot[] Assignments);
