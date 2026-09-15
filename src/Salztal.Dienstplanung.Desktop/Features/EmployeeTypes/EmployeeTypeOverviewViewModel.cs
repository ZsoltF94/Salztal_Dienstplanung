using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Desktop.Shared;

namespace Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;

internal sealed class EmployeeTypeOverviewViewModel : ObservableObject
{
    private readonly GetEmployeeTypeCatalogQuery _query;
    private readonly CreateEmployeeTypeCommand _createCommand;
    private readonly UpdateEmployeeTypeCommand _updateCommand;
    private readonly DeleteEmployeeTypeCommand _deleteCommand;
    private readonly IUnexpectedErrorReporter _errorReporter;
    private EmployeeTypeOverviewItemViewModel? _selectedEmployeeType;
    private EmployeeTypeEditorMode _editorMode;
    private bool _isLoading;
    private bool _isSaving;
    private bool _isDeletionConfirmationOpen;
    private string _editorCode = string.Empty;
    private string _editorName = string.Empty;
    private string _editorWeeklyWorkTarget = string.Empty;
    private bool _editorAllowsVacationAndSickness;
    private string _editorAbsenceDayValue = string.Empty;
    private string? _loadErrorMessage;
    private string? _codeErrorMessage;
    private string? _nameErrorMessage;
    private string? _weeklyWorkTargetErrorMessage;
    private string? _absenceDayValueErrorMessage;
    private string? _eligibilityErrorMessage;
    private string? _operationErrorMessage;
    private string? _successMessage;

    public EmployeeTypeOverviewViewModel(
        GetEmployeeTypeCatalogQuery query,
        CreateEmployeeTypeCommand createCommand,
        UpdateEmployeeTypeCommand updateCommand,
        DeleteEmployeeTypeCommand deleteCommand,
        IUnexpectedErrorReporter errorReporter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(createCommand);
        ArgumentNullException.ThrowIfNull(updateCommand);
        ArgumentNullException.ThrowIfNull(deleteCommand);
        ArgumentNullException.ThrowIfNull(errorReporter);
        _query = query;
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _deleteCommand = deleteCommand;
        _errorReporter = errorReporter;
        EmployeeTypes = [];
        EligibilityOptions = [];
        LoadCommand = new AsyncRelayCommand(LoadAsync, CanLoad);
        BeginCreateCommand = new RelayCommand(BeginCreate, CanBeginEdit);
        BeginEditCommand = new RelayCommand(BeginEdit, CanUseSelection);
        CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditorOpen && !IsSaving);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => IsEditorOpen && !IsSaving);
        RequestDeletionCommand = new RelayCommand(RequestDeletion, CanDeleteSelection);
        CancelDeletionCommand = new RelayCommand(
            CancelDeletion,
            () => IsDeletionConfirmationOpen && !IsSaving);
        ConfirmDeletionCommand = new AsyncRelayCommand(
            ConfirmDeletionAsync,
            () => IsDeletionConfirmationOpen && !IsSaving);
    }

    public ObservableCollection<EmployeeTypeOverviewItemViewModel> EmployeeTypes { get; }

    public ObservableCollection<EmployeeTypeEligibilityEditorItemViewModel> EligibilityOptions
    {
        get;
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IRelayCommand BeginCreateCommand { get; }

    public IRelayCommand BeginEditCommand { get; }

    public IRelayCommand CancelEditCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand RequestDeletionCommand { get; }

    public IRelayCommand CancelDeletionCommand { get; }

    public IAsyncRelayCommand ConfirmDeletionCommand { get; }

    public EmployeeTypeOverviewItemViewModel? SelectedEmployeeType
    {
        get => _selectedEmployeeType;
        set
        {
            if (SetProperty(ref _selectedEmployeeType, value))
            {
                ClearFeedback();
                NotifyStateChanged();
            }
        }
    }

    public string EditorCode
    {
        get => _editorCode;
        set => SetEditorValue(ref _editorCode, value, nameof(EditorCode), ClearCodeError);
    }

    public string EditorName
    {
        get => _editorName;
        set => SetEditorValue(ref _editorName, value, nameof(EditorName), ClearNameError);
    }

    public string EditorWeeklyWorkTarget
    {
        get => _editorWeeklyWorkTarget;
        set => SetEditorValue(
            ref _editorWeeklyWorkTarget,
            value,
            nameof(EditorWeeklyWorkTarget),
            ClearWeeklyWorkTargetError);
    }

    public bool EditorAllowsVacationAndSickness
    {
        get => _editorAllowsVacationAndSickness;
        set
        {
            if (SetProperty(ref _editorAllowsVacationAndSickness, value))
            {
                if (!value)
                {
                    EditorAbsenceDayValue = string.Empty;
                }

                ClearAbsenceDayValueError();
                OnPropertyChanged(nameof(CanEditAbsenceDayValue));
            }
        }
    }

    public string EditorAbsenceDayValue
    {
        get => _editorAbsenceDayValue;
        set => SetEditorValue(
            ref _editorAbsenceDayValue,
            value,
            nameof(EditorAbsenceDayValue),
            ClearAbsenceDayValueError);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                NotifyStateChanged();
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
                NotifyStateChanged();
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
                NotifyStateChanged();
            }
        }
    }

    public string? CodeErrorMessage
    {
        get => _codeErrorMessage;
        private set => SetFeedbackMessage(
            ref _codeErrorMessage,
            value,
            nameof(CodeErrorMessage),
            nameof(HasCodeError));
    }

    public string? NameErrorMessage
    {
        get => _nameErrorMessage;
        private set => SetFeedbackMessage(
            ref _nameErrorMessage,
            value,
            nameof(NameErrorMessage),
            nameof(HasNameError));
    }

    public string? WeeklyWorkTargetErrorMessage
    {
        get => _weeklyWorkTargetErrorMessage;
        private set => SetFeedbackMessage(
            ref _weeklyWorkTargetErrorMessage,
            value,
            nameof(WeeklyWorkTargetErrorMessage),
            nameof(HasWeeklyWorkTargetError));
    }

    public string? AbsenceDayValueErrorMessage
    {
        get => _absenceDayValueErrorMessage;
        private set => SetFeedbackMessage(
            ref _absenceDayValueErrorMessage,
            value,
            nameof(AbsenceDayValueErrorMessage),
            nameof(HasAbsenceDayValueError));
    }

    public string? EligibilityErrorMessage
    {
        get => _eligibilityErrorMessage;
        private set => SetFeedbackMessage(
            ref _eligibilityErrorMessage,
            value,
            nameof(EligibilityErrorMessage),
            nameof(HasEligibilityError));
    }

    public string? OperationErrorMessage
    {
        get => _operationErrorMessage;
        private set => SetFeedbackMessage(
            ref _operationErrorMessage,
            value,
            nameof(OperationErrorMessage),
            nameof(HasOperationError));
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set => SetFeedbackMessage(
            ref _successMessage,
            value,
            nameof(SuccessMessage),
            nameof(HasSuccessMessage));
    }

    public bool HasLoadError => !string.IsNullOrWhiteSpace(LoadErrorMessage);

    public bool HasCodeError => !string.IsNullOrWhiteSpace(CodeErrorMessage);

    public bool HasNameError => !string.IsNullOrWhiteSpace(NameErrorMessage);

    public bool HasWeeklyWorkTargetError =>
        !string.IsNullOrWhiteSpace(WeeklyWorkTargetErrorMessage);

    public bool HasAbsenceDayValueError =>
        !string.IsNullOrWhiteSpace(AbsenceDayValueErrorMessage);

    public bool HasEligibilityError => !string.IsNullOrWhiteSpace(EligibilityErrorMessage);

    public bool HasOperationError => !string.IsNullOrWhiteSpace(OperationErrorMessage);

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool HasEmployeeTypes => EmployeeTypes.Count > 0;

    public bool IsEmpty => !IsLoading && !HasLoadError && !HasEmployeeTypes;

    public bool IsEditorOpen => _editorMode != EmployeeTypeEditorMode.None;

    public bool IsCreateMode => _editorMode == EmployeeTypeEditorMode.Create;

    public bool IsEditMode => _editorMode == EmployeeTypeEditorMode.Edit;

    public bool IsDeletionConfirmationOpen => _isDeletionConfirmationOpen;

    public bool ShowSelectedDetails => SelectedEmployeeType is not null
        && !IsEditorOpen
        && !IsDeletionConfirmationOpen;

    public bool CanEditAbsenceDayValue => EditorAllowsVacationAndSickness;

    public string EditorTitle => IsCreateMode
        ? "Mitarbeitertyp anlegen"
        : "Mitarbeitertyp bearbeiten";

    public string EditorPlanningRoleDisplay => IsCreateMode
        ? "Normaler Mitarbeitertyp"
        : SelectedEmployeeType?.PlanningRoleDisplay ?? string.Empty;

    private bool CanLoad()
    {
        return !IsSaving && !IsEditorOpen && !IsDeletionConfirmationOpen;
    }

    private bool CanBeginEdit()
    {
        return !IsLoading && !IsSaving && !IsEditorOpen && !IsDeletionConfirmationOpen;
    }

    private bool CanUseSelection()
    {
        return CanBeginEdit() && SelectedEmployeeType is not null;
    }

    private bool CanDeleteSelection()
    {
        return CanUseSelection() && SelectedEmployeeType!.IsNormal;
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
        EmployeeTypes.Clear();
        EligibilityOptions.Clear();
        SelectedEmployeeType = null;

        try
        {
            EmployeeTypeCatalogSnapshot catalog = await _query.ExecuteAsync(cancellationToken);
            ApplyCatalog(catalog);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "LoadEmployeeTypes");
            LoadErrorMessage =
                "Die Mitarbeitertypen konnten nicht geladen werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyCatalog(EmployeeTypeCatalogSnapshot catalog)
    {
        foreach (EmployeeTypeSnapshot employeeType in catalog.EmployeeTypes
                     .OrderBy(value => value.Code, StringComparer.OrdinalIgnoreCase))
        {
            EmployeeTypes.Add(new EmployeeTypeOverviewItemViewModel(employeeType));
        }

        IEnumerable<EmployeeTypeEligibilitySnapshot> targets = catalog.EmployeeTypes
            .SelectMany(value => value.ShiftEligibilities)
            .GroupBy(value => new { value.TargetId, value.TargetKind })
            .Select(group => group.First())
            .OrderBy(value => value.TargetKind)
            .ThenBy(value => value.TargetName, StringComparer.OrdinalIgnoreCase);
        foreach (EmployeeTypeEligibilitySnapshot target in targets)
        {
            EligibilityOptions.Add(new EmployeeTypeEligibilityEditorItemViewModel(
                target.TargetId,
                target.TargetName,
                target.TargetKind));
        }

        SelectedEmployeeType = EmployeeTypes.FirstOrDefault();
        NotifyStateChanged();
    }

    private void BeginCreate()
    {
        ClearFeedback();
        EditorCode = string.Empty;
        EditorName = string.Empty;
        EditorWeeklyWorkTarget = string.Empty;
        EditorAllowsVacationAndSickness = true;
        EditorAbsenceDayValue = string.Empty;
        foreach (EmployeeTypeEligibilityEditorItemViewModel option in EligibilityOptions)
        {
            option.Apply([]);
        }

        SetEditorMode(EmployeeTypeEditorMode.Create);
    }

    private void BeginEdit()
    {
        EmployeeTypeSnapshot snapshot = SelectedEmployeeType?.Snapshot
            ?? throw new InvalidOperationException("No employee type is selected.");
        ClearFeedback();
        EditorCode = snapshot.Code;
        EditorName = snapshot.Name;
        EditorWeeklyWorkTarget = EmployeeTypeDurationFormatter.Format(
            snapshot.WeeklyWorkTargetMinutes);
        EditorAllowsVacationAndSickness = snapshot.AllowsVacationAndSickness;
        EditorAbsenceDayValue = snapshot.AbsenceDayValueMinutes.HasValue
            ? EmployeeTypeDurationFormatter.Format(snapshot.AbsenceDayValueMinutes.Value)
            : string.Empty;
        foreach (EmployeeTypeEligibilityEditorItemViewModel option in EligibilityOptions)
        {
            option.Apply(snapshot.ShiftEligibilities);
        }

        SetEditorMode(EmployeeTypeEditorMode.Edit);
    }

    private void CancelEdit()
    {
        SetEditorMode(EmployeeTypeEditorMode.None);
        ClearEditor();
        ClearFeedback();
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        ClearFeedback();
        if (!TryCreateDurations(out int weeklyMinutes, out int? absenceMinutes))
        {
            return;
        }

        EmployeeTypeEligibilityRequest[] eligibilities = EligibilityOptions
            .SelectMany(option => option.CreateRequests())
            .ToArray();
        await ExecuteWriteAsync(
            IsCreateMode
                ? () => _createCommand.ExecuteAsync(
                    new CreateEmployeeTypeRequest(
                        EditorCode,
                        EditorName,
                        weeklyMinutes,
                        EditorAllowsVacationAndSickness,
                        absenceMinutes,
                        eligibilities),
                    cancellationToken)
                : () => _updateCommand.ExecuteAsync(
                    new UpdateEmployeeTypeRequest(
                        SelectedEmployeeType?.Id ?? Guid.Empty,
                        EditorName,
                        weeklyMinutes,
                        EditorAllowsVacationAndSickness,
                        absenceMinutes,
                        eligibilities),
                    cancellationToken),
            cancellationToken);
    }

    private bool TryCreateDurations(out int weeklyMinutes, out int? absenceMinutes)
    {
        bool weeklyValid = EmployeeTypeDurationFormatter.TryParse(
            EditorWeeklyWorkTarget,
            out weeklyMinutes);
        if (!weeklyValid)
        {
            WeeklyWorkTargetErrorMessage =
                "Bitte geben Sie das Wochen-Soll als Stunden:Minuten ein, zum Beispiel 25:00.";
        }

        absenceMinutes = null;
        int parsedAbsenceMinutes = 0;
        bool absenceValid = !EditorAllowsVacationAndSickness
            || EmployeeTypeDurationFormatter.TryParse(
                EditorAbsenceDayValue,
                out parsedAbsenceMinutes);
        if (EditorAllowsVacationAndSickness && absenceValid)
        {
            absenceMinutes = parsedAbsenceMinutes;
        }
        else if (!absenceValid)
        {
            AbsenceDayValueErrorMessage =
                "Bitte geben Sie den U-/K-Tageswert als Stunden:Minuten ein, zum Beispiel 5:00.";
        }

        return weeklyValid && absenceValid;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ExecuteWriteAsync(
        Func<Task<EmployeeTypeCommandResult>> executeAsync,
        CancellationToken cancellationToken)
    {
        IsSaving = true;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            EmployeeTypeCommandResult result = await executeAsync();
            if (result.Status != EmployeeTypeCommandStatus.Succeeded)
            {
                ApplyCommandErrors(result.Errors);
                return;
            }

            ApplySuccessfulWrite(result.Value!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "SaveEmployeeType");
            OperationErrorMessage =
                "Der Mitarbeitertyp konnte nicht gespeichert werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ApplySuccessfulWrite(EmployeeTypeSnapshot snapshot)
    {
        EmployeeTypeOverviewItemViewModel replacement = new(snapshot);
        EmployeeTypeOverviewItemViewModel? existing = EmployeeTypes.SingleOrDefault(
            value => value.Id == snapshot.Id);
        if (existing is not null)
        {
            EmployeeTypes.Remove(existing);
        }

        InsertSorted(replacement);
        SelectedEmployeeType = replacement;
        SetEditorMode(EmployeeTypeEditorMode.None);
        ClearEditor();
        SuccessMessage = existing is null
            ? "Der Mitarbeitertyp wurde angelegt."
            : "Die Änderungen am Mitarbeitertyp wurden gespeichert.";
        NotifyStateChanged();
    }

    private void RequestDeletion()
    {
        ClearFeedback();
        _isDeletionConfirmationOpen = true;
        OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
        NotifyStateChanged();
    }

    private void CancelDeletion()
    {
        _isDeletionConfirmationOpen = false;
        OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
        ClearFeedback();
        NotifyStateChanged();
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The view-model boundary translates unexpected failures into a visible state and reports them.")]
    private async Task ConfirmDeletionAsync(CancellationToken cancellationToken)
    {
        Guid id = SelectedEmployeeType?.Id ?? Guid.Empty;
        IsSaving = true;
        ClearFeedback();

        try
        {
            EmployeeTypeCommandResult result = await _deleteCommand.ExecuteAsync(
                new DeleteEmployeeTypeRequest(id, EmployeeTypeDeletionConfirmation.Confirmed),
                cancellationToken);
            if (result.Status != EmployeeTypeCommandStatus.Succeeded)
            {
                ApplyCommandErrors(result.Errors);
                return;
            }

            EmployeeTypeOverviewItemViewModel deleted = EmployeeTypes.Single(
                value => value.Id == id);
            EmployeeTypes.Remove(deleted);
            _isDeletionConfirmationOpen = false;
            OnPropertyChanged(nameof(IsDeletionConfirmationOpen));
            SelectedEmployeeType = EmployeeTypes.FirstOrDefault();
            SuccessMessage = "Der Mitarbeitertyp wurde endgültig gelöscht.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _errorReporter.Report(exception, "DeleteEmployeeType");
            OperationErrorMessage =
                "Der Mitarbeitertyp konnte nicht gelöscht werden. Bitte versuchen Sie es erneut.";
        }
        finally
        {
            IsSaving = false;
            NotifyStateChanged();
        }
    }

    private void ApplyCommandErrors(IEnumerable<EmployeeTypeCommandError> errors)
    {
        List<string> operationErrors = [];
        foreach (EmployeeTypeCommandError error in errors)
        {
            switch (error.Code)
            {
                case EmployeeTypeCommandErrorCode.CodeRequired:
                case EmployeeTypeCommandErrorCode.DuplicateCode:
                    CodeErrorMessage = error.Message;
                    break;
                case EmployeeTypeCommandErrorCode.NameRequired:
                    NameErrorMessage = error.Message;
                    break;
                case EmployeeTypeCommandErrorCode.WeeklyWorkTargetMustBePositive:
                case EmployeeTypeCommandErrorCode.WeeklyWorkTargetExceedsWeek:
                    WeeklyWorkTargetErrorMessage = error.Message;
                    break;
                case EmployeeTypeCommandErrorCode.AbsenceDayValueRequired:
                case EmployeeTypeCommandErrorCode.AbsenceDayValueMustNotBeSet:
                case EmployeeTypeCommandErrorCode.AbsenceDayValueMustBePositive:
                case EmployeeTypeCommandErrorCode.AbsenceDayValueExceedsDay:
                    AbsenceDayValueErrorMessage = error.Message;
                    break;
                case EmployeeTypeCommandErrorCode.ShiftEligibilityRequired:
                case EmployeeTypeCommandErrorCode.DuplicateShiftEligibility:
                case EmployeeTypeCommandErrorCode.EligibilityTargetRequired:
                case EmployeeTypeCommandErrorCode.UnknownShiftType:
                case EmployeeTypeCommandErrorCode.UnknownShiftPattern:
                case EmployeeTypeCommandErrorCode.UnsupportedEligibilityMode:
                case EmployeeTypeCommandErrorCode.UnsupportedEligibilityActivation:
                    EligibilityErrorMessage = error.Message;
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

    private void InsertSorted(EmployeeTypeOverviewItemViewModel item)
    {
        int index = 0;
        while (index < EmployeeTypes.Count
               && StringComparer.OrdinalIgnoreCase.Compare(EmployeeTypes[index].Code, item.Code) < 0)
        {
            index++;
        }

        EmployeeTypes.Insert(index, item);
    }

    private void SetEditorMode(EmployeeTypeEditorMode mode)
    {
        _editorMode = mode;
        OnPropertyChanged(nameof(IsEditorOpen));
        OnPropertyChanged(nameof(IsCreateMode));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(EditorPlanningRoleDisplay));
        NotifyStateChanged();
    }

    private void ClearEditor()
    {
        EditorCode = string.Empty;
        EditorName = string.Empty;
        EditorWeeklyWorkTarget = string.Empty;
        EditorAllowsVacationAndSickness = false;
        EditorAbsenceDayValue = string.Empty;
    }

    private void ClearFeedback()
    {
        CodeErrorMessage = null;
        NameErrorMessage = null;
        WeeklyWorkTargetErrorMessage = null;
        AbsenceDayValueErrorMessage = null;
        EligibilityErrorMessage = null;
        OperationErrorMessage = null;
        SuccessMessage = null;
    }

    private void SetEditorValue(
        ref string field,
        string value,
        string propertyName,
        Action clearError)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            clearError();
            OperationErrorMessage = null;
            SuccessMessage = null;
        }
    }

    private void SetFeedbackMessage(
        ref string? field,
        string? value,
        string messageProperty,
        string hasMessageProperty)
    {
        if (SetProperty(ref field, value, messageProperty))
        {
            OnPropertyChanged(hasMessageProperty);
        }
    }

    private void ClearCodeError() => CodeErrorMessage = null;

    private void ClearNameError() => NameErrorMessage = null;

    private void ClearWeeklyWorkTargetError() => WeeklyWorkTargetErrorMessage = null;

    private void ClearAbsenceDayValueError() => AbsenceDayValueErrorMessage = null;

    private void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(HasLoadError));
        OnPropertyChanged(nameof(HasEmployeeTypes));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(ShowSelectedDetails));
        OnPropertyChanged(nameof(EditorPlanningRoleDisplay));
        LoadCommand.NotifyCanExecuteChanged();
        BeginCreateCommand.NotifyCanExecuteChanged();
        BeginEditCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        RequestDeletionCommand.NotifyCanExecuteChanged();
        CancelDeletionCommand.NotifyCanExecuteChanged();
        ConfirmDeletionCommand.NotifyCanExecuteChanged();
    }
}
