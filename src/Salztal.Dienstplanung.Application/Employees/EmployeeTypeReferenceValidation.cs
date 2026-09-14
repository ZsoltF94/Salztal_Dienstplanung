using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

internal enum EmployeeTypeReferenceValidationStatus
{
    Valid,
    NotFound,
    CatalogInvalid,
}

internal sealed record EmployeeTypeReferenceValidation(
    EmployeeTypeReferenceValidationStatus Status,
    EmployeeType? EmployeeType)
{
    public static EmployeeTypeReferenceValidation Validate(
        EmployeeTypeId employeeTypeId,
        EmployeeReadData data)
    {
        EmployeeType? employeeType = data.EmployeeTypes.SingleOrDefault(
            candidate => candidate.Id == employeeTypeId);
        if (employeeType is null)
        {
            return new EmployeeTypeReferenceValidation(
                EmployeeTypeReferenceValidationStatus.NotFound,
                null);
        }

        HashSet<Guid> shiftTypeIds = data.ServiceCatalog.ShiftTypes
            .Select(shiftType => shiftType.Id.Value)
            .ToHashSet();
        HashSet<Guid> shiftPatternIds =
        [
            data.ServiceCatalog.SplitShiftPattern.Id.Value,
            data.ServiceCatalog.ReliefShiftPattern.Id.Value,
        ];

        bool hasInvalidReference = employeeType.ShiftEligibilities.Any(
            eligibility => eligibility.TargetKind switch
            {
                ShiftEligibilityTargetKind.ShiftType =>
                    eligibility.ShiftTypeId is null
                    || eligibility.ShiftPatternId is not null
                    || !shiftTypeIds.Contains(eligibility.ShiftTypeId.Value),
                ShiftEligibilityTargetKind.ShiftPattern =>
                    eligibility.ShiftPatternId is null
                    || eligibility.ShiftTypeId is not null
                    || !shiftPatternIds.Contains(eligibility.ShiftPatternId.Value),
                _ => true,
            });

        return hasInvalidReference
            ? new EmployeeTypeReferenceValidation(
                EmployeeTypeReferenceValidationStatus.CatalogInvalid,
                null)
            : new EmployeeTypeReferenceValidation(
                EmployeeTypeReferenceValidationStatus.Valid,
                employeeType);
    }
}
