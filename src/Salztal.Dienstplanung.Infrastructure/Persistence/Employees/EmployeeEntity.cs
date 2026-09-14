namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal sealed class EmployeeEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Guid EmployeeTypeId { get; set; }

    public bool IsActive { get; set; }

    public EmployeeTypeEntity EmployeeType { get; set; } = null!;
}
