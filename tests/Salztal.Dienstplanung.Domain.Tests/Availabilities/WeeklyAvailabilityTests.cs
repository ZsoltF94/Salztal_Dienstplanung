using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Tests.Availabilities;

public sealed class WeeklyAvailabilityTests
{
    private static readonly Guid EmployeeIdentifier =
        new("5edb9561-ec59-44a5-b761-c22f347b15f1");

    private static readonly DateOnly WeekMonday = new(2026, 12, 21);

    [Fact]
    public void CalculateForType25WithOneVacationReturnsTwentyHours()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            CreateEntry(WeekMonday, AvailabilityEntryKind.Vacation));

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(1_500, week.UncutWorkTarget.Minutes);
        Assert.Equal(1_200, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateForType30WithTwoSicknessDaysReturnsEighteenHours()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type30;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            CreateEntry(WeekMonday, AvailabilityEntryKind.Sickness),
            CreateEntry(WeekMonday.AddDays(1), AvailabilityEntryKind.Sickness));

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(1_080, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateCountsVacationAndSicknessButNotFixedDayOff()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            CreateEntry(WeekMonday, AvailabilityEntryKind.Vacation),
            CreateEntry(WeekMonday.AddDays(1), AvailabilityEntryKind.Sickness),
            CreateEntry(WeekMonday.AddDays(2), AvailabilityEntryKind.FixedDayOff));

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(900, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateCountsHolidayAndWeekendEntriesLikeEveryOtherDay()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            CreateEntry(new DateOnly(2026, 12, 25), AvailabilityEntryKind.Vacation),
            CreateEntry(new DateOnly(2026, 12, 26), AvailabilityEntryKind.Sickness));

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(900, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateIncludesMondayAndSundayButIgnoresAdjacentWeeks()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            CreateEntry(WeekMonday.AddDays(-1), AvailabilityEntryKind.Vacation),
            CreateEntry(WeekMonday, AvailabilityEntryKind.Vacation),
            CreateEntry(WeekMonday.AddDays(6), AvailabilityEntryKind.Sickness),
            CreateEntry(WeekMonday.AddDays(7), AvailabilityEntryKind.Sickness));

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(900, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateWhenReductionExceedsUncutTargetClampsAtZeroMinutes()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type20;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            Enumerable.Range(0, 7)
                .Select(day => CreateEntry(
                    WeekMonday.AddDays(day),
                    AvailabilityEntryKind.Vacation))
                .ToArray());

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(0, week.EffectiveWorkTarget.Minutes);
    }

    [Theory]
    [InlineData(AvailabilityEntryKind.Vacation)]
    [InlineData(AvailabilityEntryKind.Sickness)]
    public void CalculateForAuxiliaryTypeWhenVacationOrSicknessExistsRejectsEntry(
        AvailabilityEntryKind kind)
    {
        EmployeeType type = InitialEmployeeTypeCatalog.TypeAh1;
        Employee employee = CreateEmployee(type);
        AvailabilityEntry entry = CreateEntry(WeekMonday, kind);

        WeeklyAvailabilityValidationResult result = WeeklyAvailability.Calculate(
            employee,
            type,
            WeekMonday,
            CreateSet(entry));

        WeeklyAvailabilityValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            WeeklyAvailabilityValidationCode.VacationAndSicknessNotAllowed,
            error.Code);
        Assert.Equal(entry.Date, error.Date);
    }

    [Fact]
    public void CalculateForAuxiliaryTypeWithFixedDayOffKeepsUncutTarget()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.TypeAh1;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            CreateEntry(WeekMonday, AvailabilityEntryKind.FixedDayOff));

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.Equal(600, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateWhenWeekDoesNotStartOnMondayReturnsStructuredError()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type);

        WeeklyAvailabilityValidationResult result = WeeklyAvailability.Calculate(
            employee,
            type,
            WeekMonday.AddDays(1),
            CreateSet());

        WeeklyAvailabilityValidationError error = Assert.Single(result.Errors);
        Assert.Equal(WeeklyAvailabilityValidationCode.WeekMustStartOnMonday, error.Code);
    }

    [Fact]
    public void CalculateWhenEmployeeTypeDoesNotMatchReturnsStructuredError()
    {
        Employee employee = CreateEmployee(InitialEmployeeTypeCatalog.Type25);

        WeeklyAvailabilityValidationResult result = WeeklyAvailability.Calculate(
            employee,
            InitialEmployeeTypeCatalog.Type30,
            WeekMonday,
            CreateSet());

        WeeklyAvailabilityValidationError error = Assert.Single(result.Errors);
        Assert.Equal(WeeklyAvailabilityValidationCode.EmployeeTypeMismatch, error.Code);
    }

    [Fact]
    public void CalculateForType1WhenAllSevenDaysAreUnavailableMarksFullWeek()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type1;
        Employee employee = CreateEmployee(type);
        AvailabilityEntryKind[] kinds =
        [
            AvailabilityEntryKind.Vacation,
            AvailabilityEntryKind.Sickness,
            AvailabilityEntryKind.FixedDayOff,
            AvailabilityEntryKind.Vacation,
            AvailabilityEntryKind.Sickness,
            AvailabilityEntryKind.FixedDayOff,
            AvailabilityEntryKind.Vacation,
        ];
        AvailabilityEntrySet entries = CreateSet(
            kinds.Select((kind, day) => CreateEntry(WeekMonday.AddDays(day), kind))
                .ToArray());

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.True(week.IsFullyUnavailable);
        Assert.Equal(0, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateForType1WhenOneDayIsAvailableDoesNotMarkFullWeek()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type1;
        Employee employee = CreateEmployee(type);
        AvailabilityEntrySet entries = CreateSet(
            Enumerable.Range(0, 6)
                .Select(day => CreateEntry(
                    WeekMonday.AddDays(day),
                    AvailabilityEntryKind.FixedDayOff))
                .ToArray());

        WeeklyAvailability week = Calculate(employee, type, WeekMonday, entries);

        Assert.False(week.IsFullyUnavailable);
        Assert.Equal(2_400, week.EffectiveWorkTarget.Minutes);
    }

    [Fact]
    public void CalculateAfterVacationIsReplacedWithFixedDayOffUsesOnlyLatestEntry()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type);
        AvailabilityEntry vacation = CreateEntry(
            WeekMonday,
            AvailabilityEntryKind.Vacation);
        AvailabilityEntry fixedDayOff = CreateEntry(
            WeekMonday,
            AvailabilityEntryKind.FixedDayOff);
        AvailabilityEntrySet original = CreateSet(vacation);

        WeeklyAvailability before = Calculate(employee, type, WeekMonday, original);
        WeeklyAvailability after = Calculate(
            employee,
            type,
            WeekMonday,
            original.WithEntry(fixedDayOff));

        Assert.Equal(1_200, before.EffectiveWorkTarget.Minutes);
        Assert.Equal(1_500, after.EffectiveWorkTarget.Minutes);
    }

    private static Employee CreateEmployee(EmployeeType employeeType)
    {
        return Assert.IsType<Employee>(
            Employee.Create(
                EmployeeIdentifier,
                "Erika",
                "Muster",
                employeeType.Id.Value).Value);
    }

    private static AvailabilityEntry CreateEntry(
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(EmployeeIdentifier, date, kind).Value);
    }

    private static AvailabilityEntrySet CreateSet(params AvailabilityEntry[] entries)
    {
        return Assert.IsType<AvailabilityEntrySet>(AvailabilityEntrySet.Create(entries).Value);
    }

    private static WeeklyAvailability Calculate(
        Employee employee,
        EmployeeType employeeType,
        DateOnly weekMonday,
        AvailabilityEntrySet entries)
    {
        return Assert.IsType<WeeklyAvailability>(
            WeeklyAvailability.Calculate(employee, employeeType, weekMonday, entries).Value);
    }
}
