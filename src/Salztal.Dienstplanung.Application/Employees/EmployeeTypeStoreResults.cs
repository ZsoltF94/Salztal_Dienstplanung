namespace Salztal.Dienstplanung.Application.Employees;

public enum EmployeeTypeWriteStoreResult
{
    Succeeded,
    DuplicateCode,
    Conflict,
}

public enum EmployeeTypeDeleteStoreResult
{
    Succeeded,
    Referenced,
    Conflict,
}
