using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class CreateEmployeeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly ICreateEmployeeStore _store;

    public CreateEmployeeCommand(IEmployeeReader reader, ICreateEmployeeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeCommandResult> ExecuteAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        EmployeeValidationResult validation = Employee.Create(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            request.EmployeeTypeId);
        if (!validation.IsSuccess)
        {
            return EmployeeCommandResult.Failure(
                EmployeeCommandStatus.ValidationFailed,
                validation.Errors.Select(EmployeeCommandErrors.FromValidation));
        }

        Employee employee = validation.Value!;
        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        EmployeeTypeReferenceValidation typeValidation =
            EmployeeTypeReferenceValidation.Validate(employee.EmployeeTypeId, data);
        if (typeValidation.Status == EmployeeTypeReferenceValidationStatus.NotFound)
        {
            return EmployeeCommandErrors.EmployeeTypeNotFound();
        }

        if (typeValidation.Status == EmployeeTypeReferenceValidationStatus.CatalogInvalid)
        {
            return EmployeeCommandErrors.EmployeeTypeCatalogInvalid();
        }

        EmployeeWriteStoreResult storeResult = await _store.CreateAsync(
            employee,
            InitialEmployeeTypeCatalog.Type1.Id,
            cancellationToken);

        return EmployeeCommandErrors.FromStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateDetails(employee));
    }
}
