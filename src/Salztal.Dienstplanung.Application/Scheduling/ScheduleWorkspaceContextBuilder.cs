using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class ScheduleWorkspaceContextBuilder
{
    public static ScheduleWorkspaceContextResult Build(
        SchedulePeriod period,
        ScheduleWorkspaceReadData data)
    {
        AvailabilityEntrySetValidationResult availabilityResult =
            AvailabilityEntrySet.Create(
                data.Availability.Entries.Select(item => item.Entry));
        StandardStaffingDemandRevisionSetValidationResult revisionResult =
            StandardStaffingDemandRevisionSet.Create(
                data.StaffingDemands.StandardRevisions);
        StaffingDemandDateExceptionSetValidationResult exceptionResult =
            StaffingDemandDateExceptionSet.Create(
                data.StaffingDemands.DateExceptions);

        if (!availabilityResult.IsSuccess
            || !revisionResult.IsSuccess
            || !exceptionResult.IsSuccess
            || data.Availability.Entries.Any(item => item.ChangeVersion <= 0))
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.StoredDataInvalid,
                ScheduleWorkspaceErrorCode.StoredDataInvalid,
                "Die gespeicherten Tages- oder Bedarfsdaten sind widersprüchlich.");
        }

        EmployeeType[] employeeTypes = data.Availability.EmployeeTypes.ToArray();
        Employee[] employees = data.Availability.Employees.ToArray();
        if (employeeTypes.Select(type => type.Id).Distinct().Count() != employeeTypes.Length
            || employees.Select(employee => employee.Id).Distinct().Count() != employees.Length)
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.CatalogInvalid,
                ScheduleWorkspaceErrorCode.CatalogInvalid,
                "Der Mitarbeitertypenkatalog enthält doppelte Kennungen.");
        }

        Dictionary<EmployeeTypeId, EmployeeType> employeeTypesById =
            employeeTypes.ToDictionary(type => type.Id);
        if (employees.Any(employee =>
                !employeeTypesById.ContainsKey(employee.EmployeeTypeId)))
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.CatalogInvalid,
                ScheduleWorkspaceErrorCode.CatalogInvalid,
                "Mindestens eine Person verweist auf einen unbekannten Mitarbeitertyp.");
        }

        int activeServiceManagementEmployees = employees.Count(employee =>
            employee.IsActive
            && employeeTypesById[employee.EmployeeTypeId].PlanningPolicy.Role
                == EmployeeTypePlanningRole.ServiceManagement);
        if (activeServiceManagementEmployees > 1)
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.CatalogInvalid,
                ScheduleWorkspaceErrorCode.CatalogInvalid,
                "Es darf höchstens eine aktive Serviceleitungs-Person vorhanden sein.");
        }

        if (data.StaffingDemands.ServiceCatalog.WorkLocations
                .Select(location => location.Id).Distinct().Count()
            != data.StaffingDemands.ServiceCatalog.WorkLocations.Count
            || data.StaffingDemands.ServiceCatalog.ShiftTypes
                .Select(shiftType => shiftType.Id).Distinct().Count()
            != data.StaffingDemands.ServiceCatalog.ShiftTypes.Count)
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.CatalogInvalid,
                ScheduleWorkspaceErrorCode.CatalogInvalid,
                "Der Einsatzort- oder Diensttypkatalog enthält doppelte Kennungen.");
        }

        List<EffectiveStaffingDemand> demands = [];
        for (int weekIndex = 0; weekIndex < 3; weekIndex++)
        {
            StaffingDemandWeekResolutionResult weekResult = StaffingDemandWeek.Resolve(
                period.StartMonday.AddDays(weekIndex * 7),
                revisionResult.Value!,
                exceptionResult.Value!);
            if (!weekResult.IsSuccess)
            {
                return Failure(
                    OpenOrCreateScheduleDraftStatus.StoredDataInvalid,
                    ScheduleWorkspaceErrorCode.StoredDataInvalid,
                    "Die Bedarfe können für den ausgewählten Zeitraum nicht eindeutig aufgelöst werden.");
            }

            demands.AddRange(weekResult.Value!.Demands);
        }

        DemandSlotSetValidationResult slotResult = DemandSlotSet.Create(
            period,
            demands,
            data.StaffingDemands.ServiceCatalog.WorkLocations.Select(location => location.Id),
            data.StaffingDemands.ServiceCatalog.ShiftTypes.Select(shiftType => shiftType.Id));
        if (!slotResult.IsSuccess)
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.CatalogInvalid,
                ScheduleWorkspaceErrorCode.CatalogInvalid,
                "Mindestens ein Bedarf verweist auf einen unbekannten Einsatzort oder Diensttyp.");
        }
        if (data.ExactDraft is not null
            && !HasValidDraftReferences(
                data.ExactDraft,
                employees,
                data.StaffingDemands.ServiceCatalog.WorkLocations.Select(location => location.Id),
                data.StaffingDemands.ServiceCatalog.ShiftTypes.Select(shiftType => shiftType.Id)))
        {
            return Failure(
                OpenOrCreateScheduleDraftStatus.StoredDataInvalid,
                ScheduleWorkspaceErrorCode.StoredDataInvalid,
                "Der gespeicherte Entwurf enthält unbekannte Mitarbeiter- oder Katalogreferenzen.");
        }

        Dictionary<(EmployeeId EmployeeId, DateOnly WeekMonday), WeeklyAvailability>
            weeklyAvailabilities = [];
        foreach (Employee employee in employees)
        {
            EmployeeType employeeType = employeeTypesById[employee.EmployeeTypeId];
            for (int weekIndex = 0; weekIndex < 3; weekIndex++)
            {
                DateOnly weekMonday = period.StartMonday.AddDays(weekIndex * 7);
                WeeklyAvailabilityValidationResult weeklyResult =
                    WeeklyAvailability.Calculate(
                        employee,
                        employeeType,
                        weekMonday,
                        availabilityResult.Value!);
                if (!weeklyResult.IsSuccess)
                {
                    return Failure(
                        OpenOrCreateScheduleDraftStatus.StoredDataInvalid,
                        ScheduleWorkspaceErrorCode.StoredDataInvalid,
                        "Mindestens ein Tageskennzeichen ist für den zugeordneten Mitarbeitertyp unzulässig.");
                }

                weeklyAvailabilities.Add(
                    (employee.Id, weekMonday),
                    weeklyResult.Value!);
            }
        }

        return new ScheduleWorkspaceContextResult(
            new ScheduleWorkspaceContext(
                availabilityResult.Value!,
                slotResult.Value!,
                employeeTypesById,
                weeklyAvailabilities),
            OpenOrCreateScheduleDraftStatus.Succeeded,
            Array.Empty<ScheduleWorkspaceError>());
    }

    private static bool HasValidDraftReferences(
        ScheduleDraft draft,
        IEnumerable<Employee> employees,
        IEnumerable<WorkLocationId> workLocationIds,
        IEnumerable<ShiftTypeId> shiftTypeIds)
    {
        Dictionary<EmployeeId, Employee> employeesById = employees
            .ToDictionary(employee => employee.Id);
        HashSet<WorkLocationId> knownWorkLocationIds = workLocationIds.ToHashSet();
        HashSet<ShiftTypeId> knownShiftTypeIds = shiftTypeIds.ToHashSet();

        return draft.Assignments.All(assignment =>
                employeesById.ContainsKey(assignment.EmployeeId))
            && draft.AvailabilityEntries.Entries.All(entry =>
                employeesById.ContainsKey(entry.EmployeeId))
            && draft.DemandSlots.Slots.All(slot =>
                knownWorkLocationIds.Contains(slot.Id.WorkLocationId)
                && knownShiftTypeIds.Contains(slot.Id.ShiftTypeId));
    }

    private static ScheduleWorkspaceContextResult Failure(
        OpenOrCreateScheduleDraftStatus status,
        ScheduleWorkspaceErrorCode code,
        string message)
    {
        return new ScheduleWorkspaceContextResult(
            null,
            status,
            [new ScheduleWorkspaceError(code, message)]);
    }
}
