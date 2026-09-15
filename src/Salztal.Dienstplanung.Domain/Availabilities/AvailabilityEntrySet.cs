using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Availabilities;

public sealed class AvailabilityEntrySet
{
    private AvailabilityEntrySet(ReadOnlyCollection<AvailabilityEntry> entries)
    {
        Entries = entries;
    }

    public IReadOnlyList<AvailabilityEntry> Entries { get; }

    public static AvailabilityEntrySetValidationResult Create(
        IEnumerable<AvailabilityEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        AvailabilityEntry[] snapshot = entries.ToArray();
        List<AvailabilityEntrySetValidationError> errors = snapshot
            .GroupBy(entry => (entry.EmployeeId, entry.Date))
            .Where(group => group.Count() > 1)
            .Select(
                group => new AvailabilityEntrySetValidationError(
                    AvailabilityEntrySetValidationCode.DuplicateEmployeeAndDate,
                    group.Key.EmployeeId,
                    group.Key.Date))
            .ToList();

        if (errors.Count > 0)
        {
            return AvailabilityEntrySetValidationResult.Failure(errors);
        }

        return AvailabilityEntrySetValidationResult.Success(CreateValidated(snapshot));
    }

    public AvailabilityEntry? Find(EmployeeId employeeId, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        return Entries.SingleOrDefault(
            entry => entry.EmployeeId == employeeId && entry.Date == date);
    }

    public AvailabilityEntrySet WithEntry(AvailabilityEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        AvailabilityEntry[] changed = Entries
            .Where(candidate =>
                candidate.EmployeeId != entry.EmployeeId || candidate.Date != entry.Date)
            .Append(entry)
            .ToArray();

        return CreateValidated(changed);
    }

    private static AvailabilityEntrySet CreateValidated(
        IEnumerable<AvailabilityEntry> entries)
    {
        AvailabilityEntry[] ordered = entries
            .OrderBy(entry => entry.EmployeeId.Value)
            .ThenBy(entry => entry.Date)
            .ToArray();

        return new AvailabilityEntrySet(Array.AsReadOnly(ordered));
    }
}
