using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.Employees;

public sealed class EmployeeTypeManagementCommandsTests
{
    [Fact]
    public async Task CreateWhenDefinitionIsValidStoresNormalType()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        CreateEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            CreateValidRequest("  TypSynthetisch  "),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Succeeded, result.Status);
        EmployeeTypeSnapshot value = Assert.IsType<EmployeeTypeSnapshot>(result.Value);
        Assert.NotEqual(Guid.Empty, value.Id);
        Assert.Equal("TypSynthetisch", value.Code);
        Assert.Equal("Synthetischer Typ", value.Name);
        Assert.Equal(1_200, value.WeeklyWorkTargetMinutes);
        Assert.True(value.AllowsVacationAndSickness);
        Assert.Equal(240, value.AbsenceDayValueMinutes);
        Assert.Equal(EmployeeTypePlanningRoleKind.Normal, value.PlanningRole);
        EmployeeTypeEligibilitySnapshot eligibility = Assert.Single(value.ShiftEligibilities);
        Assert.Equal(EmployeeTypeEligibilityTargetKind.ShiftType, eligibility.TargetKind);
        Assert.Equal(EmployeeTypeEligibilityMode.Regular, eligibility.Mode);
        Assert.Equal(EmployeeTypeEligibilityActivation.Always, eligibility.Activation);
        Assert.Empty(result.Errors);
        EmployeeType created = Assert.IsType<EmployeeType>(store.Replacement);
        Assert.Equal(EmployeeTypePlanningRole.Normal, created.PlanningPolicy.Role);
        Assert.Equal(1, store.CreateCallCount);
    }

    [Fact]
    public async Task CreateWhenCodeAlreadyExistsIgnoringCaseDoesNotWrite()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        CreateEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            CreateValidRequest("typ25"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.DuplicateCode, result.Status);
        EmployeeTypeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeCommandErrorCode.DuplicateCode, error.Code);
        Assert.Contains("eindeutigen Typcode", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task CreateWhenStoreDetectsConcurrentDuplicateReturnsDuplicateCode()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new()
        {
            WriteResult = EmployeeTypeWriteStoreResult.DuplicateCode,
        };
        CreateEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            CreateValidRequest("TypSynthetisch"),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.DuplicateCode, result.Status);
        Assert.Equal(EmployeeTypeCommandErrorCode.DuplicateCode, Assert.Single(result.Errors).Code);
        Assert.Equal(1, store.CreateCallCount);
    }

    [Fact]
    public async Task CreateWhenDefinitionIsInvalidReturnsAllCorrectableErrors()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        CreateEmployeeTypeCommand command = new(reader, store);
        CreateEmployeeTypeRequest request = new(
            " ",
            null,
            0,
            true,
            null,
            [null]);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.ValidationFailed, result.Status);
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(EmployeeTypeCommandErrorCode.ShiftEligibilityRequired, error.Code),
            error => Assert.Equal(EmployeeTypeCommandErrorCode.CodeRequired, error.Code),
            error => Assert.Equal(EmployeeTypeCommandErrorCode.NameRequired, error.Code),
            error => Assert.Equal(
                EmployeeTypeCommandErrorCode.WeeklyWorkTargetMustBePositive,
                error.Code),
            error => Assert.Equal(EmployeeTypeCommandErrorCode.AbsenceDayValueRequired, error.Code));
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task CreateWhenEligibilityReferencesUnknownShiftReturnsCatalogError()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        CreateEmployeeTypeCommand command = new(reader, store);
        CreateEmployeeTypeRequest request = new(
            "TypSynthetisch",
            "Synthetischer Typ",
            1_200,
            true,
            240,
            [new EmployeeTypeEligibilityRequest(
                Guid.NewGuid(),
                EmployeeTypeEligibilityTargetKind.ShiftType,
                EmployeeTypeEligibilityMode.Regular,
                EmployeeTypeEligibilityActivation.Always)]);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.CatalogInvalid, result.Status);
        EmployeeTypeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeCommandErrorCode.UnknownShiftType, error.Code);
        Assert.Contains("nicht gefunden", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task CreateWhenShiftTypeUsesPlanningOptionRejectsStructuredSetting()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        CreateEmployeeTypeCommand command = new(reader, store);
        CreateEmployeeTypeRequest request = new(
            "TypSynthetisch",
            "Synthetischer Typ",
            1_200,
            true,
            240,
            [new EmployeeTypeEligibilityRequest(
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                EmployeeTypeEligibilityTargetKind.ShiftType,
                EmployeeTypeEligibilityMode.Regular,
                EmployeeTypeEligibilityActivation.ExplicitPlanningRunOption)]);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.ValidationFailed, result.Status);
        Assert.Equal(
            EmployeeTypeCommandErrorCode.UnsupportedEligibilityActivation,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task UpdateWhenDetailsAreValidPreservesIdentifierCodeAndSpecialRole()
    {
        EmployeeType current = InitialEmployeeTypeCatalog.TypeAh2;
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        UpdateEmployeeTypeCommand command = new(reader, store);
        UpdateEmployeeTypeRequest request = new(
            current.Id.Value,
            "AH aktuell",
            660,
            false,
            null,
            CreateEligibilityRequests(current));

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Succeeded, result.Status);
        EmployeeTypeSnapshot value = Assert.IsType<EmployeeTypeSnapshot>(result.Value);
        Assert.Equal(current.Id.Value, value.Id);
        Assert.Equal(current.Code.Value, value.Code);
        Assert.Equal("AH aktuell", value.Name);
        Assert.Equal(660, value.WeeklyWorkTargetMinutes);
        Assert.False(value.AllowsVacationAndSickness);
        Assert.Null(value.AbsenceDayValueMinutes);
        Assert.Equal(EmployeeTypePlanningRoleKind.Auxiliary, value.PlanningRole);
        Assert.Same(current, store.Expected);
        Assert.Equal(EmployeeTypePlanningRole.Auxiliary, store.Replacement?.PlanningPolicy.Role);
        Assert.Equal(current.Code, store.Replacement?.Code);
        Assert.Equal(1, store.UpdateCallCount);
    }

    [Fact]
    public async Task UpdateWhenTypeDoesNotExistReturnsNotFound()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        UpdateEmployeeTypeCommand command = new(reader, store);
        UpdateEmployeeTypeRequest request = CreateUpdateRequest(Guid.NewGuid());

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.NotFound, result.Status);
        Assert.Equal(EmployeeTypeCommandErrorCode.NotFound, Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task UpdateWhenCurrentTypeHasUnknownCatalogReferenceRejectsReadState()
    {
        EmployeeType invalidCurrent = CreateTypeWithUnknownShiftReference();
        FakeEmployeeReader reader = new(CreateReadData([], [invalidCurrent]));
        FakeEmployeeTypeStore store = new();
        UpdateEmployeeTypeCommand command = new(reader, store);
        UpdateEmployeeTypeRequest request = CreateUpdateRequest(invalidCurrent.Id.Value);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.CatalogInvalid, result.Status);
        Assert.Equal(EmployeeTypeCommandErrorCode.CatalogInvalid, Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task UpdateWhenStoredTypeChangedReturnsConflictWithoutMutatingCurrent()
    {
        EmployeeType current = InitialEmployeeTypeCatalog.Type25;
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new()
        {
            WriteResult = EmployeeTypeWriteStoreResult.Conflict,
        };
        UpdateEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            CreateUpdateRequest(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Conflict, result.Status);
        Assert.Null(result.Value);
        Assert.Contains("zwischenzeitlich geändert", Assert.Single(result.Errors).Message, StringComparison.Ordinal);
        Assert.Equal("Restaurant - 25 Stunden", current.Name.Value);
        Assert.Equal(1, store.UpdateCallCount);
    }

    [Fact]
    public async Task DeleteWithoutConfirmationDoesNotReadOrWrite()
    {
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        DeleteEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            new DeleteEmployeeTypeRequest(
                InitialEmployeeTypeCatalog.Type25.Id.Value,
                EmployeeTypeDeletionConfirmation.NotConfirmed),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.ConfirmationRequired, result.Status);
        Assert.Equal(
            EmployeeTypeCommandErrorCode.ConfirmationRequired,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task DeleteWhenNormalTypeIsUnreferencedRemovesType()
    {
        EmployeeType current = CreateNormalType("TypSynthetisch");
        FakeEmployeeReader reader = new(CreateReadData([], [current]));
        FakeEmployeeTypeStore store = new();
        DeleteEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            ConfirmDelete(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Succeeded, result.Status);
        Assert.Equal(current.Id.Value, Assert.IsType<EmployeeTypeSnapshot>(result.Value).Id);
        Assert.Same(current, store.Expected);
        Assert.Equal(1, store.DeleteCallCount);
    }

    [Theory]
    [InlineData("Typ1")]
    [InlineData("TypAH1")]
    public async Task DeleteWhenRoleIsProtectedDoesNotWrite(string typeCode)
    {
        EmployeeType current = Assert.Single(
            InitialEmployeeTypeCatalog.All,
            employeeType => employeeType.Code.Value == typeCode);
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        DeleteEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            ConfirmDelete(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Protected, result.Status);
        Assert.Equal(EmployeeTypeCommandErrorCode.Protected, Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteWhenAssignedEmployeeExistsNamesBlockingReference(bool isActive)
    {
        EmployeeType current = CreateNormalType("TypSynthetisch");
        Employee employee = CreateEmployee(current.Id.Value, isActive);
        FakeEmployeeReader reader = new(CreateReadData([employee], [current]));
        FakeEmployeeTypeStore store = new();
        DeleteEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            ConfirmDelete(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Referenced, result.Status);
        EmployeeTypeCommandError error = Assert.Single(result.Errors);
        Assert.Equal(EmployeeTypeCommandErrorCode.AssignedEmployee, error.Code);
        Assert.Contains("aktiven oder deaktivierten Person", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task DeleteWhenStoreFindsOtherReferenceExplainsConflict()
    {
        EmployeeType current = CreateNormalType("TypSynthetisch");
        FakeEmployeeReader reader = new(CreateReadData([], [current]));
        FakeEmployeeTypeStore store = new()
        {
            DeleteResult = EmployeeTypeDeleteStoreResult.Referenced,
        };
        DeleteEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            ConfirmDelete(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Referenced, result.Status);
        Assert.Equal(EmployeeTypeCommandErrorCode.Referenced, Assert.Single(result.Errors).Code);
        Assert.Equal(1, store.DeleteCallCount);
    }

    [Fact]
    public async Task DeleteWhenStoredTypeChangedReturnsConflict()
    {
        EmployeeType current = CreateNormalType("TypSynthetisch");
        FakeEmployeeReader reader = new(CreateReadData([], [current]));
        FakeEmployeeTypeStore store = new()
        {
            DeleteResult = EmployeeTypeDeleteStoreResult.Conflict,
        };
        DeleteEmployeeTypeCommand command = new(reader, store);

        EmployeeTypeCommandResult result = await command.ExecuteAsync(
            ConfirmDelete(current.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeTypeCommandStatus.Conflict, result.Status);
        Assert.Equal(EmployeeTypeCommandErrorCode.Conflict, Assert.Single(result.Errors).Code);
        Assert.Equal(1, store.DeleteCallCount);
    }

    [Theory]
    [InlineData(EmployeeTypeCommandKind.Create)]
    [InlineData(EmployeeTypeCommandKind.Update)]
    [InlineData(EmployeeTypeCommandKind.Delete)]
    public async Task CommandWhenCancelledDoesNotReadOrWrite(
        EmployeeTypeCommandKind commandKind)
    {
        EmployeeType current = InitialEmployeeTypeCatalog.Type25;
        FakeEmployeeReader reader = new(CreateReadData([], InitialEmployeeTypeCatalog.All));
        FakeEmployeeTypeStore store = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task execution = commandKind switch
        {
            EmployeeTypeCommandKind.Create => new CreateEmployeeTypeCommand(
                reader,
                store).ExecuteAsync(CreateValidRequest("TypSynthetisch"), cancellation.Token),
            EmployeeTypeCommandKind.Update => new UpdateEmployeeTypeCommand(
                reader,
                store).ExecuteAsync(CreateUpdateRequest(current.Id.Value), cancellation.Token),
            EmployeeTypeCommandKind.Delete => new DeleteEmployeeTypeCommand(
                reader,
                store).ExecuteAsync(ConfirmDelete(current.Id.Value), cancellation.Token),
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
        Assert.Equal(
            "CreateAsync",
            Assert.Single(typeof(ICreateEmployeeTypeStore).GetMethods()).Name);
        Assert.Equal(
            "UpdateAsync",
            Assert.Single(typeof(IUpdateEmployeeTypeStore).GetMethods()).Name);
        Assert.Equal(
            "DeleteAsync",
            Assert.Single(typeof(IDeleteEmployeeTypeStore).GetMethods()).Name);
    }

    private static CreateEmployeeTypeRequest CreateValidRequest(string code)
    {
        return new CreateEmployeeTypeRequest(
            code,
            "Synthetischer Typ",
            1_200,
            true,
            240,
            [new EmployeeTypeEligibilityRequest(
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                EmployeeTypeEligibilityTargetKind.ShiftType,
                EmployeeTypeEligibilityMode.Regular,
                EmployeeTypeEligibilityActivation.Always)]);
    }

    private static UpdateEmployeeTypeRequest CreateUpdateRequest(Guid employeeTypeId)
    {
        return new UpdateEmployeeTypeRequest(
            employeeTypeId,
            "Synthetischer Typ aktuell",
            1_260,
            true,
            240,
            [new EmployeeTypeEligibilityRequest(
                InitialShiftTypeCatalog.LateShift.Id.Value,
                EmployeeTypeEligibilityTargetKind.ShiftType,
                EmployeeTypeEligibilityMode.Regular,
                EmployeeTypeEligibilityActivation.Always)]);
    }

    private static DeleteEmployeeTypeRequest ConfirmDelete(Guid employeeTypeId)
    {
        return new DeleteEmployeeTypeRequest(
            employeeTypeId,
            EmployeeTypeDeletionConfirmation.Confirmed);
    }

    private static EmployeeTypeEligibilityRequest[] CreateEligibilityRequests(
        EmployeeType employeeType)
    {
        return employeeType.ShiftEligibilities.Select(eligibility =>
            new EmployeeTypeEligibilityRequest(
                eligibility.ShiftTypeId?.Value ?? eligibility.ShiftPatternId!.Value,
                eligibility.TargetKind == ShiftEligibilityTargetKind.ShiftType
                    ? EmployeeTypeEligibilityTargetKind.ShiftType
                    : EmployeeTypeEligibilityTargetKind.ShiftPattern,
                eligibility.Mode == ShiftEligibilityMode.Regular
                    ? EmployeeTypeEligibilityMode.Regular
                    : EmployeeTypeEligibilityMode.ManualSuggestion,
                eligibility.Activation == ShiftEligibilityActivation.Always
                    ? EmployeeTypeEligibilityActivation.Always
                    : EmployeeTypeEligibilityActivation.ExplicitPlanningRunOption)).ToArray();
    }

    private static EmployeeReadData CreateReadData(
        IEnumerable<Employee> employees,
        IEnumerable<EmployeeType> employeeTypes)
    {
        ServiceCatalogData serviceCatalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);

        return new EmployeeReadData(employees, employeeTypes, serviceCatalog);
    }

    private static EmployeeType CreateNormalType(string code)
    {
        EmployeeTypeShiftEligibility eligibility =
            Assert.IsType<EmployeeTypeShiftEligibility>(
                EmployeeTypeShiftEligibility.CreateForShiftType(
                    InitialShiftTypeCatalog.EarlyShift.Id.Value,
                    ShiftEligibilityMode.Regular,
                    InitialShiftTypeCatalog.All.Select(shiftType => shiftType.Id).ToArray()).Value);

        return Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                Guid.NewGuid(),
                code,
                "Synthetischer Typ",
                1_200,
                true,
                240,
                [eligibility],
                EmployeeTypePlanningPolicy.Standard).Value);
    }

    private static EmployeeType CreateTypeWithUnknownShiftReference()
    {
        Guid unknownId = Guid.NewGuid();
        ShiftTypeId.TryCreate(unknownId, out ShiftTypeId? unknownShiftTypeId);
        EmployeeTypeShiftEligibility eligibility =
            Assert.IsType<EmployeeTypeShiftEligibility>(
                EmployeeTypeShiftEligibility.CreateForShiftType(
                    unknownId,
                    ShiftEligibilityMode.Regular,
                    [unknownShiftTypeId!]).Value);

        return Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                Guid.NewGuid(),
                "TypSynthetisch",
                "Synthetischer Typ",
                1_200,
                true,
                240,
                [eligibility],
                EmployeeTypePlanningPolicy.Standard).Value);
    }

    private static Employee CreateEmployee(Guid employeeTypeId, bool isActive)
    {
        Employee employee = Assert.IsType<Employee>(
            Employee.Create(
                Guid.NewGuid(),
                "Erika",
                "Beispiel",
                employeeTypeId).Value);

        return isActive ? employee : employee.Deactivate();
    }

    public enum EmployeeTypeCommandKind
    {
        Create,
        Update,
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

    private sealed class FakeEmployeeTypeStore :
        ICreateEmployeeTypeStore,
        IUpdateEmployeeTypeStore,
        IDeleteEmployeeTypeStore
    {
        public EmployeeTypeWriteStoreResult WriteResult { get; init; } =
            EmployeeTypeWriteStoreResult.Succeeded;

        public EmployeeTypeDeleteStoreResult DeleteResult { get; init; } =
            EmployeeTypeDeleteStoreResult.Succeeded;

        public int CreateCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public int TotalWriteCallCount =>
            CreateCallCount + UpdateCallCount + DeleteCallCount;

        public EmployeeType? Expected { get; private set; }

        public EmployeeType? Replacement { get; private set; }

        public Task<EmployeeTypeWriteStoreResult> CreateAsync(
            EmployeeType employeeType,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CreateCallCount++;
            Replacement = employeeType;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeTypeWriteStoreResult> UpdateAsync(
            EmployeeType expected,
            EmployeeType replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateCallCount++;
            Expected = expected;
            Replacement = replacement;
            return Task.FromResult(WriteResult);
        }

        public Task<EmployeeTypeDeleteStoreResult> DeleteAsync(
            EmployeeType expected,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteCallCount++;
            Expected = expected;
            return Task.FromResult(DeleteResult);
        }
    }
}
