using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.StaffingDemands;

public sealed class StaffingDemandTests
{
    private static readonly Guid CafeteriaId = InitialWorkLocationCatalog.Cafeteria.Id.Value;
    private static readonly Guid CafeteriaShiftAId = InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value;

    [Fact]
    public void CreateWhenValuesAreValidReturnsImmutableMinutePreciseDemand()
    {
        Guid id = new("6964dcec-0b02-4f9f-b6c4-c87fb58ca0e4");
        DateOnly date = new(2026, 9, 14);

        StaffingDemandValidationResult result = StaffingDemand.Create(
            id,
            date,
            CafeteriaId,
            CafeteriaShiftAId,
            new TimeOnly(13, 30),
            new TimeOnly(20, 30),
            1);

        StaffingDemand demand = Assert.IsType<StaffingDemand>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(id, demand.Id.Value);
        Assert.Equal(date, demand.Date);
        Assert.Equal(CafeteriaId, demand.WorkLocationId.Value);
        Assert.Equal(CafeteriaShiftAId, demand.ShiftTypeId.Value);
        Assert.Equal(new TimeOnly(13, 30), demand.ActualTime.Start);
        Assert.Equal(new TimeOnly(20, 30), demand.ActualTime.End);
        Assert.Equal(1, demand.RequiredEmployeeCount.Value);
        Assert.Equal(420, demand.DurationMinutes);
        Assert.Equal(420L, demand.RequiredWorkMinutes);
    }

    [Theory]
    [InlineData(13, 30, 1_680)]
    [InlineData(12, 30, 1_440)]
    public void CreateForFourEarlyShiftEmployeesCalculatesConfirmedRequiredWorkMinutes(
        int endHour,
        int endMinute,
        long expectedRequiredWorkMinutes)
    {
        StaffingDemand demand = CreateDemand(
            new TimeOnly(6, 30),
            new TimeOnly(endHour, endMinute),
            4);

        Assert.Equal(expectedRequiredWorkMinutes, demand.RequiredWorkMinutes);
    }

    [Fact]
    public void CreateWithLargestSupportedEmployeeCountCalculatesWithoutIntegerOverflow()
    {
        StaffingDemand demand = CreateDemand(
            new TimeOnly(0, 0),
            new TimeOnly(23, 30),
            int.MaxValue);

        Assert.Equal(1_410, demand.DurationMinutes);
        Assert.Equal((long)int.MaxValue * 1_410, demand.RequiredWorkMinutes);
        Assert.True(demand.RequiredWorkMinutes > int.MaxValue);
    }

    [Fact]
    public void CreateWhenIdentifiersAndCountAreInvalidReturnsAllValidationCodes()
    {
        StaffingDemandValidationResult result = StaffingDemand.Create(
            Guid.Empty,
            new DateOnly(2026, 9, 14),
            Guid.Empty,
            Guid.Empty,
            new TimeOnly(13, 30),
            new TimeOnly(20, 30),
            0);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                StaffingDemandValidationCode.IdentifierRequired,
                error.Code),
            error => Assert.Equal(
                StaffingDemandValidationCode.WorkLocationRequired,
                error.Code),
            error => Assert.Equal(
                StaffingDemandValidationCode.ShiftTypeRequired,
                error.Code),
            error => Assert.Equal(
                StaffingDemandValidationCode.RequiredEmployeeCountMustBePositive,
                error.Code));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CreateWhenEmployeeCountIsNotPositiveReturnsError(int employeeCount)
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(13, 30),
            new TimeOnly(20, 30),
            employeeCount);

        StaffingDemandValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandValidationCode.RequiredEmployeeCountMustBePositive,
            error.Code);
    }

    [Fact]
    public void CreateWhenStartContainsSecondsReturnsWholeMinuteError()
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(13, 30, 1),
            new TimeOnly(20, 30),
            1);

        StaffingDemandValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandValidationCode.ActualStartMustUseWholeMinute,
            error.Code);
    }

    [Fact]
    public void CreateWhenEndContainsSecondsReturnsWholeMinuteError()
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(13, 30),
            new TimeOnly(20, 30, 1),
            1);

        StaffingDemandValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandValidationCode.ActualEndMustUseWholeMinute,
            error.Code);
    }

    [Theory]
    [InlineData(13, 15)]
    [InlineData(13, 45)]
    public void CreateWhenStartIsOutsideThirtyMinuteIncrementReturnsError(
        int hour,
        int minute)
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(hour, minute),
            new TimeOnly(20, 30),
            1);

        StaffingDemandValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandValidationCode.ActualStartMustUseThirtyMinuteIncrement,
            error.Code);
    }

    [Theory]
    [InlineData(20, 15)]
    [InlineData(20, 45)]
    public void CreateWhenEndIsOutsideThirtyMinuteIncrementReturnsError(
        int hour,
        int minute)
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(13, 30),
            new TimeOnly(hour, minute),
            1);

        StaffingDemandValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandValidationCode.ActualEndMustUseThirtyMinuteIncrement,
            error.Code);
    }

    [Theory]
    [InlineData(13, 30)]
    [InlineData(6, 0)]
    public void CreateWhenEndDoesNotFollowStartRejectsEmptyOrOvernightRange(
        int endHour,
        int endMinute)
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(13, 30),
            new TimeOnly(endHour, endMinute),
            1);

        StaffingDemandValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            StaffingDemandValidationCode.ActualEndMustBeAfterStart,
            error.Code);
    }

    [Fact]
    public void CreateWhenSeveralValuesAreInvalidReturnsIndependentTimeAndCountErrors()
    {
        StaffingDemandValidationResult result = CreateDemandResult(
            new TimeOnly(13, 15),
            new TimeOnly(13, 0),
            0);

        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                StaffingDemandValidationCode.RequiredEmployeeCountMustBePositive,
                error.Code),
            error => Assert.Equal(
                StaffingDemandValidationCode.ActualStartMustUseThirtyMinuteIncrement,
                error.Code),
            error => Assert.Equal(
                StaffingDemandValidationCode.ActualEndMustBeAfterStart,
                error.Code));
    }

    [Fact]
    public void PublicDemandContractWhenInspectedHasNoIndependentHoursInput()
    {
        Dictionary<string, Type> properties = typeof(StaffingDemand)
            .GetProperties()
            .ToDictionary(property => property.Name, property => property.PropertyType);

        Assert.Equal(8, properties.Count);
        Assert.Equal(typeof(StaffingDemandId), properties[nameof(StaffingDemand.Id)]);
        Assert.Equal(typeof(DateOnly), properties[nameof(StaffingDemand.Date)]);
        Assert.Equal(
            typeof(WorkLocationId),
            properties[nameof(StaffingDemand.WorkLocationId)]);
        Assert.Equal(
            typeof(ShiftTypeId),
            properties[nameof(StaffingDemand.ShiftTypeId)]);
        Assert.Equal(typeof(StaffingDemandTime), properties[nameof(StaffingDemand.ActualTime)]);
        Assert.Equal(
            typeof(RequiredEmployeeCount),
            properties[nameof(StaffingDemand.RequiredEmployeeCount)]);
        Assert.Equal(typeof(int), properties[nameof(StaffingDemand.DurationMinutes)]);
        Assert.Equal(typeof(long), properties[nameof(StaffingDemand.RequiredWorkMinutes)]);

        System.Reflection.ParameterInfo[] createParameters = typeof(StaffingDemand)
            .GetMethod(nameof(StaffingDemand.Create))!
            .GetParameters();

        Assert.Equal(7, createParameters.Length);
        Assert.DoesNotContain(
            createParameters,
            parameter => parameter.Name!.Contains("workMinutes", StringComparison.OrdinalIgnoreCase)
                || parameter.Name.Contains("hours", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StaffingDemandIdWhenValueIsEmptyCannotBeCreated()
    {
        bool wasCreated = StaffingDemandId.TryCreate(
            Guid.Empty,
            out StaffingDemandId? staffingDemandId);

        Assert.False(wasCreated);
        Assert.Null(staffingDemandId);
    }

    private static StaffingDemand CreateDemand(
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        return Assert.IsType<StaffingDemand>(
            CreateDemandResult(actualStart, actualEnd, requiredEmployeeCount).Value);
    }

    private static StaffingDemandValidationResult CreateDemandResult(
        TimeOnly actualStart,
        TimeOnly actualEnd,
        int requiredEmployeeCount)
    {
        return StaffingDemand.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 14),
            CafeteriaId,
            CafeteriaShiftAId,
            actualStart,
            actualEnd,
            requiredEmployeeCount);
    }
}
