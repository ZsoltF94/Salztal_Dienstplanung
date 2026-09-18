using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed class PlanningDemandReportViewModel : ObservableObject
{
    private readonly AutomaticSchedulePlanningReport _report;
    private PlanningDemandWeekViewModel _selectedWeek;
    private ReadOnlyCollection<PlanningDemandRowViewModel> _demands;
    private bool _showOnlyOpenDemands;

    public PlanningDemandReportViewModel(AutomaticSchedulePlanningReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        _report = report;
        Weeks = Array.AsReadOnly(report.DemandWeeks
            .Select((summary, index) => new PlanningDemandWeekViewModel(index + 1, summary))
            .ToArray());
        _selectedWeek = Weeks[0];
        _demands = CreateVisibleDemands();
        TotalSummary = new PlanningDemandSummaryViewModel(report.DemandTotal);
    }

    public ReadOnlyCollection<PlanningDemandWeekViewModel> Weeks { get; }

    public PlanningDemandWeekViewModel SelectedWeek
    {
        get => _selectedWeek;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (!Weeks.Contains(value))
            {
                throw new ArgumentException(
                    "The selected week must belong to this report.",
                    nameof(value));
            }

            if (SetProperty(ref _selectedWeek, value))
            {
                RefreshVisibleDemands();
            }
        }
    }

    public bool ShowOnlyOpenDemands
    {
        get => _showOnlyOpenDemands;
        set
        {
            if (SetProperty(ref _showOnlyOpenDemands, value))
            {
                RefreshVisibleDemands();
            }
        }
    }

    public ReadOnlyCollection<PlanningDemandRowViewModel> Demands
    {
        get => _demands;
        private set => SetProperty(ref _demands, value);
    }

    public PlanningDemandSummaryViewModel SelectedWeekSummary => SelectedWeek.Summary;

    public PlanningDemandSummaryViewModel TotalSummary { get; }

    private void RefreshVisibleDemands()
    {
        Demands = CreateVisibleDemands();
        OnPropertyChanged(nameof(SelectedWeekSummary));
    }

    private ReadOnlyCollection<PlanningDemandRowViewModel> CreateVisibleDemands()
    {
        DateOnly weekMonday = SelectedWeek.Summary.PeriodMonday;
        DateOnly weekSunday = SelectedWeek.Summary.PeriodSunday;
        return Array.AsReadOnly(_report.Demands
            .Where(value => value.Date >= weekMonday && value.Date <= weekSunday)
            .Where(value => !ShowOnlyOpenDemands
                || value.Status != PlanningDemandCoverageStatus.FullyCovered)
            .Select(value => new PlanningDemandRowViewModel(value))
            .ToArray());
    }
}
