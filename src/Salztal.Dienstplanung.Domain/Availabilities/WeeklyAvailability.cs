using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed class WeeklyAvailability
{
    private WeeklyAvailability(
        EmployeeId employeeId,
        DateOnly weekMonday,
        WeeklyWorkTarget uncutWorkTarget,
        EffectiveWeeklyWorkTarget effectiveWorkTarget,
        bool isFullyUnavailable)
    {
        EmployeeId = employeeId;
        WeekMonday = weekMonday;
        UncutWorkTarget = uncutWorkTarget;
        EffectiveWorkTarget = effectiveWorkTarget;
        IsFullyUnavailable = isFullyUnavailable;
    }

    public EmployeeId EmployeeId { get; }

    public DateOnly WeekMonday { get; }

    public WeeklyWorkTarget UncutWorkTarget { get; }

    public EffectiveWeeklyWorkTarget EffectiveWorkTarget { get; }

    public bool IsFullyUnavailable { get; }

    public static WeeklyAvailabilityValidationResult Calculate(
        Employee employee,
        EmployeeType employeeType,
        DateOnly weekMonday,
        AvailabilityEntrySet entries)
    {
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentNullException.ThrowIfNull(employeeType);
        ArgumentNullException.ThrowIfNull(entries);

        List<WeeklyAvailabilityValidationError> errors = [];

        if (weekMonday.DayOfWeek != DayOfWeek.Monday)
        {
            errors.Add(new WeeklyAvailabilityValidationError(
                WeeklyAvailabilityValidationCode.WeekMustStartOnMonday));
        }

        if (employee.EmployeeTypeId != employeeType.Id)
        {
            errors.Add(new WeeklyAvailabilityValidationError(
                WeeklyAvailabilityValidationCode.EmployeeTypeMismatch));
        }

        AvailabilityEntry[] weekEntries = entries.Entries
            .Where(entry => entry.EmployeeId == employee.Id)
            .Where(entry =>
                entry.Date.DayNumber - weekMonday.DayNumber is >= 0 and <= 6)
            .ToArray();

        if (!employeeType.AbsencePolicy.AllowsVacationAndSickness)
        {
            errors.AddRange(
                weekEntries
                    .Where(ReducesWorkTarget)
                    .Select(entry => new WeeklyAvailabilityValidationError(
                        WeeklyAvailabilityValidationCode.VacationAndSicknessNotAllowed,
                        entry.Date)));
        }

        if (errors.Count > 0)
        {
            return WeeklyAvailabilityValidationResult.Failure(errors);
        }

        int reducingDayCount = weekEntries.Count(ReducesWorkTarget);
        int dayValueMinutes = employeeType.AbsencePolicy.DayValue?.Minutes ?? 0;
        int effectiveMinutes = Math.Max(
            0,
            employeeType.WeeklyWorkTarget.Minutes - (reducingDayCount * dayValueMinutes));

        WeeklyAvailability value = new(
            employee.Id,
            weekMonday,
            employeeType.WeeklyWorkTarget,
            EffectiveWeeklyWorkTarget.Create(effectiveMinutes),
            weekEntries.Length == 7);

        return WeeklyAvailabilityValidationResult.Success(value);
    }

    private static bool ReducesWorkTarget(AvailabilityEntry entry)
    {
        return entry.Kind is AvailabilityEntryKind.Vacation
            or AvailabilityEntryKind.Sickness;
    }
}
