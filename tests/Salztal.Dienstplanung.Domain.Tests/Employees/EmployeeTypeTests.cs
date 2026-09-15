using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftTypes;

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
    public void CreateWhenAbsenceIsAllowedWithoutDayValueReturnsRequiredError()
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "TypTest",
            "Testtyp",
            1_500,
            true,
            null,
            [],
            EmployeeTypePlanningPolicy.Standard);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.AbsenceDayValueRequired, error.Code);
    }

    [Fact]
    public void CreateWhenAbsenceIsNotAllowedWithDayValueReturnsMustNotBeSetError()
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "TypTest",
            "Testtyp",
            1_500,
            false,
            300,
            [],
            EmployeeTypePlanningPolicy.Standard);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.AbsenceDayValueMustNotBeSet, error.Code);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CreateWhenAbsenceDayValueIsNotPositiveReturnsError(int minutes)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "TypTest",
            "Testtyp",
            1_500,
            true,
            minutes,
            [],
            EmployeeTypePlanningPolicy.Standard);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.AbsenceDayValueMustBePositive, error.Code);
    }

    [Theory]
    [InlineData(AbsenceDayValue.MaximumMinutes + 1)]
    [InlineData(int.MaxValue)]
    public void CreateWhenAbsenceDayValueExceedsDayReturnsError(int minutes)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "TypTest",
            "Testtyp",
            1_500,
            true,
            minutes,
            [],
            EmployeeTypePlanningPolicy.Standard);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.AbsenceDayValueExceedsDay, error.Code);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(AbsenceDayValue.MaximumMinutes)]
    public void CreateWhenAbsenceDayValueIsAtBoundaryPreservesMinutes(int minutes)
    {
        EmployeeType employeeType = Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                Guid.NewGuid(),
                "TypTest",
                "Testtyp",
                1_500,
                true,
                minutes,
                [],
                EmployeeTypePlanningPolicy.Standard).Value);

        Assert.True(employeeType.AbsencePolicy.AllowsVacationAndSickness);
        Assert.Equal(minutes, employeeType.AbsencePolicy.DayValue?.Minutes);
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
    public void WithDetailsWhenPlanningValuesChangePreservesIdentifierCodeAndRole()
    {
        EmployeeType original = InitialEmployeeTypeCatalog.Type25;
        EmployeeTypeShiftEligibility lateShift = Assert.IsType<EmployeeTypeShiftEligibility>(
            original.ShiftEligibilities.Single(eligibility =>
                eligibility.ShiftTypeId == InitialShiftTypeCatalog.LateShift.Id));

        EmployeeTypeValidationResult result = original.WithDetails(
            "Restaurant angepasst",
            1_560,
            false,
            null,
            [lateShift]);

        EmployeeType changed = Assert.IsType<EmployeeType>(result.Value);
        Assert.Equal(original.Id, changed.Id);
        Assert.Equal(original.Code, changed.Code);
        Assert.Equal(original.PlanningPolicy, changed.PlanningPolicy);
        Assert.Equal(EmployeeTypePlanningRole.Normal, changed.PlanningPolicy.Role);
        Assert.False(changed.AbsencePolicy.AllowsVacationAndSickness);
        Assert.Null(changed.AbsencePolicy.DayValue);
        Assert.Equal([lateShift], changed.ShiftEligibilities);
        Assert.True(original.AbsencePolicy.AllowsVacationAndSickness);
        Assert.Equal(300, original.AbsencePolicy.DayValue?.Minutes);
        Assert.Equal(3, original.ShiftEligibilities.Count);
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

    [Fact]
    public void CreateWhenPlanningDefinitionIsMissingReturnsStructuredErrors()
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "TypTest",
            "Testtyp",
            1_500,
            null,
            null);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                EmployeeTypeValidationCode.ShiftEligibilityRequired,
                error.Code),
            error => Assert.Equal(
                EmployeeTypeValidationCode.PlanningPolicyRequired,
                error.Code));
    }

    [Fact]
    public void CreateWhenEligibilityIsDuplicatedReturnsStructuredError()
    {
        IReadOnlyCollection<ShiftTypeId> knownShiftTypeIds = InitialShiftTypeCatalog.All
            .Select(shiftType => shiftType.Id)
            .ToArray();
        EmployeeTypeShiftEligibility eligibility =
            Assert.IsType<EmployeeTypeShiftEligibility>(
                EmployeeTypeShiftEligibility.CreateForShiftType(
                    InitialShiftTypeCatalog.LateShift.Id.Value,
                    ShiftEligibilityMode.Regular,
                    knownShiftTypeIds).Value);

        EmployeeTypeValidationResult result = EmployeeType.Create(
            Guid.NewGuid(),
            "TypTest",
            "Testtyp",
            1_500,
            [eligibility, eligibility],
            EmployeeTypePlanningPolicy.Standard);

        EmployeeTypeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeValidationCode.DuplicateShiftEligibility, error.Code);
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
