namespace Salztal.Dienstplanung.Planning.Tests.Reference;

public sealed class ExhaustiveScheduleReferenceSolverTests
{
    public static TheoryData<AutomaticScheduleObjectiveScenario> Scenarios =>
        new(AutomaticScheduleObjectiveScenarios.All);

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void SharedScenarioSelectsExpectedBestCandidate(
        AutomaticScheduleObjectiveScenario scenario)
    {
        ReferenceScheduleCandidate<string> best = Assert.IsType<
            ReferenceScheduleCandidate<string>>(
                ExhaustiveScheduleReferenceSolver.SelectBest(scenario.Candidates));

        Assert.Equal(scenario.ExpectedBestKey, best.Key);
    }

    [Fact]
    public void CandidateOrderDoesNotChangeBestResult()
    {
        AutomaticScheduleObjectiveScenario scenario =
            AutomaticScheduleObjectiveScenarios.All[0];

        ReferenceScheduleCandidate<string> forward = Assert.IsType<
            ReferenceScheduleCandidate<string>>(
                ExhaustiveScheduleReferenceSolver.SelectBest(scenario.Candidates));
        ReferenceScheduleCandidate<string> reversed = Assert.IsType<
            ReferenceScheduleCandidate<string>>(
                ExhaustiveScheduleReferenceSolver.SelectBest(
                    scenario.Candidates.Reverse()));

        Assert.Equal(forward, reversed);
    }

    [Fact]
    public void NoFeasibleCandidateReturnsNoResult()
    {
        ReferenceScheduleCandidate<string> candidate = new(
            "blocked",
            false,
            AutomaticScheduleObjectiveScenarios.All[0].Candidates[0].ObjectiveVector,
            "blocked");

        ReferenceScheduleCandidate<string>? result =
            ExhaustiveScheduleReferenceSolver.SelectBest([candidate]);

        Assert.Null(result);
    }
}
