using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Tests.ShiftTypes;

public sealed class ShiftStandardTimeTests
{
    [Fact]
    public void CreateWhenRangeIsValidReturnsMinutePreciseDuration()
    {
        ShiftStandardTimeValidationResult result = ShiftStandardTime.Create(
            new TimeOnly(6, 30),
            new TimeOnly(13, 30));

        ShiftStandardTime standardTime = Assert.IsType<ShiftStandardTime>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(new TimeOnly(6, 30), standardTime.Start);
        Assert.Equal(new TimeOnly(13, 30), standardTime.End);
        Assert.Equal(420, standardTime.DurationMinutes);
    }

    [Theory]
    [InlineData(6, 15, ShiftStandardTimeValidationCode.StartMustUseThirtyMinuteIncrement)]
    [InlineData(6, 45, ShiftStandardTimeValidationCode.StartMustUseThirtyMinuteIncrement)]
    public void CreateWhenStartIsOutsideThirtyMinuteIncrementReturnsError(
        int hour,
        int minute,
        ShiftStandardTimeValidationCode expectedCode)
    {
        ShiftStandardTimeValidationResult result = ShiftStandardTime.Create(
            new TimeOnly(hour, minute),
            new TimeOnly(13, 30));

        ShiftStandardTimeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(expectedCode, error.Code);
    }

    [Theory]
    [InlineData(13, 15, ShiftStandardTimeValidationCode.EndMustUseThirtyMinuteIncrement)]
    [InlineData(13, 45, ShiftStandardTimeValidationCode.EndMustUseThirtyMinuteIncrement)]
    public void CreateWhenEndIsOutsideThirtyMinuteIncrementReturnsError(
        int hour,
        int minute,
        ShiftStandardTimeValidationCode expectedCode)
    {
        ShiftStandardTimeValidationResult result = ShiftStandardTime.Create(
            new TimeOnly(6, 30),
            new TimeOnly(hour, minute));

        ShiftStandardTimeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(expectedCode, error.Code);
    }

    [Fact]
    public void CreateWhenTimeContainsSecondsReturnsWholeMinuteError()
    {
        ShiftStandardTimeValidationResult result = ShiftStandardTime.Create(
            new TimeOnly(6, 30, 1),
            new TimeOnly(13, 30));

        ShiftStandardTimeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(ShiftStandardTimeValidationCode.StartMustUseWholeMinute, error.Code);
    }

    [Theory]
    [InlineData(13, 30)]
    [InlineData(6, 0)]
    public void CreateWhenEndDoesNotFollowStartRejectsOvernightOrEmptyRange(
        int endHour,
        int endMinute)
    {
        ShiftStandardTimeValidationResult result = ShiftStandardTime.Create(
            new TimeOnly(13, 30),
            new TimeOnly(endHour, endMinute));

        ShiftStandardTimeValidationError error = Assert.Single(result.Errors);
        Assert.Equal(ShiftStandardTimeValidationCode.EndMustBeAfterStart, error.Code);
    }
}
