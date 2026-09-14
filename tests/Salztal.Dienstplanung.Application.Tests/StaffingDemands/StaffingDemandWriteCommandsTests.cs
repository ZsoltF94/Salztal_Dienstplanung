using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.StaffingDemands;

public sealed class StaffingDemandWriteCommandsTests
{
    private static readonly DateOnly WeekMonday = new(2026, 9, 14);

    [Fact]
    public async Task StandardReplacementWritesValidatedRevisionWithExpectedPredecessor()
    {
        StandardStaffingDemandRevision current = FindInitialStandard(
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        FakeStaffingDemandReader reader = new(CreateReadData());
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(reader, store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Replace,
            WeekMonday,
            current.Id.Value);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(1, reader.LoadCallCount);
        Assert.Equal(1, store.StandardAppendCallCount);
        Assert.Same(current, store.ExpectedStandardRevision);
        StandardStaffingDemandRevision written =
            Assert.IsType<StandardStaffingDemandRevision>(store.WrittenStandardRevision);
        Assert.Equal(StandardStaffingDemandRevisionKind.Replace, written.Kind);
        Assert.Equal(WeekMonday, written.EffectiveFromMonday);
        Assert.Equal(1, written.CorrectionSequence);
        Assert.Equal(new TimeOnly(7, 0), written.ActualTime!.Start);
        Assert.Equal(new TimeOnly(13, 0), written.ActualTime.End);
        Assert.Equal(3, written.RequiredEmployeeCount!.Value);
    }

    [Fact]
    public async Task StandardRemovalWritesRevisionWithoutTimeOrEmployeeCount()
    {
        StandardStaffingDemandRevision current = FindInitialStandard(
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Remove,
            WeekMonday,
            current.Id.Value) with
        {
            ActualStart = null,
            ActualEnd = null,
            RequiredEmployeeCount = null,
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        StandardStaffingDemandRevision written =
            Assert.IsType<StandardStaffingDemandRevision>(store.WrittenStandardRevision);
        Assert.Equal(StandardStaffingDemandRevisionKind.Remove, written.Kind);
        Assert.Null(written.ActualTime);
        Assert.Null(written.RequiredEmployeeCount);
        Assert.Null(written.RequiredWorkMinutes);
    }

    [Fact]
    public async Task StandardAdditionAfterRemovalUsesRemovalAsExpectedPredecessor()
    {
        StandardStaffingDemandRevision current = FindInitialStandard(
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        StandardStaffingDemandRevision removal = Assert.IsType<
            StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateRemoval(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                InitialWorkLocationCatalog.Restaurant.Id.Value,
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                WeekMonday).Value);
        List<StandardStaffingDemandRevision> standards =
            [.. InitialStaffingDemandCatalog.All, removal];
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(standards: standards)),
            store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Add,
            WeekMonday.AddDays(7),
            removal.Id.Value);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        Assert.Same(removal, store.ExpectedStandardRevision);
        Assert.NotEqual(current.Id.Value, store.ExpectedStandardRevision!.Id.Value);
        Assert.Equal(
            StandardStaffingDemandRevisionKind.Add,
            store.WrittenStandardRevision!.Kind);
    }

    [Fact]
    public async Task StandardWithNonMondayStartIsRejectedWithoutReadingOrWriting()
    {
        FakeStaffingDemandReader reader = new(CreateReadData());
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(reader, store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Replace,
            WeekMonday.AddDays(1),
            FindInitialStandard(
                DayOfWeek.Monday,
                InitialShiftTypeCatalog.EarlyShift.Id.Value).Id.Value);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.ValidationFailed, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandCommandErrorCode
                .EffectiveDateMustBeMonday);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task StandardWithUnknownWorkLocationIsRejectedWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Add,
            WeekMonday,
            null) with
        {
            WorkLocationId = Guid.NewGuid(),
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.NotFound, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandCommandErrorCode.WorkLocationNotFound);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task StandardWithShiftTypeFromDifferentLocationIsRejectedWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Add,
            WeekMonday,
            null) with
        {
            WorkLocationId = InitialWorkLocationCatalog.Cafeteria.Id.Value,
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.ValidationFailed, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandCommandErrorCode
                .ShiftTypeWorkLocationMismatch);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task ChangedCurrentStandardReturnsConflictWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Replace,
            WeekMonday,
            Guid.NewGuid());

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.Conflict, result.Status);
        Assert.Equal(StaffingDemandCommandErrorCode.Conflict, Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task RepeatedStandardCorrectionForSameMondayAppendsNextSequence()
    {
        StandardStaffingDemandRevision initial = FindInitialStandard(
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        StandardStaffingDemandRevision firstCorrection = Assert.IsType<
            StandardStaffingDemandRevision>(
            StandardStaffingDemandRevision.CreateReplacement(
                Guid.NewGuid(),
                DayOfWeek.Monday,
                InitialWorkLocationCatalog.Restaurant.Id.Value,
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                WeekMonday,
                new TimeOnly(6, 30),
                new TimeOnly(13, 0),
                3,
                correctionSequence: 1).Value);
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(
                standards: InitialStaffingDemandCatalog.All.Append(firstCorrection))),
            store);

        StaffingDemandCommandResult result = await ExecuteAsync(
            command,
            CreateStandardRequest(
                StandardStaffingDemandRevisionKind.Replace,
                WeekMonday,
                firstCorrection.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Same(firstCorrection, store.ExpectedStandardRevision);
        Assert.NotEqual(initial.Id, store.ExpectedStandardRevision!.Id);
        Assert.Equal(2, store.WrittenStandardRevision!.CorrectionSequence);
    }

    [Fact]
    public async Task StandardStoreConflictReturnsReadableConflict()
    {
        StandardStaffingDemandRevision current = FindInitialStandard(
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        FakeStaffingDemandStore store = new()
        {
            StandardResult = StaffingDemandWriteStoreResult.Conflict,
        };
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);

        StaffingDemandCommandResult result = await ExecuteAsync(
            command,
            CreateStandardRequest(
                StandardStaffingDemandRevisionKind.Replace,
                WeekMonday,
                current.Id.Value));

        Assert.Equal(StaffingDemandCommandStatus.Conflict, result.Status);
        Assert.Equal(
            "Der Bedarf wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.",
            Assert.Single(result.Errors).Message);
    }

    [Theory]
    [InlineData(StaffingDemandDateExceptionKind.Add)]
    [InlineData(StaffingDemandDateExceptionKind.Replace)]
    [InlineData(StaffingDemandDateExceptionKind.Remove)]
    public async Task EveryDateExceptionKindWritesAtomically(
        StaffingDemandDateExceptionKind kind)
    {
        FakeStaffingDemandStore store = new();
        SaveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        SaveStaffingDemandDateExceptionRequest request = CreateDateExceptionRequest(kind);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, store.DateExceptionSaveCallCount);
        Assert.Null(store.ExpectedDateException);
        Assert.Equal(kind, store.WrittenDateException!.Kind);
        if (kind == StaffingDemandDateExceptionKind.Remove)
        {
            Assert.Null(store.WrittenDateException.ActualTime);
            Assert.Null(store.WrittenDateException.RequiredEmployeeCount);
        }
    }

    [Fact]
    public async Task ExistingDateExceptionRequiresMatchingExpectedIdentifier()
    {
        StaffingDemandDateException current = CreateDateReplacement();
        FakeStaffingDemandStore store = new();
        SaveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(exceptions: [current])),
            store);
        SaveStaffingDemandDateExceptionRequest request = CreateDateExceptionRequest(
            StaffingDemandDateExceptionKind.Replace) with
        {
            ExpectedCurrentExceptionId = current.Id.Value,
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        Assert.Same(current, store.ExpectedDateException);
        Assert.NotEqual(current.Id.Value, store.WrittenDateException!.Id.Value);
    }

    [Fact]
    public async Task DateExceptionWithUnknownShiftTypeIsRejectedWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        SaveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        SaveStaffingDemandDateExceptionRequest request = CreateDateExceptionRequest(
            StaffingDemandDateExceptionKind.Add) with
        {
            ShiftTypeId = Guid.NewGuid(),
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.NotFound, result.Status);
        Assert.Contains(
            result.Errors,
            error => error.Code == StaffingDemandCommandErrorCode.ShiftTypeNotFound);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task DateAdditionOverExistingStandardIsRejectedWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        SaveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        SaveStaffingDemandDateExceptionRequest request = CreateDateExceptionRequest(
            StaffingDemandDateExceptionKind.Add) with
        {
            ShiftTypeId = InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value,
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.ValidationFailed, result.Status);
        Assert.Equal(
            StaffingDemandCommandErrorCode.StandardAlreadyExists,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task DateReplacementWithoutStandardReturnsNotFoundWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        SaveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(standards: [])),
            store);
        SaveStaffingDemandDateExceptionRequest request = CreateDateExceptionRequest(
            StaffingDemandDateExceptionKind.Replace);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.NotFound, result.Status);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task DateExceptionStoreConflictReturnsReadableConflict()
    {
        FakeStaffingDemandStore store = new()
        {
            DateExceptionResult = StaffingDemandWriteStoreResult.Conflict,
        };
        SaveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);

        StaffingDemandCommandResult result = await ExecuteAsync(
            command,
            CreateDateExceptionRequest(StaffingDemandDateExceptionKind.Replace));

        Assert.Equal(StaffingDemandCommandStatus.Conflict, result.Status);
        Assert.Equal(1, store.DateExceptionSaveCallCount);
    }

    [Fact]
    public async Task RemovingExistingExceptionUsesExpectedSnapshot()
    {
        StaffingDemandDateException current = CreateDateReplacement();
        FakeStaffingDemandStore store = new();
        RemoveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(exceptions: [current])),
            store);
        RemoveStaffingDemandDateExceptionRequest request = CreateRemoveRequest(current);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, store.DateExceptionRemoveCallCount);
        Assert.Same(current, store.RemovedDateException);
    }

    [Fact]
    public async Task RemovingAlreadyMissingExceptionIsIdempotentWithoutWriting()
    {
        FakeStaffingDemandStore store = new();
        RemoveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);
        RemoveStaffingDemandDateExceptionRequest request = new(
            WeekMonday,
            InitialWorkLocationCatalog.Restaurant.Id.Value,
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            Guid.NewGuid());

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task RemoveStoreNotFoundIsIdempotentSuccess()
    {
        StaffingDemandDateException current = CreateDateReplacement();
        FakeStaffingDemandStore store = new()
        {
            RemoveResult = StaffingDemandWriteStoreResult.NotFound,
        };
        RemoveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(exceptions: [current])),
            store);

        StaffingDemandCommandResult result = await ExecuteAsync(
            command,
            CreateRemoveRequest(current));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, store.DateExceptionRemoveCallCount);
    }

    [Fact]
    public async Task RemovingDifferentCurrentExceptionReturnsConflictWithoutWriting()
    {
        StaffingDemandDateException current = CreateDateReplacement();
        FakeStaffingDemandStore store = new();
        RemoveStaffingDemandDateExceptionCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(exceptions: [current])),
            store);
        RemoveStaffingDemandDateExceptionRequest request = CreateRemoveRequest(current) with
        {
            ExpectedExceptionId = Guid.NewGuid(),
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.Conflict, result.Status);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Fact]
    public async Task InvalidRemoveIdentifierIsRejectedWithoutReadingOrWriting()
    {
        FakeStaffingDemandReader reader = new(CreateReadData());
        FakeStaffingDemandStore store = new();
        RemoveStaffingDemandDateExceptionCommand command = new(reader, store);
        RemoveStaffingDemandDateExceptionRequest request = new(
            WeekMonday,
            InitialWorkLocationCatalog.Restaurant.Id.Value,
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            Guid.Empty);

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.ValidationFailed, result.Status);
        Assert.Equal(
            StaffingDemandCommandErrorCode.ExpectedIdentifierRequired,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    [Theory]
    [InlineData(WriteCommandKind.Standard)]
    [InlineData(WriteCommandKind.SaveDateException)]
    [InlineData(WriteCommandKind.RemoveDateException)]
    public async Task CancellationBeforeReadDoesNotReadOrWrite(WriteCommandKind commandKind)
    {
        FakeStaffingDemandReader reader = new(CreateReadData());
        FakeStaffingDemandStore store = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<StaffingDemandCommandResult> execution = commandKind switch
        {
            WriteCommandKind.Standard => new ChangeStandardStaffingDemandCommand(
                reader,
                store).ExecuteAsync(
                    CreateStandardRequest(
                        StandardStaffingDemandRevisionKind.Replace,
                        WeekMonday,
                        FindInitialStandard(
                            DayOfWeek.Monday,
                            InitialShiftTypeCatalog.EarlyShift.Id.Value).Id.Value),
                    cancellation.Token),
            WriteCommandKind.SaveDateException =>
                new SaveStaffingDemandDateExceptionCommand(reader, store).ExecuteAsync(
                    CreateDateExceptionRequest(
                        StaffingDemandDateExceptionKind.Replace),
                    cancellation.Token),
            WriteCommandKind.RemoveDateException =>
                new RemoveStaffingDemandDateExceptionCommand(reader, store).ExecuteAsync(
                    new RemoveStaffingDemandDateExceptionRequest(
                        WeekMonday,
                        InitialWorkLocationCatalog.Restaurant.Id.Value,
                        InitialShiftTypeCatalog.EarlyShift.Id.Value,
                        Guid.NewGuid()),
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
    public async Task CancellationDuringStoreDoesNotReportSuccess()
    {
        using CancellationTokenSource cancellation = new();
        StandardStaffingDemandRevision current = FindInitialStandard(
            DayOfWeek.Monday,
            InitialShiftTypeCatalog.EarlyShift.Id.Value);
        FakeStaffingDemandStore store = new()
        {
            CancellationToRequestDuringWrite = cancellation,
        };
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData()),
            store);

        Task<StaffingDemandCommandResult> execution = command.ExecuteAsync(
            CreateStandardRequest(
                StandardStaffingDemandRevisionKind.Replace,
                WeekMonday,
                current.Id.Value),
            cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(1, store.StandardAppendCallCount);
    }

    [Fact]
    public async Task ContradictoryStoredDataIsRejectedWithoutWriting()
    {
        StandardStaffingDemandRevision duplicate = InitialStaffingDemandCatalog.All[0];
        FakeStaffingDemandStore store = new();
        ChangeStandardStaffingDemandCommand command = new(
            new FakeStaffingDemandReader(CreateReadData(
                standards: [duplicate, duplicate])),
            store);
        ChangeStandardStaffingDemandRequest request = CreateStandardRequest(
            StandardStaffingDemandRevisionKind.Add,
            WeekMonday,
            null) with
        {
            ShiftTypeId = InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value,
            WorkLocationId = InitialWorkLocationCatalog.Cafeteria.Id.Value,
        };

        StaffingDemandCommandResult result = await ExecuteAsync(command, request);

        Assert.Equal(StaffingDemandCommandStatus.StoredDataInvalid, result.Status);
        Assert.Equal(0, store.TotalWriteCallCount);
    }

    private static Task<StaffingDemandCommandResult> ExecuteAsync(
        ChangeStandardStaffingDemandCommand command,
        ChangeStandardStaffingDemandRequest request)
    {
        return command.ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    private static Task<StaffingDemandCommandResult> ExecuteAsync(
        SaveStaffingDemandDateExceptionCommand command,
        SaveStaffingDemandDateExceptionRequest request)
    {
        return command.ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    private static Task<StaffingDemandCommandResult> ExecuteAsync(
        RemoveStaffingDemandDateExceptionCommand command,
        RemoveStaffingDemandDateExceptionRequest request)
    {
        return command.ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    private static ChangeStandardStaffingDemandRequest CreateStandardRequest(
        StandardStaffingDemandRevisionKind kind,
        DateOnly effectiveFromMonday,
        Guid? expectedCurrentRevisionId)
    {
        return new ChangeStandardStaffingDemandRequest(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            InitialWorkLocationCatalog.Restaurant.Id.Value,
            InitialShiftTypeCatalog.EarlyShift.Id.Value,
            effectiveFromMonday,
            kind,
            new TimeOnly(7, 0),
            new TimeOnly(13, 0),
            3,
            expectedCurrentRevisionId);
    }

    private static SaveStaffingDemandDateExceptionRequest CreateDateExceptionRequest(
        StaffingDemandDateExceptionKind kind)
    {
        bool isAddition = kind == StaffingDemandDateExceptionKind.Add;
        return new SaveStaffingDemandDateExceptionRequest(
            Guid.NewGuid(),
            WeekMonday,
            isAddition
                ? InitialWorkLocationCatalog.Cafeteria.Id.Value
                : InitialWorkLocationCatalog.Restaurant.Id.Value,
            isAddition
                ? InitialShiftTypeCatalog.CafeteriaShiftB.Id.Value
                : InitialShiftTypeCatalog.EarlyShift.Id.Value,
            kind,
            kind == StaffingDemandDateExceptionKind.Remove
                ? null
                : new TimeOnly(7, 0),
            kind == StaffingDemandDateExceptionKind.Remove
                ? null
                : new TimeOnly(12, 30),
            kind == StaffingDemandDateExceptionKind.Remove ? null : 3,
            null);
    }

    private static RemoveStaffingDemandDateExceptionRequest CreateRemoveRequest(
        StaffingDemandDateException dateException)
    {
        return new RemoveStaffingDemandDateExceptionRequest(
            dateException.Key.Date,
            dateException.Key.WorkLocationId.Value,
            dateException.Key.ShiftTypeId.Value,
            dateException.Id.Value);
    }

    private static StaffingDemandDateException CreateDateReplacement()
    {
        return Assert.IsType<StaffingDemandDateException>(
            StaffingDemandDateException.CreateReplacement(
                Guid.NewGuid(),
                WeekMonday,
                InitialWorkLocationCatalog.Restaurant.Id.Value,
                InitialShiftTypeCatalog.EarlyShift.Id.Value,
                new TimeOnly(7, 0),
                new TimeOnly(12, 30),
                3).Value);
    }

    private static StandardStaffingDemandRevision FindInitialStandard(
        DayOfWeek dayOfWeek,
        Guid shiftTypeId)
    {
        return Assert.Single(
            InitialStaffingDemandCatalog.All,
            revision => revision.Key.DayOfWeek == dayOfWeek
                && revision.Key.ShiftTypeId.Value == shiftTypeId);
    }

    private static StaffingDemandReadData CreateReadData(
        IEnumerable<StandardStaffingDemandRevision>? standards = null,
        IEnumerable<StaffingDemandDateException>? exceptions = null)
    {
        ServiceCatalogData catalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);
        return new StaffingDemandReadData(
            standards ?? InitialStaffingDemandCatalog.All,
            exceptions ?? [],
            catalog);
    }

    public enum WriteCommandKind
    {
        Standard,
        SaveDateException,
        RemoveDateException,
    }

    private sealed class FakeStaffingDemandReader : IStaffingDemandReader
    {
        private readonly StaffingDemandReadData _data;

        public FakeStaffingDemandReader(StaffingDemandReadData data)
        {
            _data = data;
        }

        public int LoadCallCount { get; private set; }

        public Task<StaffingDemandReadData> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCallCount++;
            return Task.FromResult(_data);
        }
    }

    private sealed class FakeStaffingDemandStore :
        IStandardStaffingDemandRevisionStore,
        IStaffingDemandDateExceptionStore,
        IRemoveStaffingDemandDateExceptionStore
    {
        public StaffingDemandWriteStoreResult StandardResult { get; init; } =
            StaffingDemandWriteStoreResult.Succeeded;

        public StaffingDemandWriteStoreResult DateExceptionResult { get; init; } =
            StaffingDemandWriteStoreResult.Succeeded;

        public StaffingDemandWriteStoreResult RemoveResult { get; init; } =
            StaffingDemandWriteStoreResult.Succeeded;

        public CancellationTokenSource? CancellationToRequestDuringWrite { get; init; }

        public int StandardAppendCallCount { get; private set; }

        public int DateExceptionSaveCallCount { get; private set; }

        public int DateExceptionRemoveCallCount { get; private set; }

        public int TotalWriteCallCount =>
            StandardAppendCallCount
            + DateExceptionSaveCallCount
            + DateExceptionRemoveCallCount;

        public StandardStaffingDemandRevision? ExpectedStandardRevision { get; private set; }

        public StandardStaffingDemandRevision? WrittenStandardRevision { get; private set; }

        public StaffingDemandDateException? ExpectedDateException { get; private set; }

        public StaffingDemandDateException? WrittenDateException { get; private set; }

        public StaffingDemandDateException? RemovedDateException { get; private set; }

        public Task<StaffingDemandWriteStoreResult> AppendAsync(
            StandardStaffingDemandRevision? expectedCurrent,
            StandardStaffingDemandRevision revision,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StandardAppendCallCount++;
            ExpectedStandardRevision = expectedCurrent;
            WrittenStandardRevision = revision;
            CancellationToRequestDuringWrite?.Cancel();
            return Task.FromResult(StandardResult);
        }

        public Task<StaffingDemandWriteStoreResult> SaveAsync(
            StaffingDemandDateException? expectedCurrent,
            StaffingDemandDateException replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateExceptionSaveCallCount++;
            ExpectedDateException = expectedCurrent;
            WrittenDateException = replacement;
            CancellationToRequestDuringWrite?.Cancel();
            return Task.FromResult(DateExceptionResult);
        }

        public Task<StaffingDemandWriteStoreResult> RemoveAsync(
            StaffingDemandDateException expected,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateExceptionRemoveCallCount++;
            RemovedDateException = expected;
            CancellationToRequestDuringWrite?.Cancel();
            return Task.FromResult(RemoveResult);
        }
    }
}
