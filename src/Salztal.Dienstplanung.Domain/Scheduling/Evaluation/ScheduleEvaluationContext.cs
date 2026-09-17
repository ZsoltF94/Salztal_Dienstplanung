using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

public sealed class ScheduleEvaluationContext
{
    public ScheduleEvaluationContext(
        ScheduleDraft draft,
        RuleCatalog ruleCatalog,
        IEnumerable<ScheduleEvaluationEmployee> employees,
        IEnumerable<ScheduleEvaluationHistoryDay> historyDays,
        bool enableAuxiliaryReliefShift)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(ruleCatalog);
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(historyDays);

        ScheduleEvaluationEmployee[] employeeValues = employees.ToArray();
        ScheduleEvaluationHistoryDay[] historyValues = historyDays.ToArray();
        if (employeeValues.Any(value => value is null)
            || historyValues.Any(value => value is null))
        {
            throw new ArgumentException(
                "Evaluation context collections cannot contain null values.");
        }

        if (employeeValues
            .Select(value => value.EmployeeId)
            .Distinct()
            .Count() != employeeValues.Length)
        {
            throw new ArgumentException(
                "Evaluation employees must be unique.",
                nameof(employees));
        }

        if (historyValues.Select(value => value.Date).Distinct().Count()
            != historyValues.Length)
        {
            throw new ArgumentException(
                "Evaluation history days must have unique dates.",
                nameof(historyDays));
        }

        Draft = draft;
        RuleCatalog = ruleCatalog;
        Employees = Array.AsReadOnly(
            employeeValues.OrderBy(value => value.EmployeeId.Value).ToArray());
        HistoryDays = Array.AsReadOnly(
            historyValues.OrderBy(value => value.Date).ToArray());
        EnableAuxiliaryReliefShift = enableAuxiliaryReliefShift;
    }

    public ScheduleDraft Draft { get; }

    public RuleCatalog RuleCatalog { get; }

    public ReadOnlyCollection<ScheduleEvaluationEmployee> Employees { get; }

    public ReadOnlyCollection<ScheduleEvaluationHistoryDay> HistoryDays { get; }

    public bool EnableAuxiliaryReliefShift { get; }
}
