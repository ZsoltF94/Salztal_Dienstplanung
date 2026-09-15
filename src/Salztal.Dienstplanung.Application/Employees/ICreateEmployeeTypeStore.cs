using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface ICreateEmployeeTypeStore
{
    public Task<EmployeeTypeWriteStoreResult> CreateAsync(
        EmployeeType employeeType,
        CancellationToken cancellationToken);
}
