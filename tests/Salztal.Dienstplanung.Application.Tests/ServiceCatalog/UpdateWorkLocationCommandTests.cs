using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.Tests.ServiceCatalog;

public sealed class UpdateWorkLocationCommandTests
{
    [Fact]
    public async Task ExecuteAsyncWhenInputIsValidUpdatesExistingLocation()
    {
        FakeWorkLocationUpdateStore store = new(InitialWorkLocationCatalog.Cafeteria);
        UpdateWorkLocationCommand command = new(store);
        UpdateWorkLocationRequest request = new(
            InitialWorkLocationCatalog.Cafeteria.Id.Value,
            "Synthetischer Kiosk",
            "blue");

        UpdateWorkLocationResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateWorkLocationStatus.Succeeded, result.Status);
        WorkLocationSnapshot snapshot = Assert.IsType<WorkLocationSnapshot>(result.Value);
        Assert.Equal(request.WorkLocationId, snapshot.Id);
        Assert.Equal("Synthetischer Kiosk", snapshot.Name);
        Assert.Equal("blue", snapshot.ColorCode);
        Assert.Empty(result.Errors);
        Assert.Same(InitialWorkLocationCatalog.Cafeteria, store.ExpectedValue);
        Assert.Equal("Synthetischer Kiosk", store.ReplacementValue?.Name.Value);
    }

    [Fact]
    public async Task ExecuteAsyncWhenInputIsInvalidReturnsGermanValidationErrorsWithoutStoreAccess()
    {
        FakeWorkLocationUpdateStore store = new(InitialWorkLocationCatalog.Cafeteria);
        UpdateWorkLocationCommand command = new(store);
        UpdateWorkLocationRequest request = new(Guid.Empty, " ", null);

        UpdateWorkLocationResult result = await command.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateWorkLocationStatus.ValidationFailed, result.Status);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => AssertError(
                UpdateWorkLocationErrorCode.IdentifierRequired,
                "Der Einsatzort besitzt keine gültige Kennung.",
                error),
            error => AssertError(
                UpdateWorkLocationErrorCode.NameRequired,
                "Bitte geben Sie einen Namen für den Einsatzort ein.",
                error),
            error => AssertError(
                UpdateWorkLocationErrorCode.ColorRequired,
                "Bitte wählen Sie eine Farbkennung für den Einsatzort aus.",
                error));
        Assert.Equal(0, store.FindCallCount);
        Assert.Equal(0, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenLocationDoesNotExistReturnsNotFound()
    {
        FakeWorkLocationUpdateStore store = new(null);
        UpdateWorkLocationCommand command = new(store);

        UpdateWorkLocationResult result = await command.ExecuteAsync(
            new UpdateWorkLocationRequest(Guid.NewGuid(), "Synthetischer Kiosk", "blue"),
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateWorkLocationStatus.NotFound, result.Status);
        UpdateWorkLocationError error = Assert.Single(result.Errors);
        AssertError(
            UpdateWorkLocationErrorCode.NotFound,
            "Der Einsatzort wurde nicht gefunden.",
            error);
        Assert.Equal(0, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenStoredValueChangedReturnsConflict()
    {
        FakeWorkLocationUpdateStore store = new(InitialWorkLocationCatalog.Restaurant)
        {
            UpdateResult = CatalogEntryUpdateStoreResult.Conflict,
        };
        UpdateWorkLocationCommand command = new(store);

        UpdateWorkLocationResult result = await command.ExecuteAsync(
            new UpdateWorkLocationRequest(
                InitialWorkLocationCatalog.Restaurant.Id.Value,
                "Synthetischer Speiseraum",
                "orange"),
            TestContext.Current.CancellationToken);

        Assert.Equal(UpdateWorkLocationStatus.Conflict, result.Status);
        UpdateWorkLocationError error = Assert.Single(result.Errors);
        Assert.Equal(UpdateWorkLocationErrorCode.Conflict, error.Code);
        Assert.Contains("Bitte laden Sie die Daten neu", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, store.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsyncWhenCancelledDoesNotAccessStore()
    {
        FakeWorkLocationUpdateStore store = new(InitialWorkLocationCatalog.Cafeteria);
        UpdateWorkLocationCommand command = new(store);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => command.ExecuteAsync(
            new UpdateWorkLocationRequest(
                InitialWorkLocationCatalog.Cafeteria.Id.Value,
                "Synthetischer Kiosk",
                "blue"),
            cancellation.Token));

        Assert.Equal(0, store.FindCallCount);
        Assert.Equal(0, store.UpdateCallCount);
    }

    private static void AssertError(
        UpdateWorkLocationErrorCode expectedCode,
        string expectedMessage,
        UpdateWorkLocationError actual)
    {
        Assert.Equal(expectedCode, actual.Code);
        Assert.Equal(expectedMessage, actual.Message);
    }

    private sealed class FakeWorkLocationUpdateStore : IWorkLocationUpdateStore
    {
        private readonly WorkLocation? _current;

        public FakeWorkLocationUpdateStore(WorkLocation? current)
        {
            _current = current;
        }

        public CatalogEntryUpdateStoreResult UpdateResult { get; init; } =
            CatalogEntryUpdateStoreResult.Updated;

        public int FindCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public WorkLocation? ExpectedValue { get; private set; }

        public WorkLocation? ReplacementValue { get; private set; }

        public Task<WorkLocation?> FindAsync(
            WorkLocationId id,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FindCallCount++;
            return Task.FromResult(_current?.Id == id ? _current : null);
        }

        public Task<CatalogEntryUpdateStoreResult> UpdateAsync(
            WorkLocation expected,
            WorkLocation replacement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateCallCount++;
            ExpectedValue = expected;
            ReplacementValue = replacement;
            return Task.FromResult(UpdateResult);
        }
    }
}
