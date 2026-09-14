using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class DeactivateEmployeeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly IDeactivateEmployeeStore _store;

    public DeactivateEmployeeCommand(IEmployeeReader reader, IDeactivateEmployeeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeCommandResult> ExecuteAsync(
        DeactivateEmployeeRequest request,
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

        Employee replacement = current.Deactivate();
        EmployeeWriteStoreResult storeResult = await _store.DeactivateAsync(
            current,
            replacement,
            cancellationToken);

        return EmployeeCommandErrors.FromStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateDetails(replacement));
    }
}
