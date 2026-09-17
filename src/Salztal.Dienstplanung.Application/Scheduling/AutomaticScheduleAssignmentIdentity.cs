using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record AutomaticScheduleDemandSlotIdentity(
    Guid SourceId,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    int Ordinal)
{
    public int Ordinal { get; } = Ordinal > 0
        ? Ordinal
        : throw new ArgumentOutOfRangeException(nameof(Ordinal));
}

public sealed class AutomaticScheduleAssignmentIdentity
{
    public AutomaticScheduleAssignmentIdentity(
        Guid snapshotId,
        Guid employeeId,
        DateOnly date,
        ScheduleAssignmentKindSnapshot kind,
        Guid? patternId,
        IEnumerable<AutomaticScheduleDemandSlotIdentity> demandSlots)
    {
        if (snapshotId == Guid.Empty)
        {
            throw new ArgumentException("Snapshot identifier is required.", nameof(snapshotId));
        }

        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException("Employee identifier is required.", nameof(employeeId));
        }

        if (kind is not ScheduleAssignmentKindSnapshot.NormalDemand
            and not ScheduleAssignmentKindSnapshot.SplitShiftPattern
            and not ScheduleAssignmentKindSnapshot.ReliefShiftPattern)
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        ArgumentNullException.ThrowIfNull(demandSlots);
        AutomaticScheduleDemandSlotIdentity[] slots = demandSlots.ToArray();
        if (slots.Any(slot => slot is null))
        {
            throw new ArgumentException(
                "Assignment demand slots cannot contain null values.",
                nameof(demandSlots));
        }

        if (slots.Distinct().Count() != slots.Length)
        {
            throw new ArgumentException(
                "Assignment demand slots must be unique.",
                nameof(demandSlots));
        }

        if (slots.Any(slot => slot.Date != date))
        {
            throw new ArgumentException(
                "Every assignment demand slot must use the assignment date.",
                nameof(demandSlots));
        }

        bool isNormal = kind == ScheduleAssignmentKindSnapshot.NormalDemand;
        int expectedSlotCount = isNormal ? 1 : 2;
        if (slots.Length != expectedSlotCount)
        {
            throw new ArgumentException(
                $"Assignment kind {kind} requires {expectedSlotCount} demand slots.",
                nameof(demandSlots));
        }

        if (isNormal == patternId.HasValue)
        {
            throw new ArgumentException(
                isNormal
                    ? "A normal assignment cannot reference a shift pattern."
                    : "A composite assignment requires a shift pattern.",
                nameof(patternId));
        }

        SnapshotId = snapshotId;
        EmployeeId = employeeId;
        Date = date;
        Kind = kind;
        PatternId = patternId;
        DemandSlots = Array.AsReadOnly(
            slots
                .OrderBy(slot => slot.Date)
                .ThenBy(slot => slot.WorkLocationId)
                .ThenBy(slot => slot.ShiftTypeId)
                .ThenBy(slot => slot.SourceId)
                .ThenBy(slot => slot.Ordinal)
                .ToArray());
    }

    public Guid SnapshotId { get; }

    public Guid EmployeeId { get; }

    public DateOnly Date { get; }

    public ScheduleAssignmentKindSnapshot Kind { get; }

    public Guid? PatternId { get; }

    public ReadOnlyCollection<AutomaticScheduleDemandSlotIdentity> DemandSlots { get; }
}

public interface IAutomaticScheduleAssignmentIdFactory
{
    public Guid Create(AutomaticScheduleAssignmentIdentity identity);
}
