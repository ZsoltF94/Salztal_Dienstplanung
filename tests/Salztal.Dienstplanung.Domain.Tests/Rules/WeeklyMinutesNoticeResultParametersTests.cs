using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Domain.Tests.Rules;

public sealed class WeeklyMinutesNoticeResultParametersTests
{
    private static readonly Guid EmployeeA = Guid.Parse(
        "15000000-0000-0000-0000-000000000001");
    private static readonly Guid EmployeeB = Guid.Parse(
        "15000000-0000-0000-0000-000000000002");
    private static readonly DateOnly Monday = new(2026, 9, 21);

    [Fact]
    public void ConstructorOrdersImmutableStructuredEmployeeWeekValues()
    {
        WeeklyMinutesNoticeResultParameters parameters = new(
        [
            new EmployeeWeekMinutesResult(EmployeeB, Monday.AddDays(7), 720),
            new EmployeeWeekMinutesResult(EmployeeA, Monday, 300),
        ]);

        Assert.Equal(EmployeeA, parameters.EmployeeWeeks[0].EmployeeId);
        Assert.Equal(Monday, parameters.EmployeeWeeks[0].WeekMonday);
        Assert.Equal(300, parameters.EmployeeWeeks[0].Minutes);
        Assert.Equal(EmployeeB, parameters.EmployeeWeeks[1].EmployeeId);
        Assert.All(
            typeof(EmployeeWeekMinutesResult).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void ConstructorRejectsDuplicateEmployeeWeek()
    {
        Assert.Throws<ArgumentException>(() => new WeeklyMinutesNoticeResultParameters(
        [
            new EmployeeWeekMinutesResult(EmployeeA, Monday, 300),
            new EmployeeWeekMinutesResult(EmployeeA, Monday, 600),
        ]));
    }

    [Fact]
    public void EmployeeWeekRejectsEmptyEmployeeNonMondayAndNegativeMinutes()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EmployeeWeekMinutesResult(Guid.Empty, Monday, 0));
        Assert.Throws<ArgumentException>(() =>
            new EmployeeWeekMinutesResult(EmployeeA, Monday.AddDays(1), 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new EmployeeWeekMinutesResult(EmployeeA, Monday, -1));
    }
}
