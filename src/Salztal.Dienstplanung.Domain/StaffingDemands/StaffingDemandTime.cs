using System.Diagnostics.CodeAnalysis;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StaffingDemandTime
{
    private StaffingDemandTime(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
        DurationMinutes = (int)(end - start).TotalMinutes;
    }

    public TimeOnly Start { get; }

    public TimeOnly End { get; }

    public int DurationMinutes { get; }

    internal static IReadOnlyList<StaffingDemandTimeValidationCode> TryCreate(
        TimeOnly start,
        TimeOnly end,
        [NotNullWhen(true)] out StaffingDemandTime? staffingDemandTime)
    {
        List<StaffingDemandTimeValidationCode> errors = [];

        ValidateTime(start, true, errors);
        ValidateTime(end, false, errors);

        if (end <= start)
        {
            errors.Add(StaffingDemandTimeValidationCode.EndMustBeAfterStart);
        }

        if (errors.Count > 0)
        {
            staffingDemandTime = null;
            return Array.AsReadOnly(errors.ToArray());
        }

        staffingDemandTime = new StaffingDemandTime(start, end);
        return Array.AsReadOnly(Array.Empty<StaffingDemandTimeValidationCode>());
    }

    private static void ValidateTime(
        TimeOnly value,
        bool isStart,
        List<StaffingDemandTimeValidationCode> errors)
    {
        if (value.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            errors.Add(
                isStart
                    ? StaffingDemandTimeValidationCode.StartMustUseWholeMinute
                    : StaffingDemandTimeValidationCode.EndMustUseWholeMinute);
            return;
        }

        if (value.Minute % 30 != 0)
        {
            errors.Add(
                isStart
                    ? StaffingDemandTimeValidationCode.StartMustUseThirtyMinuteIncrement
                    : StaffingDemandTimeValidationCode.EndMustUseThirtyMinuteIncrement);
        }
    }
}
