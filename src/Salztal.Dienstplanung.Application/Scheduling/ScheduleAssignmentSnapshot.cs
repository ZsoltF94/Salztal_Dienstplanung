using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ScheduleAssignmentKindSnapshot
{
    NormalDemand,
    SplitShiftPattern,
    ReliefShiftPattern,
    OfficeTime,
    ManualAdditional,
}

public enum ScheduleAssignmentOriginSnapshot
{
    ServiceManagement,
    AutomaticGeneration,
    ManualEdit,
}

public enum ScheduleDemandCoverageKindSnapshot
{
    Full,
    PartialReliefShift,
}

public sealed class ScheduleAssignmentSnapshot
{
    public ScheduleAssignmentSnapshot(
        Guid assignmentId,
        Guid employeeId,
        DateOnly date,
        ScheduleAssignmentKindSnapshot kind,
        ScheduleAssignmentOriginSnapshot origin,
        Guid? patternId,
        int workMinutes,
        bool isProtectedFromAutomaticGeneration,
        IEnumerable<ScheduleAssignmentSegmentSnapshot> segments,
        IEnumerable<ScheduleDemandCoverageSnapshot> coverages)
    {
        AssignmentId = assignmentId;
        EmployeeId = employeeId;
        Date = date;
        Kind = kind;
        Origin = origin;
        PatternId = patternId;
        WorkMinutes = workMinutes;
        IsProtectedFromAutomaticGeneration = isProtectedFromAutomaticGeneration;
        Segments = Array.AsReadOnly(segments.ToArray());
        Coverages = Array.AsReadOnly(coverages.ToArray());
    }

    public Guid AssignmentId { get; }

    public Guid EmployeeId { get; }

    public DateOnly Date { get; }

    public ScheduleAssignmentKindSnapshot Kind { get; }

    public ScheduleAssignmentOriginSnapshot Origin { get; }

    public Guid? PatternId { get; }

    public int WorkMinutes { get; }

    public bool IsProtectedFromAutomaticGeneration { get; }

    public ReadOnlyCollection<ScheduleAssignmentSegmentSnapshot> Segments { get; }

    public ReadOnlyCollection<ScheduleDemandCoverageSnapshot> Coverages { get; }
}

public sealed record ScheduleAssignmentSegmentSnapshot(
    Guid DemandSourceId,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    TimeOnly ActualStart,
    TimeOnly ActualEnd,
    int WorkMinutes);

public sealed record ScheduleDemandCoverageSnapshot(
    Guid DemandSourceId,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    int Ordinal,
    TimeOnly CoveredStart,
    TimeOnly CoveredEnd,
    int CoveredMinutes,
    ScheduleDemandCoverageKindSnapshot Kind);
