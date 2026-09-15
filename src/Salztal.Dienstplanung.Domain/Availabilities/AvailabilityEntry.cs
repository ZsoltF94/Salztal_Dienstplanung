using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed class AvailabilityEntry
{
    private AvailabilityEntry(
        EmployeeId employeeId,
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        EmployeeId = employeeId;
        Date = date;
        Kind = kind;
    }

    public EmployeeId EmployeeId { get; }

    public DateOnly Date { get; }

    public AvailabilityEntryKind Kind { get; }

    public static AvailabilityEntryValidationResult Create(
        Guid employeeId,
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        List<AvailabilityEntryValidationError> errors = [];

        if (!EmployeeId.TryCreate(employeeId, out EmployeeId? validatedEmployeeId))
        {
            errors.Add(new AvailabilityEntryValidationError(
                AvailabilityEntryValidationCode.EmployeeIdentifierRequired));
        }

        if (!Enum.IsDefined(kind))
        {
            errors.Add(new AvailabilityEntryValidationError(
                AvailabilityEntryValidationCode.UnsupportedKind));
        }

        if (errors.Count > 0)
        {
            return AvailabilityEntryValidationResult.Failure(errors);
        }

        return AvailabilityEntryValidationResult.Success(
            new AvailabilityEntry(validatedEmployeeId!, date, kind));
    }
}
