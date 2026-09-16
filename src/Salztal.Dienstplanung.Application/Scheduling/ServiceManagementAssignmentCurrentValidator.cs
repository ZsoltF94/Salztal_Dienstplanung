using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class ServiceManagementAssignmentCurrentValidator
{
    public static IReadOnlyList<InvalidServiceManagementAssignment> Validate(
        ScheduleDayCommandContext context)
    {
        Dictionary<EmployeeId, Employee> employees =
            context.ReadData.Availability.Employees.ToDictionary(employee => employee.Id);
        HashSet<DemandSlotId> currentSlotIds = context.Workspace.DemandSlots.Slots
            .Select(slot => slot.Id)
            .ToHashSet();
        Dictionary<DemandSlotId, DemandSlot> slots = context.Workspace.DemandSlots.Slots
            .ToDictionary(slot => slot.Id);
        List<InvalidServiceManagementAssignment> errors = [];

        foreach (ScheduleAssignment assignment in context.Draft.Assignments.Where(
                     candidate => candidate.Origin == AssignmentOrigin.ServiceManagement))
        {
            InvalidServiceManagementAssignmentCode? error = ValidateAssignment(
                assignment,
                context,
                employees,
                currentSlotIds,
                slots);
            if (error is not null)
            {
                errors.Add(new InvalidServiceManagementAssignment(
                    assignment.Id.Value,
                    error.Value));
            }
        }

        return errors;
    }

    private static InvalidServiceManagementAssignmentCode? ValidateAssignment(
        ScheduleAssignment assignment,
        ScheduleDayCommandContext context,
        Dictionary<EmployeeId, Employee> employees,
        HashSet<DemandSlotId> currentSlotIds,
        Dictionary<DemandSlotId, DemandSlot> slots)
    {
        if (!employees.TryGetValue(assignment.EmployeeId, out Employee? employee))
        {
            return InvalidServiceManagementAssignmentCode.EmployeeMissing;
        }

        if (!employee.IsActive)
        {
            return InvalidServiceManagementAssignmentCode.EmployeeInactive;
        }

        EmployeeType employeeType = context.Workspace.EmployeeTypes[employee.EmployeeTypeId];
        if (employeeType.PlanningPolicy.Role
            != EmployeeTypePlanningRole.ServiceManagement)
        {
            return InvalidServiceManagementAssignmentCode.WrongPlanningRole;
        }

        if (context.Workspace.AvailabilityEntries.Find(
                employee.Id,
                assignment.Date) is not null)
        {
            return InvalidServiceManagementAssignmentCode.AvailabilityConflict;
        }

        DemandSlotId[] referencedSlots = assignment.Segments
            .Select(segment => segment.AnchorSlotId)
            .Concat(assignment.Coverages.Select(coverage => coverage.SlotId))
            .Distinct()
            .ToArray();
        if (referencedSlots.Any(slotId => !currentSlotIds.Contains(slotId)))
        {
            return InvalidServiceManagementAssignmentCode.DemandSlotMissing;
        }

        if (!HasCurrentTimes(assignment, slots))
        {
            return InvalidServiceManagementAssignmentCode.DemandSlotTimeChanged;
        }

        if (!HasEligibility(assignment, employeeType))
        {
            return InvalidServiceManagementAssignmentCode.EligibilityMissing;
        }

        return null;
    }

    private static bool HasCurrentTimes(
        ScheduleAssignment assignment,
        Dictionary<DemandSlotId, DemandSlot> slots)
    {
        foreach (AssignmentSegment segment in assignment.Segments)
        {
            DemandSlot slot = slots[segment.AnchorSlotId];
            bool exact = assignment.Kind == ScheduleAssignmentKind.ReliefShiftPattern
                && segment != assignment.Segments[0]
                ? segment.ActualTime.Start == assignment.Segments[0].ActualTime.End
                    && segment.ActualTime.End == slot.ActualTime.End
                : segment.ActualTime == slot.ActualTime;
            if (!exact)
            {
                return false;
            }
        }

        foreach (DemandCoverage coverage in assignment.Coverages)
        {
            DemandSlot slot = slots[coverage.SlotId];
            bool exact = coverage.Kind == DemandCoverageKind.PartialReliefShift
                ? coverage.CoveredTime.Start == assignment.Segments[0].ActualTime.End
                    && coverage.CoveredTime.End == slot.ActualTime.End
                : coverage.CoveredTime == slot.ActualTime;
            if (!exact)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasEligibility(
        ScheduleAssignment assignment,
        EmployeeType employeeType)
    {
        if (assignment.PatternId is not null)
        {
            return employeeType.ShiftEligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern
                && eligibility.ShiftPatternId == assignment.PatternId
                && eligibility.Activation == ShiftEligibilityActivation.Always);
        }

        return assignment.Segments.All(segment =>
            employeeType.ShiftEligibilities.Any(eligibility =>
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType
                && eligibility.ShiftTypeId == segment.ShiftTypeId
                && eligibility.Activation == ShiftEligibilityActivation.Always));
    }
}
