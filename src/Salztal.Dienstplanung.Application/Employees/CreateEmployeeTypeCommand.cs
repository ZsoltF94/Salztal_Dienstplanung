using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

public sealed class CreateEmployeeTypeCommand
{
    private readonly IEmployeeReader _reader;
    private readonly ICreateEmployeeTypeStore _store;

    public CreateEmployeeTypeCommand(
        IEmployeeReader reader,
        ICreateEmployeeTypeStore store)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(store);
        _reader = reader;
        _store = store;
    }

    public async Task<EmployeeTypeCommandResult> ExecuteAsync(
        CreateEmployeeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        EmployeeReadData data = await _reader.LoadAsync(cancellationToken);
        EmployeeTypeEligibilityMappingResult eligibilityMapping =
            EmployeeTypeEligibilityRequestMapper.Map(
                request.ShiftEligibilities,
                data.ServiceCatalog);

        EmployeeTypeValidationResult validation = EmployeeType.Create(
            Guid.NewGuid(),
            request.Code,
            request.Name,
            request.WeeklyWorkTargetMinutes,
            request.AllowsVacationAndSickness,
            request.AbsenceDayValueMinutes,
            eligibilityMapping.Eligibilities,
            EmployeeTypePlanningPolicy.Standard);

        EmployeeTypeCommandError[] errors = eligibilityMapping.Errors
            .Concat(validation.Errors.Select(EmployeeTypeCommandErrors.FromValidation))
            .ToArray();
        if (errors.Length > 0)
        {
            return EmployeeTypeCommandResult.Failure(
                DetermineFailureStatus(errors),
                errors);
        }

        EmployeeType employeeType = validation.Value!;
        bool codeExists = data.EmployeeTypes.Any(
            current => string.Equals(
                current.Code.Value,
                employeeType.Code.Value,
                StringComparison.OrdinalIgnoreCase));
        if (codeExists)
        {
            return EmployeeTypeCommandErrors.DuplicateCode();
        }

        EmployeeTypeWriteStoreResult storeResult = await _store.CreateAsync(
            employeeType,
            cancellationToken);

        return EmployeeTypeCommandErrors.FromWriteStoreResult(
            storeResult,
            new EmployeeSnapshotProjector(data).CreateEmployeeType(employeeType));
    }

    private static EmployeeTypeCommandStatus DetermineFailureStatus(
        IEnumerable<EmployeeTypeCommandError> errors)
    {
        return errors.Any(error => error.Code is
            EmployeeTypeCommandErrorCode.UnknownShiftType
            or EmployeeTypeCommandErrorCode.UnknownShiftPattern)
            ? EmployeeTypeCommandStatus.CatalogInvalid
            : EmployeeTypeCommandStatus.ValidationFailed;
    }
}
