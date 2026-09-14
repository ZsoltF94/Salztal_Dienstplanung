namespace Salztal.Dienstplanung.Application.Employees;

public interface IEmployeeReader
{
    public Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken);
}
