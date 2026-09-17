using System.Globalization;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal static class AutomaticScheduleGenerationPresentation
{
    public static string CreateStartAvailability(
        AutomaticScheduleGenerationContext? context,
        bool hasLocalRunOptionChange,
        bool isParentBusy,
        bool isRunning,
        bool isAccepting,
        bool hasPreview)
    {
        if (isRunning)
        {
            return "Die automatische Planung läuft. Andere Änderungen am Dienstplan sind vorübergehend gesperrt.";
        }

        if (isAccepting)
        {
            return "Der Vorschlag wird vollständig übernommen.";
        }

        if (hasPreview)
        {
            return "Bitte übernimm oder verwirf zuerst den vorhandenen Vorschlag.";
        }

        if (isParentBusy)
        {
            return "Bitte warte, bis der aktuelle Dienstplanvorgang abgeschlossen ist.";
        }

        if (context is null)
        {
            return "Lade zuerst einen Drei-Wochen-Zeitraum.";
        }

        if (!context.IsServiceManagementReady)
        {
            return "Vervollständige zuerst die erforderlichen Typ1-Dienste.";
        }

        if (context.PreparedSnapshotId is null
            || context.PreparationStatus == SchedulePreparationStatus.NotPrepared)
        {
            return "Bereite den Zeitraum zuerst für die automatische Planung vor.";
        }

        if (hasLocalRunOptionChange
            || context.PreparationStatus == SchedulePreparationStatus.Outdated)
        {
            return "Aktualisiere zuerst die veraltete Planungsvorbereitung.";
        }

        return "Die aktuelle Vorbereitung kann automatisch geplant werden.";
    }

    public static string? CreateErrorCodeDisplay(
        AutomaticScheduleGenerationOutcome outcome)
    {
        if (outcome.PlanningErrors.Count == 0)
        {
            return outcome.Status is AutomaticScheduleGenerationStatus.Optimal
                or AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal
                or AutomaticScheduleGenerationStatus.Cancelled
                    ? null
                    : $"Planungscode: {outcome.Status}";
        }

        string codes = string.Join(
            ", ",
            outcome.PlanningErrors.Select(error => error.Code).Distinct());
        string? correlation = outcome.PlanningErrors
            .Select(error => error.CorrelationId)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        return correlation is null
            ? $"Planungscode: {codes}"
            : $"Planungscode: {codes} · Fehlerkennung: {correlation}";
    }

    public static string CreateElapsedDisplay(TimeSpan elapsed, TimeSpan maximumRuntime)
    {
        return string.Create(
            CultureInfo.GetCultureInfo("de-DE"),
            $"Verstrichen: {(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00} · maximal {(int)maximumRuntime.TotalMinutes:00}:{maximumRuntime.Seconds:00} min");
    }
}
