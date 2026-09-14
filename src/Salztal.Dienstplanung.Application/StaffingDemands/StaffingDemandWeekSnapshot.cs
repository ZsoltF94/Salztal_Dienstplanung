using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed class StaffingDemandWeekSnapshot
{
    internal StaffingDemandWeekSnapshot(
        DateOnly weekMonday,
        IEnumerable<StaffingDemandItemSnapshot> demands,
        IEnumerable<StandardStaffingDemandEditItemSnapshot> standardEditItems,
        IEnumerable<DateStaffingDemandEditItemSnapshot> dateEditItems,
        IEnumerable<StaffingDemandDayWorkLocationSummarySnapshot> daySummaries,
        IEnumerable<StaffingDemandWorkLocationWeekSummarySnapshot> workLocationSummaries,
        long totalRequiredWorkMinutes)
    {
        WeekMonday = weekMonday;
        WeekSunday = weekMonday.AddDays(6);
        Demands = Array.AsReadOnly(demands.ToArray());
        StandardEditItems = Array.AsReadOnly(standardEditItems.ToArray());
        DateEditItems = Array.AsReadOnly(dateEditItems.ToArray());
        DaySummaries = Array.AsReadOnly(daySummaries.ToArray());
        WorkLocationSummaries = Array.AsReadOnly(workLocationSummaries.ToArray());
        TotalRequiredWorkMinutes = totalRequiredWorkMinutes;
    }

    public DateOnly WeekMonday { get; }

    public DateOnly WeekSunday { get; }

    public ReadOnlyCollection<StaffingDemandItemSnapshot> Demands { get; }

    public ReadOnlyCollection<StandardStaffingDemandEditItemSnapshot> StandardEditItems { get; }

    public ReadOnlyCollection<DateStaffingDemandEditItemSnapshot> DateEditItems { get; }

    public ReadOnlyCollection<StaffingDemandDayWorkLocationSummarySnapshot> DaySummaries { get; }

    public ReadOnlyCollection<StaffingDemandWorkLocationWeekSummarySnapshot>
        WorkLocationSummaries
    { get; }

    public long TotalRequiredWorkMinutes { get; }
}
