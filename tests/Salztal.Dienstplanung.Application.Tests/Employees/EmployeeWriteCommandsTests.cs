using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.Employees;

public sealed class EmployeeWriteCommandsTests
{
    [Fact]
    public async Task CreateWhenInputAndTypeAreValidStoresActiveEmployee()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new();
        CreateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new CreateEmployeeRequest(
                "  Erika  ",
                "  Beispiel  ",
                InitialEmployeeTypeCatalog.Type30a.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Succeeded, result.Status);
        EmployeeDetailsSnapshot value = Assert.IsType<EmployeeDetailsSnapshot>(result.Value);
        Assert.NotEqual(Guid.Empty, value.Id);
        Assert.Equal("Erika Beispiel", value.DisplayName);
        Assert.True(value.IsActive);
        Assert.Equal("Typ30a", value.EmployeeType.Code);
        Assert.Empty(result.Errors);
        Employee created = Assert.IsType<Employee>(store.Replacement);
        Assert.Equal(value.Id, created.Id.Value);
        Assert.Equal(InitialEmployeeTypeCatalog.Type1.Id, store.Type1Id);
        Assert.Equal(1, store.CreateCallCount);
    }

    [Fact]
    public async Task CreateWhenInputIsInvalidReturnsGermanErrorsWithoutReadingOrWriting()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new();
        CreateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new CreateEmployeeRequest(" ", null, Guid.Empty),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.ValidationFailed, result.Status);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => AssertError(
                EmployeeCommandErrorCode.FirstNameRequired,
                "Bitte geben Sie einen Vornamen ein.",
                error),
            error => AssertError(
                EmployeeCommandErrorCode.LastNameRequired,
                "Bitte geben Sie einen Nachnamen ein.",
                error),
            error => AssertError(
                EmployeeCommandErrorCode.EmployeeTypeRequired,
                "Bitte wählen Sie einen Mitarbeitertyp aus.",
                error));
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task CreateWhenEmployeeTypeDoesNotExistReturnsTypeNotFound()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new();
        CreateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new CreateEmployeeRequest("Erika", "Beispiel", Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.EmployeeTypeNotFound, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        AssertError(
            EmployeeCommandErrorCode.EmployeeTypeNotFound,
            "Der ausgewählte Mitarbeitertyp wurde nicht gefunden.",
            error);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task CreateWhenEmployeeTypeReferencesUnknownServiceEntryReturnsCatalogError()
    {
        EmployeeType employeeType = CreateTypeWithUnknownShiftReference();
        FakeEmployeeReader reader = new(CreateReadData([], [employeeType]));
        FakeEmployeeWriteStore store = new();
        CreateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new CreateEmployeeRequest("Erika", "Beispiel", employeeType.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.EmployeeTypeCatalogInvalid, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeCommandErrorCode.EmployeeTypeCatalogInvalid, error.Code);
        Assert.Contains("ungültige Einsatzfreigaben", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task CreateWhenSecondActiveType1WouldResultReturnsConflict()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new()
        {
            WriteResult = EmployeeWriteStoreResult.ActiveType1Conflict,
        };
        CreateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new CreateEmployeeRequest(
                "Erika",
                "Beispiel",
                InitialEmployeeTypeCatalog.Type1.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.ActiveType1Conflict, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeCommandErrorCode.ActiveType1Conflict, error.Code);
        Assert.Contains("bereits eine aktive Person vom Typ1", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, store.CreateCallCount);
    }

    [Fact]
    public async Task UpdateNameWhenInputIsValidStoresNewVersion()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        UpdateEmployeeNameCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new UpdateEmployeeNameRequest(current.Id.Value, "Mara", "Muster"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Succeeded, result.Status);
        EmployeeDetailsSnapshot value = Assert.IsType<EmployeeDetailsSnapshot>(result.Value);
        Assert.Equal(current.Id.Value, value.Id);
        Assert.Equal("Mara Muster", value.DisplayName);
        Assert.Same(current, store.Expected);
        Assert.Equal("Mara Muster", store.Replacement?.DisplayName);
        Assert.Equal("Erika Beispiel", current.DisplayName);
        Assert.Equal(1, store.UpdateNameCallCount);
    }

    [Fact]
    public async Task UpdateNameWhenNameIsInvalidDoesNotWrite()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        UpdateEmployeeNameCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new UpdateEmployeeNameRequest(current.Id.Value, " ", "Muster"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.ValidationFailed, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeCommandErrorCode.FirstNameRequired, error.Code);
        Assert.Equal(0, store.TotalWriteCallCount);
        Assert.Equal("Erika Beispiel", current.DisplayName);
    }

    [Fact]
    public async Task UpdateNameWhenIdentifierIsInvalidDoesNotReadOrWrite()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new();
        UpdateEmployeeNameCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new UpdateEmployeeNameRequest(Guid.Empty, "Mara", "Muster"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.ValidationFailed, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        AssertError(
            EmployeeCommandErrorCode.IdentifierRequired,
            "Der Mitarbeiter besitzt keine gültige Kennung.",
            error);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task UpdateNameWhenEmployeeDoesNotExistReturnsNotFound()
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new();
        UpdateEmployeeNameCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new UpdateEmployeeNameRequest(Guid.NewGuid(), "Mara", "Muster"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.NotFound, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        AssertError(
            EmployeeCommandErrorCode.NotFound,
            "Der Mitarbeiter wurde nicht gefunden.",
            error);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task ChangeTypeWhenTargetExistsStoresNewVersion()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        ChangeEmployeeTypeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ChangeEmployeeTypeRequest(
                current.Id.Value,
                InitialEmployeeTypeCatalog.Type35a.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Succeeded, result.Status);
        EmployeeDetailsSnapshot value = Assert.IsType<EmployeeDetailsSnapshot>(result.Value);
        Assert.Equal("Typ35a", value.EmployeeType.Code);
        Assert.Same(current, store.Expected);
        Assert.Equal(InitialEmployeeTypeCatalog.Type35a.Id, store.Replacement?.EmployeeTypeId);
        Assert.Equal(InitialEmployeeTypeCatalog.Type25.Id, current.EmployeeTypeId);
        Assert.Equal(InitialEmployeeTypeCatalog.Type1.Id, store.Type1Id);
        Assert.Equal(1, store.ChangeTypeCallCount);
    }

    [Fact]
    public async Task ChangeTypeWhenTargetDoesNotExistReturnsTypeNotFound()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        ChangeEmployeeTypeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ChangeEmployeeTypeRequest(current.Id.Value, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.EmployeeTypeNotFound, result.Status);
        Assert.Equal(EmployeeCommandErrorCode.EmployeeTypeNotFound, Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task ChangeTypeWhenSecondActiveType1WouldResultReturnsConflict()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new()
        {
            WriteResult = EmployeeWriteStoreResult.ActiveType1Conflict,
        };
        ChangeEmployeeTypeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ChangeEmployeeTypeRequest(
                current.Id.Value,
                InitialEmployeeTypeCatalog.Type1.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.ActiveType1Conflict, result.Status);
        Assert.Equal(
            EmployeeCommandErrorCode.ActiveType1Conflict,
            Assert.Single(result.Errors).Code);
        Assert.Equal(1, store.ChangeTypeCallCount);
    }

    [Fact]
    public async Task DeactivateWhenEmployeeIsType1AllowsTemporarilyMissingActiveType1()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type1.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        DeactivateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new DeactivateEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Succeeded, result.Status);
        EmployeeDetailsSnapshot value = Assert.IsType<EmployeeDetailsSnapshot>(result.Value);
        Assert.False(value.IsActive);
        Assert.True(current.IsActive);
        Assert.Equal(1, store.DeactivateCallCount);
    }

    [Fact]
    public async Task ReactivateWhenInactiveEmployeeIsValidStoresActiveVersion()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        ReactivateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ReactivateEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Succeeded, result.Status);
        EmployeeDetailsSnapshot value = Assert.IsType<EmployeeDetailsSnapshot>(result.Value);
        Assert.True(value.IsActive);
        Assert.False(current.IsActive);
        Assert.Same(current, store.Expected);
        Assert.True(store.Replacement?.IsActive);
        Assert.Equal(InitialEmployeeTypeCatalog.Type1.Id, store.Type1Id);
        Assert.Equal(1, store.ReactivateCallCount);
    }

    [Fact]
    public async Task ReactivateWhenEmployeeIsAlreadyActiveDoesNotWrite()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        ReactivateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ReactivateEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.AlreadyActive, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        AssertError(
            EmployeeCommandErrorCode.AlreadyActive,
            "Der Mitarbeiter ist bereits aktiv.",
            error);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task ReactivateWhenSecondActiveType1WouldResultReturnsConflict()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type1.Id.Value)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new()
        {
            WriteResult = EmployeeWriteStoreResult.ActiveType1Conflict,
        };
        ReactivateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ReactivateEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.ActiveType1Conflict, result.Status);
        Assert.Equal(
            EmployeeCommandErrorCode.ActiveType1Conflict,
            Assert.Single(result.Errors).Code);
        Assert.Equal(1, store.ReactivateCallCount);
    }

    [Fact]
    public async Task DeleteWhenEmployeeIsInactiveRemovesEmployee()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        DeleteEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new DeleteEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Succeeded, result.Status);
        EmployeeDetailsSnapshot deleted = Assert.IsType<EmployeeDetailsSnapshot>(result.Value);
        Assert.Equal(current.Id.Value, deleted.Id);
        Assert.False(deleted.IsActive);
        Assert.Same(current, store.Expected);
        Assert.Equal(1, store.DeleteCallCount);
    }

    [Fact]
    public async Task DeleteWhenEmployeeIsActiveDoesNotWrite()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        DeleteEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new DeleteEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.MustBeInactive, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        AssertError(
            EmployeeCommandErrorCode.MustBeInactive,
            "Der Mitarbeiter muss vor dem endgültigen Löschen deaktiviert werden.",
            error);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task DeleteWhenEmployeeIsReferencedExplainsWhyItRemainsInactive()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new()
        {
            DeleteResult = EmployeeDeleteStoreResult.Referenced,
        };
        DeleteEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new DeleteEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Referenced, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeCommandErrorCode.Referenced, error.Code);
        Assert.Contains("anderen Fachdaten verwendet", error.Message, StringComparison.Ordinal);
        Assert.Contains("deaktiviert", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, store.DeleteCallCount);
    }

    [Theory]
    [InlineData(EmployeeCommandKind.Reactivate)]
    [InlineData(EmployeeCommandKind.Delete)]
    public async Task LifecycleCommandWhenEmployeeDoesNotExistReturnsNotFound(
        EmployeeCommandKind commandKind)
    {
        FakeEmployeeReader reader = new(CreateReadData([]));
        FakeEmployeeWriteStore store = new();
        Guid missingEmployeeId = new("12403ed2-4cd6-4fd2-b828-135078f541f2");

        EmployeeCommandResult result = commandKind switch
        {
            EmployeeCommandKind.Reactivate => await new ReactivateEmployeeCommand(
                reader,
                store).ExecuteAsync(
                    new ReactivateEmployeeRequest(missingEmployeeId),
                    TestContext.Current.CancellationToken),
            EmployeeCommandKind.Delete => await new DeleteEmployeeCommand(
                reader,
                store).ExecuteAsync(
                    new DeleteEmployeeRequest(missingEmployeeId),
                    TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(commandKind),
                commandKind,
                null),
        };

        Assert.Equal(EmployeeCommandStatus.NotFound, result.Status);
        Assert.Equal(EmployeeCommandErrorCode.NotFound, Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task ReactivateWhenStoredEmployeeChangedReturnsConflictAndRemainsInactive()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new()
        {
            WriteResult = EmployeeWriteStoreResult.Conflict,
        };
        ReactivateEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new ReactivateEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Conflict, result.Status);
        Assert.False(current.IsActive);
        Assert.Equal(1, store.ReactivateCallCount);
    }

    [Fact]
    public async Task DeleteWhenStoredEmployeeChangedReturnsConflictAndKeepsSnapshot()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value)
            .Deactivate();
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new()
        {
            DeleteResult = EmployeeDeleteStoreResult.Conflict,
        };
        DeleteEmployeeCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new DeleteEmployeeRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Conflict, result.Status);
        Assert.Null(result.Value);
        Assert.Equal(EmployeeCommandErrorCode.Conflict, Assert.Single(result.Errors).Code);
        Assert.Equal(1, store.DeleteCallCount);
    }

    [Fact]
    public async Task UpdateNameWhenStoredEmployeeChangedReturnsConflict()
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new()
        {
            WriteResult = EmployeeWriteStoreResult.Conflict,
        };
        UpdateEmployeeNameCommand command = new(reader, store);

        EmployeeCommandResult result = await command.ExecuteAsync(
            new UpdateEmployeeNameRequest(current.Id.Value, "Mara", "Muster"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeCommandStatus.Conflict, result.Status);
        EmployeeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeCommandErrorCode.Conflict, error.Code);
        Assert.Contains("Bitte laden Sie die Daten neu", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(EmployeeCommandKind.Create)]
    [InlineData(EmployeeCommandKind.UpdateName)]
    [InlineData(EmployeeCommandKind.ChangeType)]
    [InlineData(EmployeeCommandKind.Deactivate)]
    [InlineData(EmployeeCommandKind.Reactivate)]
    [InlineData(EmployeeCommandKind.Delete)]
    public async Task CommandWhenCancelledDoesNotReadOrWrite(EmployeeCommandKind commandKind)
    {
        Employee current = CreateEmployee(InitialEmployeeTypeCatalog.Type25.Id.Value);
        FakeEmployeeReader reader = new(CreateReadData([current]));
        FakeEmployeeWriteStore store = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task execution = commandKind switch
        {
            EmployeeCommandKind.Create => new CreateEmployeeCommand(reader, store).ExecuteAsync(
                new CreateEmployeeRequest(
                    "Erika",
                    "Beispiel",
                    InitialEmployeeTypeCatalog.Type25.Id.Value),
                cancellation.Token),
            EmployeeCommandKind.UpdateName => new UpdateEmployeeNameCommand(
                reader,
                store).ExecuteAsync(
                    new UpdateEmployeeNameRequest(current.Id.Value, "Mara", "Muster"),
                    cancellation.Token),
            EmployeeCommandKind.ChangeType => new ChangeEmployeeTypeCommand(
                reader,
                store).ExecuteAsync(
                    new ChangeEmployeeTypeRequest(
                        current.Id.Value,
                        InitialEmployeeTypeCatalog.Type30.Id.Value),
                    cancellation.Token),
            EmployeeCommandKind.Deactivate => new DeactivateEmployeeCommand(
                reader,
                store).ExecuteAsync(
                    new DeactivateEmployeeRequest(current.Id.Value),
                    cancellation.Token),
            EmployeeCommandKind.Reactivate => new ReactivateEmployeeCommand(
                reader,
                store).ExecuteAsync(
                    new ReactivateEmployeeRequest(current.Id.Value),
                    cancellation.Token),
            EmployeeCommandKind.Delete => new DeleteEmployeeCommand(
                reader,
                store).ExecuteAsync(
                    new DeleteEmployeeRequest(current.Id.Value),
                    cancellation.Token),
            _ => throw new ArgumentOutOfRangeException(
                nameof(commandKind),
                commandKind,
                null),
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public void StoreContractsWhenInspectedAreSeparatedByUseCase()
    {
        Assert.Equal("CreateAsync", Assert.Single(typeof(ICreateEmployeeStore).GetMethods()).Name);
        Assert.Equal(
            "UpdateNameAsync",
            Assert.Single(typeof(IUpdateEmployeeNameStore).GetMethods()).Name);
        Assert.Equal(
            "ChangeTypeAsync",
            Assert.Single(typeof(IChangeEmployeeTypeStore).GetMethods()).Name);
        Assert.Equal(
            "DeactivateAsync",
            Assert.Single(typeof(IDeactivateEmployeeStore).GetMethods()).Name);
        Assert.Equal(
            "ReactivateAsync",
            Assert.Single(typeof(IReactivateEmployeeStore).GetMethods()).Name);
        Assert.Equal(
            "DeleteAsync",
            Assert.Single(typeof(IDeleteEmployeeStore).GetMethods()).Name);
    }

    private static EmployeeReadData CreateReadData(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeType>? employeeTypes = null)
    {
        ServiceCatalogData serviceCatalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);

        return new EmployeeReadData(
            employees,
            employeeTypes ?? InitialEmployeeTypeCatalog.All,
            serviceCatalog);
    }

    private static Employee CreateEmployee(Guid employeeTypeId)
    {
        return Assert.IsType<Employee>(
            Employee.Create(
                new Guid("621cf6ca-bb9e-4ca3-ac23-fe4954ba52f4"),
                "Erika",
                "Beispiel",
                employeeTypeId).Value);
    }

    private static EmployeeType CreateTypeWithUnknownShiftReference()
    {
        ShiftTypeId.TryCreate(
            new Guid("c5025af8-6434-47dc-af16-81e85c609034"),
            out ShiftTypeId? unknownShiftTypeId);
        EmployeeTypeShiftEligibility eligibility =
            Assert.IsType<EmployeeTypeShiftEligibility>(
                EmployeeTypeShiftEligibility.CreateForShiftType(
                    unknownShiftTypeId!.Value,
                    ShiftEligibilityMode.Regular,
                    [unknownShiftTypeId]).Value);

        return Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                new Guid("120ab227-e15a-4f0f-86e2-d6b1ca71c477"),
                "TypSynthetisch",
                "Synthetischer Typ",
                1_500,
                [eligibility],
                EmployeeTypePlanningPolicy.Standard).Value);
    }

    private static void AssertError(
        EmployeeCommandErrorCode expectedCode,
        string expectedMessage,
        EmployeeCommandError actual)
    {
        Assert.Equal(expectedCode, actual.Code);
        Assert.Equal(expectedMessage, actual.Message);
    }

    public enum EmployeeCommandKind
    {
        Create,
        UpdateName,
        ChangeType,
        Deactivate,
        Reactivate,
        Delete,
    }

    private sealed class FakeEmployeeReader : IEmployeeReader
    {
        private readonly EmployeeReadData _data;

        public FakeEmployeeReader(EmployeeReadData data)
        {
            _data = data;
        }

        public int LoadCallCount { get; private set; }

        public Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }
    }

    private sealed class FakeEmployeeWriteStore :
        ICreateEmployeeStore,
        IUpdateEmployeeNameStore,
        IChangeEmployeeTypeStore,
        IDeactivateEmployeeStore,
        IReactivateEmployeeStore,
        IDeleteEmployeeStore
    {
        public EmployeeWriteStoreResult WriteResult { get; init; } =
            EmployeeWriteStoreResult.Succeeded;

        public EmployeeDeleteStoreResult DeleteResult { get; init; } =
            EmployeeDeleteStoreResult.Succeeded;

        public int CreateCallCount { get; private set; }

        public int UpdateNameCallCount { get; private set; }

        public int ChangeTypeCallCount { get; private set; }

        public int DeactivateCallCount { get; private set; }

        public int ReactivateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public int TotalWriteCallCount =>
            CreateCallCount
            + UpdateNameCallCount
            + ChangeTypeCallCount
            + DeactivateCallCount
            + ReactivateCallCount
            + DeleteCallCount;

        public Employee? Expected { get; private set; }

        public Employee? Replacement { get; private set; }

        public EmployeeTypeId? Type1Id { get; private set; }

        public Task<EmployeeWriteStoreResult> CreateAsync(
            Employee employee,
            EmployeeTypeId type1Id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreateCallCount++;
            Replacement = employee;
            Type1Id = type1Id;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeWriteStoreResult> UpdateNameAsync(
            Employee expected,
            Employee replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateNameCallCount++;
            Expected = expected;
            Replacement = replacement;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeWriteStoreResult> ChangeTypeAsync(
            Employee expected,
            Employee replacement,
            EmployeeTypeId type1Id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ChangeTypeCallCount++;
            Expected = expected;
            Replacement = replacement;
            Type1Id = type1Id;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeWriteStoreResult> DeactivateAsync(
            Employee expected,
            Employee replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeactivateCallCount++;
            Expected = expected;
            Replacement = replacement;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeWriteStoreResult> ReactivateAsync(
            Employee expected,
            Employee replacement,
            EmployeeTypeId type1Id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReactivateCallCount++;
            Expected = expected;
            Replacement = replacement;
            Type1Id = type1Id;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeDeleteStoreResult> DeleteAsync(
            Employee expected,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteCallCount++;
            Expected = expected;
            return Task.FromResult(DeleteResult);
        }
    }
}
