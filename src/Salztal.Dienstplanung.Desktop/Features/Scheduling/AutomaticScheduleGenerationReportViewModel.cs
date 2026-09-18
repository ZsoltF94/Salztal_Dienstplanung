using System.Collections.ObjectModel;
using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class AutomaticScheduleGenerationReportViewModel
{
    public AutomaticScheduleGenerationReportViewModel(
        AutomaticScheduleGenerationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        StatusDisplay = Status(report.Status);
        LastReachedPhaseDisplay = report.LastReachedPhase is null
            ? "Keine Planungsphase erreicht"
            : $"Zuletzt erreicht: {report.PhaseReports.Single(value =>
                value.Kind == report.LastReachedPhase).Name}";
        MeasuredDurationDisplay =
            $"Gemessene Phasenzeit: {FormatDuration(report.MeasuredPhaseDuration)}";
        Phases = Array.AsReadOnly(report.PhaseReports
            .Select(value => new AutomaticScheduleGenerationPhaseViewModel(value))
            .ToArray());
        Errors = Array.AsReadOnly(report.Errors
            .Select(value => new AutomaticScheduleGenerationErrorViewModel(value))
            .ToArray());
        TechnicalDetails = report.TechnicalDetails is null
            ? null
            : new AutomaticScheduleGenerationTechnicalViewModel(
                report.TechnicalDetails);
        HasPlanningReport = report.PlanningReport is not null;
    }

    public string StatusDisplay { get; }

    public string LastReachedPhaseDisplay { get; }

    public string MeasuredDurationDisplay { get; }

    public bool HasPlanningReport { get; }

    public ReadOnlyCollection<AutomaticScheduleGenerationPhaseViewModel> Phases
    {
        get;
    }

    public ReadOnlyCollection<AutomaticScheduleGenerationErrorViewModel> Errors
    {
        get;
    }

    public AutomaticScheduleGenerationTechnicalViewModel? TechnicalDetails { get; }

    private static string Status(AutomaticScheduleGenerationStatus status) => status switch
    {
        AutomaticScheduleGenerationStatus.Optimal =>
            "Optimalität für den vollständigen Lauf nachgewiesen",
        AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal =>
            "Zulässiger Vorschlag – Optimalität nicht vollständig nachgewiesen",
        AutomaticScheduleGenerationStatus.Cancelled => "Lauf abgebrochen",
        AutomaticScheduleGenerationStatus.TimedOutWithoutFeasibleResult =>
            "Zeitgrenze ohne zulässigen Vorschlag erreicht",
        AutomaticScheduleGenerationStatus.BlockedByInput =>
            "Durch vorbereitete Eingaben blockiert",
        AutomaticScheduleGenerationStatus.UnsupportedRule =>
            "Durch nicht unterstützte Regel blockiert",
        AutomaticScheduleGenerationStatus.ConcurrentRun =>
            "Durch bereits laufende Generierung blockiert",
        AutomaticScheduleGenerationStatus.TechnicalFailure => "Technischer Fehler",
        AutomaticScheduleGenerationStatus.ValidationFailed =>
            "Ungültiger Generierungsauftrag",
        AutomaticScheduleGenerationStatus.DraftNotFound => "Entwurf nicht gefunden",
        AutomaticScheduleGenerationStatus.PreparationMissing =>
            "Planungsvorbereitung fehlt",
        AutomaticScheduleGenerationStatus.PreparationOutdated =>
            "Planungsvorbereitung ist veraltet",
        AutomaticScheduleGenerationStatus.Conflict => "Zwischenzeitliche Änderung",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    internal static string FormatDuration(TimeSpan duration) => string.Create(
        CultureInfo.GetCultureInfo("de-DE"),
        $"{duration.TotalSeconds:0.000} s");
}

internal sealed class AutomaticScheduleGenerationPhaseViewModel
{
    public AutomaticScheduleGenerationPhaseViewModel(
        AutomaticScheduleGenerationPhaseReportSnapshot phase)
    {
        ArgumentNullException.ThrowIfNull(phase);
        Sequence = phase.Sequence;
        Name = phase.Name;
        Goal = phase.Goal;
        Explanation = phase.Explanation;
        MetricContext = phase.MetricContext;
        HasMetricContext = MetricContext is not null;
        StatusDisplay = phase.Status switch
        {
            AutomaticScheduleGenerationPhaseReportStatus.OptimalProven =>
                "Optimal bewiesen",
            AutomaticScheduleGenerationPhaseReportStatus.Completed => "Abgeschlossen",
            AutomaticScheduleGenerationPhaseReportStatus.FeasibleNotProvenOptimal =>
                "Zulässig, nicht optimal bewiesen",
            AutomaticScheduleGenerationPhaseReportStatus.NotApplicable =>
                "Nicht anwendbar",
            AutomaticScheduleGenerationPhaseReportStatus.Interrupted => "Abgebrochen",
            AutomaticScheduleGenerationPhaseReportStatus.Failed => "Fehlgeschlagen",
            AutomaticScheduleGenerationPhaseReportStatus.NotReached =>
                "Keine aufgezeichnete Ausführung",
            _ => throw new ArgumentOutOfRangeException(nameof(phase)),
        };
        DurationDisplay = phase.Duration is null
            ? "Keine Laufzeit verfügbar"
            : AutomaticScheduleGenerationReportViewModel.FormatDuration(
                phase.Duration.Value);
        Metrics = Array.AsReadOnly(phase.Metrics
            .Select(value => new AutomaticScheduleGenerationMetricViewModel(value))
            .ToArray());
        Rules = Array.AsReadOnly(phase.Rules
            .Select(value => new AutomaticScheduleGenerationRuleViewModel(value))
            .ToArray());
    }

    public int Sequence { get; }

    public string Name { get; }

    public string Goal { get; }

    public string Explanation { get; }

    public string? MetricContext { get; }

    public bool HasMetricContext { get; }

    public string StatusDisplay { get; }

    public string DurationDisplay { get; }

    public ReadOnlyCollection<AutomaticScheduleGenerationMetricViewModel> Metrics
    {
        get;
    }

    public ReadOnlyCollection<AutomaticScheduleGenerationRuleViewModel> Rules
    {
        get;
    }
}

internal sealed class AutomaticScheduleGenerationMetricViewModel
{
    public AutomaticScheduleGenerationMetricViewModel(
        AutomaticScheduleGenerationMetricSnapshot metric)
    {
        ArgumentNullException.ThrowIfNull(metric);
        Name = metric.Name;
        InitialDisplay = Format(metric.InitialValue, metric.Unit);
        AchievedDisplay = Format(metric.AchievedValue, metric.Unit);
        FrozenDisplay = Format(metric.FrozenValue, metric.Unit);
    }

    public string Name { get; }

    public string InitialDisplay { get; }

    public string AchievedDisplay { get; }

    public string FrozenDisplay { get; }

    private static string Format(
        long? value,
        AutomaticScheduleGenerationMetricUnit unit)
    {
        if (value is null)
        {
            return "–";
        }

        return unit switch
        {
            AutomaticScheduleGenerationMetricUnit.Count => value.Value.ToString(
                "N0",
                CultureInfo.GetCultureInfo("de-DE")),
            AutomaticScheduleGenerationMetricUnit.Minutes =>
                PlanningEmployeeReportPresentation.FormatMinutes(
                    checked((int)value.Value)),
            AutomaticScheduleGenerationMetricUnit.BasisPoints => string.Create(
                CultureInfo.GetCultureInfo("de-DE"),
                $"{value.Value / 100m:0.00} %"),
            _ => throw new ArgumentOutOfRangeException(nameof(unit)),
        };
    }
}

internal sealed record AutomaticScheduleGenerationRuleViewModel
{
    public AutomaticScheduleGenerationRuleViewModel(
        AutomaticScheduleGenerationRuleSnapshot rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        RuleId = rule.RuleId;
        Name = rule.Name;
        CaseCount = rule.CaseCount;
        Magnitude = rule.Magnitude;
        Display = $"{rule.Name}: {rule.CaseCount} Fälle, Ausmaß {rule.Magnitude}";
    }

    public string RuleId { get; }

    public string Name { get; }

    public int CaseCount { get; }

    public long Magnitude { get; }

    public string Display { get; }
}

internal sealed class AutomaticScheduleGenerationTechnicalViewModel
{
    public AutomaticScheduleGenerationTechnicalViewModel(
        AutomaticScheduleRunMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        SolverDisplay = $"{metadata.SolverName} {metadata.SolverVersion}";
        TimeLimitDisplay =
            $"Zeitgrenze: {AutomaticScheduleGenerationReportViewModel.FormatDuration(metadata.TimeLimit)}";
        TotalDurationDisplay =
            $"Gesamtlaufzeit: {AutomaticScheduleGenerationReportViewModel.FormatDuration(metadata.TotalDuration)}";
        Settings = Array.AsReadOnly(metadata.Settings
            .Select(value => $"{value.Key}: {value.Value}")
            .ToArray());
    }

    public string SolverDisplay { get; }

    public string TimeLimitDisplay { get; }

    public string TotalDurationDisplay { get; }

    public ReadOnlyCollection<string> Settings { get; }
}

internal sealed class AutomaticScheduleGenerationErrorViewModel
{
    public AutomaticScheduleGenerationErrorViewModel(AutomaticScheduleError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        CodeDisplay = $"Fehlercode: {error.Code}";
        RuleDisplay = string.IsNullOrWhiteSpace(error.RuleId)
            ? null
            : $"Regel: {error.RuleId}";
        CorrelationDisplay = string.IsNullOrWhiteSpace(error.CorrelationId)
            ? null
            : $"Fehlerkennung: {error.CorrelationId}";
        TechnicalStageDisplay = error.TechnicalDetails is null
            ? null
            : $"Technische Stufe: {error.TechnicalDetails.Stage}";
    }

    public string CodeDisplay { get; }

    public string? RuleDisplay { get; }

    public string? CorrelationDisplay { get; }

    public string? TechnicalStageDisplay { get; }
}
