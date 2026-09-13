using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.WorkLocations;

public sealed class WorkLocationValidationResult
{
    private WorkLocationValidationResult(
        WorkLocation? value,
        ReadOnlyCollection<WorkLocationValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public WorkLocation? Value { get; }

    public IReadOnlyList<WorkLocationValidationError> Errors { get; }

    internal static WorkLocationValidationResult Success(WorkLocation value)
    {
        return new WorkLocationValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<WorkLocationValidationError>()));
    }

    internal static WorkLocationValidationResult Failure(
        IEnumerable<WorkLocationValidationError> errors)
    {
        return new WorkLocationValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
