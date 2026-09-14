using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.StaffingDemands;

internal sealed class StaffingDemandOverviewViewModel : ObservableObject
{
    private readonly GetStaffingDemandWeekQuery _query;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private DateTime _selectedWeekMonday;
    private bool _isLoading;
    private string? _loadErrorMessage;
    private long _totalRequiredWorkMinutes;

    public StaffingDemandOverviewViewModel(
        GetStaffingDemandWeekQuery query,
        SaveStaffingDemandDateExceptionCommand saveDateExceptionCommand,
        RemoveStaffingDemandDateExceptionCommand removeDateExceptionCommand,
        DateOnly initialWeekMonday,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(saveDateExceptionCommand);
        ArgumentNullException.ThrowIfNull(removeDateExceptionCommand);
        ArgumentNullException.ThrowIfNull(errorReporter);

        if (initialWeekMonday.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(
                "The initial staffing-demand week must start on a Monday.",
                nameof(initialWeekMonday));
        }

        _query = query;
        _errorReporter = errorReporter;
        _selectedWeekMonday = initialWeekMonday.ToDateTime(TimeOnly.MinValue);
        Demands = [];
        WorkLocations = [];
        LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
        PreviousWeekCommand = new AsyncRelayCommand(
            cancellationToken => ChangeWeekAsync(-7, cancellationToken),
            CanLoad);
        NextWeekCommand = new AsyncRelayCommand(
            cancellationToken => ChangeWeekAsync(7, cancellationToken),
            CanLoad);
        DateEditor = new DateStaffingDemandEditorViewModel(
            saveDateExceptionCommand,
            removeDateExceptionCommand,
            LoadAsync,
            errorReporter);
    }

    public ObservableCollection<StaffingDemandItemViewModel> Demands { get; }

    public ObservableCollection<StaffingDemandWorkLocationGroupViewModel> WorkLocations { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand PreviousWeekCommand { get; }

    public IAsyncRelayCommand NextWeekCommand { get; }

    public DateStaffingDemandEditorViewModel DateEditor { get; }

    public DateTime SelectedWeekMonday
    {
        get => _selectedWeekMonday;
        set
        {
            DateOnly selectedDate = DateOnly.FromDateTime(value);
            DateOnly monday = GetWeekMonday(selectedDate);
            DateTime normalizedValue = monday.ToDateTime(TimeOnly.MinValue);

            if (SetProperty(ref _selectedWeekMonday, normalizedValue))
            {
                OnPropertyChanged(nameof(SelectedWeekRangeDisplay));
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                NotifyViewStateChanged();
                LoadCommand.NotifyCanExecuteChanged();
                PreviousWeekCommand.NotifyCanExecuteChanged();
                NextWeekCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string? LoadErrorMessage
    {
        get => _loadErrorMessage;
        private set
        {
            if (SetProperty(ref _loadErrorMessage, value))
            {
                NotifyViewStateChanged();
            }
        }
    }

    public long TotalRequiredWorkMinutes
    {
        get => _totalRequiredWorkMinutes;
        private set
        {
            if (SetProperty(ref _totalRequiredWorkMinutes, value))
            {
                OnPropertyChanged(nameof(TotalRequiredWorkDisplay));
            }
        }
    }

    public string SelectedWeekRangeDisplay
    {
        get
        {
            DateOnly monday = DateOnly.FromDateTime(SelectedWeekMonday);
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{monday:dd.MM.yyyy} bis {monday.AddDays(6):dd.MM.yyyy}");
        }
    }

    public string TotalRequiredWorkDisplay =>
        StaffingDemandDisplayFormatter.FormatMinutes(TotalRequiredWorkMinutes);

    public bool HasLoadError => !string.IsNullOrWhiteSpace(LoadErrorMessage);

    public bool HasDemands => Demands.Count > 0;

    public bool HasWorkLocations => WorkLocations.Count > 0;

    public bool IsEmpty => !IsLoading && !HasLoadError && !HasWorkLocations;

    private bool CanLoad()
    {
        return !IsLoading;
    }

    private async Task ChangeWeekAsync(int days, CancellationToken cancellationToken)
    {
        SelectedWeekMonday = SelectedWeekMonday.AddDays(days);
        await LoadAsync(cancellationToken);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    internal async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        LoadErrorMessage = null;
        ClearSnapshot();

        try
        {
            DateOnly requestedMonday = DateOnly.FromDateTime(SelectedWeekMonday);
            StaffingDemandWeekQueryResult result = await _query.ExecuteAsync(
                requestedMonday,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (!result.IsSuccess)
            {
                LoadErrorMessage = CreateLoadErrorMessage(result.Errors);
                return;
            }

            ApplySnapshot(
                result.Value
                ?? throw new InvalidOperationException(
                    "A successful staffing-demand query returned no week snapshot."),
                requestedMonday);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadStaffingDemandWeek");
            LoadErrorMessage =
                "Die Bedarfswoche konnte nicht geladen werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsLoading = false;
            NotifyViewStateChanged();
        }
    }

    private void ApplySnapshot(StaffingDemandWeekSnapshot snapshot, DateOnly requestedMonday)
    {
        if (snapshot.WeekMonday != requestedMonday)
        {
            throw new InvalidOperationException(
                "The staffing-demand snapshot does not match the requested week.");
        }

        foreach (StaffingDemandItemSnapshot demand in snapshot.Demands)
        {
            Demands.Add(new StaffingDemandItemViewModel(demand));
        }

        foreach (StaffingDemandWorkLocationWeekSummarySnapshot locationSummary in
                 snapshot.WorkLocationSummaries)
        {
            WorkLocations.Add(CreateWorkLocationGroup(snapshot, locationSummary));
        }

        foreach (DateStaffingDemandEditItemSnapshot location in snapshot.DateEditItems
                     .GroupBy(item => item.WorkLocationId)
                     .Where(group => WorkLocations.All(existing => existing.Id != group.Key))
                     .Select(group => group.First())
                     .OrderBy(item => item.WorkLocationName, StringComparer.CurrentCulture))
        {
            WorkLocations.Add(CreateWorkLocationGroup(
                snapshot,
                new StaffingDemandWorkLocationWeekSummarySnapshot(
                    location.WorkLocationId,
                    location.WorkLocationName,
                    location.WorkLocationColorCode,
                    0)));
        }

        DateEditor.ApplySnapshot(snapshot.DateEditItems);
        TotalRequiredWorkMinutes = snapshot.TotalRequiredWorkMinutes;
        NotifyViewStateChanged();
    }

    private StaffingDemandWorkLocationGroupViewModel CreateWorkLocationGroup(
        StaffingDemandWeekSnapshot snapshot,
        StaffingDemandWorkLocationWeekSummarySnapshot locationSummary)
    {
        List<StaffingDemandDayGroupViewModel> days = [];

        for (int dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            DateOnly date = snapshot.WeekMonday.AddDays(dayOffset);
            StaffingDemandItemViewModel[] dayDemands = Demands
                .Where(demand => demand.WorkLocationId == locationSummary.WorkLocationId)
                .Where(demand => demand.Date == date)
                .OrderBy(demand => demand.ActualStart)
                .ThenBy(demand => demand.ShiftTypeName, StringComparer.CurrentCulture)
                .ToArray();
            long dayMinutes = snapshot.DaySummaries
                .SingleOrDefault(summary => summary.Date == date
                    && summary.WorkLocationId == locationSummary.WorkLocationId)
                ?.RequiredWorkMinutes ?? 0;

            bool hasDateException = snapshot.DateEditItems.Any(item =>
                item.Date == date
                && item.WorkLocationId == locationSummary.WorkLocationId
                && item.HasDateException);
            days.Add(new StaffingDemandDayGroupViewModel(
                date,
                locationSummary.WorkLocationId,
                locationSummary.WorkLocationName,
                dayDemands,
                dayMinutes,
                hasDateException));
        }

        return new StaffingDemandWorkLocationGroupViewModel(
            locationSummary.WorkLocationId,
            locationSummary.WorkLocationName,
            locationSummary.WorkLocationColorCode,
            days,
            locationSummary.RequiredWorkMinutes);
    }

    private void ClearSnapshot()
    {
        Demands.Clear();
        WorkLocations.Clear();
        TotalRequiredWorkMinutes = 0;
        NotifyViewStateChanged();
    }

    private void NotifyViewStateChanged()
    {
        OnPropertyChanged(nameof(HasLoadError));
        OnPropertyChanged(nameof(HasDemands));
        OnPropertyChanged(nameof(HasWorkLocations));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private static string CreateLoadErrorMessage(
        IEnumerable<StaffingDemandWeekQueryError> errors)
    {
        string details = string.Join(" ", errors.Select(error => error.Message));
        return string.IsNullOrWhiteSpace(details)
            ? "Die Bedarfswoche konnte nicht geladen werden."
            : details;
    }

    private static DateOnly GetWeekMonday(DateOnly date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
