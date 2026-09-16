using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.Scheduling;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Scheduling;

internal sealed class FakeScheduleDataAccess :
    IScheduleWorkspaceReader,
    IOpenScheduleDraftStore,
    IChangeScheduleDayStore,
    IPlanningInputReader,
    IPreparePlanningSnapshotStore
{
    public static readonly Guid ServiceManagementEmployeeId =
        new("24a1c76c-3d37-4af5-adc2-f90c51d70c85");

    public static readonly Guid NormalEmployeeId =
        new("cb1d6504-744d-4461-a0e5-f2c130d40342");

    public static readonly Guid AuxiliaryEmployeeId =
        new("0c60f2a1-f0a7-4bf0-94d1-05e18e35390b");

    private readonly Dictionary<(Guid EmployeeId, DateOnly Date), StoredEntry> _entries = [];
    private readonly List<ScheduleDraft> _drafts = [];
    private readonly Employee[] _employees =
    [
        CreateEmployee(
            ServiceManagementEmployeeId,
            "Sarah",
            "Leitung",
            InitialEmployeeTypeCatalog.Type1),
        CreateEmployee(
            NormalEmployeeId,
            "Erika",
            "Muster",
            InitialEmployeeTypeCatalog.Type25),
        CreateEmployee(
            AuxiliaryEmployeeId,
            "Alex",
            "Beispiel",
            InitialEmployeeTypeCatalog.TypeAh1),
    ];

    public int LoadCallCount { get; private set; }

    public int ChangeCallCount { get; private set; }

    public int PrepareCallCount { get; private set; }

    public Exception? LoadException { get; set; }

    public bool RejectNextChange { get; set; }

    public List<SchedulePeriod> RequestedPeriods { get; } = [];

    public void Add(
        Guid employeeId,
        DateOnly date,
        AvailabilityEntryKind kind,
        long changeVersion = 1)
    {
        AvailabilityEntry entry = Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(employeeId, date, kind).Value);
        _entries[(employeeId, date)] = new StoredEntry(entry, changeVersion);
    }

    public Task<ScheduleWorkspaceReadData> LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LoadCallCount++;
        RequestedPeriods.Add(period);
        if (LoadException is not null)
        {
            throw LoadException;
        }

        return Task.FromResult(CreateReadData(period));
    }

    async Task<PlanningInputReadData> IPlanningInputReader.LoadAsync(
        SchedulePeriod period,
        CancellationToken cancellationToken)
    {
        ScheduleWorkspaceReadData workspace = await LoadAsync(period, cancellationToken);
        return new PlanningInputReadData(
            workspace,
            workspace.HistoryDays,
            workspace.PreparedSnapshot);
    }

    public Task<OpenScheduleDraftStoreResult> CreateAsync(
        ScheduleDraft draft,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_drafts.Any(item => item.Period.Overlaps(draft.Period)))
        {
            SchedulePeriod overlap = _drafts.First(item =>
                item.Period.Overlaps(draft.Period)).Period;
            return Task.FromResult(OpenScheduleDraftStoreResult.Overlap(overlap));
        }

        _drafts.Add(draft);
        return Task.FromResult(OpenScheduleDraftStoreResult.Success(draft));
    }

    public Task<ScheduleDayChangeStoreResult> ChangeAsync(
        ScheduleDayChange change,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ChangeCallCount++;
        if (RejectNextChange)
        {
            RejectNextChange = false;
            return Task.FromResult(ScheduleDayChangeStoreResult.Conflict());
        }

        int index = _drafts.FindIndex(item => item.Id == change.UpdatedDraft.Id);
        if (index < 0 || _drafts[index].Version.Value != change.ExpectedDraftVersion)
        {
            return Task.FromResult(ScheduleDayChangeStoreResult.Conflict());
        }

        ApplyAvailabilityMutation(change.AvailabilityMutation);
        _drafts[index] = change.UpdatedDraft;
        return Task.FromResult(ScheduleDayChangeStoreResult.Success(change.UpdatedDraft));
    }

    public Task<PreparePlanningSnapshotStoreResult> SaveAsync(
        PreparePlanningSnapshotChange change,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        PrepareCallCount++;
        int index = _drafts.FindIndex(item => item.Id.Value == change.Snapshot.DraftId);
        if (index < 0
            || _drafts[index].Version.Value != change.ExpectedDraftVersion
            || PreparedSnapshot?.Id != change.ExpectedPreviousSnapshotId)
        {
            return Task.FromResult(PreparePlanningSnapshotStoreResult.Conflict());
        }

        PreparedSnapshot = change.Snapshot;
        return Task.FromResult(
            PreparePlanningSnapshotStoreResult.Success(change.Snapshot));
    }

    public PlanningInputSnapshot? PreparedSnapshot { get; private set; }

    private ScheduleWorkspaceReadData CreateReadData(SchedulePeriod period)
    {
        ScheduleDraft? exact = _drafts.SingleOrDefault(item => item.Period == period);
        return new ScheduleWorkspaceReadData(
            new AvailabilityReadData(
                _employees,
                [
                    InitialEmployeeTypeCatalog.Type1,
                    InitialEmployeeTypeCatalog.Type25,
                    InitialEmployeeTypeCatalog.TypeAh1,
                ],
                _entries.Values
                    .Where(item => period.Contains(item.Entry.Date))
                    .Select(item => new AvailabilityEntryReadItem(
                        item.Entry,
                        item.ChangeVersion))),
            new StaffingDemandReadData(
                InitialStaffingDemandCatalog.All,
                [],
                new ServiceCatalogData(
                    InitialWorkLocationCatalog.All,
                    InitialShiftTypeCatalog.All,
                    InitialShiftPatternCatalog.SplitShift,
                    InitialShiftPatternCatalog.ReliefShift)),
            _drafts.Select(item => new ScheduleDraftHeader(
                item.Id,
                item.Version,
                item.Period)),
            exact,
            [],
            exact?.Id.Value == PreparedSnapshot?.DraftId ? PreparedSnapshot : null);
    }

    private void ApplyAvailabilityMutation(ScheduleAvailabilityMutation mutation)
    {
        if (mutation.Kind == ScheduleAvailabilityMutationKind.None)
        {
            return;
        }

        AvailabilityEntry keyEntry = mutation.ExpectedCurrent?.Entry
            ?? mutation.Replacement
            ?? throw new InvalidOperationException("Availability mutation has no key.");
        (Guid EmployeeId, DateOnly Date) key =
            (keyEntry.EmployeeId.Value, keyEntry.Date);
        if (mutation.Kind == ScheduleAvailabilityMutationKind.Remove)
        {
            _entries.Remove(key);
            return;
        }

        AvailabilityEntry replacement = mutation.Replacement
            ?? throw new InvalidOperationException("Upsert mutation has no replacement.");
        long version = mutation.ExpectedCurrent is null
            ? 1
            : mutation.ExpectedCurrent.ChangeVersion + 1;
        _entries[key] = new StoredEntry(replacement, version);
    }

    private static Employee CreateEmployee(
        Guid id,
        string firstName,
        string lastName,
        EmployeeType employeeType)
    {
        return Assert.IsType<Employee>(
            Employee.Create(id, firstName, lastName, employeeType.Id.Value).Value);
    }

    private sealed record StoredEntry(AvailabilityEntry Entry, long ChangeVersion);
}

internal sealed class RecordingUnexpectedErrorReporter : IUnexpectedErrorReporter
{
    public Exception? Exception { get; private set; }

    public string? Operation { get; private set; }

    public void Report(Exception exception, string operation)
    {
        Exception = exception;
        Operation = operation;
    }
}
