using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum PlanningDemandCoverageStatus
{
    FullyCovered,
    PartiallyCoveredByReliefShift,
    Uncovered,
}

public sealed class PlanningDemandIntervalSnapshot
{
    internal PlanningDemandIntervalSnapshot(TimeOnly start, TimeOnly end)
    {
        if (end <= start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(end),
                "A demand interval must end after it starts.");
        }

        Start = start;
        End = end;
    }

    public TimeOnly Start { get; }

    public TimeOnly End { get; }

    public int Minutes => (int)(End - Start).TotalMinutes;
}

public sealed class PlanningDemandCoverageSnapshot
{
    internal PlanningDemandCoverageSnapshot(
        Guid demandSourceId,
        DateOnly date,
        Guid workLocationId,
        string workLocationName,
        Guid shiftTypeId,
        string shiftTypeName,
        int ordinal,
        TimeOnly demandStart,
        TimeOnly demandEnd,
        IEnumerable<PlanningDemandIntervalSnapshot> coveredIntervals,
        IEnumerable<PlanningDemandIntervalSnapshot> openIntervals,
        PlanningDemandCoverageStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workLocationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(shiftTypeName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentNullException.ThrowIfNull(coveredIntervals);
        ArgumentNullException.ThrowIfNull(openIntervals);
        if (demandEnd <= demandStart)
        {
            throw new ArgumentOutOfRangeException(
                nameof(demandEnd),
                "A demand must end after it starts.");
        }

        PlanningDemandIntervalSnapshot[] covered = coveredIntervals.ToArray();
        PlanningDemandIntervalSnapshot[] open = openIntervals.ToArray();
        if (covered.Any(value => value is null)
            || open.Any(value => value is null)
            || covered.Concat(open).Any(value =>
                value.Start < demandStart || value.End > demandEnd))
        {
            throw new ArgumentException(
                "Demand intervals must be complete and lie inside the demand range.");
        }

        int requiredMinutes = (int)(demandEnd - demandStart).TotalMinutes;
        int coveredMinutes = covered.Sum(value => value.Minutes);
        int openMinutes = open.Sum(value => value.Minutes);
        if (coveredMinutes + openMinutes != requiredMinutes)
        {
            throw new ArgumentException(
                "Covered and open intervals must partition the demand range.");
        }

        TimeOnly cursor = demandStart;
        foreach (PlanningDemandIntervalSnapshot interval in covered
            .Concat(open)
            .OrderBy(value => value.Start)
            .ThenBy(value => value.End))
        {
            if (interval.Start != cursor)
            {
                throw new ArgumentException(
                    "Covered and open intervals must partition the demand range without gaps or overlaps.");
            }

            cursor = interval.End;
        }

        if (cursor != demandEnd)
        {
            throw new ArgumentException(
                "Covered and open intervals must cover the complete demand range.");
        }

        PlanningDemandCoverageStatus expectedStatus = openMinutes == 0
            ? PlanningDemandCoverageStatus.FullyCovered
            : coveredMinutes == 0
                ? PlanningDemandCoverageStatus.Uncovered
                : PlanningDemandCoverageStatus.PartiallyCoveredByReliefShift;
        if (status != expectedStatus)
        {
            throw new ArgumentException(
                "Demand intervals do not match the supplied coverage status.",
                nameof(status));
        }

        DemandSourceId = demandSourceId;
        Date = date;
        WorkLocationId = workLocationId;
        WorkLocationName = workLocationName.Trim();
        ShiftTypeId = shiftTypeId;
        ShiftTypeName = shiftTypeName.Trim();
        Ordinal = ordinal;
        DemandStart = demandStart;
        DemandEnd = demandEnd;
        RequiredPersonCount = 1;
        CoveredIntervals = Array.AsReadOnly(covered);
        OpenIntervals = Array.AsReadOnly(open);
        Status = status;
    }

    public Guid DemandSourceId { get; }

    public DateOnly Date { get; }

    public Guid WorkLocationId { get; }

    public string WorkLocationName { get; }

    public Guid ShiftTypeId { get; }

    public string ShiftTypeName { get; }

    public int Ordinal { get; }

    public TimeOnly DemandStart { get; }

    public TimeOnly DemandEnd { get; }

    public int RequiredPersonCount { get; }

    public int RequiredMinutes => (int)(DemandEnd - DemandStart).TotalMinutes;

    public int CoveredPersonCount => CoveredMinutes > 0 ? 1 : 0;

    public int CoveredMinutes => CoveredIntervals.Sum(value => value.Minutes);

    public int OpenPersonCount => OpenMinutes > 0 ? 1 : 0;

    public int OpenMinutes => OpenIntervals.Sum(value => value.Minutes);

    public PlanningDemandCoverageStatus Status { get; }

    public ReadOnlyCollection<PlanningDemandIntervalSnapshot> CoveredIntervals { get; }

    public ReadOnlyCollection<PlanningDemandIntervalSnapshot> OpenIntervals { get; }
}

public sealed class PlanningDemandSummarySnapshot
{
    internal PlanningDemandSummarySnapshot(
        DateOnly periodMonday,
        DateOnly periodSunday,
        IEnumerable<PlanningDemandCoverageSnapshot> demands)
    {
        ArgumentNullException.ThrowIfNull(demands);
        ArgumentOutOfRangeException.ThrowIfLessThan(periodSunday, periodMonday);

        PlanningDemandCoverageSnapshot[] values = demands.ToArray();
        if (values.Any(value => value is null
            || value.Date < periodMonday
            || value.Date > periodSunday))
        {
            throw new ArgumentException(
                "Summary demands must lie inside the summary period.",
                nameof(demands));
        }

        PeriodMonday = periodMonday;
        PeriodSunday = periodSunday;
        RequiredPersonCount = values.Sum(value => value.RequiredPersonCount);
        CoveredPersonCount = values.Sum(value => value.CoveredPersonCount);
        OpenPersonCount = values.Sum(value => value.OpenPersonCount);
        RequiredMinutes = values.Sum(value => value.RequiredMinutes);
        CoveredMinutes = values.Sum(value => value.CoveredMinutes);
        OpenMinutes = values.Sum(value => value.OpenMinutes);
        FullyCoveredPersonCount = values.Count(value =>
            value.Status == PlanningDemandCoverageStatus.FullyCovered);
        PartiallyCoveredPersonCount = values.Count(value =>
            value.Status == PlanningDemandCoverageStatus.PartiallyCoveredByReliefShift);
        UncoveredPersonCount = values.Count(value =>
            value.Status == PlanningDemandCoverageStatus.Uncovered);
    }

    public DateOnly PeriodMonday { get; }

    public DateOnly PeriodSunday { get; }

    public int RequiredPersonCount { get; }

    public int CoveredPersonCount { get; }

    public int OpenPersonCount { get; }

    public int RequiredMinutes { get; }

    public int CoveredMinutes { get; }

    public int OpenMinutes { get; }

    public int FullyCoveredPersonCount { get; }

    public int PartiallyCoveredPersonCount { get; }

    public int UncoveredPersonCount { get; }

    public decimal? CoveragePercentage => RequiredMinutes == 0
        ? null
        : decimal.Round(
            CoveredMinutes * 100m / RequiredMinutes,
            1,
            MidpointRounding.AwayFromZero);
}
