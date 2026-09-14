using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Tests.Employees;

public sealed class EmployeeTests
{
    [Fact]
    public void CreateWhenValuesAreValidReturnsNormalizedActiveEmployee()
    {
        Guid id = new("eb1b8a55-3a16-4263-a41f-b87824853073");
        Guid employeeTypeId = InitialEmployeeTypeCatalog.Type30a.Id.Value;

        EmployeeValidationResult result = Employee.Create(
            id,
            "  Erika  ",
            "  Beispiel  ",
            employeeTypeId);

        Employee employee = Assert.IsType<Employee>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(id, employee.Id.Value);
        Assert.Equal("Erika", employee.FirstName.Value);
        Assert.Equal("Beispiel", employee.LastName.Value);
        Assert.Equal("Erika Beispiel", employee.DisplayName);
        Assert.Equal(employeeTypeId, employee.EmployeeTypeId.Value);
        Assert.True(employee.IsActive);
    }

    [Fact]
    public void CreateWhenAllValuesAreInvalidReturnsAllValidationCodes()
    {
        EmployeeValidationResult result = Employee.Create(
            Guid.Empty,
            " ",
            null,
            Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(EmployeeValidationCode.IdentifierRequired, error.Code),
            error => Assert.Equal(EmployeeValidationCode.FirstNameRequired, error.Code),
            error => Assert.Equal(EmployeeValidationCode.LastNameRequired, error.Code),
            error => Assert.Equal(EmployeeValidationCode.EmployeeTypeRequired, error.Code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWhenFirstNameIsMissingReturnsFirstNameRequired(string? firstName)
    {
        EmployeeValidationResult result = Employee.Create(
            Guid.NewGuid(),
            firstName,
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        EmployeeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeValidationCode.FirstNameRequired, error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWhenLastNameIsMissingReturnsLastNameRequired(string? lastName)
    {
        EmployeeValidationResult result = Employee.Create(
            Guid.NewGuid(),
            "Erika",
            lastName,
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        EmployeeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeValidationCode.LastNameRequired, error.Code);
    }

    [Fact]
    public void CreateWhenEmployeeTypeIdentifierIsEmptyReturnsEmployeeTypeRequired()
    {
        EmployeeValidationResult result = Employee.Create(
            Guid.NewGuid(),
            "Erika",
            "Beispiel",
            Guid.Empty);

        EmployeeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeValidationCode.EmployeeTypeRequired, error.Code);
    }

    [Fact]
    public void EqualDisplayNamesWhenCreatedRemainSeparateEmployees()
    {
        Employee first = CreateEmployee(
            new Guid("14c0063a-61b3-4c70-9926-674d37f2508a"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);
        Employee second = CreateEmployee(
            new Guid("9fd2d2ed-5a52-4d6c-9136-511ee2bbfa7c"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        Assert.Equal(first.DisplayName, second.DisplayName);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void WithNameWhenValuesAreValidPreservesIdentityTypeAndOriginal()
    {
        Employee original = CreateEmployee(
            new Guid("9da9f2e8-fe69-4f60-8dde-fb13d5821cfb"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        EmployeeValidationResult result = original.WithName("Mara", "Muster");

        Employee changed = Assert.IsType<Employee>(result.Value);
        Assert.Equal(original.Id, changed.Id);
        Assert.Equal(original.EmployeeTypeId, changed.EmployeeTypeId);
        Assert.Equal("Mara Muster", changed.DisplayName);
        Assert.True(changed.IsActive);
        Assert.Equal("Erika Beispiel", original.DisplayName);
    }

    [Fact]
    public void WithNameWhenValueIsInvalidLeavesOriginalUnchanged()
    {
        Employee original = CreateEmployee(
            new Guid("78b1d9b9-d756-4077-86fe-7d03f76eca62"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        EmployeeValidationResult result = original.WithName(" ", "Muster");

        Assert.False(result.IsSuccess);
        EmployeeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeValidationCode.FirstNameRequired, error.Code);
        Assert.Equal("Erika Beispiel", original.DisplayName);
    }

    [Fact]
    public void WithEmployeeTypeWhenIdentifierIsValidPreservesIdentityNameAndOriginal()
    {
        Employee original = CreateEmployee(
            new Guid("d09ed95b-22e4-4971-89e9-2ecc8741f30e"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        EmployeeValidationResult result = original.WithEmployeeType(
            InitialEmployeeTypeCatalog.Type30a.Id.Value);

        Employee changed = Assert.IsType<Employee>(result.Value);
        Assert.Equal(original.Id, changed.Id);
        Assert.Equal(original.FirstName, changed.FirstName);
        Assert.Equal(original.LastName, changed.LastName);
        Assert.Equal(InitialEmployeeTypeCatalog.Type30a.Id, changed.EmployeeTypeId);
        Assert.True(changed.IsActive);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Id, original.EmployeeTypeId);
    }

    [Fact]
    public void WithEmployeeTypeWhenIdentifierIsEmptyLeavesOriginalUnchanged()
    {
        Employee original = CreateEmployee(
            new Guid("54aefbab-510f-4c7d-98d8-5ba1bda5160a"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type25.Id.Value);

        EmployeeValidationResult result = original.WithEmployeeType(Guid.Empty);

        Assert.False(result.IsSuccess);
        EmployeeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeValidationCode.EmployeeTypeRequired, error.Code);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Id, original.EmployeeTypeId);
    }

    [Fact]
    public void DeactivateWhenEmployeeIsActiveReturnsInactiveVersionAndPreservesOriginal()
    {
        Employee original = CreateEmployee(
            new Guid("3e865d0e-2c3a-422d-8d7a-f9cedb85dcf5"),
            "Erika",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type35.Id.Value);

        Employee deactivated = original.Deactivate();

        Assert.Equal(original.Id, deactivated.Id);
        Assert.Equal(original.FirstName, deactivated.FirstName);
        Assert.Equal(original.LastName, deactivated.LastName);
        Assert.Equal(original.EmployeeTypeId, deactivated.EmployeeTypeId);
        Assert.False(deactivated.IsActive);
        Assert.True(original.IsActive);
    }

    [Fact]
    public void ReactivateWhenEmployeeIsInactiveReturnsActiveVersionAndPreservesOriginal()
    {
        Employee original = CreateEmployee(
                new Guid("d3deddaa-2314-451e-91bb-ebd16b0279ca"),
                "Erika",
                "Beispiel",
                InitialEmployeeTypeCatalog.Type35.Id.Value)
            .Deactivate();

        Employee reactivated = original.Reactivate();

        Assert.Equal(original.Id, reactivated.Id);
        Assert.Equal(original.FirstName, reactivated.FirstName);
        Assert.Equal(original.LastName, reactivated.LastName);
        Assert.Equal(original.EmployeeTypeId, reactivated.EmployeeTypeId);
        Assert.True(reactivated.IsActive);
        Assert.False(original.IsActive);
    }

    [Fact]
    public void PublicEmployeeContractWhenInspectedContainsOnlyIdentityNameStatusAndTypeReference()
    {
        Dictionary<string, Type> properties = typeof(Employee)
            .GetProperties()
            .ToDictionary(property => property.Name, property => property.PropertyType);

        Assert.Equal(6, properties.Count);
        Assert.Equal(typeof(EmployeeId), properties[nameof(Employee.Id)]);
        Assert.Equal(typeof(EmployeeFirstName), properties[nameof(Employee.FirstName)]);
        Assert.Equal(typeof(EmployeeLastName), properties[nameof(Employee.LastName)]);
        Assert.Equal(typeof(string), properties[nameof(Employee.DisplayName)]);
        Assert.Equal(typeof(EmployeeTypeId), properties[nameof(Employee.EmployeeTypeId)]);
        Assert.Equal(typeof(bool), properties[nameof(Employee.IsActive)]);
        Assert.Single(properties.Values, type => type == typeof(EmployeeTypeId));
        Assert.DoesNotContain(typeof(EmployeeType), properties.Values);
        Assert.DoesNotContain(typeof(WeeklyWorkTarget), properties.Values);
        Assert.DoesNotContain(
            properties.Values,
            type => type.IsGenericType
                && type.GetGenericArguments().Contains(typeof(EmployeeTypeShiftEligibility)));
    }

    [Fact]
    public void EmployeeIdWhenValueIsEmptyCannotBeCreated()
    {
        bool wasCreated = EmployeeId.TryCreate(Guid.Empty, out EmployeeId? employeeId);

        Assert.False(wasCreated);
        Assert.Null(employeeId);
    }

    private static Employee CreateEmployee(
        Guid id,
        string firstName,
        string lastName,
        Guid employeeTypeId)
    {
        return Assert.IsType<Employee>(
            Employee.Create(id, firstName, lastName, employeeTypeId).Value);
    }
}
