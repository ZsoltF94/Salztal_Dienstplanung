using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

internal sealed class AvailabilityPeriodSnapshotProjector
{
    private readonly AvailabilityReadData _data;
    private readonly IReadOnlyDictionary<(EmployeeId EmployeeId, DateOnly Date),
        AvailabilityEntryReadItem> _entryItems;
    private readonly IReadOnlyDictionary<EmployeeTypeId, EmployeeType> _employeeTypes;
    private readonly IReadOnlyDictionary<(EmployeeId EmployeeId, DateOnly WeekMonday),
        WeeklyAvailability> _weeks;

    public AvailabilityPeriodSnapshotProjector(
        AvailabilityReadData data,
        IReadOnlyDictionary<(EmployeeId EmployeeId, DateOnly Date),
            AvailabilityEntryReadItem> entryItems,
        IReadOnlyDictionary<EmployeeTypeId, EmployeeType> employeeTypes,
        IReadOnlyDictionary<(EmployeeId EmployeeId, DateOnly WeekMonday),
            WeeklyAvailability> weeks)
    {
        _data = data;
        _entryItems = entryItems;
        _employeeTypes = employeeTypes;
        _weeks = weeks;
    }

    public AvailabilityPeriodSnapshot Create(
        DateOnly periodMonday,
        DateOnly periodSunday)
    {
        AvailabilityPeriodDaySnapshot[] days = Enumerable.Range(0, 21)
            .Select(offset => periodMonday.AddDays(offset))
            .Select(date => new AvailabilityPeriodDaySnapshot(date, date.DayOfWeek))
            .ToArray();

        AvailabilityPeriodEmployeeSnapshot[] employees = _data.Employees
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.LastName.Value)
            .ThenBy(employee => employee.FirstName.Value)
            .ThenBy(employee => employee.Id.Value)
            .Select(employee => CreateEmployee(employee, periodMonday, periodSunday))
            .ToArray();

        return new AvailabilityPeriodSnapshot(
            periodMonday,
            periodSunday,
            days,
            employees);
    }

    private AvailabilityPeriodEmployeeSnapshot CreateEmployee(
        Employee employee,
        DateOnly periodMonday,
        DateOnly periodSunday)
    {
        EmployeeType employeeType = _employeeTypes[employee.EmployeeTypeId];
        AvailabilityPeriodEntrySnapshot[] entries = _entryItems.Values
            .Where(item => item.Entry.EmployeeId == employee.Id)
            .Where(item =>
                item.Entry.Date >= periodMonday && item.Entry.Date <= periodSunday)
            .OrderBy(item => item.Entry.Date)
            .Select(item => AvailabilityEntrySnapshotMapper.Create(
                item.Entry,
                item.ChangeVersion))
            .ToArray();
        AvailabilityPeriodWeekSnapshot[] weeks = Enumerable.Range(0, 3)
            .Select(week => periodMonday.AddDays(week * 7))
            .Select(weekMonday => _weeks[(employee.Id, weekMonday)])
            .Select(week => new AvailabilityPeriodWeekSnapshot(
                week.WeekMonday,
                week.WeekMonday.AddDays(6),
                week.UncutWorkTarget.Minutes,
                week.EffectiveWorkTarget.Minutes,
                week.IsFullyUnavailable))
            .ToArray();

        return new AvailabilityPeriodEmployeeSnapshot(
            employee.Id.Value,
            employee.DisplayName,
            employeeType.Id.Value,
            employeeType.Code.Value,
            employeeType.Name.Value,
            employeeType.AbsencePolicy.AllowsVacationAndSickness,
            employeeType.AbsencePolicy.DayValue?.Minutes,
            entries,
            weeks);
    }
}
