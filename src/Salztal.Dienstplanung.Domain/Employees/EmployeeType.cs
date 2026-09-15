using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class EmployeeType
{
    private EmployeeType(
        EmployeeTypeId id,
        EmployeeTypeCode code,
        EmployeeTypeName name,
        WeeklyWorkTarget weeklyWorkTarget,
        EmployeeTypeAbsencePolicy absencePolicy,
        ReadOnlyCollection<EmployeeTypeShiftEligibility> shiftEligibilities,
        EmployeeTypePlanningPolicy planningPolicy)
    {
        Id = id;
        Code = code;
        Name = name;
        WeeklyWorkTarget = weeklyWorkTarget;
        AbsencePolicy = absencePolicy;
        ShiftEligibilities = shiftEligibilities;
        PlanningPolicy = planningPolicy;
    }

    public EmployeeTypeId Id { get; }

    public EmployeeTypeCode Code { get; }

    public EmployeeTypeName Name { get; }

    public WeeklyWorkTarget WeeklyWorkTarget { get; }

    public EmployeeTypeAbsencePolicy AbsencePolicy { get; }

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
            false,
            null,
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
        return Create(
            id,
            code,
            name,
            weeklyWorkTargetMinutes,
            false,
            null,
            shiftEligibilities,
            planningPolicy);
    }

    public static EmployeeTypeValidationResult Create(
        Guid id,
        string? code,
        string? name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
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

        EmployeeTypeValidationCode? absencePolicyError = EmployeeTypeAbsencePolicy.TryCreate(
            allowsVacationAndSickness,
            absenceDayValueMinutes,
            out EmployeeTypeAbsencePolicy? absencePolicy);

        if (absencePolicyError is not null)
        {
            errors.Add(new EmployeeTypeValidationError(absencePolicyError.Value));
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
            absencePolicy!,
            Array.AsReadOnly(validShiftEligibilities),
            planningPolicy!);

        return EmployeeTypeValidationResult.Success(employeeType);
    }

    public EmployeeTypeValidationResult WithDetails(
        string? name,
        int weeklyWorkTargetMinutes)
    {
        return WithDetails(
            name,
            weeklyWorkTargetMinutes,
            AbsencePolicy.AllowsVacationAndSickness,
            AbsencePolicy.DayValue?.Minutes,
            ShiftEligibilities);
    }

    public EmployeeTypeValidationResult WithDetails(
        string? name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        IEnumerable<EmployeeTypeShiftEligibility?>? shiftEligibilities)
    {
        return Create(
            Id.Value,
            Code.Value,
            name,
            weeklyWorkTargetMinutes,
            allowsVacationAndSickness,
            absenceDayValueMinutes,
            shiftEligibilities,
            PlanningPolicy);
    }
}
