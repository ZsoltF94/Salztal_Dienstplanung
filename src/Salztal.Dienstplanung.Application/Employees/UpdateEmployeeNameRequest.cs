namespace Salztal.Dienstplanung.Application.Employees;

public sealed record UpdateEmployeeNameRequest(
    Guid EmployeeId,
    string? FirstName,
    string? LastName);
