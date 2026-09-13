namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public enum SplitShiftPatternValidationCode
{
    IdentifierRequired,
    FirstShiftRequired,
    SecondShiftRequired,
    FirstShiftMustBeEarlyShift,
    SecondShiftMustBeLateShift,
    RestaurantRequired,
    SameWorkLocationRequired,
    SegmentsMustBeInChronologicalOrder,
    SegmentsMustNotOverlap,
    BreakRequired,
}
