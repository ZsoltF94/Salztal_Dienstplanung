using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Rules;

public sealed record EmployeeWeekMinutesResult
{
    public EmployeeWeekMinutesResult(
        Guid employeeId,
        DateOnly weekMonday,
        int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(employeeId, Guid.Empty);
        if (weekMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "The employee-week result must start on a Monday.",
                nameof(weekMonday));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(minutes);
        EmployeeId = employeeId;
        WeekMonday = weekMonday;
        Minutes = minutes;
    }

    public Guid EmployeeId { get; }

    public DateOnly WeekMonday { get; }

    public int Minutes { get; }
}

public sealed record WeeklyMinutesNoticeResultParameters : RuleResultParameters
{
    public WeeklyMinutesNoticeResultParameters(
        IEnumerable<EmployeeWeekMinutesResult> employeeWeeks)
    {
        ArgumentNullException.ThrowIfNull(employeeWeeks);
        EmployeeWeekMinutesResult[] values = employeeWeeks.ToArray();
        if (values.Any(value => value is null))
        {
            throw new ArgumentException(
                "Weekly notice results cannot contain null values.",
                nameof(employeeWeeks));
        }

        if (values.Select(value => (value.EmployeeId, value.WeekMonday))
            .Distinct().Count() != values.Length)
        {
            throw new ArgumentException(
                "Weekly notice results must be unique per employee and week.",
                nameof(employeeWeeks));
        }

        EmployeeWeeks = Array.AsReadOnly(values
            .OrderBy(value => value.EmployeeId)
            .ThenBy(value => value.WeekMonday)
            .ToArray());
    }

    public ReadOnlyCollection<EmployeeWeekMinutesResult> EmployeeWeeks { get; }
}
