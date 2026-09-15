using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IDeleteEmployeeTypeStore
{
    public Task<EmployeeTypeDeleteStoreResult> DeleteAsync(
        EmployeeType expected,
        CancellationToken cancellationToken);
}
