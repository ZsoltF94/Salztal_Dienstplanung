using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Tests.Employees;

public sealed class InitialEmployeeTypeCatalogTests
{
    private static readonly Guid EarlyShiftId = InitialShiftTypeCatalog.EarlyShift.Id.Value;
    private static readonly Guid LateShiftId = InitialShiftTypeCatalog.LateShift.Id.Value;
    private static readonly Guid CafeteriaShiftAId =
        InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value;
    private static readonly Guid CafeteriaShiftBId =
        InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value;
    private static readonly Guid SplitShiftId = InitialShiftPatternCatalog.SplitShift.Id.Value;
    private static readonly Guid ReliefShiftId = InitialShiftPatternCatalog.ReliefShift.Id.Value;

    [Fact]
    public void AllWhenReadContainsConfirmedEmployeeTypeMatrix()
    {
        Assert.Collection(
            InitialEmployeeTypeCatalog.All,
            type1 => AssertEmployeeType(
                type1,
                "6197e678-38c8-465c-8d09-fed6812a6f2b",
                "Typ1",
                "Serviceleitung",
                40 * 60,
                true,
                8 * 60,
                [EarlyShiftId, LateShiftId, CafeteriaShiftAId, CafeteriaShiftBId],
                [SplitShiftId, ReliefShiftId],
                [EarlyShiftId, LateShiftId, CafeteriaShiftAId, CafeteriaShiftBId],
                [],
                EmployeeTypePlanningPolicy.ServiceManagement),
            type20 => AssertEmployeeType(
                type20,
                "71cc48ce-172a-4580-a6b2-e1acc77b94f6",
                "Typ20",
                "Restaurant - 20 Stunden",
                20 * 60,
                true,
                4 * 60,
                [EarlyShiftId, LateShiftId],
                [SplitShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type20a => AssertEmployeeType(
                type20a,
                "a06b4fcc-dbf1-4b3a-b975-07dc2d488157",
                "Typ20a",
                "Alle Dienste - 20 Stunden",
                20 * 60,
                true,
                4 * 60,
                [EarlyShiftId, LateShiftId, CafeteriaShiftAId, CafeteriaShiftBId],
                [SplitShiftId, ReliefShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type25 => AssertEmployeeType(
                type25,
                "b75fed95-1c2f-439f-9696-217bed8c4d8f",
                "Typ25",
                "Restaurant - 25 Stunden",
                25 * 60,
                true,
                5 * 60,
                [EarlyShiftId, LateShiftId],
                [SplitShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type25a => AssertEmployeeType(
                type25a,
                "3c553205-5413-4f0c-ad1c-b4037066470c",
                "Typ25a",
                "Alle Dienste - 25 Stunden",
                25 * 60,
                true,
                5 * 60,
                [EarlyShiftId, LateShiftId, CafeteriaShiftAId, CafeteriaShiftBId],
                [SplitShiftId, ReliefShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type30 => AssertEmployeeType(
                type30,
                "6f0feaed-65eb-4568-9c1f-a130cca65e44",
                "Typ30",
                "Restaurant - 30 Stunden",
                30 * 60,
                true,
                6 * 60,
                [EarlyShiftId, LateShiftId],
                [SplitShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type30a => AssertEmployeeType(
                type30a,
                "cc8921a3-83f0-419f-a39b-f8bb37c3d6ba",
                "Typ30a",
                "Alle Dienste - 30 Stunden",
                30 * 60,
                true,
                6 * 60,
                [EarlyShiftId, LateShiftId, CafeteriaShiftAId, CafeteriaShiftBId],
                [SplitShiftId, ReliefShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type35 => AssertEmployeeType(
                type35,
                "554d2c92-d67e-439c-9bc4-7bbe564198b5",
                "Typ35",
                "Restaurant - 35 Stunden",
                35 * 60,
                true,
                7 * 60,
                [EarlyShiftId, LateShiftId],
                [SplitShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            type35a => AssertEmployeeType(
                type35a,
                "e12d71ce-5dca-45b3-bae5-eadebaad93dd",
                "Typ35a",
                "Alle Dienste - 35 Stunden",
                35 * 60,
                true,
                7 * 60,
                [EarlyShiftId, LateShiftId, CafeteriaShiftAId, CafeteriaShiftBId],
                [SplitShiftId, ReliefShiftId],
                [],
                [],
                EmployeeTypePlanningPolicy.Standard),
            typeAh1 => AssertEmployeeType(
                typeAh1,
                "86a88720-bcdb-4f56-9dc8-0acf702f00f7",
                "TypAH1",
                "Restaurant-Spätdienst - 10 Stunden",
                10 * 60,
                false,
                null,
                [LateShiftId],
                [],
                [EarlyShiftId],
                [],
                EmployeeTypePlanningPolicy.Auxiliary),
            typeAh2 => AssertEmployeeType(
                typeAh2,
                "b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6",
                "TypAH2",
                "Restaurant, Cafeteria B und Doppeldienst - 10 Stunden",
                10 * 60,
                false,
                null,
                [LateShiftId, CafeteriaShiftBId],
                [SplitShiftId],
                [EarlyShiftId],
                [ReliefShiftId],
                EmployeeTypePlanningPolicy.Auxiliary));
    }

    [Fact]
    public void AllWhenReadContainsUniqueStableIdentifiersAndCodes()
    {
        Assert.Equal(
            InitialEmployeeTypeCatalog.All.Count,
            InitialEmployeeTypeCatalog.All.Select(type => type.Id).Distinct().Count());
        Assert.Equal(
            InitialEmployeeTypeCatalog.All.Count,
            InitialEmployeeTypeCatalog.All.Select(type => type.Code).Distinct().Count());
        Assert.DoesNotContain(
            InitialEmployeeTypeCatalog.All,
            type => type.Id.Value == Guid.Empty);
    }

    [Fact]
    public void AllEligibilitiesWhenReadReferenceOnlySystem04Identifiers()
    {
        IReadOnlySet<ShiftTypeId> knownShiftTypeIds = InitialShiftTypeCatalog.All
            .Select(shiftType => shiftType.Id)
            .ToHashSet();
        IReadOnlySet<ShiftPatternId> knownShiftPatternIds = InitialShiftPatternCatalog.All
            .Select(pattern => pattern.Id)
            .ToHashSet();

        foreach (EmployeeTypeShiftEligibility eligibility in InitialEmployeeTypeCatalog.All
                     .SelectMany(employeeType => employeeType.ShiftEligibilities))
        {
            if (eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType)
            {
                Assert.NotNull(eligibility.ShiftTypeId);
                Assert.Null(eligibility.ShiftPatternId);
                Assert.Contains(eligibility.ShiftTypeId, knownShiftTypeIds);
            }
            else
            {
                Assert.Null(eligibility.ShiftTypeId);
                Assert.NotNull(eligibility.ShiftPatternId);
                Assert.Contains(eligibility.ShiftPatternId, knownShiftPatternIds);
            }
        }
    }

    [Fact]
    public void ShiftEligibilityContractWhenInspectedContainsOnlyIdentifiersAndMetadata()
    {
        Dictionary<string, Type> properties =
            typeof(EmployeeTypeShiftEligibility)
                .GetProperties()
                .ToDictionary(property => property.Name, property => property.PropertyType);

        Assert.Equal(5, properties.Count);
        Assert.Equal(typeof(ShiftEligibilityTargetKind), properties[nameof(
            EmployeeTypeShiftEligibility.TargetKind)]);
        Assert.Equal(typeof(ShiftTypeId), properties[nameof(
            EmployeeTypeShiftEligibility.ShiftTypeId)]);
        Assert.Equal(typeof(ShiftPatternId), properties[nameof(
            EmployeeTypeShiftEligibility.ShiftPatternId)]);
        Assert.Equal(typeof(ShiftEligibilityMode), properties[nameof(
            EmployeeTypeShiftEligibility.Mode)]);
        Assert.Equal(typeof(ShiftEligibilityActivation), properties[nameof(
            EmployeeTypeShiftEligibility.Activation)]);
    }

    [Fact]
    public void Type1WhenReadCarriesManualServiceManagementPolicy()
    {
        EmployeeTypePlanningPolicy policy = InitialEmployeeTypeCatalog.Type1.PlanningPolicy;

        Assert.False(policy.AllowsAutomaticAssignment);
        Assert.True(policy.RequiresWeeklyManualAssignment);
        Assert.True(policy.PreservesManualAssignmentsOnGeneration);
        Assert.Equal(ManualSuggestionPriority.LastResort, policy.ManualSuggestionPriority);
        Assert.Equal(EmployeeTypePlanningRole.ServiceManagement, policy.Role);
    }

    [Fact]
    public void AuxiliaryTypesWhenReadCarryProtectedAuxiliaryRole()
    {
        Assert.Equal(
            EmployeeTypePlanningRole.Auxiliary,
            InitialEmployeeTypeCatalog.TypeAh1.PlanningPolicy.Role);
        Assert.Equal(
            EmployeeTypePlanningRole.Auxiliary,
            InitialEmployeeTypeCatalog.TypeAh2.PlanningPolicy.Role);
    }

    [Fact]
    public void InitialEmployeeTypeWhenDetailsChangeLeavesCatalogValueUnchanged()
    {
        EmployeeTypeValidationResult result = InitialEmployeeTypeCatalog.Type25.WithDetails(
            "Restaurant - 25 Stunden geändert",
            1_560);

        EmployeeType changed = Assert.IsType<EmployeeType>(result.Value);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Id, changed.Id);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Code, changed.Code);
        Assert.Equal(
            InitialEmployeeTypeCatalog.Type25.ShiftEligibilities,
            changed.ShiftEligibilities);
        Assert.Equal(
            InitialEmployeeTypeCatalog.Type25.PlanningPolicy,
            changed.PlanningPolicy);
        Assert.Equal(
            InitialEmployeeTypeCatalog.Type25.AbsencePolicy,
            changed.AbsencePolicy);
        Assert.Equal("Restaurant - 25 Stunden geändert", changed.Name.Value);
        Assert.Equal(1_560, changed.WeeklyWorkTarget.Minutes);
        Assert.Equal(
            "Restaurant - 25 Stunden",
            InitialEmployeeTypeCatalog.Type25.Name.Value);
        Assert.Equal(1_500, InitialEmployeeTypeCatalog.Type25.WeeklyWorkTarget.Minutes);
    }

    private static void AssertEmployeeType(
        EmployeeType employeeType,
        string expectedId,
        string expectedCode,
        string expectedName,
        int expectedWeeklyMinutes,
        bool expectedAllowsVacationAndSickness,
        int? expectedAbsenceDayValueMinutes,
        Guid[] expectedRegularShiftTypeIds,
        Guid[] expectedRegularShiftPatternIds,
        Guid[] expectedManualSuggestionShiftTypeIds,
        Guid[] expectedOptionShiftPatternIds,
        EmployeeTypePlanningPolicy expectedPlanningPolicy)
    {
        Assert.Equal(new Guid(expectedId), employeeType.Id.Value);
        Assert.Equal(expectedCode, employeeType.Code.Value);
        Assert.Equal(expectedName, employeeType.Name.Value);
        Assert.Equal(expectedWeeklyMinutes, employeeType.WeeklyWorkTarget.Minutes);
        Assert.Equal(
            expectedAllowsVacationAndSickness,
            employeeType.AbsencePolicy.AllowsVacationAndSickness);
        Assert.Equal(
            expectedAbsenceDayValueMinutes,
            employeeType.AbsencePolicy.DayValue?.Minutes);
        Assert.Equal(expectedPlanningPolicy, employeeType.PlanningPolicy);

        Assert.Equal(
            expectedRegularShiftTypeIds,
            SelectShiftTypeIds(
                employeeType,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always));
        Assert.Equal(
            expectedRegularShiftPatternIds,
            SelectShiftPatternIds(
                employeeType,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.Always));
        Assert.Equal(
            expectedManualSuggestionShiftTypeIds,
            SelectShiftTypeIds(
                employeeType,
                ShiftEligibilityMode.ManualSuggestion,
                ShiftEligibilityActivation.Always));
        Assert.Equal(
            expectedOptionShiftPatternIds,
            SelectShiftPatternIds(
                employeeType,
                ShiftEligibilityMode.Regular,
                ShiftEligibilityActivation.ExplicitPlanningRunOption));
    }

    private static Guid[] SelectShiftTypeIds(
        EmployeeType employeeType,
        ShiftEligibilityMode mode,
        ShiftEligibilityActivation activation)
    {
        return employeeType.ShiftEligibilities
            .Where(eligibility => eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType)
            .Where(eligibility => eligibility.Mode == mode)
            .Where(eligibility => eligibility.Activation == activation)
            .Select(eligibility => eligibility.ShiftTypeId!.Value)
            .ToArray();
    }

    private static Guid[] SelectShiftPatternIds(
        EmployeeType employeeType,
        ShiftEligibilityMode mode,
        ShiftEligibilityActivation activation)
    {
        return employeeType.ShiftEligibilities
            .Where(eligibility => eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftPattern)
            .Where(eligibility => eligibility.Mode == mode)
            .Where(eligibility => eligibility.Activation == activation)
            .Select(eligibility => eligibility.ShiftPatternId!.Value)
            .ToArray();
    }
}
