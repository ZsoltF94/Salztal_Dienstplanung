namespace Salztal.Dienstplanung.Application.Employees;

public sealed record ChangeEmployeeTypeRequest(
    Guid EmployeeId,
    Guid EmployeeTypeId);
