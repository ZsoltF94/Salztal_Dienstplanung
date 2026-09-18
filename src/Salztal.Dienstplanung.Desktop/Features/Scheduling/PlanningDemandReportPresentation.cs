using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class PlanningDemandWeekViewModel
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    public PlanningDemandWeekViewModel(
        int number,
        PlanningDemandSummarySnapshot summary)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentNullException.ThrowIfNull(summary);
        Number = number;
        Summary = new PlanningDemandSummaryViewModel(summary);
        Display = string.Create(
            GermanCulture,
            $"Woche {number}: {summary.PeriodMonday:dd.MM.}–{summary.PeriodSunday:dd.MM.yyyy}");
    }

    public int Number { get; }

    public string Display { get; }

    public PlanningDemandSummaryViewModel Summary { get; }
}

internal sealed class PlanningDemandSummaryViewModel
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    public PlanningDemandSummaryViewModel(PlanningDemandSummarySnapshot summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        PeriodMonday = summary.PeriodMonday;
        PeriodSunday = summary.PeriodSunday;
        RequiredPersonCount = summary.RequiredPersonCount;
        CoveredPersonCount = summary.CoveredPersonCount;
        OpenPersonCount = summary.OpenPersonCount;
        RequiredMinutes = summary.RequiredMinutes;
        CoveredMinutes = summary.CoveredMinutes;
        OpenMinutes = summary.OpenMinutes;
        FullyCoveredPersonCount = summary.FullyCoveredPersonCount;
        PartiallyCoveredPersonCount = summary.PartiallyCoveredPersonCount;
        UncoveredPersonCount = summary.UncoveredPersonCount;
        CoveragePercentage = summary.CoveragePercentage;
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

    public decimal? CoveragePercentage { get; }

    public string CoverageDisplay => CoveragePercentage is decimal percentage
        ? string.Create(GermanCulture, $"Deckungsquote: {percentage:0.0} %")
        : "Deckungsquote: kein Bedarf";

    public string PersonCountDisplay =>
        $"Bedarfsplätze: {RequiredPersonCount}; vollständig: {FullyCoveredPersonCount}; teilweise: {PartiallyCoveredPersonCount}; ungedeckt: {UncoveredPersonCount}";

    public string MinutesDisplay =>
        $"Bedarf: {PlanningDemandReportPresentation.FormatMinutes(RequiredMinutes)}; gedeckt: {PlanningDemandReportPresentation.FormatMinutes(CoveredMinutes)}; offen: {PlanningDemandReportPresentation.FormatMinutes(OpenMinutes)}";
}

internal sealed class PlanningDemandRowViewModel
{
    private static readonly CultureInfo GermanCulture =
        CultureInfo.GetCultureInfo("de-DE");

    public PlanningDemandRowViewModel(PlanningDemandCoverageSnapshot demand)
    {
        ArgumentNullException.ThrowIfNull(demand);
        Date = demand.Date;
        WeekdayDisplay = demand.Date.ToString("dddd", GermanCulture);
        DateDisplay = demand.Date.ToString("dd.MM.yyyy", GermanCulture);
        WorkLocationName = demand.WorkLocationName;
        ShiftTypeName = demand.ShiftTypeName;
        Ordinal = demand.Ordinal;
        DemandTimeDisplay = PlanningDemandReportPresentation.FormatInterval(
            demand.DemandStart,
            demand.DemandEnd);
        RequiredPersonCount = demand.RequiredPersonCount;
        RequiredMinutesDisplay = PlanningDemandReportPresentation.FormatMinutes(
            demand.RequiredMinutes);
        CoveredPersonCount = demand.CoveredPersonCount;
        CoveredMinutesDisplay = PlanningDemandReportPresentation.FormatMinutes(
            demand.CoveredMinutes);
        OpenPersonCount = demand.OpenPersonCount;
        OpenMinutesDisplay = PlanningDemandReportPresentation.FormatMinutes(
            demand.OpenMinutes);
        CoveredIntervalsDisplay = PlanningDemandReportPresentation.FormatIntervals(
            demand.CoveredIntervals);
        OpenIntervalsDisplay = PlanningDemandReportPresentation.FormatIntervals(
            demand.OpenIntervals);
        StatusDisplay = demand.Status switch
        {
            PlanningDemandCoverageStatus.FullyCovered => "Vollständig gedeckt",
            PlanningDemandCoverageStatus.PartiallyCoveredByReliefShift =>
                "Teilweise durch Spr gedeckt",
            PlanningDemandCoverageStatus.Uncovered => "Ungedeckt",
            _ => throw new ArgumentOutOfRangeException(nameof(demand)),
        };
    }

    public DateOnly Date { get; }

    public string WeekdayDisplay { get; }

    public string DateDisplay { get; }

    public string WorkLocationName { get; }

    public string ShiftTypeName { get; }

    public int Ordinal { get; }

    public string DemandTimeDisplay { get; }

    public int RequiredPersonCount { get; }

    public string RequiredMinutesDisplay { get; }

    public int CoveredPersonCount { get; }

    public string CoveredMinutesDisplay { get; }

    public int OpenPersonCount { get; }

    public string OpenMinutesDisplay { get; }

    public string CoveredIntervalsDisplay { get; }

    public string OpenIntervalsDisplay { get; }

    public string StatusDisplay { get; }
}

internal static class PlanningDemandReportPresentation
{
    public static string FormatMinutes(int minutes)
    {
        int hours = Math.DivRem(minutes, 60, out int remainingMinutes);
        return hours == 0
            ? $"{remainingMinutes} min"
            : remainingMinutes == 0
                ? $"{hours} Std."
                : $"{hours} Std. {remainingMinutes:00} min";
    }

    public static string FormatInterval(TimeOnly start, TimeOnly end) =>
        $"{start:HH\\:mm}–{end:HH\\:mm}";

    public static string FormatIntervals(
        IEnumerable<PlanningDemandIntervalSnapshot> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        string[] values = intervals
            .Select(value => FormatInterval(value.Start, value.End))
            .ToArray();
        return values.Length == 0 ? "–" : string.Join(", ", values);
    }
}
