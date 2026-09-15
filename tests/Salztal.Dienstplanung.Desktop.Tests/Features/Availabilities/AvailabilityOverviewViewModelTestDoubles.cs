using Salztal.Dienstplanung.Application.Availabilities;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.Availabilities;

internal sealed class FakeAvailabilityDataAccess :
    IAvailabilityReader,
    ISetAvailabilityEntryStore,
    IRemoveAvailabilityEntryStore
{
    public static readonly Guid NormalEmployeeId =
        new("cb1d6504-744d-4461-a0e5-f2c130d40342");

    public static readonly Guid AuxiliaryEmployeeId =
        new("0c60f2a1-f0a7-4bf0-94d1-05e18e35390b");

    private readonly Dictionary<(Guid EmployeeId, DateOnly Date), StoredEntry> _entries = [];
    private readonly Employee[] _employees =
    [
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

    public int SaveCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public Exception? LoadException { get; set; }

    public bool RejectNextSave { get; set; }

    public List<(DateOnly Monday, DateOnly Sunday)> RequestedPeriods { get; } = [];

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

    public Task<AvailabilityReadData> LoadAsync(
        DateOnly periodMonday,
        DateOnly periodSunday,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LoadCallCount++;
        RequestedPeriods.Add((periodMonday, periodSunday));
        if (LoadException is not null)
        {
            throw LoadException;
        }

        AvailabilityEntryReadItem[] entries = _entries.Values
            .Where(item => item.Entry.Date >= periodMonday && item.Entry.Date <= periodSunday)
            .OrderBy(item => item.Entry.EmployeeId.Value)
            .ThenBy(item => item.Entry.Date)
            .Select(item => new AvailabilityEntryReadItem(item.Entry, item.ChangeVersion))
            .ToArray();
        return Task.FromResult(new AvailabilityReadData(
            _employees,
            [InitialEmployeeTypeCatalog.Type25, InitialEmployeeTypeCatalog.TypeAh1],
            entries));
    }

    public Task<AvailabilityEntryWriteStoreResult> SaveAsync(
        AvailabilityEntry? expectedEntry,
        long? expectedChangeVersion,
        AvailabilityEntry replacement,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveCallCount++;
        if (RejectNextSave)
        {
            RejectNextSave = false;
            return Task.FromResult(AvailabilityEntryWriteStoreResult.Conflict);
        }

        (Guid, DateOnly) key = (replacement.EmployeeId.Value, replacement.Date);
        _entries.TryGetValue(key, out StoredEntry? current);
        if (current?.ChangeVersion != expectedChangeVersion
            || current?.Entry.Kind != expectedEntry?.Kind)
        {
            return Task.FromResult(AvailabilityEntryWriteStoreResult.Conflict);
        }

        long version = expectedChangeVersion is null ? 1 : expectedChangeVersion.Value + 1;
        _entries[key] = new StoredEntry(replacement, version);
        return Task.FromResult(AvailabilityEntryWriteStoreResult.Succeeded(version));
    }

    public Task<AvailabilityEntryRemoveStoreResult> RemoveAsync(
        AvailabilityEntry expectedEntry,
        long expectedChangeVersion,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RemoveCallCount++;
        (Guid, DateOnly) key = (expectedEntry.EmployeeId.Value, expectedEntry.Date);
        if (!_entries.TryGetValue(key, out StoredEntry? current)
            || current.ChangeVersion != expectedChangeVersion
            || current.Entry.Kind != expectedEntry.Kind)
        {
            return Task.FromResult(AvailabilityEntryRemoveStoreResult.Conflict);
        }

        _entries.Remove(key);
        return Task.FromResult(AvailabilityEntryRemoveStoreResult.Succeeded);
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
