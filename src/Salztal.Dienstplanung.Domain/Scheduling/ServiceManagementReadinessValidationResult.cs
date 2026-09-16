using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class ServiceManagementReadinessValidationResult
{
    private ServiceManagementReadinessValidationResult(
        ServiceManagementReadiness? value,
        ReadOnlyCollection<ServiceManagementReadinessValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public ServiceManagementReadiness? Value { get; }

    public IReadOnlyList<ServiceManagementReadinessValidationError> Errors { get; }

    internal static ServiceManagementReadinessValidationResult Success(
        ServiceManagementReadiness value)
    {
        return new ServiceManagementReadinessValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<ServiceManagementReadinessValidationError>()));
    }

    internal static ServiceManagementReadinessValidationResult Failure(
        IEnumerable<ServiceManagementReadinessValidationError> errors)
    {
        return new ServiceManagementReadinessValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
