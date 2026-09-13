using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.ServiceCatalog;

internal sealed class WorkLocationEditorViewModel : ObservableObject
{
    private readonly UpdateWorkLocationCommand _updateCommand;
    private readonly Func<CancellationToken, Task> _refreshPatternsAsync;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private string _savedName;
    private string _savedColorCode;
    private string _name;
    private WorkLocationColorOption _selectedColor;
    private bool _isSaving;
    private string? _errorMessage;
    private string? _successMessage;

    public WorkLocationEditorViewModel(
        WorkLocationSnapshot snapshot,
        UpdateWorkLocationCommand updateCommand,
        Func<CancellationToken, Task> refreshPatternsAsync,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(updateCommand);
        ArgumentNullException.ThrowIfNull(refreshPatternsAsync);
        ArgumentNullException.ThrowIfNull(errorReporter);

        Id = snapshot.Id;
        _savedName = snapshot.Name;
        _savedColorCode = snapshot.ColorCode;
        _name = snapshot.Name;
        _updateCommand = updateCommand;
        _refreshPatternsAsync = refreshPatternsAsync;
        _errorReporter = errorReporter;

        List<WorkLocationColorOption> colors =
        [
            new("yellow", "Gelb"),
            new("red", "Rot"),
        ];

        WorkLocationColorOption? storedColor = colors.FirstOrDefault(
            color => color.Code.Equals(snapshot.ColorCode, StringComparison.OrdinalIgnoreCase));
        if (storedColor is null)
        {
            storedColor = new WorkLocationColorOption(snapshot.ColorCode, snapshot.ColorCode);
            colors.Add(storedColor);
        }

        ColorOptions = Array.AsReadOnly(colors.ToArray());
        ShiftTypes = [];
        _selectedColor = storedColor;
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    public Guid Id { get; }

    public ReadOnlyCollection<WorkLocationColorOption> ColorOptions { get; }

    public ObservableCollection<ShiftTypeEditorViewModel> ShiftTypes { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnEditableValueChanged();
            }
        }
    }

    public WorkLocationColorOption SelectedColor
    {
        get => _selectedColor;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (SetProperty(ref _selectedColor, value))
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

    public bool HasChanges => !_savedName.Equals(Name, StringComparison.Ordinal)
        || !_savedColorCode.Equals(SelectedColor.Code, StringComparison.Ordinal);

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
            UpdateWorkLocationResult result = await _updateCommand.ExecuteAsync(
                new UpdateWorkLocationRequest(Id, Name, SelectedColor.Code),
                cancellationToken);

            if (result.Status == UpdateWorkLocationStatus.Succeeded)
            {
                ApplySuccessfulUpdate(result.Value!);
                await RefreshPatternsAsync(cancellationToken);
                return;
            }

            ErrorMessage = string.Join(
                Environment.NewLine,
                result.Errors.Select(error => error.Message));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "SaveWorkLocation");
            ErrorMessage = "Die Änderungen konnten nicht gespeichert werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ApplySuccessfulUpdate(WorkLocationSnapshot snapshot)
    {
        _savedName = snapshot.Name;
        _savedColorCode = snapshot.ColorCode;
        Name = snapshot.Name;
        SuccessMessage = "Die Änderungen wurden gespeichert.";
        OnPropertyChanged(nameof(HasChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task RefreshPatternsAsync(CancellationToken cancellationToken)
    {
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
            ErrorMessage = "Die Änderungen wurden gespeichert, aber die Einsatzmuster konnten nicht neu geladen werden.";
        }
    }

    private void OnEditableValueChanged()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        OnPropertyChanged(nameof(HasChanges));
        SaveCommand.NotifyCanExecuteChanged();
    }
}
