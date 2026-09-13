using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public sealed class ReliefShiftPattern : IShiftPattern
{
    private ReliefShiftPattern(
        ShiftPatternId id,
        DayOfWeek allowedDay,
        ShiftType firstShift,
        ShiftType secondShift)
    {
        Id = id;
        AllowedDay = allowedDay;
        FirstShiftTypeId = firstShift.Id;
        FirstWorkLocationId = firstShift.WorkLocationId;
        SecondShiftTypeId = secondShift.Id;
        SecondWorkLocationId = secondShift.WorkLocationId;
        DisplayColorCode = "blue";
        SwitchRule = ReliefShiftSwitchRule.EndOfFirstActualDemand;
        HasInterruption = false;
    }

    public ShiftPatternId Id { get; }

    public string DisplayCode => "Spr";

    public string DisplayColorCode { get; }

    public DayOfWeek AllowedDay { get; }

    public ShiftTypeId FirstShiftTypeId { get; }

    public WorkLocationId FirstWorkLocationId { get; }

    public ShiftTypeId SecondShiftTypeId { get; }

    public WorkLocationId SecondWorkLocationId { get; }

    public ReliefShiftSwitchRule SwitchRule { get; }

    public bool HasInterruption { get; }

    public static ReliefShiftPatternValidationResult Create(
        Guid id,
        DayOfWeek allowedDay,
        ShiftType? firstShift,
        ShiftType? secondShift)
    {
        List<ReliefShiftPatternValidationError> errors = [];

        if (!ShiftPatternId.TryCreate(id, out ShiftPatternId? patternId))
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.IdentifierRequired));
        }

        if (allowedDay != DayOfWeek.Saturday)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.SaturdayRequired));
        }

        if (firstShift is null)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.FirstShiftRequired));
        }

        if (secondShift is null)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.SecondShiftRequired));
        }

        if (firstShift is not null && secondShift is not null)
        {
            ValidateShiftTypes(firstShift, secondShift, errors);
            ValidateWorkLocations(firstShift, secondShift, errors);
        }

        if (errors.Count > 0)
        {
            return ReliefShiftPatternValidationResult.Failure(errors);
        }

        return ReliefShiftPatternValidationResult.Success(
            new ReliefShiftPattern(patternId!, allowedDay, firstShift!, secondShift!));
    }

    private static void ValidateShiftTypes(
        ShiftType firstShift,
        ShiftType secondShift,
        List<ReliefShiftPatternValidationError> errors)
    {
        if (firstShift.Id != InitialShiftTypeCatalog.CafeteriaShiftB.Id)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.FirstShiftMustBeCafeteriaShiftB));
        }

        if (secondShift.Id != InitialShiftTypeCatalog.LateShift.Id)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.SecondShiftMustBeLateShift));
        }
    }

    private static void ValidateWorkLocations(
        ShiftType firstShift,
        ShiftType secondShift,
        List<ReliefShiftPatternValidationError> errors)
    {
        if (firstShift.WorkLocationId != InitialWorkLocationCatalog.Cafeteria.Id)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.FirstWorkLocationMustBeCafeteria));
        }

        if (secondShift.WorkLocationId != InitialWorkLocationCatalog.Restaurant.Id)
        {
            errors.Add(new ReliefShiftPatternValidationError(
                ReliefShiftPatternValidationCode.SecondWorkLocationMustBeRestaurant));
        }
    }
}
