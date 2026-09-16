using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class ScheduleAssignmentValidationResult
{
    private ScheduleAssignmentValidationResult(
        ScheduleAssignment? value,
        ReadOnlyCollection<ScheduleAssignmentValidationError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Value is not null;

    public ScheduleAssignment? Value { get; }

    public IReadOnlyList<ScheduleAssignmentValidationError> Errors { get; }

    internal static ScheduleAssignmentValidationResult Success(ScheduleAssignment value)
    {
        return new ScheduleAssignmentValidationResult(
            value,
            Array.AsReadOnly(Array.Empty<ScheduleAssignmentValidationError>()));
    }

    internal static ScheduleAssignmentValidationResult Failure(
        IEnumerable<ScheduleAssignmentValidationError> errors)
    {
        return new ScheduleAssignmentValidationResult(
            null,
            Array.AsReadOnly(errors.ToArray()));
    }
}
