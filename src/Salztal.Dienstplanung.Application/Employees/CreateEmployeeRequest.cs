namespace Salztal.Dienstplanung.Application.Employees;

public sealed record CreateEmployeeRequest(
    string? FirstName,
    string? LastName,
    Guid EmployeeTypeId);
