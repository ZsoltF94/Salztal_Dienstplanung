using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed class ShiftTypeStandardTimeUpdateData
{
    public ShiftTypeStandardTimeUpdateData(
        ShiftType current,
        ShiftType earlyShift,
        ShiftType lateShift,
        SplitShiftPattern splitShiftPattern)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(earlyShift);
        ArgumentNullException.ThrowIfNull(lateShift);
        ArgumentNullException.ThrowIfNull(splitShiftPattern);

        Current = current;
        EarlyShift = earlyShift;
        LateShift = lateShift;
        SplitShiftPattern = splitShiftPattern;
    }

    public ShiftType Current { get; }

    public ShiftType EarlyShift { get; }

    public ShiftType LateShift { get; }

    public SplitShiftPattern SplitShiftPattern { get; }
}
