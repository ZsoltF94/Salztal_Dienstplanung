using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public enum UpdateWorkLocationStatus
{
    Succeeded,
    ValidationFailed,
    NotFound,
    Conflict,
}

public enum UpdateWorkLocationErrorCode
{
    IdentifierRequired,
    NameRequired,
    ColorRequired,
    NotFound,
    Conflict,
}

public sealed record UpdateWorkLocationError(
    UpdateWorkLocationErrorCode Code,
    string Message);

public sealed class UpdateWorkLocationResult
{
    private UpdateWorkLocationResult(
        UpdateWorkLocationStatus status,
        WorkLocationSnapshot? value,
        ReadOnlyCollection<UpdateWorkLocationError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public UpdateWorkLocationStatus Status { get; }

    public WorkLocationSnapshot? Value { get; }

    public IReadOnlyList<UpdateWorkLocationError> Errors { get; }

    internal static UpdateWorkLocationResult Success(WorkLocationSnapshot value)
    {
        return new UpdateWorkLocationResult(
            UpdateWorkLocationStatus.Succeeded,
            value,
            Array.AsReadOnly(Array.Empty<UpdateWorkLocationError>()));
    }

    internal static UpdateWorkLocationResult Failure(
        UpdateWorkLocationStatus status,
        IEnumerable<UpdateWorkLocationError> errors)
    {
        return new UpdateWorkLocationResult(
            status,
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
