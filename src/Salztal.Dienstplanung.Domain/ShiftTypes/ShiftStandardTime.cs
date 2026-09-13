namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public sealed record ShiftStandardTime
{
    private ShiftStandardTime(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
        DurationMinutes = (int)(end - start).TotalMinutes;
    }

    public TimeOnly Start { get; }

    public TimeOnly End { get; }

    public int DurationMinutes { get; }

    public static ShiftStandardTimeValidationResult Create(TimeOnly start, TimeOnly end)
    {
        List<ShiftStandardTimeValidationError> errors = [];

        ValidateTime(start, true, errors);
        ValidateTime(end, false, errors);

        if (end <= start)
        {
            errors.Add(new ShiftStandardTimeValidationError(
                ShiftStandardTimeValidationCode.EndMustBeAfterStart));
        }

        if (errors.Count > 0)
        {
            return ShiftStandardTimeValidationResult.Failure(errors);
        }

        return ShiftStandardTimeValidationResult.Success(
            new ShiftStandardTime(start, end));
    }

    private static void ValidateTime(
        TimeOnly value,
        bool isStart,
        List<ShiftStandardTimeValidationError> errors)
    {
        if (value.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            errors.Add(new ShiftStandardTimeValidationError(
                isStart
                    ? ShiftStandardTimeValidationCode.StartMustUseWholeMinute
                    : ShiftStandardTimeValidationCode.EndMustUseWholeMinute));
            return;
        }

        if (value.Minute % 30 != 0)
        {
            errors.Add(new ShiftStandardTimeValidationError(
                isStart
                    ? ShiftStandardTimeValidationCode.StartMustUseThirtyMinuteIncrement
                    : ShiftStandardTimeValidationCode.EndMustUseThirtyMinuteIncrement));
        }
    }
}
