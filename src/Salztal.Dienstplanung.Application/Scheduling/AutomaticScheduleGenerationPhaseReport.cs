using System.Collections.ObjectModel;
using System.Globalization;
using Salztal.Dienstplanung.Domain.Rules;
using Salztal.Dienstplanung.Domain.Scheduling.Optimization;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum AutomaticScheduleGenerationPhaseReportStatus
{
    OptimalProven,
    Completed,
    FeasibleNotProvenOptimal,
    NotApplicable,
    Interrupted,
    Failed,
    NotReached,
}

public enum AutomaticScheduleGenerationMetricUnit
{
    Count,
    Minutes,
    BasisPoints,
}

public sealed class AutomaticScheduleGenerationMetricSnapshot
{
    internal AutomaticScheduleGenerationMetricSnapshot(
        string key,
        string name,
        AutomaticScheduleGenerationMetricUnit unit,
        long? initialValue,
        long? achievedValue,
        long? frozenValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!Enum.IsDefined(unit)
            || (initialValue is null && achievedValue is null && frozenValue is null))
        {
            throw new ArgumentException("A generation metric requires a valid value.");
        }

        Key = key.Trim();
        Name = name.Trim();
        Unit = unit;
        InitialValue = initialValue;
        AchievedValue = achievedValue;
        FrozenValue = frozenValue;
    }

    public string Key { get; }

    public string Name { get; }

    public AutomaticScheduleGenerationMetricUnit Unit { get; }

    public long? InitialValue { get; }

    public long? AchievedValue { get; }

    public long? FrozenValue { get; }
}

public sealed record AutomaticScheduleGenerationRuleSnapshot(
    string RuleId,
    string Name,
    int CaseCount,
    long Magnitude);

public sealed class AutomaticScheduleGenerationPhaseReportSnapshot
{
    internal AutomaticScheduleGenerationPhaseReportSnapshot(
        int sequence,
        AutomaticSchedulePhaseKind kind,
        string name,
        string goal,
        AutomaticScheduleGenerationPhaseReportStatus status,
        TimeSpan? duration,
        IEnumerable<AutomaticScheduleGenerationMetricSnapshot> metrics,
        IEnumerable<AutomaticScheduleGenerationRuleSnapshot> rules,
        string explanation,
        string? metricContext)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(goal);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentException.ThrowIfNullOrWhiteSpace(explanation);
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        AutomaticScheduleGenerationMetricSnapshot[] metricValues = metrics.ToArray();
        AutomaticScheduleGenerationRuleSnapshot[] ruleValues = rules.ToArray();
        if (metricValues.Any(value => value is null)
            || metricValues.Select(value => value.Key)
                .Distinct(StringComparer.Ordinal)
                .Count() != metricValues.Length
            || ruleValues.Any(value => value is null)
            || ruleValues.Select(value => value.RuleId)
                .Distinct(StringComparer.Ordinal)
                .Count() != ruleValues.Length)
        {
            throw new ArgumentException(
                "Generation phase metrics and rules must be unique.");
        }

        Sequence = sequence;
        Kind = kind;
        Name = name.Trim();
        Goal = goal.Trim();
        Status = status;
        Duration = duration;
        Metrics = Array.AsReadOnly(metricValues);
        Rules = Array.AsReadOnly(ruleValues);
        Explanation = explanation.Trim();
        MetricContext = string.IsNullOrWhiteSpace(metricContext)
            ? null
            : metricContext.Trim();
    }

    public int Sequence { get; }

    public AutomaticSchedulePhaseKind Kind { get; }

    public string Name { get; }

    public string Goal { get; }

    public AutomaticScheduleGenerationPhaseReportStatus Status { get; }

    public TimeSpan? Duration { get; }

    public ReadOnlyCollection<AutomaticScheduleGenerationMetricSnapshot> Metrics
    {
        get;
    }

    public ReadOnlyCollection<AutomaticScheduleGenerationRuleSnapshot> Rules
    {
        get;
    }

    public string Explanation { get; }

    public string? MetricContext { get; }
}

internal static class AutomaticScheduleGenerationPhaseReportCalculator
{
    private static readonly AutomaticSchedulePhaseKind[] DeclaredSequence =
        Enum.GetValues<AutomaticSchedulePhaseKind>();

    internal static ReadOnlyCollection<AutomaticScheduleGenerationPhaseReportSnapshot>
        Create(
            AutomaticScheduleGenerationStatus generationStatus,
            IEnumerable<AutomaticSchedulePhaseSnapshot> phases,
            AutomaticScheduleObjectiveSnapshot? objective,
            AutomaticSchedulePlanningReport? planningReport)
    {
        ArgumentNullException.ThrowIfNull(phases);
        AutomaticSchedulePhaseSnapshot[] phaseValues = phases.ToArray();
        Dictionary<AutomaticSchedulePhaseKind, AutomaticSchedulePhaseSnapshot> byKind =
            phaseValues.ToDictionary(value => value.Kind);
        AutomaticSchedulePhaseSnapshot? terminalPhase = phaseValues.SingleOrDefault(
            value => value.Status is AutomaticSchedulePhaseStatus.Interrupted
                or AutomaticSchedulePhaseStatus.Failed);
        return Array.AsReadOnly(DeclaredSequence
            .Select((kind, index) => CreatePhase(
                index + 1,
                kind,
                byKind.GetValueOrDefault(kind),
                terminalPhase is not null && terminalPhase.Kind < kind
                    ? terminalPhase
                    : null,
                generationStatus,
                objective,
                planningReport))
            .ToArray());
    }

    private static AutomaticScheduleGenerationPhaseReportSnapshot CreatePhase(
        int sequence,
        AutomaticSchedulePhaseKind kind,
        AutomaticSchedulePhaseSnapshot? phase,
        AutomaticSchedulePhaseSnapshot? precedingTermination,
        AutomaticScheduleGenerationStatus generationStatus,
        AutomaticScheduleObjectiveSnapshot? objective,
        AutomaticSchedulePlanningReport? planningReport)
    {
        (string name, string goal) = Describe(kind);
        return new AutomaticScheduleGenerationPhaseReportSnapshot(
            sequence,
            kind,
            name,
            goal,
            MapStatus(kind, phase, generationStatus),
            phase?.Duration,
            CreateMetrics(kind, phase, objective, planningReport),
            CreateRules(kind, objective),
            Explain(kind, phase, precedingTermination),
            CreateMetricContext(phase));
    }

    private static string Explain(
        AutomaticSchedulePhaseKind kind,
        AutomaticSchedulePhaseSnapshot? phase,
        AutomaticSchedulePhaseSnapshot? precedingTermination)
    {
        if (phase is null)
        {
            return precedingTermination is null
                ? "Für diese Phase wurden bei diesem Lauf keine Detaildaten gespeichert."
                : ExplainNotStarted(precedingTermination);
        }

        if (phase.Termination is not null)
        {
            return ExplainTermination(phase.Termination);
        }

        if (phase.DetailAvailability
            == AutomaticSchedulePhaseDetailAvailability.TerminationDetailsNotRecorded)
        {
            return "Die Phase wurde beendet. Detaildaten zum Abbruchgrund wurden bei diesem älteren Lauf nicht gespeichert.";
        }

        return phase.Status switch
        {
            AutomaticSchedulePhaseStatus.Completed when IsOptimizing(kind) =>
                "Die Optimalität dieses Teilziels wurde nachgewiesen.",
            AutomaticSchedulePhaseStatus.Completed =>
                "Die Phase wurde vollständig abgeschlossen.",
            AutomaticSchedulePhaseStatus.NotApplicable =>
                "Für diesen Lauf war die Phase nicht anwendbar.",
            _ => throw new ArgumentOutOfRangeException(nameof(phase)),
        };
    }

    private static string ExplainTermination(
        AutomaticSchedulePhaseTerminationSnapshot termination)
    {
        string target = termination.ActiveTarget is null
            ? "der laufenden Optimierung"
            : $"dem Teilziel „{DescribeTarget(termination.ActiveTarget.Value)}“";
        return termination.Reason switch
        {
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection =>
                $"Die Zeitgrenze von {FormatDuration(termination.TimeLimit!.Value)} wurde bei {target} nach {FormatDuration(termination.BudgetElapsed!.Value)} erreicht. Der bis dahin gefundene zulässige Zwischenstand wurde behalten; seine Optimalität ist nicht nachgewiesen.",
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection =>
                $"Die Zeitgrenze von {FormatDuration(termination.TimeLimit!.Value)} wurde bei {target} nach {FormatDuration(termination.BudgetElapsed!.Value)} erreicht. Bis dahin wurde keine zulässige Auswahl gefunden.",
            AutomaticSchedulePhaseTerminationReason.CancellationRequested =>
                $"Die Phase wurde auf Anforderung während {target} abgebrochen.",
            AutomaticSchedulePhaseTerminationReason.TechnicalFailure =>
                $"Die Phase wurde während {target} wegen eines technischen Fehlers beendet.",
            _ => throw new ArgumentOutOfRangeException(nameof(termination)),
        };
    }

    private static string ExplainNotStarted(
        AutomaticSchedulePhaseSnapshot precedingTermination)
    {
        string precedingName = Describe(precedingTermination.Kind).Name;
        AutomaticSchedulePhaseTerminationSnapshot? termination =
            precedingTermination.Termination;
        if (termination is null)
        {
            return $"Nicht begonnen nach der beendeten Phase „{precedingName}“. Detaildaten zum Abbruchgrund wurden bei diesem älteren Lauf nicht gespeichert.";
        }

        string reason = termination.Reason switch
        {
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection =>
                "an der Zeitgrenze beendet und der zulässige Zwischenstand behalten wurde",
            AutomaticSchedulePhaseTerminationReason.TimeLimitWithoutFeasibleSelection =>
                "an der Zeitgrenze beendet wurde, weil keine zulässige Auswahl gefunden wurde",
            AutomaticSchedulePhaseTerminationReason.CancellationRequested =>
                "auf Anforderung abgebrochen wurde",
            AutomaticSchedulePhaseTerminationReason.TechnicalFailure =>
                "wegen eines technischen Fehlers beendet wurde",
            _ => throw new ArgumentOutOfRangeException(nameof(precedingTermination)),
        };
        return $"Nicht begonnen, weil die vorherige Phase „{precedingName}“ {reason}.";
    }

    private static string? CreateMetricContext(
        AutomaticSchedulePhaseSnapshot? phase)
    {
        if (phase is null || phase.Values.Count == 0
            || phase.Status is not AutomaticSchedulePhaseStatus.Interrupted
                and not AutomaticSchedulePhaseStatus.Failed)
        {
            return null;
        }

        return phase.Termination?.Reason
            == AutomaticSchedulePhaseTerminationReason.TimeLimitWithFeasibleSelection
                ? "Die Werte beschreiben den behaltenen zulässigen Zwischenstand. Sie sind nicht als Optimum bewiesen."
                : "Die aufgezeichneten Werte sind Teilstände und kein nachgewiesenes Optimum.";
    }

    private static string DescribeTarget(
        AutomaticScheduleOptimizationTargetKind target) => target switch
        {
            AutomaticScheduleOptimizationTargetKind.HardRuleFeasibility =>
                "Zulässigkeit der zwingenden Regeln",
            AutomaticScheduleOptimizationTargetKind.RegularCoveredMinutes =>
                "regulär gedeckte Mitarbeiterminuten",
            AutomaticScheduleOptimizationTargetKind.RegularTouchedDemandSlots =>
                "vollständig berührte reguläre Bedarfsplätze",
            AutomaticScheduleOptimizationTargetKind.ReliefCoveredMinutes =>
                "zusätzlich durch Spr gedeckte Mitarbeiterminuten",
            AutomaticScheduleOptimizationTargetKind.ReliefTouchedDemandSlots =>
                "zusätzlich durch Spr berührte Bedarfsplätze",
            AutomaticScheduleOptimizationTargetKind.HighPriorityRules =>
                "hohe Regeln",
            AutomaticScheduleOptimizationTargetKind.ReliefShiftAssignments =>
                "Spr-Einsätze",
            AutomaticScheduleOptimizationTargetKind.SplitShiftAssignments =>
                "D-Einsätze",
            AutomaticScheduleOptimizationTargetKind.AuxiliaryMinimumFulfillment =>
                "AH-Mindestintegration",
            AutomaticScheduleOptimizationTargetKind.RelativeWeeklyTargetDeviation =>
                "relative Wochenzielannäherung",
            AutomaticScheduleOptimizationTargetKind.MediumPriorityRules =>
                "mittlere Regeln",
            AutomaticScheduleOptimizationTargetKind.LowPriorityRules =>
                "niedrige Regeln",
            AutomaticScheduleOptimizationTargetKind.StabilityRules =>
                "Stabilität und Verteilung",
            AutomaticScheduleOptimizationTargetKind.TechnicalTieBreak =>
                "technischer Gleichstand",
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };

    private static string FormatDuration(TimeSpan duration) => string.Create(
        CultureInfo.GetCultureInfo("de-DE"),
        $"{duration.TotalSeconds:0.000} s");

    private static AutomaticScheduleGenerationPhaseReportStatus MapStatus(
        AutomaticSchedulePhaseKind kind,
        AutomaticSchedulePhaseSnapshot? phase,
        AutomaticScheduleGenerationStatus generationStatus)
    {
        if (phase is null)
        {
            return AutomaticScheduleGenerationPhaseReportStatus.NotReached;
        }

        return phase.Status switch
        {
            AutomaticSchedulePhaseStatus.NotApplicable =>
                AutomaticScheduleGenerationPhaseReportStatus.NotApplicable,
            AutomaticSchedulePhaseStatus.Interrupted
                when generationStatus
                    == AutomaticScheduleGenerationStatus.FeasibleNotProvenOptimal =>
                AutomaticScheduleGenerationPhaseReportStatus.FeasibleNotProvenOptimal,
            AutomaticSchedulePhaseStatus.Interrupted =>
                AutomaticScheduleGenerationPhaseReportStatus.Interrupted,
            AutomaticSchedulePhaseStatus.Failed =>
                AutomaticScheduleGenerationPhaseReportStatus.Failed,
            AutomaticSchedulePhaseStatus.Completed when IsOptimizing(kind) =>
                AutomaticScheduleGenerationPhaseReportStatus.OptimalProven,
            AutomaticSchedulePhaseStatus.Completed =>
                AutomaticScheduleGenerationPhaseReportStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(phase)),
        };
    }

    private static bool IsOptimizing(AutomaticSchedulePhaseKind kind) => kind is
        AutomaticSchedulePhaseKind.RegularCoverage
        or AutomaticSchedulePhaseKind.ReliefCoverage
        or AutomaticSchedulePhaseKind.HighPriorityRules
        or AutomaticSchedulePhaseKind.ReliefShiftMinimization
        or AutomaticSchedulePhaseKind.SplitShiftMinimization
        or AutomaticSchedulePhaseKind.AuxiliaryMinimum
        or AutomaticSchedulePhaseKind.RelativeWeeklyTarget
        or AutomaticSchedulePhaseKind.MediumPriorityRules
        or AutomaticSchedulePhaseKind.LowPriorityRules
        or AutomaticSchedulePhaseKind.Stability
        or AutomaticSchedulePhaseKind.TechnicalTieBreak;

    private static IEnumerable<AutomaticScheduleGenerationMetricSnapshot> CreateMetrics(
        AutomaticSchedulePhaseKind kind,
        AutomaticSchedulePhaseSnapshot? phase,
        AutomaticScheduleObjectiveSnapshot? objective,
        AutomaticSchedulePlanningReport? planningReport)
    {
        if (phase is null)
        {
            return [];
        }

        Dictionary<string, long> values = phase.Values.ToDictionary(
            value => value.Key,
            value => value.Value,
            StringComparer.Ordinal);
        IEnumerable<AutomaticScheduleGenerationMetricSnapshot?> metrics = kind switch
        {
            AutomaticSchedulePhaseKind.InputValidation =>
            [
                Metric(values, "issue_count", "Prüfhinweise", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            AutomaticSchedulePhaseKind.ModelBuilding =>
            [
                Metric(values, "candidate_count", "Zulässige Einsatzmöglichkeiten", AutomaticScheduleGenerationMetricUnit.Count)!,
                Metric(values, "remaining_demand_count", "Noch zu planende Bedarfsabschnitte", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            AutomaticSchedulePhaseKind.RegularCoverage =>
            [
                Metric(values, "required_minutes", "Zu deckende Mitarbeiterminuten", AutomaticScheduleGenerationMetricUnit.Minutes)!,
                Metric(values, "covered_minutes", "Regulär gedeckte Mitarbeiterminuten", AutomaticScheduleGenerationMetricUnit.Minutes, initialValue: 0)!,
                Metric(values, "uncovered_minutes", "Nach regulärer Deckung offene Mitarbeiterminuten", AutomaticScheduleGenerationMetricUnit.Minutes)!,
                Metric(values, "covered_full_demand_count", "Regulär vollständig erreichte Bedarfsplätze", AutomaticScheduleGenerationMetricUnit.Count)!,
                Metric(values, "fully_uncovered_demand_count", "Nach regulärer Deckung vollständig offene Bedarfsplätze", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            AutomaticSchedulePhaseKind.ReliefCoverage =>
            [
                Metric(values, "covered_minutes", "Mit Spr gedeckte Mitarbeiterminuten", AutomaticScheduleGenerationMetricUnit.Minutes, Initial(values, "initial_covered_minutes"))!,
                Metric(values, "additional_covered_minutes", "Durch Spr zusätzlich gedeckte Mitarbeiterminuten", AutomaticScheduleGenerationMetricUnit.Minutes)!,
                Metric(values, "uncovered_minutes", "Nach Spr-Notfalldeckung offene Mitarbeiterminuten", AutomaticScheduleGenerationMetricUnit.Minutes, Initial(values, "initial_uncovered_minutes"))!,
                Metric(values, "covered_full_demand_count", "Mit Spr vollständig erreichte Bedarfsplätze", AutomaticScheduleGenerationMetricUnit.Count)!,
                Metric(values, "fully_uncovered_demand_count", "Nach Spr-Notfalldeckung vollständig offene Bedarfsplätze", AutomaticScheduleGenerationMetricUnit.Count, Initial(values, "initial_fully_uncovered_demand_count"))!,
            ],
            AutomaticSchedulePhaseKind.HighPriorityRules =>
            [
                Metric(values, "violation_count", "Verletzte Fälle hoher Regeln", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            AutomaticSchedulePhaseKind.ReliefShiftMinimization =>
            [
                Metric(values, "assignment_count", "Spr-Einsätze", AutomaticScheduleGenerationMetricUnit.Count, Initial(values, "initial_assignment_count"))!,
                PlanPatternTotal(
                    "plan_total_assignment_count",
                    "Spr-Einsätze im vollständigen Plan",
                    planningReport,
                    reliefShift: true)!,
            ],
            AutomaticSchedulePhaseKind.SplitShiftMinimization =>
            [
                Metric(values, "assignment_count", "D-Einsätze", AutomaticScheduleGenerationMetricUnit.Count, Initial(values, "initial_assignment_count"))!,
                PlanPatternTotal(
                    "plan_total_assignment_count",
                    "D-Einsätze im vollständigen Plan",
                    planningReport,
                    reliefShift: false)!,
            ],
            AutomaticSchedulePhaseKind.AuxiliaryMinimum =>
            [
                Metric(values, "violation_count", "AH-Wochen unter dem Mindestziel", AutomaticScheduleGenerationMetricUnit.Count)!,
                Metric(values, "missing_minutes", "Fehlende AH-Mindestminuten", AutomaticScheduleGenerationMetricUnit.Minutes)!,
            ],
            AutomaticSchedulePhaseKind.RelativeWeeklyTarget =>
                RelativeMetrics(objective),
            AutomaticSchedulePhaseKind.MediumPriorityRules =>
            [
                Metric(values, "violation_count", "Verletzte Fälle mittlerer Regeln", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            AutomaticSchedulePhaseKind.Stability =>
            [
                Metric(values, "total_spread", "Gesamte Verteilungsspannweite", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            AutomaticSchedulePhaseKind.TechnicalTieBreak when objective is not null =>
            [
                ValueMetric("technical_key_count", "Technische Gleichstandsschlüssel", AutomaticScheduleGenerationMetricUnit.Count, objective.TechnicalTieBreakerKeys.Count),
            ],
            AutomaticSchedulePhaseKind.ResultMapping =>
            [
                Metric(values, "assignment_count", "Abgebildete Einteilungen", AutomaticScheduleGenerationMetricUnit.Count)!,
            ],
            _ => [],
        };
        return metrics.OfType<AutomaticScheduleGenerationMetricSnapshot>();
    }

    private static List<AutomaticScheduleGenerationMetricSnapshot> RelativeMetrics(
        AutomaticScheduleObjectiveSnapshot? objective)
    {
        if (objective is null)
        {
            return [];
        }

        AutomaticScheduleRelativeWeeklyTargetCase[] comparable = objective
            .RelativeWeeklyTargetCases
            .Where(value => value.TargetMinutes > 0)
            .ToArray();
        long totalDeviation = comparable.Sum(value => Math.Abs(
            (long)value.AssignedMinutes - value.TargetMinutes));
        long maximumBasisPoints = comparable.Length == 0
            ? 0
            : comparable.Max(value => checked(
                Math.Abs((long)value.AssignedMinutes - value.TargetMinutes)
                    * 10_000L / value.TargetMinutes));
        return
        [
            ValueMetric("case_count", "Verglichene Personenwochen", AutomaticScheduleGenerationMetricUnit.Count, comparable.Length),
            ValueMetric("total_deviation_minutes", "Gesamte absolute Wochenzielabweichung", AutomaticScheduleGenerationMetricUnit.Minutes, totalDeviation),
            ValueMetric("maximum_deviation_basis_points", "Größte relative Wochenzielabweichung", AutomaticScheduleGenerationMetricUnit.BasisPoints, maximumBasisPoints),
        ];
    }

    private static AutomaticScheduleGenerationMetricSnapshot? Metric(
        Dictionary<string, long> values,
        string key,
        string name,
        AutomaticScheduleGenerationMetricUnit unit,
        long? initialValue = null)
    {
        return values.TryGetValue(key, out long value)
            ? new AutomaticScheduleGenerationMetricSnapshot(
                key,
                name,
                unit,
                initialValue,
                value,
                value)
            : null;
    }

    private static long? Initial(
        Dictionary<string, long> values,
        string key) => values.TryGetValue(key, out long value) ? value : null;

    private static AutomaticScheduleGenerationMetricSnapshot ValueMetric(
        string key,
        string name,
        AutomaticScheduleGenerationMetricUnit unit,
        long value) => new(key, name, unit, null, value, value);

    private static AutomaticScheduleGenerationMetricSnapshot? PlanPatternTotal(
        string key,
        string name,
        AutomaticSchedulePlanningReport? planningReport,
        bool reliefShift)
    {
        if (planningReport is null)
        {
            return null;
        }

        int total = planningReport.EmployeeWeekSummaries.Sum(value => reliefShift
            ? value.ReliefShiftCount
            : value.SplitShiftCount);
        return ValueMetric(
            key,
            name,
            AutomaticScheduleGenerationMetricUnit.Count,
            total);
    }

    private static IEnumerable<AutomaticScheduleGenerationRuleSnapshot> CreateRules(
        AutomaticSchedulePhaseKind kind,
        AutomaticScheduleObjectiveSnapshot? objective)
    {
        if (objective is null)
        {
            return [];
        }

        return kind switch
        {
            AutomaticSchedulePhaseKind.HighPriorityRules => RuleRows(
                CurrentSoftRuleDefinitions.All.Where(value =>
                    value.Priority == RulePriority.High),
                objective.HighPriorityViolations),
            AutomaticSchedulePhaseKind.MediumPriorityRules => RuleRows(
                [CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend],
                objective.MediumPriorityViolations),
            AutomaticSchedulePhaseKind.LowPriorityRules => RuleRows(
                [],
                objective.LowPriorityViolations),
            AutomaticSchedulePhaseKind.Stability => RuleRows(
                InitialStabilityRuleDefinitions.All,
                objective.StabilityViolations),
            _ => [],
        };
    }

    private static IEnumerable<AutomaticScheduleGenerationRuleSnapshot> RuleRows(
        IEnumerable<RuleDefinition> definitions,
        IEnumerable<AutomaticScheduleRuleViolation> violations)
    {
        AutomaticScheduleRuleViolation[] values = violations.ToArray();
        return definitions.Select(definition =>
        {
            AutomaticScheduleRuleViolation[] ruleValues = values
                .Where(value => value.RuleId == definition.Id.Value)
                .ToArray();
            return new AutomaticScheduleGenerationRuleSnapshot(
                definition.Id.Value,
                RuleName(definition.Id.Value),
                ruleValues.Length,
                ruleValues.Sum(value => value.Magnitude));
        });
    }

    private static string RuleName(string ruleId) => ruleId switch
    {
        "NORMAL_WEEKLY_MINIMUM" => "Mindestannäherung regulärer Personen",
        "WEEKLY_CONSECUTIVE_DAYS_OFF" => "Zusammenhängende freie Tage je Woche",
        "RED_X_ADJACENT_DAY_OFF" => "Freier Nachbartag zu rotem X",
        "PRE_VACATION_WEEKEND_FREE" => "Freies Wochenende vor Urlaub",
        "SPLIT_SHIFT_WEEKLY_MAXIMUM" => "Höchstens ein D je Woche",
        "THREE_WEEK_FREE_WEEKEND" => "Freies Wochenende im Drei-Wochen-Zeitraum",
        "CURRENT_PERIOD_FAIR_DISTRIBUTION" => "Stabile Verteilung belastender Dienste",
        _ => ruleId,
    };

    private static (string Name, string Goal) Describe(
        AutomaticSchedulePhaseKind kind) => kind switch
        {
            AutomaticSchedulePhaseKind.InputValidation =>
                ("Eingangs- und Strukturprüfung", "Widersprüchliche oder nicht unterstützte Eingaben vor der Berechnung erkennen."),
            AutomaticSchedulePhaseKind.ModelBuilding =>
                ("Modellaufbau", "Zulässige Einsatzmöglichkeiten und verbleibenden Bedarf technisch aufbauen."),
            AutomaticSchedulePhaseKind.HardRules =>
                ("Zwingende Regeln", "Alle bestätigten nicht verletzbaren Regeln durchgehend sichern."),
            AutomaticSchedulePhaseKind.RegularCoverage =>
                ("Reguläre Bedarfsdeckung", "Ohne Spr möglichst viele Mitarbeiterminuten und vollständige Bedarfsplätze decken."),
            AutomaticSchedulePhaseKind.ReliefCoverage =>
                ("Zusätzliche Spr-Notfalldeckung", "Nur im bestätigten Notfall weitere Bedarfszeit durch Spr decken."),
            AutomaticSchedulePhaseKind.HighPriorityRules =>
                ("Hohe Regeln", "Zuerst die Zahl verletzter Fälle hoher Regeln minimieren."),
            AutomaticSchedulePhaseKind.ReliefShiftMinimization =>
                ("Spr minimieren", "Bei festgehaltener Deckung und hohen Regeln möglichst wenige Spr einsetzen."),
            AutomaticSchedulePhaseKind.SplitShiftMinimization =>
                ("D minimieren", "Nach Spr möglichst wenige D einsetzen."),
            AutomaticSchedulePhaseKind.AuxiliaryMinimum =>
                ("AH-Mindestintegration", "Geeignete AH möglichst auf mindestens drei Stunden je Woche bringen."),
            AutomaticSchedulePhaseKind.RelativeWeeklyTarget =>
                ("Relative Wochenzielannäherung", "Alle Personen relativ fair an ihr persönliches Wochenziel annähern."),
            AutomaticSchedulePhaseKind.MediumPriorityRules =>
                ("Mittlere Regeln", "Verletzungen mittlerer Regeln minimieren."),
            AutomaticSchedulePhaseKind.LowPriorityRules =>
                ("Niedrige Regeln", "Vorhandene niedrige Regeln nach den höheren Zielen optimieren."),
            AutomaticSchedulePhaseKind.Stability =>
                ("Stabilität und Verteilung", "Den aktuellen Plan nach den bestätigten Stabilitätswerten ordnen."),
            AutomaticSchedulePhaseKind.TechnicalTieBreak =>
                ("Technischer Gleichstand", "Gleichwertige Lösungen reproduzierbar auswählen."),
            AutomaticSchedulePhaseKind.ResultMapping =>
                ("Ergebnisabbildung", "Das geprüfte technische Ergebnis verlustfrei in den Vorschlag übertragen."),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
}
