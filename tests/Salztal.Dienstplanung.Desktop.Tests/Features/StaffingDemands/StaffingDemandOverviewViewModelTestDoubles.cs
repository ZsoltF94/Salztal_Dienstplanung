using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Application.StaffingDemands;
using Salztal.Dienstplanung.Desktop.Shared;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Domain.StaffingDemands;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Desktop.Tests.Features.StaffingDemands;

internal sealed class FakeStaffingDemandReader : IStaffingDemandReader
{
    public FakeStaffingDemandReader(StaffingDemandReadData data)
    {
        Data = data;
        LoadHandler = _ => Task.FromResult(Data);
    }

    public StaffingDemandReadData Data { get; private set; }

    public Func<CancellationToken, Task<StaffingDemandReadData>> LoadHandler { get; set; }

    public int LoadCallCount { get; private set; }

    public Task<StaffingDemandReadData> LoadAsync(CancellationToken cancellationToken)
    {
        LoadCallCount++;
        return LoadHandler(cancellationToken);
    }

    public void AppendStandardRevision(StandardStaffingDemandRevision revision)
    {
        Data = new StaffingDemandReadData(
            Data.StandardRevisions.Append(revision),
            Data.DateExceptions,
            Data.ServiceCatalog);
    }

    public void SaveDateException(StaffingDemandDateException dateException)
    {
        Data = new StaffingDemandReadData(
            Data.StandardRevisions,
            Data.DateExceptions
                .Where(candidate => candidate.Key != dateException.Key)
                .Append(dateException),
            Data.ServiceCatalog);
    }

    public void RemoveDateException(StaffingDemandDateException dateException)
    {
        Data = new StaffingDemandReadData(
            Data.StandardRevisions,
            Data.DateExceptions.Where(candidate => candidate.Key != dateException.Key),
            Data.ServiceCatalog);
    }

    public static StaffingDemandReadData CreateData(
        IEnumerable<StandardStaffingDemandRevision>? standards = null,
        IEnumerable<StaffingDemandDateException>? dateExceptions = null)
    {
        ServiceCatalogData serviceCatalog = new(
            InitialWorkLocationCatalog.All,
            InitialShiftTypeCatalog.All,
            InitialShiftPatternCatalog.SplitShift,
            InitialShiftPatternCatalog.ReliefShift);

        return new StaffingDemandReadData(
            standards ?? InitialStaffingDemandCatalog.All,
            dateExceptions ?? [],
            serviceCatalog);
    }
}

internal sealed class FakeStaffingDemandDateExceptionStore :
    IStaffingDemandDateExceptionStore,
    IRemoveStaffingDemandDateExceptionStore
{
    public StaffingDemandWriteStoreResult SaveResult { get; set; } =
        StaffingDemandWriteStoreResult.Succeeded;

    public StaffingDemandWriteStoreResult RemoveResult { get; set; } =
        StaffingDemandWriteStoreResult.Succeeded;

    public Func<StaffingDemandDateException?, StaffingDemandDateException,
        CancellationToken, Task<StaffingDemandWriteStoreResult>>? SaveHandler
    { get; set; }

    public Func<StaffingDemandDateException, CancellationToken,
        Task<StaffingDemandWriteStoreResult>>? RemoveHandler
    { get; set; }

    public int SaveCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public StaffingDemandDateException? ExpectedCurrent { get; private set; }

    public StaffingDemandDateException? WrittenException { get; private set; }

    public StaffingDemandDateException? RemovedException { get; private set; }

    public Task<StaffingDemandWriteStoreResult> SaveAsync(
        StaffingDemandDateException? expectedCurrent,
        StaffingDemandDateException replacement,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveCallCount++;
        ExpectedCurrent = expectedCurrent;
        WrittenException = replacement;
        return SaveHandler?.Invoke(expectedCurrent, replacement, cancellationToken)
            ?? Task.FromResult(SaveResult);
    }

    public Task<StaffingDemandWriteStoreResult> RemoveAsync(
        StaffingDemandDateException expected,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RemoveCallCount++;
        RemovedException = expected;
        return RemoveHandler?.Invoke(expected, cancellationToken)
            ?? Task.FromResult(RemoveResult);
    }
}

internal sealed class CollectingUnexpectedErrorReporter : IUnexpectedErrorReporter
{
    public List<(Exception Exception, string Operation)> Reports { get; } = [];

    public void Report(Exception exception, string operation)
    {
        Reports.Add((exception, operation));
    }
}

internal sealed class FakeStandardStaffingDemandRevisionStore :
    IStandardStaffingDemandRevisionStore
{
    public StaffingDemandWriteStoreResult Result { get; set; } =
        StaffingDemandWriteStoreResult.Succeeded;

    public Func<
        StandardStaffingDemandRevision?,
        StandardStaffingDemandRevision,
        CancellationToken,
        Task<StaffingDemandWriteStoreResult>>? AppendHandler
    { get; set; }

    public int AppendCallCount { get; private set; }

    public StandardStaffingDemandRevision? ExpectedCurrent { get; private set; }

    public StandardStaffingDemandRevision? WrittenRevision { get; private set; }

    public Task<StaffingDemandWriteStoreResult> AppendAsync(
        StandardStaffingDemandRevision? expectedCurrent,
        StandardStaffingDemandRevision revision,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AppendCallCount++;
        ExpectedCurrent = expectedCurrent;
        WrittenRevision = revision;
        return AppendHandler?.Invoke(expectedCurrent, revision, cancellationToken)
            ?? Task.FromResult(Result);
    }

}
