using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class GetEmployeeDetailsQuery
{
    private readonly IEmployeeReader _reader;

    public GetEmployeeDetailsQuery(IEmployeeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<EmployeeDetailsSnapshot?> ExecuteAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!EmployeeId.TryCreate(employeeId, out EmployeeId? validatedEmployeeId))
        {
            return null;
        }

        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        Employee? employee = data.Employees.SingleOrDefault(
            candidate => candidate.Id == validatedEmployeeId);

        return employee is null
            ? null
            : new EmployeeSnapshotProjector(data).CreateDetails(employee);
    }
}
