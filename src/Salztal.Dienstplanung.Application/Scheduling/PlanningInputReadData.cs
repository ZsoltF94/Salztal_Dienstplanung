using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record PlanningHistoryAssignmentReadItem(
    Guid EmployeeId,
    int WorkMinutes);

public sealed class PlanningHistoryDayReadItem
{
    public PlanningHistoryDayReadItem(
        DateOnly date,
        IEnumerable<PlanningHistoryAssignmentReadItem> assignments)
    {
        ArgumentNullException.ThrowIfNull(assignments);
        Date = date;
        Assignments = Array.AsReadOnly(assignments.ToArray());
    }

    public DateOnly Date { get; }

    public ReadOnlyCollection<PlanningHistoryAssignmentReadItem> Assignments { get; }
}

public sealed class PlanningInputReadData
{
    public PlanningInputReadData(
        ScheduleWorkspaceReadData workspace,
        IEnumerable<PlanningHistoryDayReadItem> historyDays,
        PlanningInputSnapshot? preparedSnapshot)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(historyDays);
        Workspace = workspace;
        HistoryDays = Array.AsReadOnly(historyDays.ToArray());
        PreparedSnapshot = preparedSnapshot;
    }

    public ScheduleWorkspaceReadData Workspace { get; }

    public ReadOnlyCollection<PlanningHistoryDayReadItem> HistoryDays { get; }

    public PlanningInputSnapshot? PreparedSnapshot { get; }
}

public interface IPlanningInputReader
{
    public Task<PlanningInputReadData> LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken);
}
