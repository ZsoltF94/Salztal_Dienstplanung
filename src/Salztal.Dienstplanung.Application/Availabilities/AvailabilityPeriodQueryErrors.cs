using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

internal static class AvailabilityPeriodQueryErrors
{
    public static AvailabilityPeriodQueryError PeriodDoesNotFit()
    {
        return new AvailabilityPeriodQueryError(
            AvailabilityPeriodQueryErrorCode.PeriodMustFitTwentyOneDays,
            "Ab dem ausgewählten Montag kann kein vollständiger Zeitraum mit 21 Tagen gebildet werden.");
    }

    public static IEnumerable<AvailabilityPeriodQueryError> ValidateCatalog(
        AvailabilityReadData data)
    {
        foreach (IGrouping<EmployeeId, Employee> group in data.Employees
                     .GroupBy(employee => employee.Id)
                     .Where(group => group.Count() > 1))
        {
            yield return new AvailabilityPeriodQueryError(
                AvailabilityPeriodQueryErrorCode.DuplicateEmployeeIdentifier,
                "Dieselbe Mitarbeiterkennung ist mehrfach gespeichert.",
                group.Key.Value);
        }

        foreach (IGrouping<EmployeeTypeId, EmployeeType> group in data.EmployeeTypes
                     .GroupBy(employeeType => employeeType.Id)
                     .Where(group => group.Count() > 1))
        {
            yield return new AvailabilityPeriodQueryError(
                AvailabilityPeriodQueryErrorCode.DuplicateEmployeeTypeIdentifier,
                "Dieselbe Mitarbeitertyp-Kennung ist mehrfach gespeichert.");
        }

        HashSet<EmployeeTypeId> employeeTypeIds = data.EmployeeTypes
            .Select(employeeType => employeeType.Id)
            .ToHashSet();
        foreach (Employee employee in data.Employees
                     .Where(employee => !employeeTypeIds.Contains(employee.EmployeeTypeId)))
        {
            yield return new AvailabilityPeriodQueryError(
                AvailabilityPeriodQueryErrorCode.EmployeeTypeMissing,
                "Für eine gespeicherte Person fehlt der zugehörige Mitarbeitertyp.",
                employee.Id.Value);
        }

        HashSet<EmployeeId> employeeIds = data.Employees
            .Select(employee => employee.Id)
            .ToHashSet();
        foreach (AvailabilityEntryReadItem item in data.Entries
                     .Where(item => !employeeIds.Contains(item.Entry.EmployeeId)))
        {
            AvailabilityEntry entry = item.Entry;
            yield return new AvailabilityPeriodQueryError(
                AvailabilityPeriodQueryErrorCode.AvailabilityEmployeeMissing,
                $"Für den Tageseintrag am {entry.Date:dd.MM.yyyy} fehlt die zugehörige Person.",
                entry.EmployeeId.Value,
                entry.Date);
        }
    }

    public static AvailabilityPeriodQueryError InvalidChangeVersion(
        AvailabilityEntryReadItem item)
    {
        return new AvailabilityPeriodQueryError(
            AvailabilityPeriodQueryErrorCode.InvalidChangeVersion,
            $"Der Tageseintrag am {item.Entry.Date:dd.MM.yyyy} besitzt keinen gültigen Änderungsstand.",
            item.Entry.EmployeeId.Value,
            item.Entry.Date);
    }

    public static AvailabilityPeriodQueryError FromDuplicateEntry(
        AvailabilityEntrySetValidationError error)
    {
        return new AvailabilityPeriodQueryError(
            AvailabilityPeriodQueryErrorCode.DuplicateEmployeeAndDate,
            $"Für dieselbe Person ist am {error.Date:dd.MM.yyyy} mehr als eine aktuelle Eingabe gespeichert.",
            error.EmployeeId.Value,
            error.Date);
    }

    public static AvailabilityPeriodQueryError FromWeeklyAvailability(
        Employee employee,
        WeeklyAvailabilityValidationError error)
    {
        return error.Code switch
        {
            WeeklyAvailabilityValidationCode.VacationAndSicknessNotAllowed => new(
                AvailabilityPeriodQueryErrorCode.VacationAndSicknessNotAllowed,
                $"Der Mitarbeitertyp erlaubt am {error.Date:dd.MM.yyyy} kein U oder K.",
                employee.Id.Value,
                error.Date),
            _ => throw new InvalidOperationException(
                $"Unsupported weekly availability validation code: {error.Code}"),
        };
    }
}
