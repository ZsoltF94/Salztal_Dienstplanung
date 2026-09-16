using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class SchedulePeriodSelection
{
    public static SchedulePeriodValidationResult Create(DateOnly selectedDate)
    {
        int daysSinceMonday = ((int)selectedDate.DayOfWeek + 6) % 7;
        DateOnly monday = selectedDate.AddDays(-daysSinceMonday);
        return SchedulePeriod.Create(monday);
    }
}
