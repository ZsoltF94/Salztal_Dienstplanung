namespace Salztal.Dienstplanung.Domain.Employees;

public sealed class Employee
{
    private Employee(
        EmployeeId id,
        EmployeeFirstName firstName,
        EmployeeLastName lastName,
        EmployeeTypeId employeeTypeId,
        bool isActive)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        EmployeeTypeId = employeeTypeId;
        IsActive = isActive;
    }

    public EmployeeId Id { get; }

    public EmployeeFirstName FirstName { get; }

    public EmployeeLastName LastName { get; }

    public string DisplayName => string.Concat(FirstName.Value, " ", LastName.Value);

    public EmployeeTypeId EmployeeTypeId { get; }

    public bool IsActive { get; }

    public static EmployeeValidationResult Create(
        Guid id,
        string? firstName,
        string? lastName,
        Guid employeeTypeId)
    {
        return Create(id, firstName, lastName, employeeTypeId, true);
    }

    public EmployeeValidationResult WithName(string? firstName, string? lastName)
    {
        return Create(
            Id.Value,
            firstName,
            lastName,
            EmployeeTypeId.Value,
            IsActive);
    }

    public EmployeeValidationResult WithEmployeeType(Guid employeeTypeId)
    {
        return Create(
            Id.Value,
            FirstName.Value,
            LastName.Value,
            employeeTypeId,
            IsActive);
    }

    public Employee Deactivate()
    {
        return new Employee(Id, FirstName, LastName, EmployeeTypeId, false);
    }

    public Employee Reactivate()
    {
        return new Employee(Id, FirstName, LastName, EmployeeTypeId, true);
    }

    private static EmployeeValidationResult Create(
        Guid id,
        string? firstName,
        string? lastName,
        Guid employeeTypeId,
        bool isActive)
    {
        List<EmployeeValidationError> errors = [];

        if (!EmployeeId.TryCreate(id, out EmployeeId? validatedEmployeeId))
        {
            errors.Add(new EmployeeValidationError(
                EmployeeValidationCode.IdentifierRequired));
        }

        if (!EmployeeFirstName.TryCreate(
                firstName,
                out EmployeeFirstName? validatedFirstName))
        {
            errors.Add(new EmployeeValidationError(
                EmployeeValidationCode.FirstNameRequired));
        }

        if (!EmployeeLastName.TryCreate(
                lastName,
                out EmployeeLastName? validatedLastName))
        {
            errors.Add(new EmployeeValidationError(
                EmployeeValidationCode.LastNameRequired));
        }

        if (!EmployeeTypeId.TryCreate(
                employeeTypeId,
                out EmployeeTypeId? validatedEmployeeTypeId))
        {
            errors.Add(new EmployeeValidationError(
                EmployeeValidationCode.EmployeeTypeRequired));
        }

        if (errors.Count > 0)
        {
            return EmployeeValidationResult.Failure(errors);
        }

        Employee employee = new(
            validatedEmployeeId!,
            validatedFirstName!,
            validatedLastName!,
            validatedEmployeeTypeId!,
            isActive);

        return EmployeeValidationResult.Success(employee);
    }
}
