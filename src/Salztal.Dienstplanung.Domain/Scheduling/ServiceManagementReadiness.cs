using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class ServiceManagementReadiness
{
    private ServiceManagementReadiness(
        ReadOnlyCollection<ServiceManagementWeekReadiness> weeks)
    {
        Weeks = weeks;
        CanPrepare = weeks.All(week =>
            week.Status != ServiceManagementWeekReadinessStatus.MissingAssignment);
    }

    public bool CanPrepare { get; }

    public IReadOnlyList<ServiceManagementWeekReadiness> Weeks { get; }

    public static ServiceManagementReadinessValidationResult Evaluate(
        ScheduleDraft draft,
        EmployeeId serviceManagementEmployeeId,
        IEnumerable<WeeklyAvailability> weeklyAvailabilities)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(serviceManagementEmployeeId);
        ArgumentNullException.ThrowIfNull(weeklyAvailabilities);

        WeeklyAvailability[] availabilitySnapshot = weeklyAvailabilities.ToArray();
        DateOnly[] expectedWeekMondays = Enumerable.Range(0, 3)
            .Select(index => draft.Period.StartMonday.AddDays(index * 7))
            .ToArray();
        List<ServiceManagementReadinessValidationError> errors = [];

        errors.AddRange(
            availabilitySnapshot
                .Where(availability =>
                    availability.EmployeeId != serviceManagementEmployeeId)
                .Select(availability => new ServiceManagementReadinessValidationError(
                    ServiceManagementReadinessValidationCode.EmployeeMismatch,
                    availability.WeekMonday,
                    availability.EmployeeId)));

        errors.AddRange(
            availabilitySnapshot
                .Where(availability =>
                    !expectedWeekMondays.Contains(availability.WeekMonday))
                .Select(availability => new ServiceManagementReadinessValidationError(
                    ServiceManagementReadinessValidationCode.WeekOutsidePeriod,
                    availability.WeekMonday,
                    availability.EmployeeId)));

        foreach (DateOnly weekMonday in expectedWeekMondays)
        {
            int count = availabilitySnapshot.Count(availability =>
                availability.EmployeeId == serviceManagementEmployeeId
                && availability.WeekMonday == weekMonday);
            if (count == 0)
            {
                errors.Add(new ServiceManagementReadinessValidationError(
                    ServiceManagementReadinessValidationCode.MissingWeek,
                    weekMonday,
                    serviceManagementEmployeeId));
            }
            else if (count > 1)
            {
                errors.Add(new ServiceManagementReadinessValidationError(
                    ServiceManagementReadinessValidationCode.DuplicateWeek,
                    weekMonday,
                    serviceManagementEmployeeId));
            }
        }

        if (errors.Count > 0)
        {
            return ServiceManagementReadinessValidationResult.Failure(errors);
        }

        ServiceManagementWeekReadiness[] weeks = expectedWeekMondays
            .Select(weekMonday => CreateWeekReadiness(
                draft,
                serviceManagementEmployeeId,
                availabilitySnapshot.Single(availability =>
                    availability.EmployeeId == serviceManagementEmployeeId
                    && availability.WeekMonday == weekMonday)))
            .ToArray();

        return ServiceManagementReadinessValidationResult.Success(
            new ServiceManagementReadiness(Array.AsReadOnly(weeks)));
    }

    private static ServiceManagementWeekReadiness CreateWeekReadiness(
        ScheduleDraft draft,
        EmployeeId employeeId,
        WeeklyAvailability availability)
    {
        DateOnly weekSunday = availability.WeekMonday.AddDays(6);
        ScheduleAssignment[] assignments = draft.Assignments
            .Where(assignment => assignment.EmployeeId == employeeId)
            .Where(assignment =>
                assignment.Origin == AssignmentOrigin.ServiceManagement)
            .Where(assignment => assignment.Date >= availability.WeekMonday)
            .Where(assignment => assignment.Date <= weekSunday)
            .ToArray();
        int workMinutes = assignments.Sum(assignment => assignment.WorkMinutes);
        ServiceManagementWeekReadinessStatus status = availability.IsFullyUnavailable
            ? ServiceManagementWeekReadinessStatus.ExemptFullyUnavailable
            : assignments.Length > 0
                ? ServiceManagementWeekReadinessStatus.Ready
                : ServiceManagementWeekReadinessStatus.MissingAssignment;

        return new ServiceManagementWeekReadiness(
            availability.WeekMonday,
            status,
            workMinutes);
    }
}
