using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

internal sealed class ShiftTypeEditorViewModel : ObservableObject
{
    private static readonly ReadOnlyCollection<ShiftTypeTimeOption> AvailableTimes =
        Array.AsReadOnly(
            Enumerable.Range(0, 48)
                .Select(index => new ShiftTypeTimeOption(
                    TimeOnly.MinValue.AddMinutes(index * 30)))
                .ToArray());

    private readonly UpdateShiftTypeStandardTimeCommand _updateCommand;
    private readonly Func<CancellationToken, Task> _refreshPatternsAsync;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private TimeOnly _savedStart;
    private TimeOnly _savedEnd;
    private ShiftTypeTimeOption _selectedStartTime;
    private ShiftTypeTimeOption _selectedEndTime;
    private bool _isSaving;
    private string? _errorMessage;
    private string? _successMessage;

    public ShiftTypeEditorViewModel(
        ShiftTypeSnapshot snapshot,
        UpdateShiftTypeStandardTimeCommand updateCommand,
        Func<CancellationToken, Task> refreshPatternsAsync,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(updateCommand);
        ArgumentNullException.ThrowIfNull(refreshPatternsAsync);
        ArgumentNullException.ThrowIfNull(errorReporter);

        Id = snapshot.Id;
        Name = snapshot.Name;
        WorkLocationId = snapshot.WorkLocationId;
        Abbreviation = snapshot.Abbreviation;
        UsesActualTimeAsDisplay = snapshot.UsesActualTimeAsDisplay;
        _savedStart = snapshot.StandardStart;
        _savedEnd = snapshot.StandardEnd;
        _selectedStartTime = FindTime(snapshot.StandardStart);
        _selectedEndTime = FindTime(snapshot.StandardEnd);
        _updateCommand = updateCommand;
        _refreshPatternsAsync = refreshPatternsAsync;
        _errorReporter = errorReporter;
        TimeOptions = AvailableTimes;
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    public Guid Id { get; }

    public string Name { get; }

    public Guid WorkLocationId { get; }

    public string? Abbreviation { get; }

    public bool UsesActualTimeAsDisplay { get; }

    public ReadOnlyCollection<ShiftTypeTimeOption> TimeOptions { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public string DisplayCode => UsesActualTimeAsDisplay
        ? $"{FormatTime(_savedStart)}-{FormatTime(_savedEnd)}"
        : Abbreviation ?? Name;

    public string StandardTimeText =>
        $"{FormatTime(_savedStart)} bis {FormatTime(_savedEnd)} Uhr";

    public ShiftTypeTimeOption SelectedStartTime
    {
        get => _selectedStartTime;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (SetProperty(ref _selectedStartTime, value))
            {
                OnEditableValueChanged();
            }
        }
    }

    public ShiftTypeTimeOption SelectedEndTime
    {
        get => _selectedEndTime;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (SetProperty(ref _selectedEndTime, value))
            {
                OnEditableValueChanged();
            }
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value))
            {
                OnPropertyChanged(nameof(HasSuccessMessage));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool HasChanges => _savedStart != SelectedStartTime.Value
        || _savedEnd != SelectedEndTime.Value;

    private bool CanSave()
    {
        return !IsSaving && HasChanges;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            UpdateShiftTypeStandardTimeResult result = await _updateCommand.ExecuteAsync(
                new UpdateShiftTypeStandardTimeRequest(
                    Id,
                    SelectedStartTime.Value,
                    SelectedEndTime.Value),
                cancellationToken);

            if (result.Status != UpdateShiftTypeStandardTimeStatus.Succeeded)
            {
                ErrorMessage = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(error => error.Message));
                return;
            }

            ApplySuccessfulUpdate(result.Value!);

            try
            {
                await _refreshPatternsAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _errorReporter.Report(exception, "RefreshShiftPatterns");
                SuccessMessage = null;
                ErrorMessage = "Die Standardzeit wurde gespeichert, aber die Einsatzmuster konnten nicht neu geladen werden.";
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "SaveShiftTypeStandardTime");
            ErrorMessage = "Die Standardzeit konnte nicht gespeichert werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ApplySuccessfulUpdate(ShiftTypeSnapshot snapshot)
    {
        _savedStart = snapshot.StandardStart;
        _savedEnd = snapshot.StandardEnd;
        _selectedStartTime = FindTime(snapshot.StandardStart);
        _selectedEndTime = FindTime(snapshot.StandardEnd);
        OnPropertyChanged(nameof(SelectedStartTime));
        OnPropertyChanged(nameof(SelectedEndTime));
        OnPropertyChanged(nameof(DisplayCode));
        OnPropertyChanged(nameof(StandardTimeText));
        OnPropertyChanged(nameof(HasChanges));
        SuccessMessage = "Die Standardzeit wurde gespeichert.";
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void OnEditableValueChanged()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        OnPropertyChanged(nameof(HasChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }

    private static ShiftTypeTimeOption FindTime(TimeOnly time)
    {
        return AvailableTimes.First(option => option.Value == time);
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
