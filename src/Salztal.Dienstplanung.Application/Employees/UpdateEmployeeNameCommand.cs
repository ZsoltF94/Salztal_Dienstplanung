using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class UpdateEmployeeNameCommand
{
    private readonly IEmployeeReader _reader;
    private readonly IUpdateEmployeeNameStore _store;

    public UpdateEmployeeNameCommand(IEmployeeReader reader, IUpdateEmployeeNameStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeCommandResult> ExecuteAsync(
        UpdateEmployeeNameRequest request,
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

        EmployeeValidationResult validation = current.WithName(
            request.FirstName,
            request.LastName);
        if (!validation.IsSuccess)
        {
            return EmployeeCommandResult.Failure(
                EmployeeCommandStatus.ValidationFailed,
                validation.Errors.Select(EmployeeCommandErrors.FromValidation));
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

        Employee replacement = validation.Value!;
        EmployeeWriteStoreResult storeResult = await _store.UpdateNameAsync(
            current,
            replacement,
            cancellationToken);

        return EmployeeCommandErrors.FromStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateDetails(replacement));
    }
}
