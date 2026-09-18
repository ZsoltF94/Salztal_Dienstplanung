using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Planning.Candidates;
using Salztal.Dienstplanung.Planning.Optimization;
using Salztal.Dienstplanung.Planning.Tests.Candidates;

namespace Salztal.Dienstplanung.Planning.Tests.Optimization;

public sealed class PlanningInterruptionRecordingTests
{
    [Fact]
    public void ExpiredBudgetWithRetainedSelectionRecordsTargetBudgetAndValues()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(snapshot);
        TimeSpan timeLimit = TimeSpan.FromSeconds(1);
        PlanningSolveBudget? budget = null;
        budget = new PlanningSolveBudget(
            timeLimit,
            () => budget?.HasRecordedSelection == true
                ? timeLimit
                : TimeSpan.Zero,
            CancellationToken.None);

        AutomaticScheduleOptimizationRun run = DemandCoverageOptimizer.Optimize(
            snapshot,
            candidates,
            budget);

        Assert.False(run.IsProvenOptimal);
        AutomaticSchedulePhaseSnapshot phase = Assert.Single(
            run.Phases,
            value => value.Kind == AutomaticSchedulePhaseKind.RegularCoverage);
        Assert.Equal(AutomaticSchedulePhaseStatus.Interrupted, phase.Status);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection,
            phase.Termination?.Reason);
        Assert.Equal(
            AutomaticScheduleOptimizationTargetKind.RegularTouchedDemandSlots,
            phase.Termination?.ActiveTarget);
        Assert.Equal(budget.TimeLimit, phase.Termination?.TimeLimit);
        Assert.True(phase.Termination?.BudgetElapsed >= budget.TimeLimit);
        Assert.Equal(
            phase.Values.Single(value => value.Key == "required_minutes").Value,
            phase.Values.Single(value => value.Key == "covered_minutes").Value
                + phase.Values.Single(value => value.Key == "uncovered_minutes").Value);
        Assert.Equal(
            run.Result.UncoveredEmployeeMinutes,
            phase.Values.Single(value => value.Key == "uncovered_minutes").Value);
    }

    [Fact]
    public void ExpiredBudgetWithoutSelectionRecordsDistinctTimeoutReason()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(snapshot);
        PlanningSolveBudget budget = new(
            TimeSpan.FromTicks(1),
            CancellationToken.None);

        AutomaticScheduleOptimizationFailureException exception = Assert.Throws<
            AutomaticScheduleOptimizationFailureException>(() =>
            DemandCoverageOptimizer.Optimize(snapshot, candidates, budget));

        Assert.IsType<PlanningTimeLimitWithoutFeasibleSelectionException>(
            exception.InnerException);
        AutomaticSchedulePhaseSnapshot phase = exception.Phases[^1];
        Assert.Equal(AutomaticSchedulePhaseKind.RegularCoverage, phase.Kind);
        Assert.Equal(AutomaticSchedulePhaseStatus.Interrupted, phase.Status);
        Assert.Empty(phase.Values);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection,
            phase.Termination?.Reason);
        Assert.Equal(
            AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes,
            phase.Termination?.ActiveTarget);
        Assert.Equal(budget.TimeLimit, phase.Termination?.TimeLimit);
        Assert.True(phase.Termination?.BudgetElapsed >= budget.TimeLimit);
    }

    [Fact]
    public void CancellationRecordsReasonAndActiveOptimizationTarget()
    {
        PlanningInputSnapshot snapshot = CreateSnapshot();
        PlanningCandidateSet candidates = PlanningCandidateBuilder.Build(snapshot);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        PlanningSolveBudget budget = new(
            TimeSpan.FromSeconds(1),
            cancellation.Token);

        AutomaticScheduleOptimizationFailureException exception = Assert.Throws<
            AutomaticScheduleOptimizationFailureException>(() =>
            DemandCoverageOptimizer.Optimize(snapshot, candidates, budget));

        Assert.IsType<OperationCanceledException>(exception.InnerException);
        AutomaticSchedulePhaseSnapshot phase = exception.Phases[^1];
        Assert.Equal(AutomaticSchedulePhaseKind.RegularCoverage, phase.Kind);
        Assert.Equal(AutomaticSchedulePhaseStatus.Interrupted, phase.Status);
        Assert.Equal(
            AutomaticSchedulePhaseTerminationReason.CancellationRequested,
            phase.Termination?.Reason);
        Assert.Equal(
            AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes,
            phase.Termination?.ActiveTarget);
        Assert.Equal(budget.TimeLimit, phase.Termination?.TimeLimit);
        Assert.NotNull(phase.Termination?.BudgetElapsed);
    }

    private static PlanningInputSnapshot CreateSnapshot()
    {
        PlanningEmployeeSnapshot employee = CandidateScenarioFactory.CreateEmployees()
            .Single(item => item.Id == CandidateScenarioFactory.NormalEmployeeId);
        PlanningEmployeeTypeSnapshot type = CandidateScenarioFactory.CreateEmployeeTypes()
            .Single(item => item.Id == employee.EmployeeTypeId);
        ScheduleDemandSlotSnapshot slot = DemandCoverageOptimizerTests.Slot(
            CandidateScenarioFactory.Saturday,
            1,
            CandidateScenarioFactory.LateShiftId,
            180,
            startHour: 16,
            startMinute: 30);
        return DemandCoverageOptimizerTests.Clone(
            CandidateScenarioFactory.Create(),
            [employee],
            [type],
            [slot],
            []);
    }
}
