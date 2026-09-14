using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IDeactivateEmployeeStore
{
    public Task<EmployeeWriteStoreResult> DeactivateAsync(
        Employee expected,
        Employee replacement,
        CancellationToken cancellationToken);
}
