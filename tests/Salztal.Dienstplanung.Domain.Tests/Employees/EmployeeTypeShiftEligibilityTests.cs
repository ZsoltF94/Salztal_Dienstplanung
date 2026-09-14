using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Tests.Employees;

public sealed class EmployeeTypeShiftEligibilityTests
{
    private static readonly IReadOnlyCollection<ShiftTypeId> KnownShiftTypeIds =
        InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id).ToArray();

    private static readonly IReadOnlyCollection<ShiftPatternId> KnownShiftPatternIds =
        InitialShiftPatternCatalog.All.Select(pattern => pattern.Id).ToArray();

    [Fact]
    public void CreateForShiftTypeWhenSuggestionIsKnownReturnsIdentifierReference()
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftType(
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                ShiftEligibilityMode.ManualSuggestion,
                KnownShiftTypeIds);

        EmployeeTypeShiftEligibility eligibility =
            Assert.IsType<EmployeeTypeShiftEligibility>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(ShiftEligibilityTargetKind.ShiftType, eligibility.TargetKind);
        Assert.Equal(InitialShiftTypeCatalog.EarlyShift.Id, eligibility.ShiftTypeId);
        Assert.Null(eligibility.ShiftPatternId);
        Assert.Equal(ShiftEligibilityMode.ManualSuggestion, eligibility.Mode);
        Assert.Equal(ShiftEligibilityActivation.Always, eligibility.Activation);
    }

    [Fact]
    public void CreateForShiftPatternWhenOptionIsRequiredReturnsConditionalReference()
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftPattern(
                InitialShiftPatternCatalog.ReliefShift.Id.Value,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.ExplicitPlanningRunOption,
                KnownShiftPatternIds);

        EmployeeTypeShiftEligibility eligibility =
            Assert.IsType<EmployeeTypeShiftEligibility>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(ShiftEligibilityTargetKind.ShiftPattern, eligibility.TargetKind);
        Assert.Null(eligibility.ShiftTypeId);
        Assert.Equal(InitialShiftPatternCatalog.ReliefShift.Id, eligibility.ShiftPatternId);
        Assert.Equal(ShiftEligibilityMode.Regular, eligibility.Mode);
        Assert.Equal(
            ShiftEligibilityActivation.ExplicitPlanningRunOption,
            eligibility.Activation);
    }

    [Fact]
    public void CreateForShiftTypeWhenIdentifierIsEmptyReturnsIdentifierRequired()
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftType(
                Guid.Empty,
                ShiftEligibilityMode.Regular,
                KnownShiftTypeIds);

        EmployeeTypeShiftEligibilityValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            EmployeeTypeShiftEligibilityValidationCode.IdentifierRequired,
            error.Code);
    }

    [Fact]
    public void CreateForShiftTypeWhenIdentifierIsUnknownReturnsUnknownShiftType()
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftType(
                new Guid("33c78e3e-24ee-48b5-b1e8-b53c2be7eb0c"),
                ShiftEligibilityMode.Regular,
                KnownShiftTypeIds);

        EmployeeTypeShiftEligibilityValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            EmployeeTypeShiftEligibilityValidationCode.UnknownShiftType,
            error.Code);
    }

    [Fact]
    public void CreateForShiftPatternWhenIdentifierIsUnknownReturnsUnknownShiftPattern()
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftPattern(
                new Guid("04b1a8c0-132b-4ca2-aa85-e4500500a02e"),
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always,
                KnownShiftPatternIds);

        EmployeeTypeShiftEligibilityValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            EmployeeTypeShiftEligibilityValidationCode.UnknownShiftPattern,
            error.Code);
    }

    [Fact]
    public void CreateForShiftPatternWhenEnumsAreUnknownReturnsStructuredErrors()
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftPattern(
                InitialShiftPatternCatalog.SplitShift.Id.Value,
                (ShiftEligibilityMode)999,
                (ShiftEligibilityActivation)999,
                KnownShiftPatternIds);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                EmployeeTypeShiftEligibilityValidationCode.UnsupportedMode,
                error.Code),
            error => Assert.Equal(
                EmployeeTypeShiftEligibilityValidationCode.UnsupportedActivation,
                error.Code));
    }
}
