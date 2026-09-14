using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.Employees;

internal sealed class EmployeeOverviewViewModel : ObservableObject
{
    private readonly GetEmployeeOverviewQuery _overviewQuery;
    private readonly GetEmployeeTypeCatalogQuery _employeeTypeCatalogQuery;
    private readonly CreateEmployeeCommand _createEmployeeCommand;
    private readonly UpdateEmployeeNameCommand _updateEmployeeNameCommand;
    private readonly ChangeEmployeeTypeCommand _changeEmployeeTypeCommand;
    private readonly DeactivateEmployeeCommand _deactivateEmployeeCommand;
    private readonly ReactivateEmployeeCommand _reactivateEmployeeCommand;
    private readonly DeleteEmployeeCommand _deleteEmployeeCommand;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private EmployeeOverviewItemViewModel? _selectedEmployee;
    private EmployeeTypeOptionViewModel? _selectedEmployeeType;
    private EmployeeEditorMode _editorMode;
    private string _editorFirstName = string.Empty;
    private string _editorLastName = string.Empty;
    private bool _isLoading;
    private bool _isSaving;
    private bool _isDeactivationConfirmationOpen;
    private bool _isDeletionConfirmationOpen;
    private string? _loadErrorMessage;
    private string? _firstNameErrorMessage;
    private string? _lastNameErrorMessage;
    private string? _employeeTypeErrorMessage;
    private string? _operationErrorMessage;
    private string? _successMessage;

    public EmployeeOverviewViewModel(
        GetEmployeeOverviewQuery overviewQuery,
        GetEmployeeTypeCatalogQuery employeeTypeCatalogQuery,
        CreateEmployeeCommand createEmployeeCommand,
        UpdateEmployeeNameCommand updateEmployeeNameCommand,
        ChangeEmployeeTypeCommand changeEmployeeTypeCommand,
        DeactivateEmployeeCommand deactivateEmployeeCommand,
        ReactivateEmployeeCommand reactivateEmployeeCommand,
        DeleteEmployeeCommand deleteEmployeeCommand,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(overviewQuery);
        ArgumentNullException.ThrowIfNull(employeeTypeCatalogQuery);
        ArgumentNullException.ThrowIfNull(createEmployeeCommand);
        ArgumentNullException.ThrowIfNull(updateEmployeeNameCommand);
        ArgumentNullException.ThrowIfNull(changeEmployeeTypeCommand);
        ArgumentNullException.ThrowIfNull(deactivateEmployeeCommand);
        ArgumentNullException.ThrowIfNull(reactivateEmployeeCommand);
        ArgumentNullException.ThrowIfNull(deleteEmployeeCommand);
        ArgumentNullException.ThrowIfNull(errorReporter);

        _overviewQuery = overviewQuery;
        _employeeTypeCatalogQuery = employeeTypeCatalogQuery;
        _createEmployeeCommand = createEmployeeCommand;
        _updateEmployeeNameCommand = updateEmployeeNameCommand;
        _changeEmployeeTypeCommand = changeEmployeeTypeCommand;
        _deactivateEmployeeCommand = deactivateEmployeeCommand;
        _reactivateEmployeeCommand = reactivateEmployeeCommand;
        _deleteEmployeeCommand = deleteEmployeeCommand;
        _errorReporter = errorReporter;
        Employees = [];
        EmployeeTypes = [];
        LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
        BeginCreateCommand = new RelayCommand(BeginCreate, CanBeginCreate);
        BeginRenameCommand = new RelayCommand(BeginRename, CanUseSelectedEmployee);
        BeginTypeChangeCommand = new RelayCommand(BeginTypeChange, CanUseSelectedEmployee);
        CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditorOpen && !IsSaving);
        SaveCreateCommand = new AsyncRelayCommand(SaveCreateAsync, () => IsCreateMode && !IsSaving);
        SaveNameCommand = new AsyncRelayCommand(SaveNameAsync, () => IsRenameMode && !IsSaving);
        SaveTypeChangeCommand = new AsyncRelayCommand(
            SaveTypeChangeAsync,
            () => IsTypeChangeMode && !IsSaving);
        RequestDeactivationCommand = new RelayCommand(
            RequestDeactivation,
            CanRequestDeactivation);
        CancelDeactivationCommand = new RelayCommand(
            CancelDeactivation,
            () => IsDeactivationConfirmationOpen && !IsSaving);
        ConfirmDeactivationCommand = new AsyncRelayCommand(
            ConfirmDeactivationAsync,
            () => IsDeactivationConfirmationOpen && !IsSaving);
        ReactivateEmployeeCommand = new AsyncRelayCommand(
            ReactivateEmployeeAsync,
            CanReactivateEmployee);
        RequestDeletionCommand = new RelayCommand(
            RequestDeletion,
            CanDeleteEmployee);
        CancelDeletionCommand = new RelayCommand(
            CancelDeletion,
            () => IsDeletionConfirmationOpen && !IsSaving);
        ConfirmDeletionCommand = new AsyncRelayCommand(
            ConfirmDeletionAsync,
            () => IsDeletionConfirmationOpen && !IsSaving);
    }

    public ObservableCollection<EmployeeOverviewItemViewModel> Employees { get; }

    public ObservableCollection<EmployeeTypeOptionViewModel> EmployeeTypes { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public IRelayCommand BeginCreateCommand { get; }

    public IRelayCommand BeginRenameCommand { get; }

    public IRelayCommand BeginTypeChangeCommand { get; }

    public IRelayCommand CancelEditCommand { get; }

    public IAsyncRelayCommand SaveCreateCommand { get; }

    public IAsyncRelayCommand SaveNameCommand { get; }

    public IAsyncRelayCommand SaveTypeChangeCommand { get; }

    public IRelayCommand RequestDeactivationCommand { get; }

    public IRelayCommand CancelDeactivationCommand { get; }

    public IAsyncRelayCommand ConfirmDeactivationCommand { get; }

    public IAsyncRelayCommand ReactivateEmployeeCommand { get; }

    public IRelayCommand RequestDeletionCommand { get; }

    public IRelayCommand CancelDeletionCommand { get; }

    public IAsyncRelayCommand ConfirmDeletionCommand { get; }

    public EmployeeOverviewItemViewModel? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            if (SetProperty(ref _selectedEmployee, value))
            {
                NotifyViewStateChanged();
            }
        }
    }

    public EmployeeTypeOptionViewModel? SelectedEmployeeType
    {
        get => _selectedEmployeeType;
        set
        {
            if (SetProperty(ref _selectedEmployeeType, value))
            {
                EmployeeTypeErrorMessage = null;
                OperationErrorMessage = null;
                SuccessMessage = null;
                OnPropertyChanged(nameof(HasSelectedEmployeeType));
            }
        }
    }

    public string EditorFirstName
    {
        get => _editorFirstName;
        set
        {
            if (SetProperty(ref _editorFirstName, value))
            {
                FirstNameErrorMessage = null;
                OperationErrorMessage = null;
                SuccessMessage = null;
            }
        }
    }

    public string EditorLastName
    {
        get => _editorLastName;
        set
        {
            if (SetProperty(ref _editorLastName, value))
            {
                LastNameErrorMessage = null;
                OperationErrorMessage = null;
                SuccessMessage = null;
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
                NotifyViewStateChanged();
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

    public string? FirstNameErrorMessage
    {
        get => _firstNameErrorMessage;
        private set
        {
            if (SetProperty(ref _firstNameErrorMessage, value))
            {
                OnPropertyChanged(nameof(HasFirstNameError));
            }
        }
    }

    public string? LastNameErrorMessage
    {
        get => _lastNameErrorMessage;
        private set
        {
            if (SetProperty(ref _lastNameErrorMessage, value))
            {
                OnPropertyChanged(nameof(HasLastNameError));
            }
        }
    }

    public string? EmployeeTypeErrorMessage
    {
        get => _employeeTypeErrorMessage;
        private set
        {
            if (SetProperty(ref _employeeTypeErrorMessage, value))
            {
                OnPropertyChanged(nameof(HasEmployeeTypeError));
            }
        }
    }

    public string? OperationErrorMessage
    {
        get => _operationErrorMessage;
        private set
        {
            if (SetProperty(ref _operationErrorMessage, value))
            {
                OnPropertyChanged(nameof(HasOperationError));
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

    public bool HasLoadError => !string.IsNullOrWhiteSpace(LoadErrorMessage);

    public bool HasFirstNameError => !string.IsNullOrWhiteSpace(FirstNameErrorMessage);

    public bool HasLastNameError => !string.IsNullOrWhiteSpace(LastNameErrorMessage);

    public bool HasEmployeeTypeError => !string.IsNullOrWhiteSpace(EmployeeTypeErrorMessage);

    public bool HasOperationError => !string.IsNullOrWhiteSpace(OperationErrorMessage);

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool HasSelectedEmployeeType => SelectedEmployeeType is not null;

    public bool HasEmployees => Employees.Count > 0;

    public bool IsEmpty => !IsLoading && !HasLoadError && !HasEmployees && !IsEditorOpen;

    public bool HasEmployeeWorkspace => HasEmployees || IsEditorOpen;

    public bool IsEditorOpen => _editorMode != EmployeeEditorMode.None;

    public bool IsCreateMode => _editorMode == EmployeeEditorMode.Create;

    public bool IsRenameMode => _editorMode == EmployeeEditorMode.Rename;

    public bool IsTypeChangeMode => _editorMode == EmployeeEditorMode.ChangeType;

    public bool IsDeactivationConfirmationOpen => _isDeactivationConfirmationOpen;

    public bool IsDeletionConfirmationOpen => _isDeletionConfirmationOpen;

    public bool ShowSelectedEmployeeDetails => SelectedEmployee is not null
        && !IsEditorOpen
        && !IsDeactivationConfirmationOpen
        && !IsDeletionConfirmationOpen;

    public string EditorTitle => _editorMode switch
    {
        EmployeeEditorMode.Create => "Mitarbeiter anlegen",
        EmployeeEditorMode.Rename => "Namen bearbeiten",
        EmployeeEditorMode.ChangeType => "Mitarbeitertyp wechseln",
        _ => string.Empty,
    };

    private bool CanLoad()
    {
        return !IsSaving
            && !IsEditorOpen
            && !IsDeactivationConfirmationOpen
            && !IsDeletionConfirmationOpen;
    }

    private bool CanBeginCreate()
    {
        return !IsLoading
            && !IsSaving
            && !IsEditorOpen
            && !IsDeactivationConfirmationOpen
            && !IsDeletionConfirmationOpen
            && EmployeeTypes.Count > 0;
    }

    private bool CanUseSelectedEmployee()
    {
        return SelectedEmployee is not null
            && !IsLoading
            && !IsSaving
            && !IsEditorOpen
            && !IsDeactivationConfirmationOpen
            && !IsDeletionConfirmationOpen;
    }

    private bool CanRequestDeactivation()
    {
        return CanUseSelectedEmployee() && SelectedEmployee!.IsActive;
    }

    private bool CanReactivateEmployee()
    {
        return CanUseSelectedEmployee() && SelectedEmployee!.IsInactive;
    }

    private bool CanDeleteEmployee()
    {
        return CanUseSelectedEmployee() && SelectedEmployee!.IsInactive;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        LoadErrorMessage = null;
        SuccessMessage = null;
        SelectedEmployee = null;
        Employees.Clear();
        EmployeeTypes.Clear();
        NotifyViewStateChanged();

        try
        {
            Task<EmployeeOverviewSnapshot> overviewTask =
                _overviewQuery.ExecuteAsync(cancellationToken);
            Task<EmployeeTypeCatalogSnapshot> employeeTypesTask =
                _employeeTypeCatalogQuery.ExecuteAsync(cancellationToken);
            await Task.WhenAll(overviewTask, employeeTypesTask);
            EmployeeOverviewSnapshot overview = await overviewTask;
            EmployeeTypeCatalogSnapshot employeeTypes = await employeeTypesTask;

            ApplyLoadedSnapshots(overview, employeeTypes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadEmployeeOverview");
            LoadErrorMessage =
                "Die Mitarbeitenden konnten nicht geladen werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsLoading = false;
            NotifyViewStateChanged();
        }
    }

    private void ApplyLoadedSnapshots(
        EmployeeOverviewSnapshot overview,
        EmployeeTypeCatalogSnapshot employeeTypes)
    {
        Dictionary<Guid, EmployeeTypeSnapshot> employeeTypesById =
            employeeTypes.EmployeeTypes.ToDictionary(employeeType => employeeType.Id);

        foreach (EmployeeTypeSnapshot employeeType in employeeTypes.EmployeeTypes)
        {
            EmployeeTypes.Add(new EmployeeTypeOptionViewModel(employeeType));
        }

        foreach (EmployeeOverviewItemSnapshot employee in overview.Employees)
        {
            if (!employeeTypesById.TryGetValue(
                    employee.EmployeeTypeId,
                    out EmployeeTypeSnapshot? employeeType))
            {
                throw new InvalidOperationException(
                    $"Employee '{employee.Id}' references an unknown employee type.");
            }

            Employees.Add(new EmployeeOverviewItemViewModel(employee, employeeType));
        }

        SelectedEmployee = Employees.FirstOrDefault();
    }

    private void BeginCreate()
    {
        ClearFeedback();
        EditorFirstName = string.Empty;
        EditorLastName = string.Empty;
        SelectedEmployeeType = null;
        SetEditorMode(EmployeeEditorMode.Create);
    }

    private void BeginRename()
    {
        EmployeeOverviewItemViewModel employee = SelectedEmployee
            ?? throw new InvalidOperationException("No employee is selected.");
        ClearFeedback();
        EditorFirstName = employee.FirstName;
        EditorLastName = employee.LastName;
        SelectedEmployeeType = null;
        SetEditorMode(EmployeeEditorMode.Rename);
    }

    private void BeginTypeChange()
    {
        EmployeeOverviewItemViewModel employee = SelectedEmployee
            ?? throw new InvalidOperationException("No employee is selected.");
        EmployeeTypeOptionViewModel? employeeType = EmployeeTypes.FirstOrDefault(
            option => option.Code.Equals(employee.EmployeeTypeCode, StringComparison.Ordinal));
        if (employeeType is null)
        {
            OperationErrorMessage =
                "Der aktuelle Mitarbeitertyp ist nicht verfügbar. Bitte laden Sie die Daten neu.";
            return;
        }

        ClearFeedback();
        EditorFirstName = string.Empty;
        EditorLastName = string.Empty;
        SelectedEmployeeType = employeeType;
        SetEditorMode(EmployeeEditorMode.ChangeType);
    }

    private void CancelEdit()
    {
        SetEditorMode(EmployeeEditorMode.None);
        ClearEditorValues();
        ClearFeedback();
    }

    private void RequestDeactivation()
    {
        ClearFeedback();
        _isDeactivationConfirmationOpen = true;
        OnPropertyChanged(nameof(IsDeactivationConfirmationOpen));
        NotifyViewStateChanged();
    }

    private void CancelDeactivation()
    {
        _isDeactivationConfirmationOpen = false;
        OnPropertyChanged(nameof(IsDeactivationConfirmationOpen));
        ClearFeedback();
        NotifyViewStateChanged();
    }

    private void RequestDeletion()
    {
        ClearFeedback();
        _isDeletionConfirmationOpen = true;
        OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
        NotifyViewStateChanged();
    }

    private void CancelDeletion()
    {
        _isDeletionConfirmationOpen = false;
        OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
        ClearFeedback();
        NotifyViewStateChanged();
    }

    private async Task SaveCreateAsync(CancellationToken cancellationToken)
    {
        Guid employeeTypeId = SelectedEmployeeType?.Id ?? Guid.Empty;
        await ExecuteWriteAsync(
            () => _createEmployeeCommand.ExecuteAsync(
                new CreateEmployeeRequest(EditorFirstName, EditorLastName, employeeTypeId),
                cancellationToken),
            "CreateEmployee",
            "Der Mitarbeiter wurde angelegt.",
            cancellationToken);
    }

    private async Task SaveNameAsync(CancellationToken cancellationToken)
    {
        Guid employeeId = SelectedEmployee?.Id ?? Guid.Empty;
        await ExecuteWriteAsync(
            () => _updateEmployeeNameCommand.ExecuteAsync(
                new UpdateEmployeeNameRequest(employeeId, EditorFirstName, EditorLastName),
                cancellationToken),
            "UpdateEmployeeName",
            "Der Name wurde gespeichert.",
            cancellationToken);
    }

    private async Task SaveTypeChangeAsync(CancellationToken cancellationToken)
    {
        Guid employeeId = SelectedEmployee?.Id ?? Guid.Empty;
        Guid employeeTypeId = SelectedEmployeeType?.Id ?? Guid.Empty;
        await ExecuteWriteAsync(
            () => _changeEmployeeTypeCommand.ExecuteAsync(
                new ChangeEmployeeTypeRequest(employeeId, employeeTypeId),
                cancellationToken),
            "ChangeEmployeeType",
            "Der Mitarbeitertyp wurde gespeichert.",
            cancellationToken);
    }

    private async Task ConfirmDeactivationAsync(CancellationToken cancellationToken)
    {
        Guid employeeId = SelectedEmployee?.Id ?? Guid.Empty;
        await ExecuteWriteAsync(
            () => _deactivateEmployeeCommand.ExecuteAsync(
                new DeactivateEmployeeRequest(employeeId),
                cancellationToken),
            "DeactivateEmployee",
            "Der Mitarbeiter wurde deaktiviert.",
            cancellationToken);
    }

    private async Task ReactivateEmployeeAsync(CancellationToken cancellationToken)
    {
        Guid employeeId = SelectedEmployee?.Id ?? Guid.Empty;
        await ExecuteWriteAsync(
            () => _reactivateEmployeeCommand.ExecuteAsync(
                new ReactivateEmployeeRequest(employeeId),
                cancellationToken),
            "ReactivateEmployee",
            "Der Mitarbeiter wurde reaktiviert.",
            cancellationToken);
    }

    private async Task ConfirmDeletionAsync(CancellationToken cancellationToken)
    {
        Guid employeeId = SelectedEmployee?.Id ?? Guid.Empty;
        await ExecuteDeleteAsync(employeeId, cancellationToken);
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ExecuteWriteAsync(
        Func<Task<EmployeeCommandResult>> executeAsync,
        string operation,
        string successMessage,
        CancellationToken cancellationToken)
    {
        IsSaving = true;
        ClearFeedback();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            EmployeeCommandResult result = await executeAsync();
            if (result.Status != EmployeeCommandStatus.Succeeded)
            {
                ApplyCommandErrors(result.Errors);
                return;
            }

            ApplySuccessfulWrite(result.Value!, successMessage);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, operation);
            OperationErrorMessage =
                "Die Änderung konnte nicht gespeichert werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ExecuteDeleteAsync(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        IsSaving = true;
        ClearFeedback();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            EmployeeCommandResult result = await _deleteEmployeeCommand.ExecuteAsync(
                new DeleteEmployeeRequest(employeeId),
                cancellationToken);
            if (result.Status != EmployeeCommandStatus.Succeeded)
            {
                ApplyCommandErrors(result.Errors);
                return;
            }

            ApplySuccessfulDeletion(result.Value!.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "DeleteEmployee");
            OperationErrorMessage =
                "Der Mitarbeiter konnte nicht endgültig gelöscht werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ApplySuccessfulWrite(
        EmployeeDetailsSnapshot employee,
        string successMessage)
    {
        EmployeeOverviewItemViewModel replacement = new(employee);
        List<EmployeeOverviewItemViewModel> updatedEmployees = Employees
            .Where(existing => existing.Id != replacement.Id)
            .Append(replacement)
            .OrderBy(existing => existing.LastName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(existing => existing.FirstName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(existing => existing.Id)
            .ToList();

        Employees.Clear();
        foreach (EmployeeOverviewItemViewModel existing in updatedEmployees)
        {
            Employees.Add(existing);
        }

        SetEditorMode(EmployeeEditorMode.None);
        _isDeactivationConfirmationOpen = false;
        OnPropertyChanged(nameof(IsDeactivationConfirmationOpen));
        _isDeletionConfirmationOpen = false;
        OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
        ClearEditorValues();
        SelectedEmployee = Employees.Single(existing => existing.Id == replacement.Id);
        SuccessMessage = successMessage;
        NotifyViewStateChanged();
    }

    private void ApplySuccessfulDeletion(Guid employeeId)
    {
        EmployeeOverviewItemViewModel? deletedEmployee = Employees.SingleOrDefault(
            employee => employee.Id == employeeId);
        if (deletedEmployee is null)
        {
            throw new InvalidOperationException(
                "The successfully deleted employee is missing from the overview.");
        }

        Employees.Remove(deletedEmployee);
        _isDeletionConfirmationOpen = false;
        OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
        SelectedEmployee = Employees.FirstOrDefault();
        SuccessMessage = "Der Mitarbeiter wurde endgültig gelöscht.";
        NotifyViewStateChanged();
    }

    private void ApplyCommandErrors(IEnumerable<EmployeeCommandError> errors)
    {
        List<string> operationErrors = [];

        foreach (EmployeeCommandError error in errors)
        {
            switch (error.Code)
            {
                case EmployeeCommandErrorCode.FirstNameRequired:
                    FirstNameErrorMessage = error.Message;
                    break;
                case EmployeeCommandErrorCode.LastNameRequired:
                    LastNameErrorMessage = error.Message;
                    break;
                case EmployeeCommandErrorCode.EmployeeTypeRequired:
                case EmployeeCommandErrorCode.EmployeeTypeNotFound:
                    EmployeeTypeErrorMessage = error.Message;
                    break;
                default:
                    operationErrors.Add(error.Message);
                    break;
            }
        }

        OperationErrorMessage = operationErrors.Count == 0
            ? null
            : string.Join(Environment.NewLine, operationErrors);
    }

    private void SetEditorMode(EmployeeEditorMode editorMode)
    {
        _editorMode = editorMode;
        OnPropertyChanged(nameof(IsEditorOpen));
        OnPropertyChanged(nameof(IsCreateMode));
        OnPropertyChanged(nameof(IsRenameMode));
        OnPropertyChanged(nameof(IsTypeChangeMode));
        OnPropertyChanged(nameof(EditorTitle));
        NotifyViewStateChanged();
    }

    private void ClearEditorValues()
    {
        EditorFirstName = string.Empty;
        EditorLastName = string.Empty;
        SelectedEmployeeType = null;
    }

    private void ClearFeedback()
    {
        FirstNameErrorMessage = null;
        LastNameErrorMessage = null;
        EmployeeTypeErrorMessage = null;
        OperationErrorMessage = null;
        SuccessMessage = null;
    }

    private void NotifyViewStateChanged()
    {
        OnPropertyChanged(nameof(HasLoadError));
        OnPropertyChanged(nameof(HasEmployees));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasEmployeeWorkspace));
        OnPropertyChanged(nameof(ShowSelectedEmployeeDetails));
        LoadCommand.NotifyCanExecuteChanged();
        BeginCreateCommand.NotifyCanExecuteChanged();
        BeginRenameCommand.NotifyCanExecuteChanged();
        BeginTypeChangeCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
        SaveCreateCommand.NotifyCanExecuteChanged();
        SaveNameCommand.NotifyCanExecuteChanged();
        SaveTypeChangeCommand.NotifyCanExecuteChanged();
        RequestDeactivationCommand.NotifyCanExecuteChanged();
        CancelDeactivationCommand.NotifyCanExecuteChanged();
        ConfirmDeactivationCommand.NotifyCanExecuteChanged();
        ReactivateEmployeeCommand.NotifyCanExecuteChanged();
        RequestDeletionCommand.NotifyCanExecuteChanged();
        CancelDeletionCommand.NotifyCanExecuteChanged();
        ConfirmDeletionCommand.NotifyCanExecuteChanged();
    }
}
