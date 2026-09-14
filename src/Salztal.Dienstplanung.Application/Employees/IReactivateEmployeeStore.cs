using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IReactivateEmployeeStore
{
    public Task<EmployeeWriteStoreResult> ReactivateAsync(
        Employee expected,
        Employee replacement,
        EmployeeTypeId type1Id,
        CancellationToken cancellationToken);
}
