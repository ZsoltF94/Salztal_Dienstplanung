using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class DeleteEmployeeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly IDeleteEmployeeStore _store;

    public DeleteEmployeeCommand(IEmployeeReader reader, IDeleteEmployeeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeCommandResult> ExecuteAsync(
        DeleteEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!EmployeeId.TryCreate(request.EmployeeId, out EmployeeId? employeeId))
        {
            return EmployeeCommandResult.Failure(
                EmployeeCommandStatus.ValidationFailed,
                [new EmployeeCommandError(
                    EmployeeCommandErrorCode.IdentifierRequired,
                    "Der Mitarbeiter besitzt keine gültige Kennung.")]);
        }

        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        Employee? current = data.Employees.SingleOrDefault(
            employee => employee.Id == employeeId);
        if (current is null)
        {
            return EmployeeCommandErrors.EmployeeNotFound();
        }

        if (current.IsActive)
        {
            return EmployeeCommandErrors.EmployeeMustBeInactive();
        }

        EmployeeTypeReferenceValidation typeValidation =
            EmployeeTypeReferenceValidation.Validate(current.EmployeeTypeId, data);
        if (typeValidation.Status == EmployeeTypeReferenceValidationStatus.NotFound)
        {
            return EmployeeCommandErrors.EmployeeTypeNotFound();
        }

        if (typeValidation.Status == EmployeeTypeReferenceValidationStatus.CatalogInvalid)
        {
            return EmployeeCommandErrors.EmployeeTypeCatalogInvalid();
        }

        EmployeeDetailsSnapshot deletedEmployee =
            new EmployeeSnapshotProjector(data).CreateDetails(current);
        EmployeeDeleteStoreResult storeResult = await _store.DeleteAsync(
            current,
            cancellationToken);

        return EmployeeCommandErrors.FromDeleteStoreResult(
            storeResult,
            deletedEmployee);
    }
}
