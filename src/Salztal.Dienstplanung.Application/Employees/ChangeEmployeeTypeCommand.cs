using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class ChangeEmployeeTypeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly IChangeEmployeeTypeStore _store;

    public ChangeEmployeeTypeCommand(IEmployeeReader reader, IChangeEmployeeTypeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeCommandResult> ExecuteAsync(
        ChangeEmployeeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        List<EmployeeCommandError> errors = [];
        if (!EmployeeId.TryCreate(request.EmployeeId, out EmployeeId? employeeId))
        {
            errors.Add(new EmployeeCommandError(
                EmployeeCommandErrorCode.IdentifierRequired,
                "Der Mitarbeiter besitzt keine gültige Kennung."));
        }

        if (!EmployeeTypeId.TryCreate(
                request.EmployeeTypeId,
                out EmployeeTypeId? employeeTypeId))
        {
            errors.Add(new EmployeeCommandError(
                EmployeeCommandErrorCode.EmployeeTypeRequired,
                "Bitte wählen Sie einen Mitarbeitertyp aus."));
        }

        if (errors.Count > 0)
        {
            return EmployeeCommandResult.Failure(
                EmployeeCommandStatus.ValidationFailed,
                errors);
        }

        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        Employee? current = data.Employees.SingleOrDefault(
            employee => employee.Id == employeeId);
        if (current is null)
        {
            return EmployeeCommandErrors.EmployeeNotFound();
        }

        EmployeeTypeReferenceValidation typeValidation =
            EmployeeTypeReferenceValidation.Validate(employeeTypeId!, data);
        if (typeValidation.Status == EmployeeTypeReferenceValidationStatus.NotFound)
        {
            return EmployeeCommandErrors.EmployeeTypeNotFound();
        }

        if (typeValidation.Status == EmployeeTypeReferenceValidationStatus.CatalogInvalid)
        {
            return EmployeeCommandErrors.EmployeeTypeCatalogInvalid();
        }

        Employee replacement = current.WithEmployeeType(employeeTypeId!.Value).Value
            ?? throw new InvalidOperationException("A validated employee type was rejected.");
        EmployeeWriteStoreResult storeResult = await _store.ChangeTypeAsync(
            current,
            replacement,
            InitialEmployeeTypeCatalog.Type1.Id,
            cancellationToken);

        return EmployeeCommandErrors.FromStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateDetails(replacement));
    }
}
