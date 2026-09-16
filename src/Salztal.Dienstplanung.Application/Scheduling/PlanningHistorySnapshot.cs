using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum PlanningHistoryCompleteness
{
    Complete,
    Partial,
    Missing,
}

public enum PlanningHistoryDayStatus
{
    Available,
    Missing,
}

public sealed record PlanningHistoryAssignmentSnapshot(
    Guid EmployeeId,
    int WorkMinutes);

public sealed class PlanningHistoryDaySnapshot
{
    public PlanningHistoryDaySnapshot(
        DateOnly date,
        PlanningHistoryDayStatus status,
        IEnumerable<PlanningHistoryAssignmentSnapshot> assignments)
    {
        ArgumentNullException.ThrowIfNull(assignments);
        Date = date;
        Status = status;
        Assignments = Array.AsReadOnly(assignments.ToArray());
    }

    public DateOnly Date { get; }

    public PlanningHistoryDayStatus Status { get; }

    public ReadOnlyCollection<PlanningHistoryAssignmentSnapshot> Assignments { get; }
}

public sealed class PlanningHistorySnapshot
{
    public PlanningHistorySnapshot(
        PlanningHistoryCompleteness completeness,
        IEnumerable<PlanningHistoryDaySnapshot> days)
    {
        ArgumentNullException.ThrowIfNull(days);
        Completeness = completeness;
        Days = Array.AsReadOnly(days.ToArray());
    }

    public PlanningHistoryCompleteness Completeness { get; }

    public ReadOnlyCollection<PlanningHistoryDaySnapshot> Days { get; }

    public int AvailableDayCount => Days.Count(day =>
        day.Status == PlanningHistoryDayStatus.Available);
}
