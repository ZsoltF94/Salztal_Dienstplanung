using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Tests.Availabilities;

public sealed class AvailabilityEntryCommandsTests
{
    private static readonly Guid EmployeeIdentifier =
        new("2688013c-90a2-4dd4-883e-283d8fc54b32");

    private static readonly DateOnly EntryDate = new(2026, 12, 23);

    [Theory]
    [InlineData(AvailabilityDayEntryKind.Vacation)]
    [InlineData(AvailabilityDayEntryKind.Sickness)]
    [InlineData(AvailabilityDayEntryKind.FixedDayOff)]
    public async Task SaveWhenDayIsEmptyStoresEachSupportedKind(
        AvailabilityDayEntryKind kind)
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        FakeAvailabilityReader reader = CreateReader(type, true);
        FakeSetAvailabilityEntryStore store = new();
        SaveAvailabilityEntryCommand command = new(reader, store);
        SaveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            kind,
            null,
            AvailabilityEntryReplacementConfirmation.NotRequired);

        AvailabilityEntryCommandResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        AvailabilityPeriodEntrySnapshot value =
            Assert.IsType<AvailabilityPeriodEntrySnapshot>(result.Value);
        Assert.Equal(AvailabilityEntryCommandStatus.Succeeded, result.Status);
        Assert.Equal(kind, value.Kind);
        Assert.Equal(1, value.ChangeVersion);
        Assert.Null(store.ExpectedEntry);
        Assert.Null(store.ExpectedChangeVersion);
        Assert.Equal(kind, MapKind(Assert.IsType<AvailabilityEntry>(store.Replacement).Kind));
    }

    [Fact]
    public async Task SaveWhenCurrentKindIsReplacedStoresConfirmedNewVersion()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        AvailabilityEntry current = CreateEntry(AvailabilityEntryKind.Vacation);
        AvailabilityEntryReadItem item = new(current, 7);
        FakeAvailabilityReader reader = CreateReader(type, true, item);
        FakeSetAvailabilityEntryStore store = new()
        {
            Result = AvailabilityEntryWriteStoreResult.Succeeded(8),
        };
        SaveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            AvailabilityDayEntryKind.Sickness,
            7,
            AvailabilityEntryReplacementConfirmation.Confirmed);

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        AvailabilityPeriodEntrySnapshot value =
            Assert.IsType<AvailabilityPeriodEntrySnapshot>(result.Value);
        Assert.Equal(AvailabilityDayEntryKind.Sickness, value.Kind);
        Assert.Equal(8, value.ChangeVersion);
        Assert.Same(current, store.ExpectedEntry);
        Assert.Equal(7, store.ExpectedChangeVersion);
        Assert.Equal(AvailabilityEntryKind.Vacation, item.Entry.Kind);
    }

    [Fact]
    public async Task SaveWhenReplacementIsNotConfirmedDoesNotWrite()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        FakeAvailabilityReader reader = CreateReader(
            type,
            true,
            new AvailabilityEntryReadItem(
                CreateEntry(AvailabilityEntryKind.Vacation),
                4));
        FakeSetAvailabilityEntryStore store = new();
        SaveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            AvailabilityDayEntryKind.Sickness,
            4,
            AvailabilityEntryReplacementConfirmation.NotConfirmed);

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        AvailabilityEntryCommandError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityEntryCommandStatus.ConfirmationRequired, result.Status);
        Assert.Equal(AvailabilityEntryCommandErrorCode.ConfirmationRequired, error.Code);
        Assert.Equal(0, store.CallCount);
    }

    [Theory]
    [InlineData(AvailabilityDayEntryKind.Vacation)]
    [InlineData(AvailabilityDayEntryKind.Sickness)]
    public async Task SaveForAuxiliaryTypeWhenVacationOrSicknessIsSelectedRejectsKind(
        AvailabilityDayEntryKind kind)
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.TypeAh1,
            true);
        FakeSetAvailabilityEntryStore store = new();
        SaveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            kind,
            null,
            AvailabilityEntryReplacementConfirmation.NotRequired);

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        AvailabilityEntryCommandError error = Assert.Single(result.Errors);
        Assert.Equal(
            AvailabilityEntryCommandErrorCode.VacationAndSicknessNotAllowed,
            error.Code);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task SaveForAuxiliaryTypeWhenFixedDayOffIsSelectedSucceeds()
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.TypeAh1,
            true);
        FakeSetAvailabilityEntryStore store = new();
        SaveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            AvailabilityDayEntryKind.FixedDayOff,
            null,
            AvailabilityEntryReplacementConfirmation.NotRequired);

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.Succeeded, result.Status);
        Assert.Equal(1, store.CallCount);
    }

    [Fact]
    public async Task SaveWhenEmployeeIsInactiveDoesNotWrite()
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            false);
        FakeSetAvailabilityEntryStore store = new();

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(
                CreateSaveRequest(),
                TestContext.Current.CancellationToken);

        AvailabilityEntryCommandError error = Assert.Single(result.Errors);
        Assert.Equal(AvailabilityEntryCommandStatus.InactiveEmployee, result.Status);
        Assert.Equal(AvailabilityEntryCommandErrorCode.EmployeeInactive, error.Code);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task SaveWhenEmployeeDoesNotExistReturnsNotFound()
    {
        FakeAvailabilityReader reader = new(new AvailabilityReadData(
            [],
            InitialEmployeeTypeCatalog.All,
            []));
        FakeSetAvailabilityEntryStore store = new();

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(
                CreateSaveRequest(),
                TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.NotFound, result.Status);
        Assert.Equal(AvailabilityEntryCommandErrorCode.EmployeeNotFound, result.Errors[0].Code);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task SaveWhenEmployeeTypeIsMissingReturnsCatalogError()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        Employee employee = CreateEmployee(type, true);
        FakeAvailabilityReader reader = new(new AvailabilityReadData(
            [employee],
            [],
            []));
        FakeSetAvailabilityEntryStore store = new();

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(
                CreateSaveRequest(),
                TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.CatalogInvalid, result.Status);
        Assert.Equal(AvailabilityEntryCommandErrorCode.CatalogInvalid, result.Errors[0].Code);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task SaveWhenExpectedVersionIsStaleReturnsConflictWithoutWriting()
    {
        EmployeeType type = InitialEmployeeTypeCatalog.Type25;
        FakeAvailabilityReader reader = CreateReader(
            type,
            true,
            new AvailabilityEntryReadItem(
                CreateEntry(AvailabilityEntryKind.Vacation),
                3));
        FakeSetAvailabilityEntryStore store = new();
        SaveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            AvailabilityDayEntryKind.Sickness,
            2,
            AvailabilityEntryReplacementConfirmation.Confirmed);

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.Conflict, result.Status);
        Assert.Equal(AvailabilityEntryCommandErrorCode.Conflict, result.Errors[0].Code);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task SaveWhenStoreDetectsConcurrentChangeReturnsConflict()
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            true);
        FakeSetAvailabilityEntryStore store = new()
        {
            Result = AvailabilityEntryWriteStoreResult.Conflict,
        };

        AvailabilityEntryCommandResult result = await new SaveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(
                CreateSaveRequest(),
                TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.Conflict, result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task RemoveWhenConfirmedAndCurrentVersionMatchesDeletesEntry()
    {
        AvailabilityEntry current = CreateEntry(AvailabilityEntryKind.FixedDayOff);
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            true,
            new AvailabilityEntryReadItem(current, 5));
        FakeRemoveAvailabilityEntryStore store = new();
        RemoveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            5,
            AvailabilityEntryRemovalConfirmation.Confirmed);

        AvailabilityEntryCommandResult result = await new RemoveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        AvailabilityPeriodEntrySnapshot removed =
            Assert.IsType<AvailabilityPeriodEntrySnapshot>(result.Value);
        Assert.Equal(AvailabilityEntryCommandStatus.Succeeded, result.Status);
        Assert.Equal(AvailabilityDayEntryKind.FixedDayOff, removed.Kind);
        Assert.Same(current, store.ExpectedEntry);
        Assert.Equal(5, store.ExpectedChangeVersion);
    }

    [Fact]
    public async Task RemoveWhenNotConfirmedDoesNotReadOrDelete()
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            true);
        FakeRemoveAvailabilityEntryStore store = new();
        RemoveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            5,
            AvailabilityEntryRemovalConfirmation.NotConfirmed);

        AvailabilityEntryCommandResult result = await new RemoveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.ConfirmationRequired, result.Status);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task RemoveWhenEntryDoesNotExistReturnsNotFound()
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            true);
        FakeRemoveAvailabilityEntryStore store = new();
        RemoveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            5,
            AvailabilityEntryRemovalConfirmation.Confirmed);

        AvailabilityEntryCommandResult result = await new RemoveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.NotFound, result.Status);
        Assert.Equal(AvailabilityEntryCommandErrorCode.EntryNotFound, result.Errors[0].Code);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task RemoveWhenStoreDetectsConcurrentChangeReturnsConflict()
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            true,
            new AvailabilityEntryReadItem(
                CreateEntry(AvailabilityEntryKind.Sickness),
                5));
        FakeRemoveAvailabilityEntryStore store = new()
        {
            Result = AvailabilityEntryRemoveStoreResult.Conflict,
        };
        RemoveAvailabilityEntryRequest request = new(
            EmployeeIdentifier,
            EntryDate,
            5,
            AvailabilityEntryRemovalConfirmation.Confirmed);

        AvailabilityEntryCommandResult result = await new RemoveAvailabilityEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(AvailabilityEntryCommandStatus.Conflict, result.Status);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData(AvailabilityCommandKind.Save)]
    [InlineData(AvailabilityCommandKind.Remove)]
    public async Task CommandWhenCancelledDoesNotReadOrWrite(
        AvailabilityCommandKind commandKind)
    {
        FakeAvailabilityReader reader = CreateReader(
            InitialEmployeeTypeCatalog.Type25,
            true,
            new AvailabilityEntryReadItem(
                CreateEntry(AvailabilityEntryKind.Vacation),
                5));
        FakeSetAvailabilityEntryStore saveStore = new();
        FakeRemoveAvailabilityEntryStore removeStore = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<AvailabilityEntryCommandResult> execution = commandKind switch
        {
            AvailabilityCommandKind.Save => new SaveAvailabilityEntryCommand(
                reader,
                saveStore).ExecuteAsync(CreateSaveRequest(), cancellation.Token),
            AvailabilityCommandKind.Remove => new RemoveAvailabilityEntryCommand(
                reader,
                removeStore).ExecuteAsync(
                    new RemoveAvailabilityEntryRequest(
                        EmployeeIdentifier,
                        EntryDate,
                        5,
                        AvailabilityEntryRemovalConfirmation.Confirmed),
                    cancellation.Token),
            _ => throw new ArgumentOutOfRangeException(nameof(commandKind)),
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, saveStore.CallCount);
        Assert.Equal(0, removeStore.CallCount);
    }

    private static SaveAvailabilityEntryRequest CreateSaveRequest()
    {
        return new SaveAvailabilityEntryRequest(
            EmployeeIdentifier,
            EntryDate,
            AvailabilityDayEntryKind.FixedDayOff,
            null,
            AvailabilityEntryReplacementConfirmation.NotRequired);
    }

    private static FakeAvailabilityReader CreateReader(
        EmployeeType employeeType,
        bool isActive,
        params AvailabilityEntryReadItem[] entries)
    {
        return new FakeAvailabilityReader(new AvailabilityReadData(
            [CreateEmployee(employeeType, isActive)],
            [employeeType],
            entries));
    }

    private static Employee CreateEmployee(EmployeeType employeeType, bool isActive)
    {
        Employee employee = Assert.IsType<Employee>(
            Employee.Create(
                EmployeeIdentifier,
                "Erika",
                "Muster",
                employeeType.Id.Value).Value);
        return isActive ? employee : employee.Deactivate();
    }

    private static AvailabilityEntry CreateEntry(AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(EmployeeIdentifier, EntryDate, kind).Value);
    }

    private static AvailabilityDayEntryKind MapKind(AvailabilityEntryKind kind)
    {
        return kind switch
        {
            AvailabilityEntryKind.Vacation => AvailabilityDayEntryKind.Vacation,
            AvailabilityEntryKind.Sickness => AvailabilityDayEntryKind.Sickness,
            AvailabilityEntryKind.FixedDayOff => AvailabilityDayEntryKind.FixedDayOff,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    public enum AvailabilityCommandKind
    {
        Save,
        Remove,
    }

    private sealed class FakeAvailabilityReader : IAvailabilityReader
    {
        private readonly AvailabilityReadData _data;

        public FakeAvailabilityReader(AvailabilityReadData data)
        {
            _data = data;
        }

        public int LoadCallCount { get; private set; }

        public Task<AvailabilityReadData> LoadAsync(
            DateOnly periodMonday,
            DateOnly periodSunday,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }
    }

    private sealed class FakeSetAvailabilityEntryStore : ISetAvailabilityEntryStore
    {
        public AvailabilityEntryWriteStoreResult Result { get; set; } =
            AvailabilityEntryWriteStoreResult.Succeeded(1);

        public int CallCount { get; private set; }

        public AvailabilityEntry? ExpectedEntry { get; private set; }

        public long? ExpectedChangeVersion { get; private set; }

        public AvailabilityEntry? Replacement { get; private set; }

        public Task<AvailabilityEntryWriteStoreResult> SaveAsync(
            AvailabilityEntry? expectedEntry,
            long? expectedChangeVersion,
            AvailabilityEntry replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            ExpectedEntry = expectedEntry;
            ExpectedChangeVersion = expectedChangeVersion;
            Replacement = replacement;
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeRemoveAvailabilityEntryStore : IRemoveAvailabilityEntryStore
    {
        public AvailabilityEntryRemoveStoreResult Result { get; set; } =
            AvailabilityEntryRemoveStoreResult.Succeeded;

        public int CallCount { get; private set; }

        public AvailabilityEntry? ExpectedEntry { get; private set; }

        public long? ExpectedChangeVersion { get; private set; }

        public Task<AvailabilityEntryRemoveStoreResult> RemoveAsync(
            AvailabilityEntry expectedEntry,
            long expectedChangeVersion,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            ExpectedEntry = expectedEntry;
            ExpectedChangeVersion = expectedChangeVersion;
            return Task.FromResult(Result);
        }
    }
}
