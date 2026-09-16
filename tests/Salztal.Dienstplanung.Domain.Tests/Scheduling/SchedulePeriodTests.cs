using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling;

public sealed class SchedulePeriodTests
{
    [Fact]
    public void CreateForMondayReturnsExactlyThreeCompleteWeeks()
    {
        DateOnly monday = new(2026, 12, 21);

        SchedulePeriod period = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(monday).Value);

        Assert.Equal(monday, period.StartMonday);
        Assert.Equal(new DateOnly(2027, 1, 10), period.EndSunday);
        Assert.Equal(DayOfWeek.Sunday, period.EndSunday.DayOfWeek);
        Assert.True(period.Contains(monday));
        Assert.True(period.Contains(period.EndSunday));
        Assert.False(period.Contains(monday.AddDays(-1)));
        Assert.False(period.Contains(period.EndSunday.AddDays(1)));
    }

    [Fact]
    public void CreateForNonMondayReturnsStructuredError()
    {
        SchedulePeriodValidationResult result = SchedulePeriod.Create(
            new DateOnly(2026, 9, 15));

        SchedulePeriodValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(SchedulePeriodValidationCode.StartMustBeMonday, error.Code);
    }

    [Fact]
    public void CreateWhenTwentyOneDaysDoNotFitReturnsStructuredError()
    {
        DateOnly latestMonday = FindLatestMonday();

        SchedulePeriodValidationResult result = SchedulePeriod.Create(
            latestMonday.AddDays(7));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == SchedulePeriodValidationCode.PeriodMustFitTwentyOneDays);
    }

    [Fact]
    public void CreateForLatestFittingMondaySucceeds()
    {
        DateOnly latestMonday = FindLatestMonday();

        SchedulePeriod period = Assert.IsType<SchedulePeriod>(
            SchedulePeriod.Create(latestMonday).Value);

        Assert.Equal(DayOfWeek.Monday, period.StartMonday.DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, period.EndSunday.DayOfWeek);
        Assert.True(period.EndSunday <= DateOnly.MaxValue);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(7, true)]
    [InlineData(14, true)]
    [InlineData(21, false)]
    [InlineData(-21, false)]
    public void OverlapsUsesInclusiveCalendarDayBoundaries(
        int otherStartOffsetDays,
        bool expected)
    {
        SchedulePeriod period = CreatePeriod(new DateOnly(2026, 9, 14));
        SchedulePeriod other = CreatePeriod(
            period.StartMonday.AddDays(otherStartOffsetDays));

        Assert.Equal(expected, period.Overlaps(other));
        Assert.Equal(expected, other.Overlaps(period));
    }

    [Fact]
    public void EqualPeriodsHaveValueEquality()
    {
        DateOnly monday = new(2026, 9, 14);

        Assert.Equal(CreatePeriod(monday), CreatePeriod(monday));
    }

    private static SchedulePeriod CreatePeriod(DateOnly monday)
    {
        return Assert.IsType<SchedulePeriod>(SchedulePeriod.Create(monday).Value);
    }

    private static DateOnly FindLatestMonday()
    {
        DateOnly date = DateOnly.MaxValue.AddDays(-(SchedulePeriod.DayCount - 1));

        while (date.DayOfWeek != DayOfWeek.Monday)
        {
            date = date.AddDays(-1);
        }

        return date;
    }
}
