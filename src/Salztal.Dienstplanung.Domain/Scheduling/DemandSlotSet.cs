using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class DemandSlotSet
{
    private DemandSlotSet(ReadOnlyCollection<DemandSlot> slots)
    {
        Slots = slots;
    }

    public IReadOnlyList<DemandSlot> Slots { get; }

    public static DemandSlotSetValidationResult Create(
        SchedulePeriod period,
        IEnumerable<EffectiveStaffingDemand> demands,
        IEnumerable<WorkLocationId> knownWorkLocationIds,
        IEnumerable<ShiftTypeId> knownShiftTypeIds)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(demands);
        ArgumentNullException.ThrowIfNull(knownWorkLocationIds);
        ArgumentNullException.ThrowIfNull(knownShiftTypeIds);

        EffectiveStaffingDemand[] orderedDemands = demands
            .OrderBy(demand => demand.Date)
            .ThenBy(demand => demand.WorkLocationId.Value)
            .ThenBy(demand => demand.ShiftTypeId.Value)
            .ThenBy(demand => demand.SourceKind)
            .ThenBy(demand => demand.SourceId.Value)
            .ToArray();
        HashSet<WorkLocationId> workLocationIds = knownWorkLocationIds.ToHashSet();
        HashSet<ShiftTypeId> shiftTypeIds = knownShiftTypeIds.ToHashSet();
        List<DemandSlotSetValidationError> errors = ValidateReferences(
            period,
            orderedDemands,
            workLocationIds,
            shiftTypeIds);

        if (errors.Count > 0)
        {
            return DemandSlotSetValidationResult.Failure(errors);
        }

        List<DemandSlot> slots = [];
        HashSet<DemandSlotId> slotIds = [];

        foreach (EffectiveStaffingDemand demand in orderedDemands)
        {
            AddSlots(demand, slots, slotIds, errors);
        }

        return errors.Count == 0
            ? DemandSlotSetValidationResult.Success(
                new DemandSlotSet(Array.AsReadOnly(slots.ToArray())))
            : DemandSlotSetValidationResult.Failure(errors);
    }

    public static DemandSlotSetRestoreResult Restore(
        SchedulePeriod period,
        IEnumerable<DemandSlotRestoreValue> values,
        IEnumerable<WorkLocationId> knownWorkLocationIds,
        IEnumerable<ShiftTypeId> knownShiftTypeIds)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(knownWorkLocationIds);
        ArgumentNullException.ThrowIfNull(knownShiftTypeIds);

        HashSet<WorkLocationId> workLocationIds = knownWorkLocationIds.ToHashSet();
        HashSet<ShiftTypeId> shiftTypeIds = knownShiftTypeIds.ToHashSet();
        List<DemandSlotRestoreError> errors = [];
        List<DemandSlot> slots = [];
        HashSet<DemandSlotId> slotIds = [];

        foreach (DemandSlotRestoreValue value in values)
        {
            if (!StaffingDemandId.TryCreate(value.SourceId, out StaffingDemandId? sourceId)
                || !WorkLocationId.TryCreate(
                    value.WorkLocationId,
                    out WorkLocationId? workLocationId)
                || !ShiftTypeId.TryCreate(value.ShiftTypeId, out ShiftTypeId? shiftTypeId))
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.IdentifierInvalid,
                    value));
                continue;
            }

            if (!Enum.IsDefined(value.SourceKind))
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.SourceKindInvalid,
                    value));
            }

            if (!period.Contains(value.Date))
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.DateOutsidePeriod,
                    value));
            }

            if (!workLocationIds.Contains(workLocationId))
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.WorkLocationUnknown,
                    value));
            }

            if (!shiftTypeIds.Contains(shiftTypeId))
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.ShiftTypeUnknown,
                    value));
            }

            if (value.Ordinal <= 0)
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.OrdinalInvalid,
                    value));
            }

            IReadOnlyList<StaffingDemandTimeValidationCode> timeErrors =
                StaffingDemandTime.TryCreate(
                    value.ActualStart,
                    value.ActualEnd,
                    out StaffingDemandTime? actualTime);
            if (timeErrors.Count > 0)
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.ActualTimeInvalid,
                    value));
            }

            if (errors.Any(error =>
                    error.SourceId == value.SourceId
                    && error.Date == value.Date
                    && error.WorkLocationId == value.WorkLocationId
                    && error.ShiftTypeId == value.ShiftTypeId
                    && error.Ordinal == value.Ordinal))
            {
                continue;
            }

            DemandSlotId slotId = new(
                sourceId!,
                value.SourceKind,
                value.Date,
                workLocationId!,
                shiftTypeId!,
                value.Ordinal);
            if (!slotIds.Add(slotId))
            {
                errors.Add(CreateRestoreError(
                    DemandSlotRestoreCode.DuplicateIdentity,
                    value));
                continue;
            }

            slots.Add(new DemandSlot(slotId, actualTime!));
        }

        if (errors.Count > 0)
        {
            return new DemandSlotSetRestoreResult(
                null,
                Array.AsReadOnly(errors.ToArray()));
        }

        DemandSlot[] ordered = slots
            .OrderBy(slot => slot.Id.Date)
            .ThenBy(slot => slot.Id.WorkLocationId.Value)
            .ThenBy(slot => slot.Id.ShiftTypeId.Value)
            .ThenBy(slot => slot.Id.SourceKind)
            .ThenBy(slot => slot.Id.SourceId.Value)
            .ThenBy(slot => slot.Id.Ordinal)
            .ToArray();
        return new DemandSlotSetRestoreResult(
            new DemandSlotSet(Array.AsReadOnly(ordered)),
            Array.AsReadOnly(Array.Empty<DemandSlotRestoreError>()));
    }

    private static List<DemandSlotSetValidationError> ValidateReferences(
        SchedulePeriod period,
        IEnumerable<EffectiveStaffingDemand> demands,
        HashSet<WorkLocationId> knownWorkLocationIds,
        HashSet<ShiftTypeId> knownShiftTypeIds)
    {
        List<DemandSlotSetValidationError> errors = [];

        foreach (EffectiveStaffingDemand demand in demands)
        {
            if (!period.Contains(demand.Date))
            {
                errors.Add(CreateError(
                    DemandSlotSetValidationCode.DemandOutsidePeriod,
                    demand));
            }

            if (!knownWorkLocationIds.Contains(demand.WorkLocationId))
            {
                errors.Add(CreateError(
                    DemandSlotSetValidationCode.UnknownWorkLocation,
                    demand));
            }

            if (!knownShiftTypeIds.Contains(demand.ShiftTypeId))
            {
                errors.Add(CreateError(
                    DemandSlotSetValidationCode.UnknownShiftType,
                    demand));
            }
        }

        return errors;
    }

    private static void AddSlots(
        EffectiveStaffingDemand demand,
        List<DemandSlot> slots,
        HashSet<DemandSlotId> slotIds,
        List<DemandSlotSetValidationError> errors)
    {
        for (int ordinal = 1; ordinal <= demand.RequiredEmployeeCount.Value; ordinal++)
        {
            DemandSlotId slotId = DemandSlotId.CreateValidated(demand, ordinal);

            if (!slotIds.Add(slotId))
            {
                errors.Add(CreateError(
                    DemandSlotSetValidationCode.DuplicateSlotIdentity,
                    demand,
                    slotId));
                continue;
            }

            slots.Add(new DemandSlot(slotId, demand.ActualTime));
        }
    }

    private static DemandSlotSetValidationError CreateError(
        DemandSlotSetValidationCode code,
        EffectiveStaffingDemand demand,
        DemandSlotId? slotId = null)
    {
        return new DemandSlotSetValidationError(
            code,
            demand.SourceId,
            demand.Date,
            demand.WorkLocationId,
            demand.ShiftTypeId,
            slotId);
    }

    private static DemandSlotRestoreError CreateRestoreError(
        DemandSlotRestoreCode code,
        DemandSlotRestoreValue value)
    {
        return new DemandSlotRestoreError(
            code,
            value.SourceId,
            value.Date,
            value.WorkLocationId,
            value.ShiftTypeId,
            value.Ordinal);
    }
}
