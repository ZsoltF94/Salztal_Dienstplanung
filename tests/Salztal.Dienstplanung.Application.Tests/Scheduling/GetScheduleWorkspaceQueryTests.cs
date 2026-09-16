using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class GetScheduleWorkspaceQueryTests
{
    [Fact]
    public async Task ExecuteReturnsCoherentTwentyOneDayWorkspace()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        FakeScheduleWorkspaceReader reader = new(CreateReadDataForDraft(draft));

        ScheduleWorkspaceQueryResult result = await Execute(
            reader,
            ScheduleWorkspaceTestContext.PeriodMonday.AddDays(5));

        ScheduleWorkspaceSnapshot snapshot = Assert.IsType<ScheduleWorkspaceSnapshot>(
            result.Value);
        Assert.Equal(ScheduleWorkspaceQueryStatus.Succeeded, result.Status);
        Assert.Equal(draft.Id.Value, snapshot.DraftId);
        Assert.Equal(ScheduleWorkspaceTestContext.PeriodMonday, snapshot.PeriodMonday);
        Assert.Equal(ScheduleWorkspaceTestContext.PeriodMonday.AddDays(20), snapshot.PeriodSunday);
        Assert.Equal(21, snapshot.Availability.Days.Count);
        Assert.Equal(2, snapshot.Availability.Employees.Count);
        Assert.DoesNotContain(
            snapshot.Availability.Employees,
            employee => employee.EmployeeId == ScheduleWorkspaceTestContext.InactiveEmployeeId);
        Assert.All(
            snapshot.Availability.Employees,
            employee => Assert.Equal(3, employee.Weeks.Count));
        Assert.Equal(195, snapshot.DemandSlots.Count);
        Assert.Empty(snapshot.Assignments);
        ServiceManagementReadinessSnapshot readiness = Assert.Single(
            snapshot.ServiceManagementReadiness);
        Assert.False(readiness.CanPrepare);
        Assert.All(
            readiness.Weeks,
            week => Assert.Equal(
                ServiceManagementWeekReadinessStatusSnapshot.MissingAssignment,
                week.Status));
        Assert.True(
            ((ICollection<ServiceManagementReadinessSnapshot>)
                snapshot.ServiceManagementReadiness).IsReadOnly);
        Assert.Equal(SchedulePreparationStatus.NotPrepared, snapshot.PreparationStatus);
        Assert.Equal([ScheduleWorkspaceTestContext.Period], reader.RequestedPeriods);
    }

    [Fact]
    public async Task ExecuteProjectsDemandIdentityNamesTimesAndOrdinals()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();

        ScheduleWorkspaceSnapshot snapshot = await ExecuteSuccessfully(
            new FakeScheduleWorkspaceReader(CreateReadDataForDraft(draft)));

        ScheduleDemandSlotSnapshot early = Assert.Single(
            snapshot.DemandSlots,
            slot =>
                slot.Date == ScheduleWorkspaceTestContext.PeriodMonday
                && slot.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id.Value
                && slot.Ordinal == 1);
        Assert.Equal(ScheduleDemandSourceKindSnapshot.Standard, early.SourceKind);
        Assert.Equal("Restaurant", early.WorkLocationName);
        Assert.Equal("Frühdienst", early.ShiftTypeName);
        Assert.Equal(InitialShiftTypeCatalog.EarlyShift.StandardTime.Start, early.ActualStart);
        Assert.Equal(InitialShiftTypeCatalog.EarlyShift.StandardTime.End, early.ActualEnd);
        Assert.Equal(
            InitialShiftTypeCatalog.EarlyShift.StandardTime.DurationMinutes,
            early.DurationMinutes);
        Assert.Equal(
            [1, 2, 3, 4],
            snapshot.DemandSlots
                .Where(slot =>
                    slot.Date == ScheduleWorkspaceTestContext.PeriodMonday
                    && slot.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id.Value)
                .Select(slot => slot.Ordinal));
    }

    [Fact]
    public async Task ExecuteProjectsOnlyStructurallyAllowedTyp1Options()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();

        ScheduleWorkspaceSnapshot snapshot = await ExecuteSuccessfully(
            new FakeScheduleWorkspaceReader(CreateReadDataForDraft(draft)));

        Assert.NotEmpty(snapshot.AssignmentOptions);
        Assert.All(snapshot.AssignmentOptions, option => Assert.Equal(
            ScheduleWorkspaceTestContext.ServiceManagementEmployeeId,
            option.EmployeeId));
        Assert.Contains(snapshot.AssignmentOptions, option =>
            option.Kind == ServiceManagementAssignmentSelectionKind.NormalDemand
            && option.FirstSlot.Date == ScheduleWorkspaceTestContext.PeriodMonday
            && option.FirstSlot.ShiftTypeId
                == InitialShiftTypeCatalog.EarlyShift.Id.Value);
        Assert.Contains(snapshot.AssignmentOptions, option =>
            option.Kind == ServiceManagementAssignmentSelectionKind.OfficeTime
            && option.FirstSlot.ShiftTypeId
                == InitialShiftTypeCatalog.LateShift.Id.Value);
        Assert.DoesNotContain(snapshot.AssignmentOptions, option =>
            option.Kind == ServiceManagementAssignmentSelectionKind.OfficeTime
            && option.FirstSlot.ShiftTypeId
                == InitialShiftTypeCatalog.CafeteriaShiftA.Id.Value);
        Assert.Contains(snapshot.AssignmentOptions, option =>
            option.Kind == ServiceManagementAssignmentSelectionKind.SplitShiftPattern
            && option.SecondSlot is not null);
        Assert.Contains(snapshot.AssignmentOptions, option =>
            option.Kind == ServiceManagementAssignmentSelectionKind.ReliefShiftPattern
            && option.FirstSlot.Date.DayOfWeek == DayOfWeek.Saturday
            && option.SecondSlot is not null);
    }

    [Fact]
    public async Task ExecuteProjectsTypedAssignmentWithoutUiTextInference()
    {
        DemandSlotSet slots = ScheduleWorkspaceTestContext.CreateDemandSlots(
            ScheduleWorkspaceTestContext.Period);
        DemandSlot slot = slots.Slots.First(item =>
            item.Id.Date == ScheduleWorkspaceTestContext.PeriodMonday
            && item.Id.ShiftTypeId == InitialShiftTypeCatalog.EarlyShift.Id
            && item.Id.Ordinal == 1);
        ScheduleAssignment assignment = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                new Guid("9f054f1a-33a0-44d6-b5b2-b3231dad882f"),
                ScheduleWorkspaceTestContext.ServiceManagementEmployee.Id,
                slot,
                AssignmentOrigin.ServiceManagement).Value);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
            assignments: [assignment]);

        ScheduleWorkspaceSnapshot snapshot = await ExecuteSuccessfully(
            new FakeScheduleWorkspaceReader(CreateReadDataForDraft(draft)));

        ScheduleAssignmentSnapshot projected = Assert.Single(snapshot.Assignments);
        Assert.Equal(ScheduleAssignmentKindSnapshot.NormalDemand, projected.Kind);
        Assert.Equal(ScheduleAssignmentOriginSnapshot.ServiceManagement, projected.Origin);
        Assert.True(projected.IsProtectedFromAutomaticGeneration);
        Assert.Equal(slot.DurationMinutes, projected.WorkMinutes);
        Assert.Single(projected.Segments);
        ScheduleDemandCoverageSnapshot coverage = Assert.Single(projected.Coverages);
        Assert.Equal(ScheduleDemandCoverageKindSnapshot.Full, coverage.Kind);
        Assert.Equal(slot.DurationMinutes, coverage.CoveredMinutes);
        ServiceManagementReadinessSnapshot readiness = Assert.Single(
            snapshot.ServiceManagementReadiness);
        Assert.Collection(
            readiness.Weeks,
            firstWeek =>
            {
                Assert.Equal(ServiceManagementWeekReadinessStatusSnapshot.Ready, firstWeek.Status);
                Assert.Equal(slot.DurationMinutes, firstWeek.WorkMinutes);
            },
            secondWeek => Assert.Equal(
                ServiceManagementWeekReadinessStatusSnapshot.MissingAssignment,
                secondWeek.Status),
            thirdWeek => Assert.Equal(
                ServiceManagementWeekReadinessStatusSnapshot.MissingAssignment,
                thirdWeek.Status));
    }

    [Fact]
    public async Task ExecuteWhenDraftDoesNotExistReturnsNotFound()
    {
        ScheduleWorkspaceQueryResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData()));

        Assert.Equal(ScheduleWorkspaceQueryStatus.NotFound, result.Status);
        Assert.Equal(ScheduleWorkspaceErrorCode.DraftNotFound, Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenHeaderAndDraftDifferReturnsStoredDataError()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(version: 2);
        ScheduleDraftHeader mismatchedHeader = new(
            draft.Id,
            ScheduleWorkspaceTestContext.CreateDraft(version: 1).Version,
            draft.Period);

        ScheduleWorkspaceQueryResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(
                    draftHeaders: [mismatchedHeader],
                    exactDraft: draft)));

        Assert.Equal(ScheduleWorkspaceQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            ScheduleWorkspaceErrorCode.StoredDataInvalid,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenEmployeeTypeIsMissingReturnsCatalogError()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();

        ScheduleWorkspaceQueryResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(
                    employeeTypes: [],
                    draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(draft)],
                    exactDraft: draft)));

        Assert.Equal(ScheduleWorkspaceQueryStatus.CatalogInvalid, result.Status);
        Assert.Equal(ScheduleWorkspaceErrorCode.CatalogInvalid, Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenAvailabilityIsDuplicatedReturnsStoredDataError()
    {
        AvailabilityEntryReadItem entry = ScheduleWorkspaceTestContext.CreateEntry(
            ScheduleWorkspaceTestContext.StandardEmployeeId,
            ScheduleWorkspaceTestContext.PeriodMonday,
            AvailabilityEntryKind.FixedDayOff);
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft(
            availabilityEntries: [entry.Entry]);

        ScheduleWorkspaceQueryResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(
                    entries: [entry, entry],
                    draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(draft)],
                    exactDraft: draft)));

        Assert.Equal(ScheduleWorkspaceQueryStatus.StoredDataInvalid, result.Status);
        Assert.Equal(
            ScheduleWorkspaceErrorCode.StoredDataInvalid,
            Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenServiceCatalogIdentifierIsDuplicatedReturnsCatalogError()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        ServiceCatalogData duplicateCatalog = new(
            [.. ScheduleWorkspaceTestContext.ServiceCatalog.WorkLocations,
                ScheduleWorkspaceTestContext.ServiceCatalog.WorkLocations[0]],
            ScheduleWorkspaceTestContext.ServiceCatalog.ShiftTypes,
            ScheduleWorkspaceTestContext.ServiceCatalog.SplitShiftPattern,
            ScheduleWorkspaceTestContext.ServiceCatalog.ReliefShiftPattern);

        ScheduleWorkspaceQueryResult result = await Execute(
            new FakeScheduleWorkspaceReader(
                ScheduleWorkspaceTestContext.CreateReadData(
                    serviceCatalog: duplicateCatalog,
                    draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(draft)],
                    exactDraft: draft)));

        Assert.Equal(ScheduleWorkspaceQueryStatus.CatalogInvalid, result.Status);
        Assert.Equal(ScheduleWorkspaceErrorCode.CatalogInvalid, Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task ExecuteWhenAlreadyCancelledDoesNotRead()
    {
        ScheduleDraft draft = ScheduleWorkspaceTestContext.CreateDraft();
        FakeScheduleWorkspaceReader reader = new(CreateReadDataForDraft(draft));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Task<ScheduleWorkspaceQueryResult> execution =
            new GetScheduleWorkspaceQuery(reader).ExecuteAsync(
                ScheduleWorkspaceTestContext.PeriodMonday,
                cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Equal(0, reader.LoadCallCount);
    }

    private static ScheduleWorkspaceReadData CreateReadDataForDraft(ScheduleDraft draft)
    {
        return ScheduleWorkspaceTestContext.CreateReadData(
            draftHeaders: [ScheduleWorkspaceTestContext.CreateHeader(draft)],
            exactDraft: draft);
    }

    private static async Task<ScheduleWorkspaceSnapshot> ExecuteSuccessfully(
        FakeScheduleWorkspaceReader reader)
    {
        ScheduleWorkspaceQueryResult result = await Execute(reader);
        Assert.Equal(ScheduleWorkspaceQueryStatus.Succeeded, result.Status);
        return Assert.IsType<ScheduleWorkspaceSnapshot>(result.Value);
    }

    private static Task<ScheduleWorkspaceQueryResult> Execute(
        FakeScheduleWorkspaceReader reader,
        DateOnly? selectedDate = null)
    {
        return new GetScheduleWorkspaceQuery(reader).ExecuteAsync(
            selectedDate ?? ScheduleWorkspaceTestContext.PeriodMonday,
            TestContext.Current.CancellationToken);
    }
}
