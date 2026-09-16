using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class PlanningPreparationViewModel : ObservableObject
{
    private bool _enableAuxiliaryReliefShift;
    private bool _storedEnableAuxiliaryReliefShift;

    public SchedulePreparationStatus Status { get; private set; }

    public Guid? SnapshotId { get; private set; }

    public PlanningHistoryCompleteness? HistoryCompleteness { get; private set; }

    public string ChangedCategoriesDisplay { get; private set; } = string.Empty;

    public bool EnableAuxiliaryReliefShift
    {
        get => _enableAuxiliaryReliefShift;
        set
        {
            if (SetProperty(ref _enableAuxiliaryReliefShift, value))
            {
                OnPropertyChanged(nameof(HasLocalRunOptionChange));
                OnPropertyChanged(nameof(StatusDisplay));
                OnPropertyChanged(nameof(ActionDisplay));
            }
        }
    }

    public bool HasLocalRunOptionChange => SnapshotId is not null
        && EnableAuxiliaryReliefShift != _storedEnableAuxiliaryReliefShift;

    public bool HasChangedCategories => !string.IsNullOrEmpty(ChangedCategoriesDisplay);

    public string StatusDisplay => (Status, HasLocalRunOptionChange) switch
    {
        (SchedulePreparationStatus.NotPrepared, _) => "Noch nicht vorbereitet",
        (_, true) => "Laufoption geändert – Aktualisierung erforderlich",
        (SchedulePreparationStatus.Prepared, false) => "Vorbereitet und aktuell",
        (SchedulePreparationStatus.Outdated, false) =>
            "Vorbereitung veraltet – Aktualisierung erforderlich",
        _ => "Unbekannter Vorbereitungsstand",
    };

    public string HistoryDisplay => HistoryCompleteness switch
    {
        PlanningHistoryCompleteness.Complete =>
            "Vorgeschichte: sieben vorhergehende Kalendertage vollständig vorhanden",
        PlanningHistoryCompleteness.Partial =>
            "Vorgeschichte: teilweise vorhanden; einzelne Regeln sind nicht vollständig prüfbar",
        PlanningHistoryCompleteness.Missing =>
            "Vorgeschichte: fehlt; einzelne Regeln sind nicht vollständig prüfbar",
        null => "Vorgeschichte wird beim Vorbereiten geprüft",
        _ => "Vorgeschichte: unbekannter Stand",
    };

    public string ActionDisplay => SnapshotId is null
        ? "Planung vorbereiten"
        : "Vorbereitung bewusst aktualisieren";

    public PlanningRunOptions CreateRunOptions()
    {
        return new PlanningRunOptions(EnableAuxiliaryReliefShift);
    }

    public void Apply(ScheduleWorkspaceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Status = snapshot.PreparationStatus;
        SnapshotId = snapshot.PreparedSnapshotId;
        HistoryCompleteness = snapshot.PreparedHistoryCompleteness;
        _storedEnableAuxiliaryReliefShift =
            snapshot.PreparedRunOptions?.EnableAuxiliaryReliefShift ?? false;
        _enableAuxiliaryReliefShift = _storedEnableAuxiliaryReliefShift;
        ChangedCategoriesDisplay = string.Join(
            ", ",
            snapshot.ChangedCategories.Select(GetCategoryDisplay));
        OnPropertyChanged(string.Empty);
    }

    private static string GetCategoryDisplay(PlanningInputChangeCategory category)
    {
        return category switch
        {
            PlanningInputChangeCategory.EmployeesAndTypes =>
                "Mitarbeitende oder Typwerte",
            PlanningInputChangeCategory.ShiftEligibilities => "Einsatzfreigaben",
            PlanningInputChangeCategory.ServiceCatalog => "Einsatzorte oder Dienste",
            PlanningInputChangeCategory.StaffingDemands => "Bedarfe",
            PlanningInputChangeCategory.AvailabilityEntries => "Tageskennzeichen",
            PlanningInputChangeCategory.ServiceManagementAssignments => "Typ1-Einteilungen",
            PlanningInputChangeCategory.RuleCatalog => "Regelkatalog",
            PlanningInputChangeCategory.RunOptions => "Laufoptionen",
            PlanningInputChangeCategory.History => "Vorgeschichte",
            _ => throw new InvalidOperationException(
                $"Unsupported planning change category: {category}"),
        };
    }
}
