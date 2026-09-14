namespace Salztal.Dienstplanung.Application.Employees;

public sealed record EmployeeOverviewItemSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    Guid EmployeeTypeId,
    string EmployeeTypeCode,
    string EmployeeTypeName,
    int WeeklyWorkTargetMinutes,
    string WeeklyWorkTargetDisplay);
