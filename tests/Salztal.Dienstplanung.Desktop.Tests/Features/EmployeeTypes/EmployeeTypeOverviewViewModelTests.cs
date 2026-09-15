using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.EmployeeTypes;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.EmployeeTypes;

public sealed class EmployeeTypeOverviewViewModelTests
{
    [Fact]
    public async Task LoadCommandShowsAllTypesTargetsAndProtectedEligibilityStates()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(11, viewModel.EmployeeTypes.Count);
        Assert.Equal(6, viewModel.EligibilityOptions.Count);
        EmployeeTypeOverviewItemViewModel auxiliary = Assert.Single(
            viewModel.EmployeeTypes,
            value => value.Id == InitialEmployeeTypeCatalog.TypeAh2.Id.Value);
        Assert.Equal("Aushilfe (geschützt)", auxiliary.PlanningRoleDisplay);
        Assert.Equal("U/K nicht zulässig", auxiliary.AbsenceDisplay);
        viewModel.SelectedEmployeeType = auxiliary;
        viewModel.BeginEditCommand.Execute(null);
        EmployeeTypeEligibilityEditorItemViewModel earlyShift = Assert.Single(
            viewModel.EligibilityOptions,
            value => value.TargetId == InitialShiftTypeCatalog.EarlyShift.Id.Value);
        EmployeeTypeEligibilityEditorItemViewModel relief = Assert.Single(
            viewModel.EligibilityOptions,
            value => value.TargetId == InitialShiftPatternCatalog.ReliefShift.Id.Value);
        Assert.False(earlyShift.IsRegularAllowed);
        Assert.True(earlyShift.IsManualSuggestionAllowed);
        Assert.False(relief.IsRegularAllowed);
        Assert.True(relief.RequiresPlanningOption);
    }

    [Fact]
    public async Task LoadCommandWhenCatalogIsEmptyShowsEmptyStateAndAllowsCreation()
    {
        FakeEmployeeTypeDataAccess dataAccess = new([]);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsEmpty);
        Assert.True(viewModel.BeginCreateCommand.CanExecute(null));
        Assert.Empty(viewModel.EmployeeTypes);
    }

    [Fact]
    public async Task LoadCommandWhenReaderFailsShowsGermanErrorAndReportsFailure()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All)
        {
            LoadException = new InvalidOperationException("synthetic failure"),
        };
        RecordingUnexpectedErrorReporter reporter = new();
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess, reporter);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasLoadError);
        Assert.Contains("nicht geladen", viewModel.LoadErrorMessage, StringComparison.Ordinal);
        Assert.Equal("LoadEmployeeTypes", reporter.Operation);
    }

    [Fact]
    public async Task SaveCommandInCreateModeAddsNormalTypeWithExactValues()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorCode = "TypNeu";
        viewModel.EditorName = "Synthetischer Typ";
        viewModel.EditorWeeklyWorkTarget = "22:30";
        viewModel.EditorAllowsVacationAndSickness = true;
        viewModel.EditorAbsenceDayValue = "4:30";
        Assert.Single(
            viewModel.EligibilityOptions,
            value => value.TargetId == InitialShiftTypeCatalog.EarlyShift.Id.Value)
            .IsRegularAllowed = true;
        Assert.Single(
            viewModel.EligibilityOptions,
            value => value.TargetId == InitialShiftPatternCatalog.ReliefShift.Id.Value)
            .RequiresPlanningOption = true;
        HashSet<string?> changedProperties = RecordPropertyChanges(viewModel);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.True(viewModel.HasSuccessMessage);
        Assert.Equal("Der Mitarbeitertyp wurde angelegt.", viewModel.SuccessMessage);
        Assert.Contains(nameof(viewModel.SuccessMessage), changedProperties);
        Assert.Contains(nameof(viewModel.HasSuccessMessage), changedProperties);
        Assert.Equal(12, viewModel.EmployeeTypes.Count);
        EmployeeType created = Assert.IsType<EmployeeType>(dataAccess.LastReplacement);
        Assert.Equal("TypNeu", created.Code.Value);
        Assert.Equal(1_350, created.WeeklyWorkTarget.Minutes);
        Assert.Equal(270, created.AbsencePolicy.DayValue?.Minutes);
        Assert.Equal(EmployeeTypePlanningRole.Normal, created.PlanningPolicy.Role);
        Assert.Equal(2, created.ShiftEligibilities.Count);
    }

    [Fact]
    public async Task SaveCommandInEditModePreservesCodeRoleAndSpecialEligibilityStates()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            value => value.Id == InitialEmployeeTypeCatalog.TypeAh2.Id.Value);
        viewModel.BeginEditCommand.Execute(null);
        viewModel.EditorName = "Geänderte Aushilfe";
        viewModel.EditorWeeklyWorkTarget = "11:15";
        HashSet<string?> changedProperties = RecordPropertyChanges(viewModel);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(
            "Die Änderungen am Mitarbeitertyp wurden gespeichert.",
            viewModel.SuccessMessage);
        Assert.Contains(nameof(viewModel.SuccessMessage), changedProperties);
        Assert.Contains(nameof(viewModel.HasSuccessMessage), changedProperties);
        EmployeeType replacement = Assert.IsType<EmployeeType>(dataAccess.LastReplacement);
        Assert.Equal(InitialEmployeeTypeCatalog.TypeAh2.Code, replacement.Code);
        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, replacement.PlanningPolicy.Role);
        Assert.Contains(
            replacement.ShiftEligibilities,
            value => value.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id
                && value.Mode == ShiftEligibilityMode.ManualSuggestion);
        Assert.Contains(
            replacement.ShiftEligibilities,
            value => value.ShiftPatternId == InitialShiftPatternCatalog.ReliefShift.Id
                && value.Activation == ShiftEligibilityActivation.ExplicitPlanningRunOption);
    }

    [Fact]
    public async Task SaveCommandWhenTimeFormatIsInvalidKeepsInputWithoutCallingApplication()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorCode = "TypZeit";
        viewModel.EditorName = "Zeitformat";
        viewModel.EditorWeeklyWorkTarget = "25 Stunden";
        viewModel.EditorAbsenceDayValue = "5";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasWeeklyWorkTargetError);
        Assert.True(viewModel.HasAbsenceDayValueError);
        Assert.Equal("TypZeit", viewModel.EditorCode);
        Assert.Equal(0, dataAccess.TotalWriteCallCount);
    }

    [Fact]
    public async Task SaveCommandWhenApplicationRejectsValuesShowsFieldErrorsAndKeepsInput()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorCode = " ";
        viewModel.EditorName = string.Empty;
        viewModel.EditorWeeklyWorkTarget = "0:00";
        viewModel.EditorAbsenceDayValue = "0:00";
        HashSet<string?> changedProperties = RecordPropertyChanges(viewModel);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasCodeError);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.CodeErrorMessage));
        Assert.Contains(nameof(viewModel.CodeErrorMessage), changedProperties);
        Assert.Contains(nameof(viewModel.HasCodeError), changedProperties);
        Assert.True(viewModel.HasNameError);
        Assert.True(viewModel.HasWeeklyWorkTargetError);
        Assert.True(viewModel.HasAbsenceDayValueError);
        Assert.True(viewModel.IsEditorOpen);
        Assert.Equal("0:00", viewModel.EditorWeeklyWorkTarget);
        Assert.Equal(0, dataAccess.TotalWriteCallCount);
    }

    [Fact]
    public async Task SaveCommandWhenStoreFailsKeepsInputAndReportsTechnicalFailure()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All)
        {
            WriteException = new InvalidOperationException("synthetic failure"),
        };
        RecordingUnexpectedErrorReporter reporter = new();
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess, reporter);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorCode = "TypFehler";
        viewModel.EditorName = "Bleibt erhalten";
        viewModel.EditorWeeklyWorkTarget = "20:00";
        viewModel.EditorAbsenceDayValue = "4:00";
        HashSet<string?> changedProperties = RecordPropertyChanges(viewModel);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasOperationError);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.OperationErrorMessage));
        Assert.Contains(nameof(viewModel.OperationErrorMessage), changedProperties);
        Assert.Contains(nameof(viewModel.HasOperationError), changedProperties);
        Assert.Equal("TypFehler", viewModel.EditorCode);
        Assert.Equal("Bleibt erhalten", viewModel.EditorName);
        Assert.Equal("SaveEmployeeType", reporter.Operation);
    }

    [Fact]
    public async Task SaveCommandWhenCancelledKeepsInputAndLeavesWorkingState()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All)
        {
            BlockCreate = true,
        };
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorCode = "TypAbbruch";
        viewModel.EditorName = "Eingabe bleibt";
        viewModel.EditorWeeklyWorkTarget = "20:00";
        viewModel.EditorAbsenceDayValue = "4:00";

        Task execution = viewModel.SaveCommand.ExecuteAsync(null);
        await dataAccess.CreateStarted.Task;
        Assert.True(viewModel.IsSaving);
        viewModel.SaveCommand.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);

        Assert.False(viewModel.IsSaving);
        Assert.True(viewModel.IsEditorOpen);
        Assert.Equal("TypAbbruch", viewModel.EditorCode);
        Assert.Equal("Eingabe bleibt", viewModel.EditorName);
    }

    [Fact]
    public async Task DeletionCommandsProtectSpecialRoleAndCancelNormalConfirmationWithoutWrite()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            value => value.Id == InitialEmployeeTypeCatalog.Type1.Id.Value);
        Assert.False(viewModel.RequestDeletionCommand.CanExecute(null));
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            value => value.Id == InitialEmployeeTypeCatalog.Type25.Id.Value);

        viewModel.RequestDeletionCommand.Execute(null);
        Assert.True(viewModel.IsDeletionConfirmationOpen);
        viewModel.CancelDeletionCommand.Execute(null);

        Assert.False(viewModel.IsDeletionConfirmationOpen);
        Assert.Equal(0, dataAccess.TotalWriteCallCount);
    }

    [Fact]
    public async Task ConfirmDeletionWhenUnusedRemovesTypeAfterExplicitConfirmation()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All);
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        Guid deletedId = InitialEmployeeTypeCatalog.Type25.Id.Value;
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            value => value.Id == deletedId);
        viewModel.RequestDeletionCommand.Execute(null);
        HashSet<string?> changedProperties = RecordPropertyChanges(viewModel);

        await viewModel.ConfirmDeletionCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsDeletionConfirmationOpen);
        Assert.DoesNotContain(viewModel.EmployeeTypes, value => value.Id == deletedId);
        Assert.True(viewModel.HasSuccessMessage);
        Assert.Equal("Der Mitarbeitertyp wurde endgültig gelöscht.", viewModel.SuccessMessage);
        Assert.Contains(nameof(viewModel.SuccessMessage), changedProperties);
        Assert.Contains(nameof(viewModel.HasSuccessMessage), changedProperties);
        Assert.Equal(1, dataAccess.DeleteCallCount);
    }

    [Fact]
    public async Task ConfirmDeletionWhenReferencedKeepsConfirmationAndVisibleType()
    {
        FakeEmployeeTypeDataAccess dataAccess = new(InitialEmployeeTypeCatalog.All)
        {
            DeleteResult = EmployeeTypeDeleteStoreResult.Referenced,
        };
        EmployeeTypeOverviewViewModel viewModel = CreateViewModel(dataAccess);
        await viewModel.LoadCommand.ExecuteAsync(null);
        Guid selectedId = InitialEmployeeTypeCatalog.Type25.Id.Value;
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            value => value.Id == selectedId);
        viewModel.RequestDeletionCommand.Execute(null);

        await viewModel.ConfirmDeletionCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDeletionConfirmationOpen);
        Assert.True(viewModel.HasOperationError);
        Assert.Contains("verwendet", viewModel.OperationErrorMessage, StringComparison.Ordinal);
        Assert.Contains(viewModel.EmployeeTypes, value => value.Id == selectedId);
    }

    private static EmployeeTypeOverviewViewModel CreateViewModel(
        FakeEmployeeTypeDataAccess dataAccess,
        IUnexpectedErrorReporter? errorReporter = null)
    {
        return new EmployeeTypeOverviewViewModel(
            new GetEmployeeTypeCatalogQuery(dataAccess),
            new CreateEmployeeTypeCommand(dataAccess, dataAccess),
            new UpdateEmployeeTypeCommand(dataAccess, dataAccess),
            new DeleteEmployeeTypeCommand(dataAccess, dataAccess),
            errorReporter ?? new RecordingUnexpectedErrorReporter());
    }

    private static HashSet<string?> RecordPropertyChanges(
        EmployeeTypeOverviewViewModel viewModel)
    {
        HashSet<string?> changedProperties = [];
        viewModel.PropertyChanged += (_, eventArgs) =>
            changedProperties.Add(eventArgs.PropertyName);
        return changedProperties;
    }

    private sealed class FakeEmployeeTypeDataAccess :
        IEmployeeReader,
        ICreateEmployeeTypeStore,
        IUpdateEmployeeTypeStore,
        IDeleteEmployeeTypeStore
    {
        private readonly List<EmployeeType> _employeeTypes;

        public FakeEmployeeTypeDataAccess(IEnumerable<EmployeeType> employeeTypes)
        {
            _employeeTypes = employeeTypes.ToList();
        }

        public Exception? LoadException { get; init; }

        public Exception? WriteException { get; init; }

        public bool BlockCreate { get; init; }

        public TaskCompletionSource CreateStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public EmployeeTypeDeleteStoreResult DeleteResult { get; init; } =
            EmployeeTypeDeleteStoreResult.Succeeded;

        public EmployeeType? LastReplacement { get; private set; }

        public int CreateCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public int TotalWriteCallCount => CreateCallCount + UpdateCallCount + DeleteCallCount;

        public Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (LoadException is not null)
            {
                throw LoadException;
            }

            return Task.FromResult(new EmployeeReadData(
                [],
                _employeeTypes,
                new ServiceCatalogData(
                    InitialWorkLocationCatalog.All,
                    InitialShiftTypeCatalog.All,
                    InitialShiftPatternCatalog.SplitShift,
                    InitialShiftPatternCatalog.ReliefShift)));
        }

        public async Task<EmployeeTypeWriteStoreResult> CreateAsync(
            EmployeeType employeeType,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowWriteException();
            CreateCallCount++;
            CreateStarted.TrySetResult();
            if (BlockCreate)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            LastReplacement = employeeType;
            _employeeTypes.Add(employeeType);
            return EmployeeTypeWriteStoreResult.Succeeded;
        }

        public Task<EmployeeTypeWriteStoreResult> UpdateAsync(
            EmployeeType expected,
            EmployeeType replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowWriteException();
            UpdateCallCount++;
            LastReplacement = replacement;
            _employeeTypes.RemoveAll(value => value.Id == expected.Id);
            _employeeTypes.Add(replacement);
            return Task.FromResult(EmployeeTypeWriteStoreResult.Succeeded);
        }

        public Task<EmployeeTypeDeleteStoreResult> DeleteAsync(
            EmployeeType expected,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowWriteException();
            DeleteCallCount++;
            if (DeleteResult == EmployeeTypeDeleteStoreResult.Succeeded)
            {
                _employeeTypes.RemoveAll(value => value.Id == expected.Id);
            }

            return Task.FromResult(DeleteResult);
        }

        private void ThrowWriteException()
        {
            if (WriteException is not null)
            {
                throw WriteException;
            }
        }
    }

    private sealed class RecordingUnexpectedErrorReporter : IUnexpectedErrorReporter
    {
        public string? Operation { get; private set; }

        public void Report(Exception exception, string operation)
        {
            ArgumentNullException.ThrowIfNull(exception);
            Operation = operation;
        }
    }
}
