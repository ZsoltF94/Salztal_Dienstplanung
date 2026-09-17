using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Planning.Candidates;

internal enum PlanningCandidateKind
{
    NormalDemand,
    SplitShiftPattern,
    ReliefShiftPattern,
}

internal enum PlanningRemainingDemandKind
{
    FullyUncovered,
    PartiallyUncoveredByProtectedReliefShift,
}

internal sealed record PlanningCandidateSegment(
    PlanningDemandKey AnchorDemand,
    TimeOnly ActualStart,
    TimeOnly ActualEnd,
    int WorkMinutes);

internal sealed record PlanningCandidateCoverage(
    PlanningDemandKey Demand,
    TimeOnly CoveredStart,
    TimeOnly CoveredEnd,
    int CoveredMinutes);

internal sealed record PlanningRemainingDemand(
    PlanningDemandKey Demand,
    TimeOnly UncoveredStart,
    TimeOnly UncoveredEnd,
    int UncoveredMinutes,
    PlanningRemainingDemandKind Kind);

internal sealed class PlanningAssignmentCandidate
{
    public PlanningAssignmentCandidate(
        string technicalKey,
        Guid employeeId,
        DateOnly date,
        PlanningCandidateKind kind,
        Guid? patternId,
        IEnumerable<PlanningCandidateSegment> segments,
        IEnumerable<PlanningCandidateCoverage> coverages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(technicalKey);
        ArgumentOutOfRangeException.ThrowIfEqual(employeeId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(coverages);

        PlanningCandidateSegment[] segmentValues = segments.ToArray();
        PlanningCandidateCoverage[] coverageValues = coverages.ToArray();
        if (segmentValues.Length == 0
            || segmentValues.Any(segment => segment.AnchorDemand.Date != date)
            || coverageValues.Any(coverage => coverage.Demand.Date != date))
        {
            throw new ArgumentException("Candidate values must share one date.");
        }

        ValidateShape(kind, patternId, segmentValues, coverageValues);

        TechnicalKey = technicalKey;
        EmployeeId = employeeId;
        Date = date;
        Kind = kind;
        PatternId = patternId;
        Segments = Array.AsReadOnly(segmentValues);
        Coverages = Array.AsReadOnly(coverageValues);
        WorkMinutes = checked((int)segmentValues.Sum(segment =>
            (long)segment.WorkMinutes));
    }

    public string TechnicalKey { get; }

    public Guid EmployeeId { get; }

    public DateOnly Date { get; }

    public PlanningCandidateKind Kind { get; }

    public Guid? PatternId { get; }

    public ReadOnlyCollection<PlanningCandidateSegment> Segments { get; }

    public ReadOnlyCollection<PlanningCandidateCoverage> Coverages { get; }

    public int WorkMinutes { get; }

    private static void ValidateShape(
        PlanningCandidateKind kind,
        Guid? patternId,
        PlanningCandidateSegment[] segments,
        PlanningCandidateCoverage[] coverages)
    {
        bool basicValuesAreValid = Enum.IsDefined(kind)
            && segments.All(segment =>
                segment.ActualEnd > segment.ActualStart
                && segment.WorkMinutes
                    == (int)(segment.ActualEnd - segment.ActualStart).TotalMinutes)
            && coverages.All(coverage =>
                coverage.CoveredEnd > coverage.CoveredStart
                && coverage.CoveredMinutes
                    == (int)(coverage.CoveredEnd - coverage.CoveredStart).TotalMinutes);
        bool coverageMatchesSegments = segments.Length == coverages.Length
            && segments.Zip(coverages).All(pair =>
                pair.First.AnchorDemand == pair.Second.Demand
                && pair.First.ActualStart == pair.Second.CoveredStart
                && pair.First.ActualEnd == pair.Second.CoveredEnd);
        bool kindShapeIsValid = kind switch
        {
            PlanningCandidateKind.NormalDemand =>
                patternId is null
                && segments.Length == 1
                && coverages.Length == 1,
            PlanningCandidateKind.SplitShiftPattern =>
                patternId is Guid splitPatternId
                && splitPatternId != Guid.Empty
                && segments.Length == 2
                && coverages.Length == 2
                && segments[0].AnchorDemand != segments[1].AnchorDemand
                && segments[0].ActualEnd < segments[1].ActualStart,
            PlanningCandidateKind.ReliefShiftPattern =>
                patternId is Guid reliefPatternId
                && reliefPatternId != Guid.Empty
                && segments.Length == 2
                && coverages.Length == 2
                && segments[0].AnchorDemand != segments[1].AnchorDemand
                && segments[0].ActualEnd == segments[1].ActualStart,
            _ => false,
        };

        if (!basicValuesAreValid || !coverageMatchesSegments || !kindShapeIsValid)
        {
            throw new ArgumentException("Candidate structure is inconsistent.");
        }
    }
}

internal sealed record EmployeeDayCandidateChoices(
    Guid EmployeeId,
    DateOnly Date,
    IReadOnlyList<string> CandidateKeys);

internal sealed record DemandCoverageCandidateChoices(
    PlanningDemandKey Demand,
    TimeOnly Start,
    TimeOnly End,
    IReadOnlyList<string> CandidateKeys);
