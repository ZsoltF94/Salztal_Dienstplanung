using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface ICreateEmployeeStore
{
    public Task<EmployeeWriteStoreResult> CreateAsync(
        Employee employee,
        EmployeeTypeId type1Id,
        CancellationToken cancellationToken);
}
