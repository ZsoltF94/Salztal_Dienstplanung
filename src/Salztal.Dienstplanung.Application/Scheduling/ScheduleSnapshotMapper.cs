using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class ScheduleSnapshotMapper
{
    public static ScheduleDemandSlotSnapshot[] CreateDemandSlots(
        DemandSlotSet demandSlots,
        ServiceCatalogData serviceCatalog)
    {
        Dictionary<WorkLocationId, string> workLocationNames =
            serviceCatalog.WorkLocations.ToDictionary(
                location => location.Id,
                location => location.Name.Value);
        Dictionary<ShiftTypeId, ShiftType> shiftTypes =
            serviceCatalog.ShiftTypes.ToDictionary(
                shiftType => shiftType.Id,
                shiftType => shiftType);

        return demandSlots.Slots
            .Select(slot =>
            {
                ShiftType shiftType = shiftTypes[slot.Id.ShiftTypeId];
                return new ScheduleDemandSlotSnapshot(
                    slot.Id.SourceId.Value,
                    MapSourceKind(slot.Id.SourceKind),
                    slot.Id.Date,
                    slot.Id.WorkLocationId.Value,
                    workLocationNames[slot.Id.WorkLocationId],
                    slot.Id.ShiftTypeId.Value,
                    shiftType.Name.Value,
                    slot.Id.Ordinal,
                    slot.ActualTime.Start,
                    slot.ActualTime.End,
                    slot.DurationMinutes,
                    MapDisplayKind(shiftType.Display.Kind),
                    shiftType.Display.Abbreviation);
            })
            .ToArray();
    }

    public static ScheduleAssignmentSnapshot[] CreateAssignments(
        IEnumerable<ScheduleAssignment> assignments)
    {
        return assignments.Select(CreateAssignment).ToArray();
    }

    public static ScheduleDemandSlotSnapshot CreateDemandSlot(
        DemandSlot slot,
        ServiceCatalogData serviceCatalog)
    {
        WorkLocation location = serviceCatalog.WorkLocations.Single(item =>
            item.Id == slot.Id.WorkLocationId);
        ShiftType shiftType = serviceCatalog.ShiftTypes.Single(item =>
            item.Id == slot.Id.ShiftTypeId);
        return new ScheduleDemandSlotSnapshot(
            slot.Id.SourceId.Value,
            MapSourceKind(slot.Id.SourceKind),
            slot.Id.Date,
            slot.Id.WorkLocationId.Value,
            location.Name.Value,
            slot.Id.ShiftTypeId.Value,
            shiftType.Name.Value,
            slot.Id.Ordinal,
            slot.ActualTime.Start,
            slot.ActualTime.End,
            slot.DurationMinutes,
            MapDisplayKind(shiftType.Display.Kind),
            shiftType.Display.Abbreviation);
    }

    internal static ScheduleAssignmentSnapshot CreateAssignment(
        ScheduleAssignment assignment)
    {
        return new ScheduleAssignmentSnapshot(
            assignment.Id.Value,
            assignment.EmployeeId.Value,
            assignment.Date,
            MapAssignmentKind(assignment.Kind),
            MapAssignmentOrigin(assignment.Origin),
            assignment.PatternId?.Value,
            assignment.WorkMinutes,
            assignment.IsProtectedFromAutomaticGeneration,
            assignment.Segments.Select(segment => new ScheduleAssignmentSegmentSnapshot(
                segment.AnchorSlotId.SourceId.Value,
                segment.Date,
                segment.WorkLocationId.Value,
                segment.ShiftTypeId.Value,
                segment.ActualTime.Start,
                segment.ActualTime.End,
                segment.WorkMinutes)),
            assignment.Coverages.Select(coverage => new ScheduleDemandCoverageSnapshot(
                coverage.SlotId.SourceId.Value,
                coverage.SlotId.Date,
                coverage.SlotId.WorkLocationId.Value,
                coverage.SlotId.ShiftTypeId.Value,
                coverage.SlotId.Ordinal,
                coverage.CoveredTime.Start,
                coverage.CoveredTime.End,
                coverage.CoveredMinutes,
                MapCoverageKind(coverage.Kind))));
    }

    private static ScheduleDemandSourceKindSnapshot MapSourceKind(
        StaffingDemandSourceKind sourceKind)
    {
        return sourceKind switch
        {
            StaffingDemandSourceKind.Standard =>
                ScheduleDemandSourceKindSnapshot.Standard,
            StaffingDemandSourceKind.DateException =>
                ScheduleDemandSourceKindSnapshot.DateException,
            _ => throw new InvalidOperationException(
                $"Unsupported staffing demand source kind: {sourceKind}"),
        };
    }

    private static ScheduleAssignmentKindSnapshot MapAssignmentKind(
        ScheduleAssignmentKind kind)
    {
        return kind switch
        {
            ScheduleAssignmentKind.NormalDemand =>
                ScheduleAssignmentKindSnapshot.NormalDemand,
            ScheduleAssignmentKind.SplitShiftPattern =>
                ScheduleAssignmentKindSnapshot.SplitShiftPattern,
            ScheduleAssignmentKind.ReliefShiftPattern =>
                ScheduleAssignmentKindSnapshot.ReliefShiftPattern,
            ScheduleAssignmentKind.OfficeTime =>
                ScheduleAssignmentKindSnapshot.OfficeTime,
            ScheduleAssignmentKind.ManualAdditional =>
                ScheduleAssignmentKindSnapshot.ManualAdditional,
            _ => throw new InvalidOperationException(
                $"Unsupported schedule assignment kind: {kind}"),
        };
    }

    private static ScheduleShiftDisplayKindSnapshot MapDisplayKind(
        ShiftTypeDisplayKind displayKind)
    {
        return displayKind switch
        {
            ShiftTypeDisplayKind.Abbreviation =>
                ScheduleShiftDisplayKindSnapshot.Abbreviation,
            ShiftTypeDisplayKind.ActualTime =>
                ScheduleShiftDisplayKindSnapshot.ActualTime,
            _ => throw new InvalidOperationException(
                $"Unsupported shift-type display kind: {displayKind}"),
        };
    }

    private static ScheduleAssignmentOriginSnapshot MapAssignmentOrigin(
        AssignmentOrigin origin)
    {
        return origin switch
        {
            AssignmentOrigin.ServiceManagement =>
                ScheduleAssignmentOriginSnapshot.ServiceManagement,
            AssignmentOrigin.AutomaticGeneration =>
                ScheduleAssignmentOriginSnapshot.AutomaticGeneration,
            AssignmentOrigin.ManualEdit =>
                ScheduleAssignmentOriginSnapshot.ManualEdit,
            _ => throw new InvalidOperationException(
                $"Unsupported schedule assignment origin: {origin}"),
        };
    }

    private static ScheduleDemandCoverageKindSnapshot MapCoverageKind(
        DemandCoverageKind kind)
    {
        return kind switch
        {
            DemandCoverageKind.Full => ScheduleDemandCoverageKindSnapshot.Full,
            DemandCoverageKind.PartialReliefShift =>
                ScheduleDemandCoverageKindSnapshot.PartialReliefShift,
            _ => throw new InvalidOperationException(
                $"Unsupported demand coverage kind: {kind}"),
        };
    }
}
