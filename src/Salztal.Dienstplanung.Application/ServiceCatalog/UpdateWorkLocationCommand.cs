using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public sealed class UpdateWorkLocationCommand
{
    private readonly IWorkLocationUpdateStore _store;

    public UpdateWorkLocationCommand(IWorkLocationUpdateStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public async Task<UpdateWorkLocationResult> ExecuteAsync(
        UpdateWorkLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        WorkLocationValidationResult validation = WorkLocation.Create(
            request.WorkLocationId,
            request.Name,
            request.ColorCode);

        if (!validation.IsSuccess)
        {
            return UpdateWorkLocationResult.Failure(
                UpdateWorkLocationStatus.ValidationFailed,
                validation.Errors.Select(CreateValidationError));
        }

        WorkLocation replacement = validation.Value!;
        WorkLocation? current = await _store.FindAsync(replacement.Id, cancellationToken);
        if (current is null)
        {
            return UpdateWorkLocationResult.Failure(
                UpdateWorkLocationStatus.NotFound,
                [new UpdateWorkLocationError(
                    UpdateWorkLocationErrorCode.NotFound,
                    "Der Einsatzort wurde nicht gefunden.")]);
        }

        CatalogEntryUpdateStoreResult storeResult = await _store.UpdateAsync(
            current,
            replacement,
            cancellationToken);

        return storeResult switch
        {
            CatalogEntryUpdateStoreResult.Updated =>
                UpdateWorkLocationResult.Success(new WorkLocationSnapshot(
                    replacement.Id.Value,
                    replacement.Name.Value,
                    replacement.Color.Code)),
            CatalogEntryUpdateStoreResult.Conflict => UpdateWorkLocationResult.Failure(
                UpdateWorkLocationStatus.Conflict,
                [new UpdateWorkLocationError(
                    UpdateWorkLocationErrorCode.Conflict,
                    "Der Einsatzort wurde zwischenzeitlich geändert. Bitte laden Sie die Daten neu.")]),
            _ => throw new InvalidOperationException(
                $"Unsupported work-location update result: {storeResult}"),
        };
    }

    private static UpdateWorkLocationError CreateValidationError(
        WorkLocationValidationError error)
    {
        return error.Code switch
        {
            WorkLocationValidationCode.IdentifierRequired => new UpdateWorkLocationError(
                UpdateWorkLocationErrorCode.IdentifierRequired,
                "Der Einsatzort besitzt keine gültige Kennung."),
            WorkLocationValidationCode.NameRequired => new UpdateWorkLocationError(
                UpdateWorkLocationErrorCode.NameRequired,
                "Bitte geben Sie einen Namen für den Einsatzort ein."),
            WorkLocationValidationCode.ColorRequired => new UpdateWorkLocationError(
                UpdateWorkLocationErrorCode.ColorRequired,
                "Bitte wählen Sie eine Farbkennung für den Einsatzort aus."),
            _ => throw new InvalidOperationException(
                $"Unsupported work-location validation code: {error.Code}"),
        };
    }
}
