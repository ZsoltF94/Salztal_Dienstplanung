using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Availabilities;

internal sealed class AvailabilityEntryCommandContext
{
    private AvailabilityEntryCommandContext(
        Employee employee,
        EmployeeType employeeType,
        AvailabilityEntrySet entries,
        AvailabilityEntryReadItem? current)
    {
        Employee = employee;
        EmployeeType = employeeType;
        Entries = entries;
        Current = current;
    }

    public Employee Employee { get; }

    public EmployeeType EmployeeType { get; }

    public AvailabilityEntrySet Entries { get; }

    public AvailabilityEntryReadItem? Current { get; }

    public static AvailabilityEntryCommandContextValidationResult Create(
        EmployeeId employeeId,
        DateOnly date,
        AvailabilityReadData data)
    {
        AvailabilityPeriodQueryError[] catalogErrors =
            AvailabilityPeriodQueryErrors.ValidateCatalog(data).ToArray();
        if (catalogErrors.Length > 0)
        {
            return AvailabilityEntryCommandContextValidationResult.Failed(
                AvailabilityEntryCommandErrors.CatalogInvalid(catalogErrors[0].Message));
        }

        AvailabilityEntrySetValidationResult entrySetResult =
            AvailabilityEntrySet.Create(data.Entries.Select(item => item.Entry));
        if (!entrySetResult.IsSuccess)
        {
            return AvailabilityEntryCommandContextValidationResult.Failed(
                AvailabilityEntryCommandErrors.StoredDataInvalid());
        }

        AvailabilityEntryReadItem? invalidVersion = data.Entries
            .FirstOrDefault(item => item.ChangeVersion <= 0);
        if (invalidVersion is not null)
        {
            return AvailabilityEntryCommandContextValidationResult.Failed(
                AvailabilityEntryCommandErrors.StoredDataInvalid(
                    AvailabilityPeriodQueryErrors.InvalidChangeVersion(
                        invalidVersion).Message));
        }

        Employee? employee = data.Employees.SingleOrDefault(
            candidate => candidate.Id == employeeId);
        if (employee is null)
        {
            return AvailabilityEntryCommandContextValidationResult.Failed(
                AvailabilityEntryCommandErrors.EmployeeNotFound());
        }

        if (!employee.IsActive)
        {
            return AvailabilityEntryCommandContextValidationResult.Failed(
                AvailabilityEntryCommandErrors.EmployeeInactive());
        }

        EmployeeType employeeType = data.EmployeeTypes.Single(
            candidate => candidate.Id == employee.EmployeeTypeId);
        AvailabilityEntryReadItem? current = data.Entries.SingleOrDefault(
            item => item.Entry.EmployeeId == employeeId && item.Entry.Date == date);

        return AvailabilityEntryCommandContextValidationResult.Success(
            new AvailabilityEntryCommandContext(
                employee,
                employeeType,
                entrySetResult.Value!,
                current));
    }
}

internal sealed record AvailabilityEntryCommandContextValidationResult(
    AvailabilityEntryCommandContext? Value,
    AvailabilityEntryCommandResult? Failure)
{
    public static AvailabilityEntryCommandContextValidationResult Success(
        AvailabilityEntryCommandContext value)
    {
        return new AvailabilityEntryCommandContextValidationResult(value, null);
    }

    public static AvailabilityEntryCommandContextValidationResult Failed(
        AvailabilityEntryCommandResult failure)
    {
        return new AvailabilityEntryCommandContextValidationResult(null, failure);
    }
}
