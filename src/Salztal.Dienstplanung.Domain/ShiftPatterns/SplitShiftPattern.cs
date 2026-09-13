using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.ShiftPatterns;

public sealed class SplitShiftPattern : IShiftPattern
{
    private SplitShiftPattern(
        ShiftPatternId id,
        ShiftType firstShift,
        ShiftType secondShift)
    {
        Id = id;
        FirstShiftTypeId = firstShift.Id;
        SecondShiftTypeId = secondShift.Id;
        WorkLocationId = firstShift.WorkLocationId;
        StandardBreakMinutes = (int)(
            secondShift.StandardTime.Start - firstShift.StandardTime.End).TotalMinutes;
        StandardWorkMinutes =
            firstShift.StandardTime.DurationMinutes + secondShift.StandardTime.DurationMinutes;
    }

    public ShiftPatternId Id { get; }

    public string DisplayCode => "D";

    public ShiftTypeId FirstShiftTypeId { get; }

    public ShiftTypeId SecondShiftTypeId { get; }

    public WorkLocationId WorkLocationId { get; }

    public int StandardBreakMinutes { get; }

    public int StandardWorkMinutes { get; }

    public static SplitShiftPatternValidationResult Create(
        Guid id,
        ShiftType? firstShift,
        ShiftType? secondShift)
    {
        List<SplitShiftPatternValidationError> errors = [];

        if (!ShiftPatternId.TryCreate(id, out ShiftPatternId? patternId))
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.IdentifierRequired));
        }

        if (firstShift is null)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.FirstShiftRequired));
        }

        if (secondShift is null)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.SecondShiftRequired));
        }

        if (firstShift is not null && secondShift is not null)
        {
            ValidateShiftTypes(firstShift, secondShift, errors);
            ValidateWorkLocations(firstShift, secondShift, errors);
            ValidateStandardTimes(firstShift, secondShift, errors);
        }

        if (errors.Count > 0)
        {
            return SplitShiftPatternValidationResult.Failure(errors);
        }

        return SplitShiftPatternValidationResult.Success(
            new SplitShiftPattern(patternId!, firstShift!, secondShift!));
    }

    private static void ValidateShiftTypes(
        ShiftType firstShift,
        ShiftType secondShift,
        List<SplitShiftPatternValidationError> errors)
    {
        if (firstShift.Id != InitialShiftTypeCatalog.EarlyShift.Id)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.FirstShiftMustBeEarlyShift));
        }

        if (secondShift.Id != InitialShiftTypeCatalog.LateShift.Id)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.SecondShiftMustBeLateShift));
        }
    }

    private static void ValidateWorkLocations(
        ShiftType firstShift,
        ShiftType secondShift,
        List<SplitShiftPatternValidationError> errors)
    {
        if (firstShift.WorkLocationId != secondShift.WorkLocationId)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.SameWorkLocationRequired));
        }

        if (firstShift.WorkLocationId != InitialWorkLocationCatalog.Restaurant.Id
            || secondShift.WorkLocationId != InitialWorkLocationCatalog.Restaurant.Id)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.RestaurantRequired));
        }
    }

    private static void ValidateStandardTimes(
        ShiftType firstShift,
        ShiftType secondShift,
        List<SplitShiftPatternValidationError> errors)
    {
        if (firstShift.StandardTime.Start >= secondShift.StandardTime.Start)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.SegmentsMustBeInChronologicalOrder));
            return;
        }

        if (firstShift.StandardTime.End > secondShift.StandardTime.Start)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.SegmentsMustNotOverlap));
            return;
        }

        if (firstShift.StandardTime.End == secondShift.StandardTime.Start)
        {
            errors.Add(new SplitShiftPatternValidationError(
                SplitShiftPatternValidationCode.BreakRequired));
        }
    }
}
