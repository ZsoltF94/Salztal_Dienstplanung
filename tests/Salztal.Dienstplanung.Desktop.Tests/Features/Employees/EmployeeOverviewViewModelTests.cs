using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Desktop.Features.Employees;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Employees;

public sealed class EmployeeOverviewViewModelTests
{
    [Fact]
    public async Task LoadCommandWhenEmployeesExistShowsCurrentValuesAndSelectsFirstEmployee()
    {
        Employee active = CreateEmployee(
            new Guid("43837a3e-d385-4452-8518-03d3df53a0d8"),
            "Anna",
            "Aktiv",
            InitialEmployeeTypeCatalog.TypeAh2.Id);
        Employee inactive = CreateEmployee(
            new Guid("0aa433ac-e221-43c4-8d74-f67cb9820d75"),
            "Dora",
            "Deaktiviert",
            InitialEmployeeTypeCatalog.Type25.Id).Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([active, inactive]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.Employees.Count);
        EmployeeOverviewItemViewModel first = viewModel.Employees[0];
        Assert.Equal("Anna", first.FirstName);
        Assert.Equal("Aktiv", first.LastName);
        Assert.Equal("Aktiv", first.StatusDisplay);
        Assert.Equal("TypAH2", first.EmployeeTypeCode);
        Assert.Equal(
            "Restaurant, Cafeteria B und Doppeldienst - 10 Stunden",
            first.EmployeeTypeName);
        Assert.Equal("10 Stunden", first.WeeklyWorkTargetDisplay);
        Assert.Contains("Spätdienst: Regulär zulässig", first.EligibilitySummary);
        Assert.Contains(
            first.ShiftEligibilities,
            eligibility => eligibility.TargetName == "Frühdienst"
                && eligibility.AvailabilityDisplay == "Nur als manueller Lösungsvorschlag");
        Assert.Contains(
            first.ShiftEligibilities,
            eligibility => eligibility.TargetName == "Spr"
                && eligibility.AvailabilityDisplay == "Nur bei aktivierter Planungslaufoption");
        Assert.Same(first, viewModel.SelectedEmployee);
        Assert.Equal("Deaktiviert", viewModel.Employees[1].StatusDisplay);
        Assert.True(viewModel.Employees[1].IsInactive);
        EmployeeTypeOptionViewModel typeOption = Assert.Single(
            viewModel.EmployeeTypes,
            employeeType => employeeType.Code == "TypAH2");
        Assert.Contains("10 Stunden", typeOption.SelectionDisplay, StringComparison.Ordinal);
        Assert.Contains(
            typeOption.ShiftEligibilities,
            eligibility => eligibility.SummaryDisplay
                == "Frühdienst: Nur als manueller Lösungsvorschlag");
        Assert.True(viewModel.HasEmployees);
        Assert.False(viewModel.IsEmpty);
        Assert.False(viewModel.HasLoadError);
    }

    [Fact]
    public async Task SelectedEmployeeWhenChangedKeepsSingleExplicitSelection()
    {
        Employee first = CreateEmployee(
            new Guid("e975dc37-5888-4677-8370-7811881756be"),
            "Erste",
            "Person",
            InitialEmployeeTypeCatalog.Type30.Id);
        Employee second = CreateEmployee(
            new Guid("3107ac5c-b6b8-4981-b623-2a38b723fe93"),
            "Zweite",
            "Person",
            InitialEmployeeTypeCatalog.Type35.Id);
        EmployeeOverviewViewModel viewModel = CreateViewModel(
            new FakeEmployeeReader(CreateReadData([first, second])));
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectedEmployee = viewModel.Employees[1];

        Assert.Same(viewModel.Employees[1], viewModel.SelectedEmployee);
        Assert.Equal("Zweite Person", viewModel.SelectedEmployee.DisplayName);
    }

    [Fact]
    public async Task LoadCommandWhileReaderWaitsShowsWorkingStateAndDisablesItself()
    {
        TaskCompletionSource<EmployeeReadData> completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        FakeEmployeeReader reader = new(CreateReadData([]))
        {
            LoadHandler = cancellationToken => completion.Task.WaitAsync(cancellationToken),
        };
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);

        Task loading = viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsLoading);
        Assert.False(viewModel.LoadCommand.CanExecute(null));
        Assert.False(viewModel.IsEmpty);
        completion.SetResult(CreateReadData([]));
        await loading;
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadCommandWhenNoEmployeesExistShowsEmptyState()
    {
        EmployeeOverviewViewModel viewModel = CreateViewModel(
            new FakeEmployeeReader(CreateReadData([])));

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.Employees);
        Assert.Null(viewModel.SelectedEmployee);
        Assert.True(viewModel.IsEmpty);
        Assert.False(viewModel.HasEmployees);
        Assert.False(viewModel.HasLoadError);
    }

    [Fact]
    public async Task LoadCommandWhenReaderFailsShowsGermanErrorAndReportsTechnicalFailure()
    {
        InvalidOperationException failure = new("Synthetic employee load failure.");
        FakeEmployeeReader reader = new(CreateReadData([]))
        {
            LoadHandler = _ => Task.FromException<EmployeeReadData>(failure),
        };
        CollectingUnexpectedErrorReporter errorReporter = new();
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader, errorReporter);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasLoadError);
        Assert.Contains("nicht geladen", viewModel.LoadErrorMessage, StringComparison.Ordinal);
        Assert.False(viewModel.IsEmpty);
        Assert.Empty(viewModel.Employees);
        (Exception exception, string operation) = Assert.Single(errorReporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("LoadEmployeeOverview", operation);
    }

    [Fact]
    public async Task LoadCommandAfterFailureCanRetryAndShowEmployees()
    {
        Employee employee = CreateEmployee(
            new Guid("c6a21e09-7acc-4d2a-9f6e-63b366915b40"),
            "Neue",
            "Person",
            InitialEmployeeTypeCatalog.Type30a.Id);
        EmployeeReadData successfulData = CreateReadData([employee]);
        int attempts = 0;
        FakeEmployeeReader reader = new(successfulData)
        {
            LoadHandler = _ =>
            {
                attempts++;
                return attempts <= 2
                    ? Task.FromException<EmployeeReadData>(
                        new InvalidOperationException("Synthetic first attempt failure."))
                    : Task.FromResult(successfulData);
            },
        };
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);

        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasLoadError);
        Assert.Single(viewModel.Employees);
        Assert.Equal("Neue Person", viewModel.SelectedEmployee?.DisplayName);
        Assert.Equal(4, reader.LoadCallCount);
    }

    [Fact]
    public async Task SaveCreateCommandWithValidInputAddsAndSelectsEmployee()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorFirstName = "Erika";
        viewModel.EditorLastName = "Muster";
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            employeeType => employeeType.Code == "Typ30");
        await viewModel.SaveCreateCommand.ExecuteAsync(null);

        EmployeeOverviewItemViewModel employee = Assert.Single(viewModel.Employees);
        Assert.Equal("Erika Muster", employee.DisplayName);
        Assert.Equal("Typ30", employee.EmployeeTypeCode);
        Assert.Same(employee, viewModel.SelectedEmployee);
        Assert.False(viewModel.IsEditorOpen);
        Assert.Contains("angelegt", viewModel.SuccessMessage, StringComparison.Ordinal);
        Assert.Equal(1, reader.CreateCallCount);
    }

    [Fact]
    public async Task SaveCreateCommandWithMissingValuesKeepsInputAndShowsFieldErrors()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorFirstName = " ";
        viewModel.EditorLastName = string.Empty;

        await viewModel.SaveCreateCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsCreateMode);
        Assert.Equal(" ", viewModel.EditorFirstName);
        Assert.True(viewModel.HasFirstNameError);
        Assert.True(viewModel.HasLastNameError);
        Assert.True(viewModel.HasEmployeeTypeError);
        Assert.Empty(viewModel.Employees);
        Assert.Equal(0, reader.CreateCallCount);
    }

    [Fact]
    public async Task CancelEditCommandDiscardsDraftWithoutWriting()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorFirstName = "Nicht";
        viewModel.EditorLastName = "Speichern";

        viewModel.CancelEditCommand.Execute(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.True(viewModel.IsEmpty);
        Assert.Empty(viewModel.EditorFirstName);
        Assert.Empty(viewModel.EditorLastName);
        Assert.Equal(0, reader.CreateCallCount);
    }

    [Fact]
    public async Task SaveNameCommandWithValidInputUpdatesSelectedEmployee()
    {
        Employee employee = CreateEmployee(
            new Guid("c5598b6d-e69b-4312-a50e-122f17d5afbd"),
            "Alter",
            "Name",
            InitialEmployeeTypeCatalog.Type25.Id);
        FakeEmployeeReader reader = new(CreateReadData([employee]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginRenameCommand.Execute(null);
        viewModel.EditorFirstName = "Neuer";
        viewModel.EditorLastName = "Name";

        await viewModel.SaveNameCommand.ExecuteAsync(null);

        Assert.Equal("Neuer Name", viewModel.SelectedEmployee?.DisplayName);
        Assert.Equal("Typ25", viewModel.SelectedEmployee?.EmployeeTypeCode);
        Assert.False(viewModel.IsEditorOpen);
        Assert.Equal(1, reader.UpdateNameCallCount);
    }

    [Fact]
    public async Task SaveTypeChangeCommandWithValidSelectionUpdatesTypeDetails()
    {
        Employee employee = CreateEmployee(
            new Guid("59047072-b0b1-4084-9a67-c49810340a77"),
            "Typ",
            "Wechsel",
            InitialEmployeeTypeCatalog.Type25.Id);
        FakeEmployeeReader reader = new(CreateReadData([employee]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginTypeChangeCommand.Execute(null);
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            employeeType => employeeType.Code == "Typ30a");

        await viewModel.SaveTypeChangeCommand.ExecuteAsync(null);

        Assert.Equal("Typ30a", viewModel.SelectedEmployee?.EmployeeTypeCode);
        Assert.Equal("30 Stunden", viewModel.SelectedEmployee?.WeeklyWorkTargetDisplay);
        Assert.False(viewModel.IsEditorOpen);
        Assert.Equal(1, reader.ChangeTypeCallCount);
    }

    [Fact]
    public async Task SaveTypeChangeCommandOnType1ConflictKeepsEditorAndExplainsConflict()
    {
        Employee employee = CreateEmployee(
            new Guid("5fe7e3ee-5766-4f4c-a875-f55cc8dfc2ac"),
            "Zweite",
            "Leitung",
            InitialEmployeeTypeCatalog.Type25.Id);
        FakeEmployeeReader reader = new(CreateReadData([employee]))
        {
            NextWriteResult = EmployeeWriteStoreResult.ActiveType1Conflict,
        };
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginTypeChangeCommand.Execute(null);
        viewModel.SelectedEmployeeType = Assert.Single(
            viewModel.EmployeeTypes,
            employeeType => employeeType.Code == "Typ1");

        await viewModel.SaveTypeChangeCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsTypeChangeMode);
        Assert.Contains("aktive Person vom Typ1", viewModel.OperationErrorMessage, StringComparison.Ordinal);
        Assert.Equal("Typ25", viewModel.SelectedEmployee?.EmployeeTypeCode);
        Assert.Equal(1, reader.ChangeTypeCallCount);
    }

    [Fact]
    public async Task DeactivationCommandsWhenCanceledDoNotWriteAndWhenConfirmedDeactivateEmployee()
    {
        Employee employee = CreateEmployee(
            new Guid("ca7b01d9-b68a-41bc-ad02-7729c628cc77"),
            "Dora",
            "Beispiel",
            InitialEmployeeTypeCatalog.Type35.Id);
        FakeEmployeeReader reader = new(CreateReadData([employee]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.RequestDeactivationCommand.Execute(null);
        viewModel.CancelDeactivationCommand.Execute(null);

        Assert.False(viewModel.IsDeactivationConfirmationOpen);
        Assert.Equal(0, reader.DeactivateCallCount);
        Assert.True(viewModel.SelectedEmployee?.IsActive);

        viewModel.RequestDeactivationCommand.Execute(null);
        await viewModel.ConfirmDeactivationCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsDeactivationConfirmationOpen);
        Assert.False(viewModel.SelectedEmployee?.IsActive);
        Assert.Equal("Deaktiviert", viewModel.SelectedEmployee?.StatusDisplay);
        Assert.Equal(1, reader.DeactivateCallCount);
        Assert.False(viewModel.RequestDeactivationCommand.CanExecute(null));
        Assert.True(viewModel.ReactivateEmployeeCommand.CanExecute(null));
        Assert.True(viewModel.RequestDeletionCommand.CanExecute(null));
    }

    [Fact]
    public async Task ReactivateEmployeeCommandWhenInactiveReactivatesSelectedEmployee()
    {
        Employee employee = CreateEmployee(
                new Guid("04bd5012-cf2d-45ab-8ee1-6819015e5d4a"),
                "Ria",
                "Reaktivierung",
                InitialEmployeeTypeCatalog.Type30.Id)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([employee]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.ReactivateEmployeeCommand.ExecuteAsync(null);

        Assert.True(viewModel.SelectedEmployee?.IsActive);
        Assert.Equal("Aktiv", viewModel.SelectedEmployee?.StatusDisplay);
        Assert.Equal("Der Mitarbeiter wurde reaktiviert.", viewModel.SuccessMessage);
        Assert.Equal(1, reader.ReactivateCallCount);
        Assert.False(viewModel.ReactivateEmployeeCommand.CanExecute(null));
        Assert.True(viewModel.RequestDeactivationCommand.CanExecute(null));
    }

    [Fact]
    public async Task ReactivateEmployeeCommandOnType1ConflictKeepsEmployeeInactiveAndExplainsConflict()
    {
        Employee employee = CreateEmployee(
                new Guid("82099d37-769b-4b18-b74b-f0fe5857d6d8"),
                "Frühere",
                "Leitung",
                InitialEmployeeTypeCatalog.Type1.Id)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([employee]))
        {
            NextWriteResult = EmployeeWriteStoreResult.ActiveType1Conflict,
        };
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.ReactivateEmployeeCommand.ExecuteAsync(null);

        Assert.False(viewModel.SelectedEmployee?.IsActive);
        Assert.Contains("aktive Person vom Typ1", viewModel.OperationErrorMessage, StringComparison.Ordinal);
        Assert.Equal(1, reader.ReactivateCallCount);
    }

    [Fact]
    public async Task DeletionCommandsWhenCanceledDoNotWriteAndWhenConfirmedRemoveEmployee()
    {
        Employee employee = CreateEmployee(
                new Guid("9c6b2660-0c04-4f06-8a2e-5ddc73ea3986"),
                "Löschbare",
                "Person",
                InitialEmployeeTypeCatalog.Type35.Id)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([employee]));
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.RequestDeletionCommand.Execute(null);
        Assert.True(viewModel.IsDeletionConfirmationOpen);
        Assert.False(viewModel.LoadCommand.CanExecute(null));
        viewModel.CancelDeletionCommand.Execute(null);

        Assert.False(viewModel.IsDeletionConfirmationOpen);
        Assert.Equal(0, reader.DeleteCallCount);
        Assert.Single(viewModel.Employees);

        viewModel.RequestDeletionCommand.Execute(null);
        await viewModel.ConfirmDeletionCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsDeletionConfirmationOpen);
        Assert.Empty(viewModel.Employees);
        Assert.Null(viewModel.SelectedEmployee);
        Assert.True(viewModel.IsEmpty);
        Assert.Equal("Der Mitarbeiter wurde endgültig gelöscht.", viewModel.SuccessMessage);
        Assert.Equal(1, reader.DeleteCallCount);
    }

    [Fact]
    public async Task ConfirmDeletionWhenEmployeeIsReferencedKeepsConfirmationAndEmployeeVisible()
    {
        Employee employee = CreateEmployee(
                new Guid("09d8b14b-f3b2-4b66-a879-a3940c11b4bf"),
                "Verwendete",
                "Person",
                InitialEmployeeTypeCatalog.TypeAh1.Id)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([employee]))
        {
            NextDeleteResult = EmployeeDeleteStoreResult.Referenced,
        };
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.RequestDeletionCommand.Execute(null);

        await viewModel.ConfirmDeletionCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDeletionConfirmationOpen);
        Assert.Single(viewModel.Employees);
        Assert.False(viewModel.SelectedEmployee?.IsActive);
        Assert.Contains("anderen Fachdaten verwendet", viewModel.OperationErrorMessage, StringComparison.Ordinal);
        Assert.Equal(1, reader.DeleteCallCount);
    }

    [Fact]
    public async Task ConfirmDeletionWhenStoreFailsKeepsConfirmationAndReportsTechnicalFailure()
    {
        InvalidOperationException failure = new("Synthetic employee deletion failure.");
        Employee employee = CreateEmployee(
                new Guid("752b55f1-e43b-429e-9db2-d087350e55e5"),
                "Technischer",
                "Löschtest",
                InitialEmployeeTypeCatalog.TypeAh2.Id)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([employee]))
        {
            WriteException = failure,
        };
        CollectingUnexpectedErrorReporter errorReporter = new();
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader, errorReporter);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.RequestDeletionCommand.Execute(null);

        await viewModel.ConfirmDeletionCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDeletionConfirmationOpen);
        Assert.Single(viewModel.Employees);
        Assert.Contains("nicht endgültig gelöscht", viewModel.OperationErrorMessage, StringComparison.Ordinal);
        (Exception exception, string operation) = Assert.Single(errorReporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("DeleteEmployee", operation);
    }

    [Fact]
    public async Task SaveCreateCommandWhenStoreFailsKeepsEditorAndReportsTechnicalFailure()
    {
        InvalidOperationException failure = new("Synthetic employee write failure.");
        FakeEmployeeReader reader = new(CreateReadData([]))
        {
            WriteException = failure,
        };
        CollectingUnexpectedErrorReporter errorReporter = new();
        EmployeeOverviewViewModel viewModel = CreateViewModel(reader, errorReporter);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.BeginCreateCommand.Execute(null);
        viewModel.EditorFirstName = "Technischer";
        viewModel.EditorLastName = "Testfall";
        viewModel.SelectedEmployeeType = viewModel.EmployeeTypes[0];

        await viewModel.SaveCreateCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsCreateMode);
        Assert.Equal("Technischer", viewModel.EditorFirstName);
        Assert.Contains("nicht gespeichert", viewModel.OperationErrorMessage, StringComparison.Ordinal);
        (Exception exception, string operation) = Assert.Single(errorReporter.Reports);
        Assert.Same(failure, exception);
        Assert.Equal("CreateEmployee", operation);
    }

    private static EmployeeOverviewViewModel CreateViewModel(
        FakeEmployeeReader reader,
        CollectingUnexpectedErrorReporter? errorReporter = null)
    {
        return new EmployeeOverviewViewModel(
            new GetEmployeeOverviewQuery(reader),
            new GetEmployeeTypeCatalogQuery(reader),
            new CreateEmployeeCommand(reader, reader),
            new UpdateEmployeeNameCommand(reader, reader),
            new ChangeEmployeeTypeCommand(reader, reader),
            new DeactivateEmployeeCommand(reader, reader),
            new ReactivateEmployeeCommand(reader, reader),
            new DeleteEmployeeCommand(reader, reader),
            errorReporter ?? new CollectingUnexpectedErrorReporter());
    }

    private static EmployeeReadData CreateReadData(IEnumerable<Employee> employees)
    {
        ServiceCatalogData serviceCatalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);

        return new EmployeeReadData(
            employees,
            InitialEmployeeTypeCatalog.All,
            serviceCatalog);
    }

    private static Employee CreateEmployee(
        Guid id,
        string firstName,
        string lastName,
        EmployeeTypeId employeeTypeId)
    {
        return Assert.IsType<Employee>(
            Employee.Create(id, firstName, lastName, employeeTypeId.Value).Value);
    }

    private sealed class FakeEmployeeReader :
        IEmployeeReader,
        ICreateEmployeeStore,
        IUpdateEmployeeNameStore,
        IChangeEmployeeTypeStore,
        IDeactivateEmployeeStore,
        IReactivateEmployeeStore,
        IDeleteEmployeeStore
    {
        private EmployeeReadData _data;

        public FakeEmployeeReader(EmployeeReadData data)
        {
            _data = data;
        }

        public Func<CancellationToken, Task<EmployeeReadData>>? LoadHandler { get; init; }

        public EmployeeWriteStoreResult NextWriteResult { get; set; } =
            EmployeeWriteStoreResult.Succeeded;

        public EmployeeDeleteStoreResult NextDeleteResult { get; set; } =
            EmployeeDeleteStoreResult.Succeeded;

        public Exception? WriteException { get; init; }

        public int LoadCallCount { get; private set; }

        public int CreateCallCount { get; private set; }

        public int UpdateNameCallCount { get; private set; }

        public int ChangeTypeCallCount { get; private set; }

        public int DeactivateCallCount { get; private set; }

        public int ReactivateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken)
        {
            LoadCallCount++;
            return LoadHandler?.Invoke(cancellationToken) ?? Task.FromResult(_data);
        }

        public Task<EmployeeWriteStoreResult> CreateAsync(
            Employee employee,
            EmployeeTypeId type1Id,
            CancellationToken cancellationToken)
        {
            CreateCallCount++;
            return CompleteWriteAsync(employee, cancellationToken);
        }

        public Task<EmployeeWriteStoreResult> UpdateNameAsync(
            Employee expected,
            Employee replacement,
            CancellationToken cancellationToken)
        {
            UpdateNameCallCount++;
            return CompleteWriteAsync(replacement, cancellationToken);
        }

        public Task<EmployeeWriteStoreResult> ChangeTypeAsync(
            Employee expected,
            Employee replacement,
            EmployeeTypeId type1Id,
            CancellationToken cancellationToken)
        {
            ChangeTypeCallCount++;
            return CompleteWriteAsync(replacement, cancellationToken);
        }

        public Task<EmployeeWriteStoreResult> DeactivateAsync(
            Employee expected,
            Employee replacement,
            CancellationToken cancellationToken)
        {
            DeactivateCallCount++;
            return CompleteWriteAsync(replacement, cancellationToken);
        }

        public Task<EmployeeWriteStoreResult> ReactivateAsync(
            Employee expected,
            Employee replacement,
            EmployeeTypeId type1Id,
            CancellationToken cancellationToken)
        {
            ReactivateCallCount++;
            return CompleteWriteAsync(replacement, cancellationToken);
        }

        public Task<EmployeeDeleteStoreResult> DeleteAsync(
            Employee expected,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteCallCount++;
            if (WriteException is not null)
            {
                throw WriteException;
            }

            EmployeeDeleteStoreResult result = NextDeleteResult;
            NextDeleteResult = EmployeeDeleteStoreResult.Succeeded;
            if (result == EmployeeDeleteStoreResult.Succeeded)
            {
                _data = new EmployeeReadData(
                    _data.Employees.Where(employee => employee.Id != expected.Id),
                    _data.EmployeeTypes,
                    _data.ServiceCatalog);
            }

            return Task.FromResult(result);
        }

        private Task<EmployeeWriteStoreResult> CompleteWriteAsync(
            Employee employee,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (WriteException is not null)
            {
                throw WriteException;
            }

            EmployeeWriteStoreResult result = NextWriteResult;
            NextWriteResult = EmployeeWriteStoreResult.Succeeded;

            if (result == EmployeeWriteStoreResult.Succeeded)
            {
                _data = new EmployeeReadData(
                    _data.Employees
                        .Where(existing => existing.Id != employee.Id)
                        .Append(employee),
                    _data.EmployeeTypes,
                    _data.ServiceCatalog);
            }

            return Task.FromResult(result);
        }
    }

    private sealed class CollectingUnexpectedErrorReporter : IUnexpectedErrorReporter
    {
        public List<(Exception Exception, string Operation)> Reports { get; } = [];

        public void Report(Exception exception, string operation)
        {
            Reports.Add((exception, operation));
        }
    }
}
