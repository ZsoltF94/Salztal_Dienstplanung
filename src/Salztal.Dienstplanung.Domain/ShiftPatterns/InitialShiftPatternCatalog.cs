using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public static class InitialShiftPatternCatalog
{
    private static readonly ReadOnlyCollection<IShiftPattern> InitialPatterns = Array.AsReadOnly(
        new IShiftPattern[]
        {
            CreateSplitShift(),
            CreateReliefShift(),
        });

    public static SplitShiftPattern SplitShift => (SplitShiftPattern)InitialPatterns[0];

    public static ReliefShiftPattern ReliefShift => (ReliefShiftPattern)InitialPatterns[1];

    public static IReadOnlyList<IShiftPattern> All => InitialPatterns;

    private static SplitShiftPattern CreateSplitShift()
    {
        SplitShiftPatternValidationResult result = SplitShiftPattern.Create(
            new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"),
            InitialShiftTypeCatalog.EarlyShift,
            InitialShiftTypeCatalog.LateShift);

        return result.Value
            ?? throw new InvalidOperationException("The initial split-shift pattern is invalid.");
    }

    private static ReliefShiftPattern CreateReliefShift()
    {
        ReliefShiftPatternValidationResult result = ReliefShiftPattern.Create(
            new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"),
            DayOfWeek.Saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            InitialShiftTypeCatalog.LateShift);

        return result.Value
            ?? throw new InvalidOperationException("The initial relief-shift pattern is invalid.");
    }
}
