using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public interface IUpdateEmployeeTypeStore
{
    public Task<EmployeeTypeWriteStoreResult> UpdateAsync(
        EmployeeType expected,
        EmployeeType replacement,
        CancellationToken cancellationToken);
}
