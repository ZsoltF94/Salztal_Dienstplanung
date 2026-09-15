using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed class AvailabilityPeriodSnapshot
{
    internal AvailabilityPeriodSnapshot(
        DateOnly periodMonday,
        DateOnly periodSunday,
        IEnumerable<AvailabilityPeriodDaySnapshot> days,
        IEnumerable<AvailabilityPeriodEmployeeSnapshot> employees)
    {
        PeriodMonday = periodMonday;
        PeriodSunday = periodSunday;
        Days = Array.AsReadOnly(days.ToArray());
        Employees = Array.AsReadOnly(employees.ToArray());
    }

    public DateOnly PeriodMonday { get; }

    public DateOnly PeriodSunday { get; }

    public ReadOnlyCollection<AvailabilityPeriodDaySnapshot> Days { get; }

    public ReadOnlyCollection<AvailabilityPeriodEmployeeSnapshot> Employees { get; }
}
