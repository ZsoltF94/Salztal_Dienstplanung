using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class ScheduleDayChangeCommandsTests
{
    [Fact]
    public async Task SetNormalAssignmentUsesActualDemandSlotAndProtectsTyp1()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot);

        ScheduleAssignment assignment = Assert.Single(
            Assert.IsType<ScheduleDraft>(store.Change?.UpdatedDraft).Assignments);
        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Equal(AssignmentOrigin.ServiceManagement, assignment.Origin);
        Assert.True(assignment.IsProtectedFromAutomaticGeneration);
        Assert.Equal(slot.ActualTime, Assert.Single(assignment.Segments).ActualTime);
        Assert.Equal(slot.ActualTime, Assert.Single(assignment.Coverages).CoveredTime);
        Assert.Equal(2, store.Change?.UpdatedDraft.Version.Value);
        Assert.Equal(1, store.Change?.ExpectedDraftVersion);
        Assert.Equal(
            SchedulePreparationImpact.PotentiallyOutdated,
            store.Change?.PreparationImpact);
    }

    [Fact]
    public async Task SetOfficeTimeKeepsDemandSlotUncovered()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.LateShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.OfficeTime,
            slot);

        ScheduleAssignment assignment = Assert.Single(store.Change!.UpdatedDraft.Assignments);
        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Equal(ScheduleAssignmentKind.OfficeTime, assignment.Kind);
        Assert.Equal(slot.DurationMinutes, assignment.WorkMinutes);
        Assert.Empty(assignment.Coverages);
    }

    [Fact]
    public async Task SetSplitShiftUsesTwoActualRestaurantSlots()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot early = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        DemandSlot late = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.LateShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.SplitShiftPattern,
            early,
            late);

        ScheduleAssignment assignment = Assert.Single(store.Change!.UpdatedDraft.Assignments);
        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Equal(ScheduleAssignmentKind.SplitShiftPattern, assignment.Kind);
        Assert.Equal(2, assignment.Segments.Count);
        Assert.Equal(2, assignment.Coverages.Count);
    }

    [Fact]
    public async Task SetReliefShiftCreatesOnlyConfirmedPartialSecondCoverage()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DateOnly saturday = ScheduleWorkspaceTestContext.PeriodMonday.AddDays(5);
        DemandSlot cafeteria = FindSlot(
            draft,
            saturday,
            InitialShiftTypeCatalog.CafeteriaShiftB,
            1);
        DemandSlot late = FindSlot(
            draft,
            saturday,
            InitialShiftTypeCatalog.LateShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.ReliefShiftPattern,
            cafeteria,
            late);

        ScheduleAssignment assignment = Assert.Single(store.Change!.UpdatedDraft.Assignments);
        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Equal(ScheduleAssignmentKind.ReliefShiftPattern, assignment.Kind);
        Assert.Contains(
            assignment.Coverages,
            coverage => coverage.Kind == DemandCoverageKind.PartialReliefShift);
        Assert.Equal(cafeteria.ActualTime.End, assignment.Segments[1].ActualTime.Start);
    }

    [Fact]
    public async Task RemoveServiceManagementAssignmentPreservesOtherDraftState()
    {
        ScheduleDraft draft = CreateDraftWithNormalServiceAssignment();
        ScheduleAssignment assignment = Assert.Single(draft.Assignments);
        FakeChangeScheduleDayStore store = new();
        RemoveServiceManagementAssignmentRequest request = new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            assignment.Id.Value);

        ScheduleDayChangeResult result = await new RemoveServiceManagementAssignmentCommand(
            CreateReader(draft),
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Empty(store.Change!.UpdatedDraft.Assignments);
        Assert.Same(draft.DemandSlots, store.Change.UpdatedDraft.DemandSlots);
        Assert.Equal(2, store.Change.UpdatedDraft.Version.Value);
    }

    [Theory]
    [InlineData(AvailabilityDayEntryKind.Vacation)]
    [InlineData(AvailabilityDayEntryKind.Sickness)]
    [InlineData(AvailabilityDayEntryKind.FixedDayOff)]
    public async Task ChangeDayEntryReplacesTyp1OnlyAfterConfirmation(
        AvailabilityDayEntryKind kind)
    {
        ScheduleDraft draft = CreateDraftWithNormalServiceAssignment();
        ScheduleAssignment assignment = Assert.Single(draft.Assignments);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await ChangeEntry(
            draft,
            store,
            assignment.Date,
            kind,
            ScheduleReplacementConfirmation.Confirmed);

        ScheduleDayChange change = Assert.IsType<ScheduleDayChange>(store.Change);
        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Empty(change.UpdatedDraft.Assignments);
        Assert.Equal(
            kind,
            MapKind(Assert.Single(change.UpdatedDraft.AvailabilityEntries.Entries).Kind));
        Assert.Equal(ScheduleAvailabilityMutationKind.Upsert, change.AvailabilityMutation.Kind);
    }

    [Fact]
    public async Task ChangeDayEntryWithoutConfirmationDoesNotCallStore()
    {
        ScheduleDraft draft = CreateDraftWithNormalServiceAssignment();
        ScheduleAssignment assignment = Assert.Single(draft.Assignments);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await ChangeEntry(
            draft,
            store,
            assignment.Date,
            AvailabilityDayEntryKind.Vacation,
            ScheduleReplacementConfirmation.NotConfirmed);

        Assert.Equal(ScheduleDayChangeStatus.ConfirmationRequired, result.Status);
        Assert.Equal(
            ScheduleDayChangeErrorCode.ReplacementConfirmationRequired,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.ChangeCallCount);
        Assert.Single(draft.Assignments);
        Assert.Empty(draft.AvailabilityEntries.Entries);
    }

    [Fact]
    public async Task RemoveDayEntryUsesCoordinatedDraftAndAvailabilityChange()
    {
        AvailabilityEntryReadItem item = ScheduleWorkspaceTestContext.CreateEntry(
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            AvailabilityEntryKind.Vacation,
            8);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
            availabilityEntries: [item.Entry]);
        FakeChangeScheduleDayStore store = new();
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                draft,
                entries: [item]));
        RemoveScheduleDayEntryRequest request = new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            8,
            ScheduleReplacementConfirmation.Confirmed);

        ScheduleDayChangeResult result = await new RemoveScheduleDayEntryCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Empty(store.Change!.UpdatedDraft.AvailabilityEntries.Entries);
        Assert.Equal(ScheduleAvailabilityMutationKind.Remove, store.Change.AvailabilityMutation.Kind);
        Assert.Equal(2, store.Change.UpdatedDraft.Version.Value);
    }

    [Fact]
    public async Task RemoveDayEntryWithoutConfirmationDoesNotCallStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        FakeChangeScheduleDayStore store = new();
        RemoveScheduleDayEntryRequest request = new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            1,
            ScheduleReplacementConfirmation.NotConfirmed);

        ScheduleDayChangeResult result = await new RemoveScheduleDayEntryCommand(
            CreateReader(draft),
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(ScheduleDayChangeStatus.ConfirmationRequired, result.Status);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Theory]
    [InlineData(AvailabilityEntryKind.Vacation)]
    [InlineData(AvailabilityEntryKind.Sickness)]
    [InlineData(AvailabilityEntryKind.FixedDayOff)]
    public async Task SetTyp1ReplacesDayEntryOnlyAfterConfirmation(
        AvailabilityEntryKind kind)
    {
        AvailabilityEntryReadItem item = ScheduleWorkspaceTestContext.CreateEntry(
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            kind,
            7);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
            availabilityEntries: [item.Entry]);
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            entries: [item],
            expectedEntryVersion: 7,
            confirmation: ScheduleReplacementConfirmation.Confirmed);

        Assert.Equal(ScheduleDayChangeStatus.Succeeded, result.Status);
        Assert.Empty(store.Change!.UpdatedDraft.AvailabilityEntries.Entries);
        Assert.Single(store.Change.UpdatedDraft.Assignments);
        Assert.Equal(ScheduleAvailabilityMutationKind.Remove, store.Change.AvailabilityMutation.Kind);
    }

    [Fact]
    public async Task SetTyp1WithoutDayEntryConfirmationDoesNotCallStore()
    {
        AvailabilityEntryReadItem item = ScheduleWorkspaceTestContext.CreateEntry(
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            AvailabilityEntryKind.FixedDayOff,
            4);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
            availabilityEntries: [item.Entry]);
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            entries: [item],
            expectedEntryVersion: 4);

        Assert.Equal(ScheduleDayChangeStatus.ConfirmationRequired, result.Status);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetTyp1StoreConflictDoesNotRemoveLoadedDayEntry()
    {
        AvailabilityEntryReadItem item = ScheduleWorkspaceTestContext.CreateEntry(
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            AvailabilityEntryKind.Vacation,
            6);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
            availabilityEntries: [item.Entry]);
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new(ScheduleDayChangeStoreResult.Conflict());

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            entries: [item],
            expectedEntryVersion: 6,
            confirmation: ScheduleReplacementConfirmation.Confirmed);

        Assert.Equal(ScheduleDayChangeStatus.Conflict, result.Status);
        Assert.Empty(draft.Assignments);
        Assert.Same(item.Entry, Assert.Single(draft.AvailabilityEntries.Entries));
        Assert.Equal(1, draft.Version.Value);
    }

    [Fact]
    public async Task StoreConflictDoesNotMutateLoadedDraft()
    {
        ScheduleDraft draft = CreateDraftWithNormalServiceAssignment();
        ScheduleAssignment assignment = Assert.Single(draft.Assignments);
        FakeChangeScheduleDayStore store = new(ScheduleDayChangeStoreResult.Conflict());

        ScheduleDayChangeResult result = await ChangeEntry(
            draft,
            store,
            assignment.Date,
            AvailabilityDayEntryKind.FixedDayOff,
            ScheduleReplacementConfirmation.Confirmed);

        Assert.Equal(ScheduleDayChangeStatus.Conflict, result.Status);
        Assert.Single(draft.Assignments);
        Assert.Empty(draft.AvailabilityEntries.Entries);
        Assert.Equal(1, draft.Version.Value);
    }

    [Fact]
    public async Task SetWithOutdatedDraftVersionIsRejectedBeforeStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();
        FakeScheduleWorkspaceReader reader = CreateReader(draft);
        SetServiceManagementAssignmentRequest request = CreateSetRequest(
            draft,
            slot) with
        {
            ExpectedDraftVersion = 2,
        };

        ScheduleDayChangeResult result = await new SetServiceManagementAssignmentCommand(
            reader,
            store).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(ScheduleDayChangeStatus.Conflict, result.Status);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetForNonTyp1IsRejectedBeforeStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            employeeId: ScheduleWorkspaceTestContext.StandardEmployeeId);

        Assert.Equal(ScheduleDayChangeStatus.WrongPlanningRole, result.Status);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetForInactiveTyp1IsRejectedBeforeStore()
    {
        Employee inactiveTyp1 = ScheduleWorkspaceTestContext.ServiceManagementEmployee.Deactivate();
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            employees: [inactiveTyp1, ScheduleWorkspaceTestContext.StandardEmployee]);

        Assert.Equal(ScheduleDayChangeStatus.InactiveEmployee, result.Status);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetWhenDemandSlotIsMissingIsRejectedBeforeStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        ScheduleDemandSlotSelection missing = ScheduleWorkspaceTestContext
            .CreateSelection(slot) with
        {
            SourceId = Guid.NewGuid(),
        };
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            firstSelection: missing);

        Assert.Equal(
            ScheduleDayChangeErrorCode.DemandSlotNotFound,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetWhenActualTimeChangedIsRejectedBeforeStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        ScheduleDemandSlotSelection stale = ScheduleWorkspaceTestContext
            .CreateSelection(slot) with
        {
            ActualStart = slot.ActualTime.Start.AddMinutes(15),
        };
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            firstSelection: stale);

        Assert.Equal(
            ScheduleDayChangeErrorCode.DemandSlotTimeChanged,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetWhenShiftEligibilityIsMissingIsRejectedBeforeStore()
    {
        EmployeeType restrictedType = Assert.IsType<EmployeeType>(
            EmployeeType.Create(
                new Guid("4f08874b-e655-46ee-83d7-2c6ff3b32eed"),
                "T1Test",
                "Testleitung",
                2_400,
                true,
                480,
                [],
                EmployeeTypePlanningPolicy.ServiceManagement).Value);
        Employee restrictedEmployee = Assert.IsType<Employee>(
            Employee.Create(
                ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
                "Sarah",
                "Leitung",
                restrictedType.Id.Value).Value);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.NormalDemand,
            slot,
            employees: [restrictedEmployee],
            employeeTypes: [restrictedType]);

        Assert.Equal(
            ScheduleDayChangeErrorCode.ShiftNotEligible,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetOfficeTimeForCafeteriaIsRejectedBeforeStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.CafeteriaShiftA,
            1);
        FakeChangeScheduleDayStore store = new();

        ScheduleDayChangeResult result = await Set(
            draft,
            store,
            ServiceManagementAssignmentSelectionKind.OfficeTime,
            slot);

        Assert.Equal(
            ScheduleDayChangeErrorCode.OfficeTimeNotAllowedForSlot,
            Assert.Single(result.Errors).Code);
        Assert.Equal(0, store.ChangeCallCount);
    }

    [Fact]
    public async Task SetWhenAlreadyCancelledDoesNotReadOrStore()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        FakeScheduleWorkspaceReader reader = CreateReader(draft);
        FakeChangeScheduleDayStore store = new();
        DemandSlot slot = FindSlot(
            draft,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<ScheduleDayChangeResult> execution =
            new SetServiceManagementAssignmentCommand(reader, store).ExecuteAsync(
                CreateSetRequest(draft, slot),
                cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
        Assert.Equal(0, store.ChangeCallCount);
    }

    private static async Task<ScheduleDayChangeResult> Set(
        ScheduleDraft draft,
        FakeChangeScheduleDayStore store,
        ServiceManagementAssignmentSelectionKind kind,
        DemandSlot firstSlot,
        DemandSlot? secondSlot = null,
        IEnumerable<AvailabilityEntryReadItem>? entries = null,
        long? expectedEntryVersion = null,
        ScheduleReplacementConfirmation confirmation =
            ScheduleReplacementConfirmation.NotConfirmed,
        Guid? employeeId = null,
        IEnumerable<Employee>? employees = null,
        IEnumerable<EmployeeType>? employeeTypes = null,
        ScheduleDemandSlotSelection? firstSelection = null)
    {
        FakeScheduleWorkspaceReader reader = new(
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(
                draft,
                employees,
                employeeTypes,
                entries));
        SetServiceManagementAssignmentRequest request = CreateSetRequest(
            draft,
            firstSlot,
            kind,
            secondSlot,
            expectedEntryVersion,
            confirmation,
            employeeId,
            firstSelection);
        return await new SetServiceManagementAssignmentCommand(reader, store)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    private static SetServiceManagementAssignmentRequest CreateSetRequest(
        ScheduleDraft draft,
        DemandSlot firstSlot,
        ServiceManagementAssignmentSelectionKind kind =
            ServiceManagementAssignmentSelectionKind.NormalDemand,
        DemandSlot? secondSlot = null,
        long? expectedEntryVersion = null,
        ScheduleReplacementConfirmation confirmation =
            ScheduleReplacementConfirmation.NotConfirmed,
        Guid? employeeId = null,
        ScheduleDemandSlotSelection? firstSelection = null)
    {
        return new SetServiceManagementAssignmentRequest(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            employeeId ?? ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            kind,
            firstSelection ?? ScheduleWorkspaceTestContext.CreateSelection(firstSlot),
            secondSlot is null
                ? null
                : ScheduleWorkspaceTestContext.CreateSelection(secondSlot),
            expectedEntryVersion,
            confirmation);
    }

    private static Task<ScheduleDayChangeResult> ChangeEntry(
        ScheduleDraft draft,
        FakeChangeScheduleDayStore store,
        DateOnly date,
        AvailabilityDayEntryKind kind,
        ScheduleReplacementConfirmation confirmation)
    {
        ChangeScheduleDayEntryRequest request = new(
            draft.Id.Value,
            draft.Version.Value,
            draft.Period.StartMonday,
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            date,
            kind,
            null,
            confirmation);
        return new ChangeScheduleDayEntryCommand(CreateReader(draft), store)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    private static ScheduleDraft CreateDraftWithNormalServiceAssignment()
    {
        ScheduleDraft empty = ScheduleWorkspaceTestContext.CreateDraft();
        DemandSlot slot = FindSlot(
            empty,
            ScheduleWorkspaceTestContext.PeriodMonday,
            InitialShiftTypeCatalog.EarlyShift,
            1);
        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                new Guid("36e56ec1-65b2-41ff-a0e9-21c02cb029da"),
                ScheduleWorkspaceTestContext.ServiceManagementEmployee.Id,
                slot,
                AssignmentOrigin.ServiceManagement).Value);
        return ScheduleWorkspaceTestContext.CreateDraft(assignments: [assignment]);
    }

    private static FakeScheduleWorkspaceReader CreateReader(ScheduleDraft draft)
    {
        return new FakeScheduleWorkspaceReader(
            ScheduleWorkspaceTestContext.CreateReadDataForDraft(draft));
    }

    private static DemandSlot FindSlot(
        ScheduleDraft draft,
        DateOnly date,
        ShiftType shiftType,
        int ordinal)
    {
        return Assert.Single(
            draft.DemandSlots.Slots,
            slot => slot.Id.Date == date
                && slot.Id.ShiftTypeId == shiftType.Id
                && slot.Id.Ordinal == ordinal);
    }

    private static AvailabilityDayEntryKind MapKind(AvailabilityEntryKind kind)
    {
        return kind switch
        {
            AvailabilityEntryKind.Vacation => AvailabilityDayEntryKind.Vacation,
            AvailabilityEntryKind.Sickness => AvailabilityDayEntryKind.Sickness,
            AvailabilityEntryKind.FixedDayOff => AvailabilityDayEntryKind.FixedDayOff,
            _ => throw new InvalidOperationException(),
        };
    }
}

internal sealed class FakeChangeScheduleDayStore : IChangeScheduleDayStore
{
    private readonly ScheduleDayChangeStoreResult? _configuredResult;

    public FakeChangeScheduleDayStore(
        ScheduleDayChangeStoreResult? configuredResult = null)
    {
        _configuredResult = configuredResult;
    }

    public int ChangeCallCount { get; private set; }

    public ScheduleDayChange? Change { get; private set; }

    public Task<ScheduleDayChangeStoreResult> ChangeAsync(
        ScheduleDayChange change,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ChangeCallCount++;
        Change = change;
        return Task.FromResult(
            _configuredResult ?? ScheduleDayChangeStoreResult.Success(change.UpdatedDraft));
    }
}
