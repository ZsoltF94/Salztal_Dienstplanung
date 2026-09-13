namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public enum ReliefShiftPatternValidationCode
{
    IdentifierRequired,
    SaturdayRequired,
    FirstShiftRequired,
    SecondShiftRequired,
    FirstShiftMustBeCafeteriaShiftB,
    SecondShiftMustBeLateShift,
    FirstWorkLocationMustBeCafeteria,
    SecondWorkLocationMustBeRestaurant,
}
