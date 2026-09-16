using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal sealed record ScheduleWorkspaceContext(
    AvailabilityEntrySet AvailabilityEntries,
    DemandSlotSet DemandSlots,
    IReadOnlyDictionary<EmployeeTypeId, EmployeeType> EmployeeTypes,
    IReadOnlyDictionary<(EmployeeId EmployeeId, DateOnly WeekMonday),
        WeeklyAvailability> WeeklyAvailabilities);

internal sealed record ScheduleWorkspaceContextResult(
    ScheduleWorkspaceContext? Value,
    OpenOrCreateScheduleDraftStatus Status,
    IReadOnlyList<ScheduleWorkspaceError> Errors)
{
    public bool IsSuccess => Value is not null;
}
