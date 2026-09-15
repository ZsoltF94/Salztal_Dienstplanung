using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class DeleteEmployeeTypeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly IDeleteEmployeeTypeStore _store;

    public DeleteEmployeeTypeCommand(
        IEmployeeReader reader,
        IDeleteEmployeeTypeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeTypeCommandResult> ExecuteAsync(
        DeleteEmployeeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!EmployeeTypeId.TryCreate(
                request.EmployeeTypeId,
                out EmployeeTypeId? employeeTypeId))
        {
            return EmployeeTypeCommandResult.Failure(
                EmployeeTypeCommandStatus.ValidationFailed,
                [new EmployeeTypeCommandError(
                    EmployeeTypeCommandErrorCode.IdentifierRequired,
                    "Der Mitarbeitertyp besitzt keine gültige Kennung.")]);
        }

        if (request.Confirmation != EmployeeTypeDeletionConfirmation.Confirmed)
        {
            return EmployeeTypeCommandErrors.ConfirmationRequired();
        }

        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        EmployeeTypeReferenceValidation referenceValidation =
            EmployeeTypeReferenceValidation.Validate(employeeTypeId, data);
        if (referenceValidation.Status == EmployeeTypeReferenceValidationStatus.NotFound)
        {
            return EmployeeTypeCommandErrors.NotFound();
        }

        if (referenceValidation.Status == EmployeeTypeReferenceValidationStatus.CatalogInvalid)
        {
            return EmployeeTypeCommandErrors.CatalogInvalid();
        }

        EmployeeType current = referenceValidation.EmployeeType!;
        if (current.PlanningPolicy.Role != EmployeeTypePlanningRole.Normal)
        {
            return EmployeeTypeCommandErrors.Protected();
        }

        if (data.Employees.Any(employee => employee.EmployeeTypeId == current.Id))
        {
            return EmployeeTypeCommandErrors.AssignedEmployee();
        }

        EmployeeTypeDeleteStoreResult storeResult = await _store.DeleteAsync(
            current,
            cancellationToken);

        return EmployeeTypeCommandErrors.FromDeleteStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateEmployeeType(current));
    }
}
