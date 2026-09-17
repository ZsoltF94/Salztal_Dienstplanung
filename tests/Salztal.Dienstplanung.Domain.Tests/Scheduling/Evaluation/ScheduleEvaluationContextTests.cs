using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Evaluation;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling.Evaluation;

public sealed class ScheduleEvaluationContextTests
{
    [Fact]
    public void ContextKeepsImmutableOrderedEvaluationFacts()
    {
        SchedulingTestContext scheduling = SchedulingTestContext.Create();
        EmployeeId employeeId = CreateEmployeeId(
            "10000000-0000-4000-8000-000000000001");
        ScheduleEvaluationEmployee[] employees =
        [
            new ScheduleEvaluationEmployee(
                employeeId,
                EmployeeTypePlanningRole.Normal,
                true,
                [
                    new ScheduleEvaluationEligibility(
                        ShiftEligibilityTargetKind.ShiftType,
                        Guid.Parse("20000000-0000-4000-8000-000000000001"),
                        ShiftEligibilityMode.Regular,
                        ShiftEligibilityActivation.Always),
                ],
                [
                    new ScheduleEvaluationWeekTarget(
                        scheduling.Period.StartMonday,
                        2_100),
                ]),
        ];
        ScheduleEvaluationHistoryDay[] history =
        [
            new ScheduleEvaluationHistoryDay(
                scheduling.Period.StartMonday.AddDays(-1),
                ScheduleEvaluationHistoryDayStatus.Available,
                [employeeId]),
        ];
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);
        ScheduleEvaluationContext context = new(
            scheduling.CreateDraft(),
            catalog,
            employees,
            history,
            false);

        employees[0] = new ScheduleEvaluationEmployee(
            CreateEmployeeId("30000000-0000-4000-8000-000000000001"),
            EmployeeTypePlanningRole.Auxiliary,
            true,
            [],
            []);
        history[0] = new ScheduleEvaluationHistoryDay(
            scheduling.Period.StartMonday.AddDays(-2),
            ScheduleEvaluationHistoryDayStatus.Missing,
            []);

        Assert.Equal(employeeId, Assert.Single(context.Employees).EmployeeId);
        Assert.Equal(
            scheduling.Period.StartMonday.AddDays(-1),
            Assert.Single(context.HistoryDays).Date);
        Assert.Same(catalog, context.RuleCatalog);
        Assert.False(context.EnableAuxiliaryReliefShift);
    }

    [Fact]
    public void DuplicateEmployeesAndHistoryDatesAreRejected()
    {
        SchedulingTestContext scheduling = SchedulingTestContext.Create();
        EmployeeId employeeId = CreateEmployeeId(
            "10000000-0000-4000-8000-000000000001");
        ScheduleEvaluationEmployee employee = new(
            employeeId,
            EmployeeTypePlanningRole.Normal,
            true,
            [],
            []);
        DateOnly historyDate = scheduling.Period.StartMonday.AddDays(-1);
        ScheduleEvaluationHistoryDay history = new(
            historyDate,
            ScheduleEvaluationHistoryDayStatus.Available,
            []);
        RuleCatalog catalog = Assert.IsType<RuleCatalog>(
            InitialRuleCatalog.Read(InitialRuleCatalog.Version).Value);

        Assert.Throws<ArgumentException>(() => new ScheduleEvaluationContext(
            scheduling.CreateDraft(),
            catalog,
            [employee, employee],
            [],
            false));
        Assert.Throws<ArgumentException>(() => new ScheduleEvaluationContext(
            scheduling.CreateDraft(),
            catalog,
            [employee],
            [history, history],
            false));
    }

    private static EmployeeId CreateEmployeeId(string value)
    {
        return EmployeeId.TryCreate(Guid.Parse(value), out EmployeeId? employeeId)
            ? employeeId
            : throw new InvalidOperationException();
    }
}
