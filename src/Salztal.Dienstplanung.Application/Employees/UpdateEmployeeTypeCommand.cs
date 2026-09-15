using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class UpdateEmployeeTypeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly IUpdateEmployeeTypeStore _store;

    public UpdateEmployeeTypeCommand(
        IEmployeeReader reader,
        IUpdateEmployeeTypeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeTypeCommandResult> ExecuteAsync(
        UpdateEmployeeTypeRequest request,
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
        EmployeeTypeEligibilityMappingResult eligibilityMapping =
            EmployeeTypeEligibilityRequestMapper.Map(
                request.ShiftEligibilities,
                data.ServiceCatalog);
        EmployeeTypeValidationResult validation = current.WithDetails(
            request.Name,
            request.WeeklyWorkTargetMinutes,
            request.AllowsVacationAndSickness,
            request.AbsenceDayValueMinutes,
            eligibilityMapping.Eligibilities);

        EmployeeTypeCommandError[] errors = eligibilityMapping.Errors
            .Concat(validation.Errors.Select(EmployeeTypeCommandErrors.FromValidation))
            .ToArray();
        if (errors.Length > 0)
        {
            EmployeeTypeCommandStatus status = errors.Any(error => error.Code is
                EmployeeTypeCommandErrorCode.UnknownShiftType
                or EmployeeTypeCommandErrorCode.UnknownShiftPattern)
                ? EmployeeTypeCommandStatus.CatalogInvalid
                : EmployeeTypeCommandStatus.ValidationFailed;
            return EmployeeTypeCommandResult.Failure(status, errors);
        }

        EmployeeType replacement = validation.Value!;
        EmployeeTypeWriteStoreResult storeResult = await _store.UpdateAsync(
            current,
            replacement,
            cancellationToken);

        return EmployeeTypeCommandErrors.FromWriteStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateEmployeeType(replacement));
    }
}
