namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class EmployeeType
{
    private EmployeeType(
        EmployeeTypeId id,
        EmployeeTypeCode code,
        EmployeeTypeName name,
        WeeklyWorkTarget weeklyWorkTarget)
    {
        Id = id;
        Code = code;
        Name = name;
        WeeklyWorkTarget = weeklyWorkTarget;
    }

    public EmployeeTypeId Id { get; }

    public EmployeeTypeCode Code { get; }

    public EmployeeTypeName Name { get; }

    public WeeklyWorkTarget WeeklyWorkTarget { get; }

    public static EmployeeTypeValidationResult Create(
        Guid id,
        string? code,
        string? name,
        int weeklyWorkTargetMinutes)
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

        if (errors.Count > 0)
        {
            return EmployeeTypeValidationResult.Failure(errors);
        }

        EmployeeType employeeType = new(
            employeeTypeId!,
            employeeTypeCode!,
            employeeTypeName!,
            weeklyWorkTarget!);

        return EmployeeTypeValidationResult.Success(employeeType);
    }

    public EmployeeTypeValidationResult WithDetails(
        string? name,
        int weeklyWorkTargetMinutes)
    {
        return Create(Id.Value, Code.Value, name, weeklyWorkTargetMinutes);
    }
}
