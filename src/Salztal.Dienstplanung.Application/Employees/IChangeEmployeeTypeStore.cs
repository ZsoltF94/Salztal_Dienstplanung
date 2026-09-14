using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IChangeEmployeeTypeStore
{
    public Task<EmployeeWriteStoreResult> ChangeTypeAsync(
        Employee expected,
        Employee replacement,
        EmployeeTypeId type1Id,
        CancellationToken cancellationToken);
}
