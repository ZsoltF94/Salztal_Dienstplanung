using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Availabilities;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum SchedulePreparationStatus
{
    NotPrepared,
    Prepared,
    Outdated,
}

public sealed class ScheduleWorkspaceSnapshot
{
    internal ScheduleWorkspaceSnapshot(
        Guid draftId,
        long version,
        AvailabilityPeriodSnapshot availability,
        IEnumerable<ScheduleDemandSlotSnapshot> demandSlots,
        IEnumerable<ScheduleAssignmentSnapshot> assignments,
        IEnumerable<ScheduleGeneratedDayOffSnapshot> generatedDayOffs,
        IEnumerable<ServiceManagementAssignmentOptionSnapshot> assignmentOptions,
        IEnumerable<ServiceManagementReadinessSnapshot> serviceManagementReadiness,
        SchedulePreparationStatus preparationStatus,
        Guid? preparedSnapshotId,
        PlanningRunOptions? preparedRunOptions,
        PlanningHistoryCompleteness? preparedHistoryCompleteness,
        IEnumerable<PlanningInputChangeCategory> changedCategories,
        AcceptedAutomaticScheduleSnapshot? acceptedAutomaticSchedule)
    {
        DraftId = draftId;
        Version = version;
        Availability = availability;
        DemandSlots = Array.AsReadOnly(demandSlots.ToArray());
        Assignments = Array.AsReadOnly(assignments.ToArray());
        GeneratedDayOffs = Array.AsReadOnly(generatedDayOffs.ToArray());
        AssignmentOptions = Array.AsReadOnly(assignmentOptions.ToArray());
        ServiceManagementReadiness = Array.AsReadOnly(
            serviceManagementReadiness.ToArray());
        PreparationStatus = preparationStatus;
        PreparedSnapshotId = preparedSnapshotId;
        PreparedRunOptions = preparedRunOptions;
        PreparedHistoryCompleteness = preparedHistoryCompleteness;
        ChangedCategories = Array.AsReadOnly(changedCategories.ToArray());
        AcceptedAutomaticSchedule = acceptedAutomaticSchedule;
    }

    public Guid DraftId { get; }

    public long Version { get; }

    public DateOnly PeriodMonday => Availability.PeriodMonday;

    public DateOnly PeriodSunday => Availability.PeriodSunday;

    public AvailabilityPeriodSnapshot Availability { get; }

    public ReadOnlyCollection<ScheduleDemandSlotSnapshot> DemandSlots { get; }

    public ReadOnlyCollection<ScheduleAssignmentSnapshot> Assignments { get; }

    public ReadOnlyCollection<ScheduleGeneratedDayOffSnapshot> GeneratedDayOffs { get; }

    public ReadOnlyCollection<ServiceManagementAssignmentOptionSnapshot>
        AssignmentOptions
    { get; }

    public ReadOnlyCollection<ServiceManagementReadinessSnapshot> ServiceManagementReadiness
    {
        get;
    }

    public SchedulePreparationStatus PreparationStatus { get; }

    public Guid? PreparedSnapshotId { get; }

    public PlanningRunOptions? PreparedRunOptions { get; }

    public PlanningHistoryCompleteness? PreparedHistoryCompleteness { get; }

    public ReadOnlyCollection<PlanningInputChangeCategory> ChangedCategories { get; }

    public AcceptedAutomaticScheduleSnapshot? AcceptedAutomaticSchedule { get; }
}

public enum AcceptedAutomaticScheduleReportStatus
{
    Current,
    ChangedAfterGeneration,
    DetailsUnavailable,
}

public sealed record AcceptedAutomaticScheduleSnapshot(
    int AssignmentCount,
    int GeneratedDayOffCount,
    AcceptedAutomaticScheduleReportStatus ReportStatus =
        AcceptedAutomaticScheduleReportStatus.DetailsUnavailable,
    AutomaticScheduleGenerationReport? Report = null);

public enum ServiceManagementWeekReadinessStatusSnapshot
{
    Ready,
    ExemptFullyUnavailable,
    MissingAssignment,
}

public sealed class ServiceManagementReadinessSnapshot
{
    internal ServiceManagementReadinessSnapshot(
        Guid employeeId,
        bool canPrepare,
        IEnumerable<ServiceManagementWeekReadinessSnapshot> weeks)
    {
        EmployeeId = employeeId;
        CanPrepare = canPrepare;
        Weeks = Array.AsReadOnly(weeks.ToArray());
    }

    public Guid EmployeeId { get; }

    public bool CanPrepare { get; }

    public ReadOnlyCollection<ServiceManagementWeekReadinessSnapshot> Weeks { get; }
}

public sealed record ServiceManagementWeekReadinessSnapshot(
    DateOnly WeekMonday,
    ServiceManagementWeekReadinessStatusSnapshot Status,
    int WorkMinutes);
