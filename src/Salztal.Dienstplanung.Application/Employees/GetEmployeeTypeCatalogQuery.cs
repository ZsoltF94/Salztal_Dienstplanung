namespace Salztal.Dienstplanung.Application.Employees;

public sealed class GetEmployeeTypeCatalogQuery
{
    private readonly IEmployeeReader _reader;

    public GetEmployeeTypeCatalogQuery(IEmployeeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
    }

    public async Task<EmployeeTypeCatalogSnapshot> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        EmployeeSnapshotProjector projector = new(data);

        return new EmployeeTypeCatalogSnapshot(
            data.EmployeeTypes.Select(projector.CreateEmployeeType));
    }
}
