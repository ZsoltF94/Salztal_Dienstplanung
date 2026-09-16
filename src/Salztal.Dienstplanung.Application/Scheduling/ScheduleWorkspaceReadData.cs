using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed class ScheduleWorkspaceReadData
{
    public ScheduleWorkspaceReadData(
        AvailabilityReadData availability,
        StaffingDemandReadData staffingDemands,
        IEnumerable<ScheduleDraftHeader> draftHeaders,
        ScheduleDraft? exactDraft,
        IEnumerable<PlanningHistoryDayReadItem>? historyDays = null,
        PlanningInputSnapshot? preparedSnapshot = null)
    {
        ArgumentNullException.ThrowIfNull(availability);
        ArgumentNullException.ThrowIfNull(staffingDemands);
        ArgumentNullException.ThrowIfNull(draftHeaders);

        Availability = availability;
        StaffingDemands = staffingDemands;
        DraftHeaders = Array.AsReadOnly(draftHeaders.ToArray());
        ExactDraft = exactDraft;
        HistoryDays = Array.AsReadOnly((historyDays ?? []).ToArray());
        PreparedSnapshot = preparedSnapshot;
    }

    public AvailabilityReadData Availability { get; }

    public StaffingDemandReadData StaffingDemands { get; }

    public ReadOnlyCollection<ScheduleDraftHeader> DraftHeaders { get; }

    public ScheduleDraft? ExactDraft { get; }

    public ReadOnlyCollection<PlanningHistoryDayReadItem> HistoryDays { get; }

    public PlanningInputSnapshot? PreparedSnapshot { get; }
}
