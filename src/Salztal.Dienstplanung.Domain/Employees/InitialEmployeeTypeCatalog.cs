using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Employees;

public static class InitialEmployeeTypeCatalog
{
    private static readonly ReadOnlyCollection<ShiftTypeId> KnownShiftTypeIds =
        Array.AsReadOnly(InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id).ToArray());

    private static readonly ReadOnlyCollection<ShiftPatternId> KnownShiftPatternIds =
        Array.AsReadOnly(InitialShiftPatternCatalog.All.Select(pattern => pattern.Id).ToArray());

    private static readonly ReadOnlyCollection<EmployeeType> InitialEmployeeTypes =
        Array.AsReadOnly(
            new[]
            {
                CreateInitial(
                    new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"),
                    "Typ1",
                    "Serviceleitung",
                    40 * 60,
                    true,
                    8 * 60,
                    EmployeeTypePlanningPolicy.ServiceManagement,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftA),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id),
                    Pattern(InitialShiftPatternCatalog.ReliefShift.Id),
                    Shift(
                        InitialShiftTypeCatalog.EarlyShift,
                        ShiftEligibilityMode.ManualSuggestion),
                    Shift(
                        InitialShiftTypeCatalog.LateShift,
                        ShiftEligibilityMode.ManualSuggestion),
                    Shift(
                        InitialShiftTypeCatalog.CafeteriaShiftA,
                        ShiftEligibilityMode.ManualSuggestion),
                    Shift(
                        InitialShiftTypeCatalog.CafeteriaShiftB,
                        ShiftEligibilityMode.ManualSuggestion)),
                CreateInitial(
                    new Guid("71cc48ce-172a-4580-a6b2-e1acc77b94f6"),
                    "Typ20",
                    "Restaurant - 20 Stunden",
                    20 * 60,
                    true,
                    4 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id)),
                CreateInitial(
                    new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"),
                    "Typ20a",
                    "Alle Dienste - 20 Stunden",
                    20 * 60,
                    true,
                    4 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftA),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id),
                    Pattern(InitialShiftPatternCatalog.ReliefShift.Id)),
                CreateInitial(
                    new Guid("b75fed95-1c2f-439f-9696-217bed8c4d8f"),
                    "Typ25",
                    "Restaurant - 25 Stunden",
                    25 * 60,
                    true,
                    5 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id)),
                CreateInitial(
                    new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"),
                    "Typ25a",
                    "Alle Dienste - 25 Stunden",
                    25 * 60,
                    true,
                    5 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftA),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id),
                    Pattern(InitialShiftPatternCatalog.ReliefShift.Id)),
                CreateInitial(
                    new Guid("6f0feaed-65eb-4568-9c1f-a130cca65e44"),
                    "Typ30",
                    "Restaurant - 30 Stunden",
                    30 * 60,
                    true,
                    6 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id)),
                CreateInitial(
                    new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"),
                    "Typ30a",
                    "Alle Dienste - 30 Stunden",
                    30 * 60,
                    true,
                    6 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftA),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id),
                    Pattern(InitialShiftPatternCatalog.ReliefShift.Id)),
                CreateInitial(
                    new Guid("554d2c92-d67e-439c-9bc4-7bbe564198b5"),
                    "Typ35",
                    "Restaurant - 35 Stunden",
                    35 * 60,
                    true,
                    7 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id)),
                CreateInitial(
                    new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"),
                    "Typ35a",
                    "Alle Dienste - 35 Stunden",
                    35 * 60,
                    true,
                    7 * 60,
                    EmployeeTypePlanningPolicy.Standard,
                    Shift(InitialShiftTypeCatalog.EarlyShift),
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftA),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id),
                    Pattern(InitialShiftPatternCatalog.ReliefShift.Id)),
                CreateInitial(
                    new Guid("86a88720-bcdb-4f56-9dc8-0acf702f00f7"),
                    "TypAH1",
                    "Restaurant-Spätdienst - 10 Stunden",
                    10 * 60,
                    false,
                    null,
                    EmployeeTypePlanningPolicy.Auxiliary,
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(
                        InitialShiftTypeCatalog.EarlyShift,
                        ShiftEligibilityMode.ManualSuggestion)),
                CreateInitial(
                    new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"),
                    "TypAH2",
                    "Restaurant, Cafeteria B und Doppeldienst - 10 Stunden",
                    10 * 60,
                    false,
                    null,
                    EmployeeTypePlanningPolicy.Auxiliary,
                    Shift(InitialShiftTypeCatalog.LateShift),
                    Shift(InitialShiftTypeCatalog.CafeteriaShiftB),
                    Pattern(InitialShiftPatternCatalog.SplitShift.Id),
                    Shift(
                        InitialShiftTypeCatalog.EarlyShift,
                        ShiftEligibilityMode.ManualSuggestion),
                    Pattern(
                        InitialShiftPatternCatalog.ReliefShift.Id,
                        ShiftEligibilityActivation.ExplicitPlanningRunOption)),
            });

    public static EmployeeType Type1 => InitialEmployeeTypes[0];

    public static EmployeeType Type20 => InitialEmployeeTypes[1];

    public static EmployeeType Type20a => InitialEmployeeTypes[2];

    public static EmployeeType Type25 => InitialEmployeeTypes[3];

    public static EmployeeType Type25a => InitialEmployeeTypes[4];

    public static EmployeeType Type30 => InitialEmployeeTypes[5];

    public static EmployeeType Type30a => InitialEmployeeTypes[6];

    public static EmployeeType Type35 => InitialEmployeeTypes[7];

    public static EmployeeType Type35a => InitialEmployeeTypes[8];

    public static EmployeeType TypeAh1 => InitialEmployeeTypes[9];

    public static EmployeeType TypeAh2 => InitialEmployeeTypes[10];

    public static IReadOnlyList<EmployeeType> All => InitialEmployeeTypes;

    private static EmployeeTypeShiftEligibility Shift(
        ShiftType shiftType,
        ShiftEligibilityMode mode = ShiftEligibilityMode.Regular)
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftType(
                shiftType.Id.Value,
                mode,
                KnownShiftTypeIds);

        return result.Value
            ?? throw new InvalidOperationException("An initial shift eligibility is invalid.");
    }

    private static EmployeeTypeShiftEligibility Pattern(
        ShiftPatternId shiftPatternId,
        ShiftEligibilityActivation activation = ShiftEligibilityActivation.Always)
    {
        EmployeeTypeShiftEligibilityValidationResult result =
            EmployeeTypeShiftEligibility.CreateForShiftPattern(
                shiftPatternId.Value,
                ShiftEligibilityMode.Regular,
                activation,
                KnownShiftPatternIds);

        return result.Value
            ?? throw new InvalidOperationException("An initial shift-pattern eligibility is invalid.");
    }

    private static EmployeeType CreateInitial(
        Guid id,
        string code,
        string name,
        int weeklyWorkTargetMinutes,
        bool allowsVacationAndSickness,
        int? absenceDayValueMinutes,
        EmployeeTypePlanningPolicy planningPolicy,
        params EmployeeTypeShiftEligibility[] shiftEligibilities)
    {
        EmployeeTypeValidationResult result = EmployeeType.Create(
            id,
            code,
            name,
            weeklyWorkTargetMinutes,
            allowsVacationAndSickness,
            absenceDayValueMinutes,
            shiftEligibilities,
            planningPolicy);

        return result.Value
            ?? throw new InvalidOperationException("An initial employee type is invalid.");
    }
}
