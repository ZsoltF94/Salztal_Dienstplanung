using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class AutomaticSchedulePreviewViewModel
{
    public AutomaticSchedulePreviewViewModel(
        AutomaticSchedulePreview preview,
        AutomaticScheduleGenerationReport? report = null)
    {
        ArgumentNullException.ThrowIfNull(preview);
        AutomaticScheduleProposal proposal = preview.Proposal;
        int fullyOpen = proposal.OpenDemands.Count(item =>
            item.Kind == AutomaticScheduleOpenDemandKind.FullyUncovered);
        int partiallyOpen = proposal.OpenDemands.Count - fullyOpen;
        int openMinutes = proposal.OpenDemands.Sum(item => item.UncoveredMinutes);

        Proposal = proposal;
        StatusDisplay = preview.Status == AutomaticSchedulePlanningStatus.Optimal
            ? "Optimaler Vorschlag"
            : "Zulässiger Vorschlag – Optimalität nicht nachgewiesen";
        RuntimeDisplay = $"Laufzeit: {FormatDuration(proposal.Metadata.TotalDuration)}";
        OpenDemandDisplay =
            $"Offener Bedarf: {fullyOpen} vollständig, {partiallyOpen} teilweise, {FormatMinutes(openMinutes)}";
        AssignmentDisplay =
            $"Erzeugt: {proposal.Assignments.Count} Einteilungen und {FormatGeneratedDayOffs(proposal.GeneratedDayOffs.Count)}";
        System10Notice =
            "Ausführliche Ursachen und Lösungsvorschläge werden in System 10 ergänzt.";
        DemandReport = report?.PlanningReport is null
            ? null
            : new PlanningDemandReportViewModel(report.PlanningReport);
        EmployeeReport = report?.PlanningReport is null
            ? null
            : new PlanningEmployeeReportViewModel(report.PlanningReport);
        GenerationReport = report is null
            ? null
            : new AutomaticScheduleGenerationReportViewModel(report);
    }

    public AutomaticScheduleProposal Proposal { get; }

    public string StatusDisplay { get; }

    public string RuntimeDisplay { get; }

    public string OpenDemandDisplay { get; }

    public string AssignmentDisplay { get; }

    public string System10Notice { get; }

    public PlanningDemandReportViewModel? DemandReport { get; }

    public PlanningEmployeeReportViewModel? EmployeeReport { get; }

    public AutomaticScheduleGenerationReportViewModel? GenerationReport { get; }

    private static string FormatDuration(TimeSpan duration)
    {
        return string.Create(
            CultureInfo.GetCultureInfo("de-DE"),
            $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00} min");
    }

    private static string FormatMinutes(int minutes)
    {
        int hours = Math.DivRem(minutes, 60, out int remainingMinutes);
        return hours == 0
            ? $"{remainingMinutes} min"
            : $"{hours} Std. {remainingMinutes:00} min";
    }

    private static string FormatGeneratedDayOffs(int count)
    {
        return count == 1 ? "1 schwarzes X" : $"{count} schwarze X";
    }
}
