namespace Salztal.Dienstplanung.Application.Employees;

public sealed record EmployeeDetailsSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    EmployeeTypeSnapshot EmployeeType);
