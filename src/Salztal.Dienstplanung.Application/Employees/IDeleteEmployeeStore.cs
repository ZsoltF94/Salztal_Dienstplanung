using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IDeleteEmployeeStore
{
    public Task<EmployeeDeleteStoreResult> DeleteAsync(
        Employee expected,
        CancellationToken cancellationToken);
}
