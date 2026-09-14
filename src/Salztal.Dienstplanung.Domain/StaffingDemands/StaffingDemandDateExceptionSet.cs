using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StaffingDemandDateExceptionSet
{
    private StaffingDemandDateExceptionSet(
        ReadOnlyCollection<StaffingDemandDateException> exceptions)
    {
        Exceptions = exceptions;
    }

    public IReadOnlyList<StaffingDemandDateException> Exceptions { get; }

    public static StaffingDemandDateExceptionSetValidationResult Create(
        IEnumerable<StaffingDemandDateException> exceptions)
    {
        ArgumentNullException.ThrowIfNull(exceptions);

        StaffingDemandDateException[] snapshot = exceptions.ToArray();
        List<StaffingDemandDateExceptionSetValidationError> errors = snapshot
            .GroupBy(exception => exception.Key)
            .Where(group => group.Count() > 1)
            .Select(
                group => new StaffingDemandDateExceptionSetValidationError(
                    StaffingDemandDateExceptionSetValidationCode.DuplicateDateKey,
                    group.Key))
            .ToList();

        if (errors.Count > 0)
        {
            return StaffingDemandDateExceptionSetValidationResult.Failure(errors);
        }

        StaffingDemandDateException[] ordered = snapshot
            .OrderBy(exception => exception.Key.Date)
            .ThenBy(exception => exception.Key.WorkLocationId.Value)
            .ThenBy(exception => exception.Key.ShiftTypeId.Value)
            .ToArray();

        return StaffingDemandDateExceptionSetValidationResult.Success(
            new StaffingDemandDateExceptionSet(Array.AsReadOnly(ordered)));
    }
}
