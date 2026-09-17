using Salztal.Dienstplanung.Planning.Candidates;

namespace Salztal.Dienstplanung.Planning.Tests.Candidates;

public sealed class PlanningCandidateValueTests
{
    [Fact]
    public void NormalCandidateRejectsPartialCoverage()
    {
        PlanningDemandKey demand = CreateDemand(1);
        PlanningCandidateSegment segment = new(
            demand,
            new TimeOnly(8, 0),
            new TimeOnly(12, 0),
            240);
        PlanningCandidateCoverage partialCoverage = new(
            demand,
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            180);

        Assert.Throws<ArgumentException>(() => new PlanningAssignmentCandidate(
            "candidate-test",
            Guid.Parse("12000000-0000-0000-0000-000000000001"),
            demand.Date,
            PlanningCandidateKind.NormalDemand,
            null,
            [segment],
            [partialCoverage]));
    }

    [Fact]
    public void CompositeCandidateRejectsOverlappingOrDisconnectedSegments()
    {
        PlanningDemandKey firstDemand = CreateDemand(1);
        PlanningDemandKey secondDemand = CreateDemand(2);
        PlanningCandidateSegment first = new(
            firstDemand,
            new TimeOnly(8, 0),
            new TimeOnly(12, 0),
            240);
        PlanningCandidateSegment overlappingSecond = new(
            secondDemand,
            new TimeOnly(11, 30),
            new TimeOnly(13, 30),
            120);
        PlanningCandidateCoverage firstCoverage = new(
            firstDemand,
            first.ActualStart,
            first.ActualEnd,
            first.WorkMinutes);
        PlanningCandidateCoverage secondCoverage = new(
            secondDemand,
            overlappingSecond.ActualStart,
            overlappingSecond.ActualEnd,
            overlappingSecond.WorkMinutes);
        Guid patternId = Guid.Parse("52000000-0000-0000-0000-000000000001");

        Assert.Throws<ArgumentException>(() => new PlanningAssignmentCandidate(
            "candidate-split-test",
            Guid.Parse("12000000-0000-0000-0000-000000000001"),
            firstDemand.Date,
            PlanningCandidateKind.SplitShiftPattern,
            patternId,
            [first, overlappingSecond],
            [firstCoverage, secondCoverage]));
        Assert.Throws<ArgumentException>(() => new PlanningAssignmentCandidate(
            "candidate-relief-test",
            Guid.Parse("12000000-0000-0000-0000-000000000001"),
            firstDemand.Date,
            PlanningCandidateKind.ReliefShiftPattern,
            patternId,
            [first, overlappingSecond],
            [firstCoverage, secondCoverage]));
    }

    private static PlanningDemandKey CreateDemand(int ordinal) => new(
        Guid.Parse("92000000-0000-0000-0000-000000000001"),
        CandidateScenarioFactory.Saturday,
        CandidateScenarioFactory.RestaurantId,
        CandidateScenarioFactory.LateShiftId,
        ordinal);
}
