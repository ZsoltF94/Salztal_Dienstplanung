namespace Salztal.Dienstplanung.Application.Employees;

public sealed class GetEmployeeOverviewQuery
{
    private readonly IEmployeeReader _reader;

    public GetEmployeeOverviewQuery(IEmployeeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<EmployeeOverviewSnapshot> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        EmployeeSnapshotProjector projector = new(data);

        return new EmployeeOverviewSnapshot(
            data.Employees.Select(projector.CreateOverviewItem));
    }
}
