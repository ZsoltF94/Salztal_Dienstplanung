using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Tests.Employees;

public sealed class EmployeeTypeTests
{
    [Fact]
    public void CreateWhenValuesAreValidReturnsNormalizedEmployeeType()
    {
        Guid id = new("17b274f1-2c77-44d2-93ef-f61d04ad6539");

        EmployeeTypeValidationResult result = EmployeeType.Create(
            id,
            "  Typ30a  ",
            "  Vollzeit flexibel  ",
            1_801);

        EmployeeType employeeType = Assert.IsType<EmployeeType>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(id, employeeType.Id.Value);
        Assert.Equal("Typ30a", employeeType.Code.Value);
        Assert.Equal("Vollzeit flexibel", employeeType.Name.Value);
        Assert.Equal(1_801, employeeType.WeeklyWorkTarget.Minutes);
    }

    [Fact]
    public void CreateWhenAllValuesAreInvalidReturnsAllValidationCodes()
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.Empty,
            " ",
            null,
            0);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                EmployeeTypeValidationCode.IdentifierRequired,
                error.Code),
            error => Assert.Equal(
                EmployeeTypeValidationCode.CodeRequired,
                error.Code),
            error => Assert.Equal(
                EmployeeTypeValidationCode.NameRequired,
                error.Code),
            error => Assert.Equal(
                EmployeeTypeValidationCode.WeeklyWorkTargetMustBePositive,
                error.Code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWhenCodeIsMissingReturnsCodeRequired(string? code)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            code,
            "Teilzeit",
            1_500);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.CodeRequired, error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWhenNameIsMissingReturnsNameRequired(string? name)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "Typ25",
            name,
            1_500);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.NameRequired, error.Code);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CreateWhenWeeklyTargetIsNotPositiveReturnsError(int minutes)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "Typ25",
            "Teilzeit",
            minutes);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            EmployeeTypeValidationCode.WeeklyWorkTargetMustBePositive,
            error.Code);
    }

    [Theory]
    [InlineData(10_081)]
    [InlineData(int.MaxValue)]
    public void CreateWhenWeeklyTargetExceedsWeekReturnsError(int minutes)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "Typ25",
            "Teilzeit",
            minutes);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            EmployeeTypeValidationCode.WeeklyWorkTargetExceedsWeek,
            error.Code);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(WeeklyWorkTarget.MaximumMinutes)]
    public void CreateWhenWeeklyTargetIsAtBoundaryPreservesMinutes(int minutes)
    {
        EmployeeType employeeType = Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                Guid.NewGuid(),
                "TypGrenze",
                "Grenzwert",
                minutes).Value);

        Assert.Equal(minutes, employeeType.WeeklyWorkTarget.Minutes);
    }

    [Fact]
    public void EqualBasicValuesWhenCreatedSeparatelyCompareEqual()
    {
        Guid id = new("85243ccb-743f-4544-9d3c-b2b252fcb229");

        EmployeeType first = CreateEmployeeType(id, "Typ35", "Vollzeit", 2_100);
        EmployeeType second = CreateEmployeeType(id, "Typ35", "Vollzeit", 2_100);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.Code, second.Code);
        Assert.Equal(first.Name, second.Name);
        Assert.Equal(first.WeeklyWorkTarget, second.WeeklyWorkTarget);
    }

    [Fact]
    public void WithDetailsWhenValuesAreValidPreservesIdentifierAndOriginal()
    {
        EmployeeType original = CreateEmployeeType(
            new Guid("0b293bc8-4f4f-4273-bc35-bad998fc1ba9"),
            "Typ25",
            "Teilzeit",
            1_500);

        EmployeeTypeValidationResult result = original.WithDetails(
            "Teilzeit Restaurant",
            1_560);

        EmployeeType changed = Assert.IsType<EmployeeType>(result.Value);
        Assert.Equal(original.Id, changed.Id);
        Assert.Equal(original.Code, changed.Code);
        Assert.Equal("Teilzeit Restaurant", changed.Name.Value);
        Assert.Equal(1_560, changed.WeeklyWorkTarget.Minutes);
        Assert.Equal("Teilzeit", original.Name.Value);
        Assert.Equal(1_500, original.WeeklyWorkTarget.Minutes);
    }

    [Fact]
    public void EmployeeTypeIdWhenValueIsEmptyCannotBeCreated()
    {
        bool wasCreated = EmployeeTypeId.TryCreate(
            Guid.Empty,
            out EmployeeTypeId? employeeTypeId);

        Assert.False(wasCreated);
        Assert.Null(employeeTypeId);
    }

    private static EmployeeType CreateEmployeeType(
        Guid id,
        string code,
        string name,
        int weeklyWorkTargetMinutes)
    {
        return Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                id,
                code,
                name,
                weeklyWorkTargetMinutes).Value);
    }
}
