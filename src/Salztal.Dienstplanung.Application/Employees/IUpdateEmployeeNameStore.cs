using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IUpdateEmployeeNameStore
{
    public Task<EmployeeWriteStoreResult> UpdateNameAsync(
        Employee expected,
        Employee replacement,
        CancellationToken cancellationToken);
}
