using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

public enum ScheduleEvaluationHistoryDayStatus
{
    Available,
    Missing,
}

public sealed class ScheduleEvaluationHistoryDay
{
    public ScheduleEvaluationHistoryDay(
        DateOnly date,
        ScheduleEvaluationHistoryDayStatus status,
        IEnumerable<EmployeeId> workingEmployeeIds)
    {
        ArgumentNullException.ThrowIfNull(workingEmployeeIds);
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        EmployeeId[] employeeIds = workingEmployeeIds.ToArray();
        if (employeeIds.Any(employeeId => employeeId is null))
        {
            throw new ArgumentException(
                "History employees cannot contain null values.",
                nameof(workingEmployeeIds));
        }

        if (employeeIds.Distinct().Count() != employeeIds.Length)
        {
            throw new ArgumentException(
                "History employees must be unique per day.",
                nameof(workingEmployeeIds));
        }

        if (status == ScheduleEvaluationHistoryDayStatus.Missing
            && employeeIds.Length > 0)
        {
            throw new ArgumentException(
                "A missing history day cannot contain assignments.",
                nameof(workingEmployeeIds));
        }

        Date = date;
        Status = status;
        WorkingEmployeeIds = Array.AsReadOnly(
            employeeIds.OrderBy(employeeId => employeeId.Value).ToArray());
    }

    public DateOnly Date { get; }

    public ScheduleEvaluationHistoryDayStatus Status { get; }

    public ReadOnlyCollection<EmployeeId> WorkingEmployeeIds { get; }
}
