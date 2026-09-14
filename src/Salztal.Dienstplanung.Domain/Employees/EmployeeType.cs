using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class EmployeeType
{
    private EmployeeType(
        EmployeeTypeId id,
        EmployeeTypeCode code,
        EmployeeTypeName name,
        WeeklyWorkTarget weeklyWorkTarget,
        ReadOnlyCollection<EmployeeTypeShiftEligibility> shiftEligibilities,
        EmployeeTypePlanningPolicy planningPolicy)
    {
        Id = id;
        Code = code;
        Name = name;
        WeeklyWorkTarget = weeklyWorkTarget;
        ShiftEligibilities = shiftEligibilities;
        PlanningPolicy = planningPolicy;
    }

    public EmployeeTypeId Id { get; }

    public EmployeeTypeCode Code { get; }

    public EmployeeTypeName Name { get; }

    public WeeklyWorkTarget WeeklyWorkTarget { get; }

    public IReadOnlyList<EmployeeTypeShiftEligibility> ShiftEligibilities { get; }

    public EmployeeTypePlanningPolicy PlanningPolicy { get; }

    public static EmployeeTypeValidationResult Create(
        Guid id,
        string? code,
        string? name,
        int weeklyWorkTargetMinutes)
    {
        return Create(
            id,
            code,
            name,
            weeklyWorkTargetMinutes,
            Array.Empty<EmployeeTypeShiftEligibility>(),
            EmployeeTypePlanningPolicy.Standard);
    }

    public static EmployeeTypeValidationResult Create(
        Guid id,
        string? code,
        string? name,
        int weeklyWorkTargetMinutes,
        IEnumerable<EmployeeTypeShiftEligibility?>? shiftEligibilities,
        EmployeeTypePlanningPolicy? planningPolicy)
    {
        List<EmployeeTypeValidationError> errors = [];

        if (!EmployeeTypeId.TryCreate(id, out EmployeeTypeId? employeeTypeId))
        {
            errors.Add(new EmployeeTypeValidationError(
                EmployeeTypeValidationCode.IdentifierRequired));
        }

        if (!EmployeeTypeCode.TryCreate(code, out EmployeeTypeCode? employeeTypeCode))
        {
            errors.Add(new EmployeeTypeValidationError(
                EmployeeTypeValidationCode.CodeRequired));
        }

        if (!EmployeeTypeName.TryCreate(name, out EmployeeTypeName? employeeTypeName))
        {
            errors.Add(new EmployeeTypeValidationError(
                EmployeeTypeValidationCode.NameRequired));
        }

        WeeklyWorkTargetValidationCode? targetError = WeeklyWorkTarget.TryCreate(
            weeklyWorkTargetMinutes,
            out WeeklyWorkTarget? weeklyWorkTarget);

        if (targetError is not null)
        {
            errors.Add(new EmployeeTypeValidationError(
                targetError == WeeklyWorkTargetValidationCode.MustBePositive
                    ? EmployeeTypeValidationCode.WeeklyWorkTargetMustBePositive
                    : EmployeeTypeValidationCode.WeeklyWorkTargetExceedsWeek));
        }

        EmployeeTypeShiftEligibility[] validShiftEligibilities = [];
        if (shiftEligibilities is null)
        {
            errors.Add(new EmployeeTypeValidationError(
                EmployeeTypeValidationCode.ShiftEligibilityRequired));
        }
        else
        {
            EmployeeTypeShiftEligibility?[] suppliedShiftEligibilities =
                shiftEligibilities.ToArray();

            if (suppliedShiftEligibilities.Any(eligibility => eligibility is null))
            {
                errors.Add(new EmployeeTypeValidationError(
                    EmployeeTypeValidationCode.ShiftEligibilityRequired));
            }
            else
            {
                validShiftEligibilities = suppliedShiftEligibilities
                    .Select(eligibility => eligibility!)
                    .ToArray();

                if (validShiftEligibilities.Distinct().Count()
                    != validShiftEligibilities.Length)
                {
                    errors.Add(new EmployeeTypeValidationError(
                        EmployeeTypeValidationCode.DuplicateShiftEligibility));
                }
            }
        }

        if (planningPolicy is null)
        {
            errors.Add(new EmployeeTypeValidationError(
                EmployeeTypeValidationCode.PlanningPolicyRequired));
        }

        if (errors.Count > 0)
        {
            return EmployeeTypeValidationResult.Failure(errors);
        }

        EmployeeType employeeType = new(
            employeeTypeId!,
            employeeTypeCode!,
            employeeTypeName!,
            weeklyWorkTarget!,
            Array.AsReadOnly(validShiftEligibilities),
            planningPolicy!);

        return EmployeeTypeValidationResult.Success(employeeType);
    }

    public EmployeeTypeValidationResult WithDetails(
        string? name,
        int weeklyWorkTargetMinutes)
    {
        return Create(
            Id.Value,
            Code.Value,
            name,
            weeklyWorkTargetMinutes,
            ShiftEligibilities,
            PlanningPolicy);
    }
}
