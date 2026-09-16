using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class ScheduleAssignment
{
    private ScheduleAssignment(
        ScheduleAssignmentId id,
        EmployeeId employeeId,
        ScheduleAssignmentKind kind,
        AssignmentOrigin origin,
        ShiftPatternId? patternId,
        ReadOnlyCollection<AssignmentSegment> segments,
        ReadOnlyCollection<DemandCoverage> coverages)
    {
        Id = id;
        EmployeeId = employeeId;
        Kind = kind;
        Origin = origin;
        PatternId = patternId;
        Segments = segments;
        Coverages = coverages;
        WorkMinutes = segments.Sum(segment => segment.WorkMinutes);
    }

    public ScheduleAssignmentId Id { get; }

    public EmployeeId EmployeeId { get; }

    public ScheduleAssignmentKind Kind { get; }

    public AssignmentOrigin Origin { get; }

    public ShiftPatternId? PatternId { get; }

    public IReadOnlyList<AssignmentSegment> Segments { get; }

    public IReadOnlyList<DemandCoverage> Coverages { get; }

    public DateOnly Date => Segments[0].Date;

    public int WorkMinutes { get; }

    public bool IsProtectedFromAutomaticGeneration =>
        Origin == AssignmentOrigin.ServiceManagement;

    public static ScheduleAssignmentValidationResult CreateNormal(
        Guid id,
        EmployeeId? employeeId,
        DemandSlot? slot,
        AssignmentOrigin origin)
    {
        List<ScheduleAssignmentValidationError> errors = ValidateBase(
            id,
            employeeId,
            origin,
            slot);

        return CreateSingleSlotAssignment(
            id,
            employeeId,
            slot,
            ScheduleAssignmentKind.NormalDemand,
            origin,
            true,
            errors);
    }

    public static ScheduleAssignmentValidationResult CreateOfficeTime(
        Guid id,
        EmployeeId? employeeId,
        DemandSlot? slot)
    {
        List<ScheduleAssignmentValidationError> errors = ValidateBase(
            id,
            employeeId,
            AssignmentOrigin.ServiceManagement,
            slot);

        if (slot is not null
            && (slot.Id.WorkLocationId != InitialWorkLocationCatalog.Restaurant.Id
                || (slot.Id.ShiftTypeId != InitialShiftTypeCatalog.EarlyShift.Id
                    && slot.Id.ShiftTypeId != InitialShiftTypeCatalog.LateShift.Id)))
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.OfficeTimeRequiresRestaurantEarlyOrLate,
                slot.Id));
        }

        return CreateSingleSlotAssignment(
            id,
            employeeId,
            slot,
            ScheduleAssignmentKind.OfficeTime,
            AssignmentOrigin.ServiceManagement,
            false,
            errors);
    }

    public static ScheduleAssignmentValidationResult CreateManualAdditional(
        Guid id,
        EmployeeId? employeeId,
        DemandSlot? anchorSlot)
    {
        List<ScheduleAssignmentValidationError> errors = ValidateBase(
            id,
            employeeId,
            AssignmentOrigin.ManualEdit,
            anchorSlot);

        return CreateSingleSlotAssignment(
            id,
            employeeId,
            anchorSlot,
            ScheduleAssignmentKind.ManualAdditional,
            AssignmentOrigin.ManualEdit,
            false,
            errors);
    }

    public static ScheduleAssignmentValidationResult CreateSplitShift(
        Guid id,
        EmployeeId? employeeId,
        SplitShiftPattern? pattern,
        DemandSlot? firstSlot,
        DemandSlot? secondSlot,
        AssignmentOrigin origin)
    {
        List<ScheduleAssignmentValidationError> errors = ValidateCompositeBase(
            id,
            employeeId,
            pattern,
            firstSlot,
            secondSlot,
            origin);

        if (pattern is not null && firstSlot is not null && secondSlot is not null)
        {
            ValidateSlot(
                firstSlot,
                pattern.WorkLocationId,
                pattern.FirstShiftTypeId,
                errors);
            ValidateSlot(
                secondSlot,
                pattern.WorkLocationId,
                pattern.SecondShiftTypeId,
                errors);

            if (firstSlot.ActualTime.End >= secondSlot.ActualTime.Start)
            {
                errors.Add(new ScheduleAssignmentValidationError(
                    ScheduleAssignmentValidationCode.SplitShiftBreakRequired));
            }
        }

        return errors.Count == 0
            ? Success(
                id,
                employeeId!,
                ScheduleAssignmentKind.SplitShiftPattern,
                origin,
                pattern!.Id,
                [
                    AssignmentSegment.FromFullSlot(firstSlot!),
                    AssignmentSegment.FromFullSlot(secondSlot!),
                ],
                [DemandCoverage.Full(firstSlot!), DemandCoverage.Full(secondSlot!)])
            : ScheduleAssignmentValidationResult.Failure(errors);
    }

    public static ScheduleAssignmentValidationResult CreateReliefShift(
        Guid id,
        EmployeeId? employeeId,
        ReliefShiftPattern? pattern,
        DemandSlot? firstSlot,
        DemandSlot? secondSlot,
        AssignmentOrigin origin)
    {
        List<ScheduleAssignmentValidationError> errors = ValidateCompositeBase(
            id,
            employeeId,
            pattern,
            firstSlot,
            secondSlot,
            origin);

        StaffingDemandTime? partialSecondTime = null;

        if (pattern is not null && firstSlot is not null && secondSlot is not null)
        {
            ValidateSlot(
                firstSlot,
                pattern.FirstWorkLocationId,
                pattern.FirstShiftTypeId,
                errors);
            ValidateSlot(
                secondSlot,
                pattern.SecondWorkLocationId,
                pattern.SecondShiftTypeId,
                errors);

            if (firstSlot.Id.Date.DayOfWeek != pattern.AllowedDay)
            {
                errors.Add(new ScheduleAssignmentValidationError(
                    ScheduleAssignmentValidationCode.ReliefShiftWrongDay));
            }

            TimeOnly switchTime = firstSlot.ActualTime.End;
            if (switchTime <= secondSlot.ActualTime.Start
                || switchTime >= secondSlot.ActualTime.End)
            {
                errors.Add(new ScheduleAssignmentValidationError(
                    ScheduleAssignmentValidationCode.ReliefSwitchMustBeInsideSecondDemand));
            }
            else
            {
                IReadOnlyList<StaffingDemandTimeValidationCode> timeErrors =
                    StaffingDemandTime.TryCreate(
                        switchTime,
                        secondSlot.ActualTime.End,
                        out partialSecondTime);
                if (timeErrors.Count > 0)
                {
                    errors.Add(new ScheduleAssignmentValidationError(
                        ScheduleAssignmentValidationCode.ReliefSwitchMustBeInsideSecondDemand));
                }
            }
        }

        if (errors.Count > 0)
        {
            return ScheduleAssignmentValidationResult.Failure(errors);
        }

        AssignmentSegment secondSegment = new(
            secondSlot!.Id,
            secondSlot.Id.WorkLocationId,
            secondSlot.Id.ShiftTypeId,
            secondSlot.Id.Date,
            partialSecondTime!);
        DemandCoverage partialCoverage = new(
            secondSlot.Id,
            partialSecondTime!,
            DemandCoverageKind.PartialReliefShift);

        return Success(
            id,
            employeeId!,
            ScheduleAssignmentKind.ReliefShiftPattern,
            origin,
            pattern!.Id,
            [AssignmentSegment.FromFullSlot(firstSlot!), secondSegment],
            [DemandCoverage.Full(firstSlot!), partialCoverage]);
    }

    private static ScheduleAssignmentValidationResult CreateSingleSlotAssignment(
        Guid id,
        EmployeeId? employeeId,
        DemandSlot? slot,
        ScheduleAssignmentKind kind,
        AssignmentOrigin origin,
        bool coversDemand,
        List<ScheduleAssignmentValidationError> errors)
    {
        if (errors.Count > 0)
        {
            return ScheduleAssignmentValidationResult.Failure(errors);
        }

        DemandCoverage[] coverages = coversDemand
            ? [DemandCoverage.Full(slot!)]
            : [];

        return Success(
            id,
            employeeId!,
            kind,
            origin,
            null,
            [AssignmentSegment.FromFullSlot(slot!)],
            coverages);
    }

    private static ScheduleAssignmentValidationResult Success(
        Guid id,
        EmployeeId employeeId,
        ScheduleAssignmentKind kind,
        AssignmentOrigin origin,
        ShiftPatternId? patternId,
        AssignmentSegment[] segments,
        DemandCoverage[] coverages)
    {
        ScheduleAssignmentId.TryCreate(id, out ScheduleAssignmentId? assignmentId);

        return ScheduleAssignmentValidationResult.Success(
            new ScheduleAssignment(
                assignmentId!,
                employeeId,
                kind,
                origin,
                patternId,
                Array.AsReadOnly(segments),
                Array.AsReadOnly(coverages)));
    }

    private static List<ScheduleAssignmentValidationError> ValidateCompositeBase(
        Guid id,
        EmployeeId? employeeId,
        IShiftPattern? pattern,
        DemandSlot? firstSlot,
        DemandSlot? secondSlot,
        AssignmentOrigin origin)
    {
        List<ScheduleAssignmentValidationError> errors = ValidateBase(
            id,
            employeeId,
            origin,
            firstSlot);

        if (pattern is null)
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.PatternRequired));
        }

        if (secondSlot is null)
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.DemandSlotRequired));
        }

        if (firstSlot is not null && secondSlot is not null)
        {
            if (firstSlot.Id.Date != secondSlot.Id.Date)
            {
                errors.Add(new ScheduleAssignmentValidationError(
                    ScheduleAssignmentValidationCode.SlotsMustShareDate));
            }

            if (firstSlot.Id == secondSlot.Id)
            {
                errors.Add(new ScheduleAssignmentValidationError(
                    ScheduleAssignmentValidationCode.SlotsMustBeDistinct,
                    firstSlot.Id));
            }
        }

        return errors;
    }

    private static List<ScheduleAssignmentValidationError> ValidateBase(
        Guid id,
        EmployeeId? employeeId,
        AssignmentOrigin origin,
        DemandSlot? slot)
    {
        List<ScheduleAssignmentValidationError> errors = [];

        if (!ScheduleAssignmentId.TryCreate(id, out _))
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.IdentifierRequired));
        }

        if (employeeId is null)
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.EmployeeRequired));
        }

        if (!Enum.IsDefined(origin))
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.OriginInvalid));
        }

        if (slot is null)
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.DemandSlotRequired));
        }

        return errors;
    }

    private static void ValidateSlot(
        DemandSlot slot,
        WorkLocationId expectedWorkLocationId,
        ShiftTypeId expectedShiftTypeId,
        List<ScheduleAssignmentValidationError> errors)
    {
        if (slot.Id.WorkLocationId != expectedWorkLocationId
            || slot.Id.ShiftTypeId != expectedShiftTypeId)
        {
            errors.Add(new ScheduleAssignmentValidationError(
                ScheduleAssignmentValidationCode.SlotDoesNotMatchPattern,
                slot.Id));
        }
    }
}
