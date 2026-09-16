using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling;

internal sealed class SchedulingTestContext
{
    private SchedulingTestContext(SchedulePeriod period, DemandSlotSet demandSlots)
    {
        Period = period;
        DemandSlots = demandSlots;
    }

    public SchedulePeriod Period { get; }

    public DemandSlotSet DemandSlots { get; }

    public static SchedulingTestContext Create()
    {
        SchedulePeriod period = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(new DateOnly(2026, 9, 14)).Value);
        StandardStaffingDemandRevisionSet revisions =
            Assert.IsType<StandardStaffingDemandRevisionSet>(
                StandardStaffingDemandRevisionSet.Create(
                    InitialStaffingDemandCatalog.All).Value);
        StaffingDemandDateExceptionSet exceptions =
            Assert.IsType<StaffingDemandDateExceptionSet>(
                StaffingDemandDateExceptionSet.Create(
                    Array.Empty<StaffingDemandDateException>()).Value);
        EffectiveStaffingDemand[] demands = Enumerable.Range(0, 3)
            .SelectMany(index => Assert.IsType<StaffingDemandWeek>(
                StaffingDemandWeek.Resolve(
                    period.StartMonday.AddDays(index * 7),
                    revisions,
                    exceptions).Value).Demands)
            .ToArray();
        DemandSlotSet demandSlots = Assert.IsType<DemandSlotSet>(
            DemandSlotSet.Create(
                period,
                demands,
                InitialWorkLocationCatalog.All.Select(location => location.Id),
                InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id)).Value);

        return new SchedulingTestContext(period, demandSlots);
    }

    public DemandSlot FindSlot(
        DateOnly date,
        ShiftType shiftType,
        int ordinal = 1)
    {
        return DemandSlots.Slots.Single(slot =>
            slot.Id.Date == date
            && slot.Id.ShiftTypeId == shiftType.Id
            && slot.Id.Ordinal == ordinal);
    }

    public ScheduleAssignment CreateNormalAssignment(
        EmployeeId employeeId,
        DateOnly date,
        ShiftType shiftType,
        AssignmentOrigin origin,
        int ordinal = 1)
    {
        return Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                employeeId,
                FindSlot(date, shiftType, ordinal),
                origin).Value);
    }

    public ScheduleDraft CreateDraft(
        IEnumerable<AvailabilityEntry>? availabilityEntries = null,
        IEnumerable<ScheduleAssignment>? assignments = null,
        IEnumerable<GeneratedDayOffMarker>? markers = null,
        IEnumerable<AssignmentLock>? locks = null)
    {
        return Assert.IsType<ScheduleDraft>(
            CreateDraftResult(
                availabilityEntries: availabilityEntries,
                assignments: assignments,
                markers: markers,
                locks: locks).Value);
    }

    public ScheduleDraftValidationResult CreateDraftResult(
        Guid? id = null,
        int version = 1,
        IEnumerable<AvailabilityEntry>? availabilityEntries = null,
        IEnumerable<ScheduleAssignment>? assignments = null,
        IEnumerable<GeneratedDayOffMarker>? markers = null,
        IEnumerable<AssignmentLock>? locks = null)
    {
        AvailabilityEntrySet entrySet = Assert.IsType<AvailabilityEntrySet>(
            AvailabilityEntrySet.Create(
                availabilityEntries ?? Array.Empty<AvailabilityEntry>()).Value);

        return ScheduleDraft.Create(
            id ?? Guid.NewGuid(),
            version,
            Period,
            DemandSlots,
            entrySet,
            assignments ?? Array.Empty<ScheduleAssignment>(),
            markers ?? Array.Empty<GeneratedDayOffMarker>(),
            locks ?? Array.Empty<AssignmentLock>());
    }
}
