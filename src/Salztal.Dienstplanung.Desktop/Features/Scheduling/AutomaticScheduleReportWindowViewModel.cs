using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class AutomaticScheduleReportWindowViewModel
{
    private AutomaticScheduleReportWindowViewModel(
        AutomaticScheduleReportSource? source,
        string unavailableMessage)
    {
        IsCurrent = source is not null;
        UnavailableMessage = unavailableMessage;
        if (source is null)
        {
            SourceDisplay = "Bericht nicht mehr aktuell";
            StatusDisplay = "Der frühere Berichtsstand wird nicht weiter angezeigt.";
            PeriodDisplay = "–";
            SummaryDisplay = "Bitte erzeugen oder laden Sie einen aktuellen automatischen Lauf.";
            PlanningUnavailableDisplay =
                "Ohne aktuellen zulässigen Vorschlag stehen keine Planungswerte zur Verfügung.";
            return;
        }

        AutomaticScheduleGenerationReport report = source.Report;
        SourceIdentity = source.Identity;
        SourceDisplay = source.SourceDisplay;
        Generation = new AutomaticScheduleGenerationReportViewModel(report);
        StatusDisplay = Generation.StatusDisplay;
        if (report.PlanningReport is null)
        {
            PeriodDisplay = "Kein zulässiger Planungsvorschlag";
            SummaryDisplay =
                "Dieser Versuch enthält ausschließlich den Generierungs- und Fehlerbericht.";
            PlanningUnavailableDisplay =
                "Der Lauf hat keinen zulässigen Vorschlag erzeugt. Planungswerte werden nicht erfunden.";
            return;
        }

        AutomaticSchedulePlanningReport planning = report.PlanningReport;
        Demand = new PlanningDemandReportViewModel(planning);
        Employees = new PlanningEmployeeReportViewModel(planning);
        PeriodDisplay =
            $"Drei Wochen: {planning.PeriodMonday:dd.MM.yyyy}–{planning.PeriodSunday:dd.MM.yyyy}";
        SummaryDisplay =
            $"{Demand.TotalSummary.CoverageDisplay} · {Demand.TotalSummary.PersonCountDisplay}";
        PlanningUnavailableDisplay = string.Empty;
    }

    public bool IsCurrent { get; }

    public bool HasPlanningReport => Demand is not null && Employees is not null;

    public bool HasNoPlanningReport => !HasPlanningReport;

    public bool HasTechnicalDetails => Generation?.TechnicalDetails is not null;

    public bool HasUnavailableMessage => !string.IsNullOrWhiteSpace(UnavailableMessage);

    public string? SourceIdentity { get; }

    public string SourceDisplay { get; }

    public string StatusDisplay { get; }

    public string PeriodDisplay { get; }

    public string SummaryDisplay { get; }

    public string PlanningUnavailableDisplay { get; }

    public string UnavailableMessage { get; }

    public PlanningDemandReportViewModel? Demand { get; }

    public PlanningEmployeeReportViewModel? Employees { get; }

    public AutomaticScheduleGenerationReportViewModel? Generation { get; }

    public static AutomaticScheduleReportWindowViewModel Current(
        AutomaticScheduleReportSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new AutomaticScheduleReportWindowViewModel(source, string.Empty);
    }

    public static AutomaticScheduleReportWindowViewModel Unavailable(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new AutomaticScheduleReportWindowViewModel(null, message.Trim());
    }
}
