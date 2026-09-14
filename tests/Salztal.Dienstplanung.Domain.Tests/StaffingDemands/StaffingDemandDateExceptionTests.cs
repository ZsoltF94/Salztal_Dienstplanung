using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.StaffingDemands;

public sealed class StaffingDemandDateExceptionTests
{
    private static readonly Guid CafeteriaId = InitialWorkLocationCatalog.Cafeteria.Id.Value;
    private static readonly Guid CafeteriaShiftAId =
        InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value;

    [Fact]
    public void CreateReplacementWhenValuesAreValidReturnsCompleteSnapshot()
    {
        Guid id = new("80c1fd9c-39d2-4591-9576-973104678fb0");
        DateOnly date = new(2026, 9, 19);

        StaffingDemandDateExceptionValidationResult result =
            StaffingDemandDateException.CreateReplacement(
                id,
                date,
                CafeteriaId,
                CafeteriaShiftAId,
                new TimeOnly(14, 0),
                new TimeOnly(18, 0),
                2);

        StaffingDemandDateException dateException =
            Assert.IsType<StaffingDemandDateException>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(id, dateException.Id.Value);
        Assert.Equal(date, dateException.Key.Date);
        Assert.Equal(CafeteriaId, dateException.Key.WorkLocationId.Value);
        Assert.Equal(CafeteriaShiftAId, dateException.Key.ShiftTypeId.Value);
        Assert.Equal(StaffingDemandDateExceptionKind.Replace, dateException.Kind);
        Assert.Equal(new TimeOnly(14, 0), dateException.ActualTime!.Start);
        Assert.Equal(new TimeOnly(18, 0), dateException.ActualTime.End);
        Assert.Equal(2, dateException.RequiredEmployeeCount!.Value);
        Assert.Equal(240, dateException.DurationMinutes);
        Assert.Equal(480L, dateException.RequiredWorkMinutes);
    }

    [Fact]
    public void CreateRemovalWhenValuesAreValidContainsNoApparentDemandValues()
    {
        StaffingDemandDateExceptionValidationResult result =
            StaffingDemandDateException.CreateRemoval(
                Guid.NewGuid(),
                new DateOnly(2026, 9, 19),
                CafeteriaId,
                CafeteriaShiftAId);

        StaffingDemandDateException dateException =
            Assert.IsType<StaffingDemandDateException>(result.Value);
        Assert.Equal(StaffingDemandDateExceptionKind.Remove, dateException.Kind);
        Assert.Null(dateException.ActualTime);
        Assert.Null(dateException.RequiredEmployeeCount);
        Assert.Null(dateException.DurationMinutes);
        Assert.Null(dateException.RequiredWorkMinutes);
    }

    [Fact]
    public void CreateAdditionWhenDemandValuesAreInvalidReturnsStructuredErrors()
    {
        StaffingDemandDateExceptionValidationResult result =
            StaffingDemandDateException.CreateAddition(
                Guid.Empty,
                new DateOnly(2026, 9, 19),
                Guid.Empty,
                Guid.Empty,
                new TimeOnly(13, 15),
                new TimeOnly(13, 0),
                0);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                StaffingDemandDateExceptionValidationCode.IdentifierRequired,
                StaffingDemandDateExceptionValidationCode.WorkLocationRequired,
                StaffingDemandDateExceptionValidationCode.ShiftTypeRequired,
                StaffingDemandDateExceptionValidationCode
                    .RequiredEmployeeCountMustBePositive,
                StaffingDemandDateExceptionValidationCode
                    .ActualStartMustUseThirtyMinuteIncrement,
                StaffingDemandDateExceptionValidationCode.ActualEndMustBeAfterStart,
            ],
            result.Errors.Select(error => error.Code));
    }

    [Fact]
    public void CreateSetWhenDateKeyIsDuplicatedReturnsError()
    {
        DateOnly date = new(2026, 9, 19);
        StaffingDemandDateException first = CreateAddition(date);
        StaffingDemandDateException duplicate = Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateRemoval(
                Guid.NewGuid(),
                date,
                CafeteriaId,
                CafeteriaShiftAId).Value);

        StaffingDemandDateExceptionSetValidationResult result =
            StaffingDemandDateExceptionSet.Create([first, duplicate]);

        StaffingDemandDateExceptionSetValidationError error =
            Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            StaffingDemandDateExceptionSetValidationCode.DuplicateDateKey,
            error.Code);
        Assert.Equal(first.Key, error.Key);
    }

    [Fact]
    public void CreateSetWhenSourceChangesKeepsImmutableSnapshot()
    {
        StaffingDemandDateException dateException =
            CreateAddition(new DateOnly(2026, 9, 19));
        List<StaffingDemandDateException> source = [dateException];
        StaffingDemandDateExceptionSet exceptionSet =
            Assert.IsType<StaffingDemandDateExceptionSet>(
                StaffingDemandDateExceptionSet.Create(source).Value);

        source.Clear();

        Assert.Equal([dateException], exceptionSet.Exceptions);
    }

    private static StaffingDemandDateException CreateAddition(DateOnly date)
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateAddition(
                Guid.NewGuid(),
                date,
                CafeteriaId,
                CafeteriaShiftAId,
                new TimeOnly(13, 30),
                new TimeOnly(20, 30),
                1).Value);
    }
}
