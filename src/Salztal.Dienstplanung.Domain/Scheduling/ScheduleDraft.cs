using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Domain.Scheduling;

public sealed class ScheduleDraft
{
    private ScheduleDraft(
        ScheduleDraftId id,
        ScheduleDraftVersion version,
        SchedulePeriod period,
        DemandSlotSet demandSlots,
        AvailabilityEntrySet availabilityEntries,
        ReadOnlyCollection<ScheduleAssignment> assignments,
        ReadOnlyCollection<GeneratedDayOffMarker> generatedDayOffMarkers,
        ReadOnlyCollection<AssignmentLock> assignmentLocks)
    {
        Id = id;
        Version = version;
        Period = period;
        DemandSlots = demandSlots;
        AvailabilityEntries = availabilityEntries;
        Assignments = assignments;
        GeneratedDayOffMarkers = generatedDayOffMarkers;
        AssignmentLocks = assignmentLocks;
    }

    public ScheduleDraftId Id { get; }

    public ScheduleDraftVersion Version { get; }

    public SchedulePeriod Period { get; }

    public DemandSlotSet DemandSlots { get; }

    public AvailabilityEntrySet AvailabilityEntries { get; }

    public IReadOnlyList<ScheduleAssignment> Assignments { get; }

    public IReadOnlyList<GeneratedDayOffMarker> GeneratedDayOffMarkers { get; }

    public IReadOnlyList<AssignmentLock> AssignmentLocks { get; }

    public static ScheduleDraftValidationResult Create(
        Guid id,
        int version,
        SchedulePeriod period,
        DemandSlotSet demandSlots,
        AvailabilityEntrySet availabilityEntries,
        IEnumerable<ScheduleAssignment> assignments,
        IEnumerable<GeneratedDayOffMarker> generatedDayOffMarkers,
        IEnumerable<AssignmentLock> assignmentLocks)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(demandSlots);
        ArgumentNullException.ThrowIfNull(availabilityEntries);
        ArgumentNullException.ThrowIfNull(assignments);
        ArgumentNullException.ThrowIfNull(generatedDayOffMarkers);
        ArgumentNullException.ThrowIfNull(assignmentLocks);

        ScheduleAssignment[] assignmentSnapshot = assignments.ToArray();
        GeneratedDayOffMarker[] markerSnapshot = generatedDayOffMarkers.ToArray();
        AssignmentLock[] lockSnapshot = assignmentLocks.ToArray();
        List<ScheduleDraftValidationError> errors = [];

        if (!ScheduleDraftId.TryCreate(id, out ScheduleDraftId? draftId))
        {
            errors.Add(new ScheduleDraftValidationError(
                ScheduleDraftValidationCode.IdentifierRequired));
        }

        if (!ScheduleDraftVersion.TryCreate(version, out ScheduleDraftVersion? draftVersion))
        {
            errors.Add(new ScheduleDraftValidationError(
                ScheduleDraftValidationCode.VersionMustBePositive));
        }

        ValidateAssignments(
            period,
            demandSlots,
            availabilityEntries,
            assignmentSnapshot,
            errors);
        ValidateMarkers(
            period,
            availabilityEntries,
            assignmentSnapshot,
            markerSnapshot,
            errors);
        ValidateLocks(assignmentSnapshot, lockSnapshot, errors);

        if (errors.Count > 0)
        {
            return ScheduleDraftValidationResult.Failure(errors);
        }

        ScheduleAssignment[] orderedAssignments = assignmentSnapshot
            .OrderBy(assignment => assignment.EmployeeId.Value)
            .ThenBy(assignment => assignment.Date)
            .ThenBy(assignment => assignment.Id.Value)
            .ToArray();
        GeneratedDayOffMarker[] orderedMarkers = markerSnapshot
            .OrderBy(marker => marker.EmployeeId.Value)
            .ThenBy(marker => marker.Date)
            .ToArray();
        AssignmentLock[] orderedLocks = lockSnapshot
            .OrderBy(assignmentLock => assignmentLock.AssignmentId.Value)
            .ToArray();

        return ScheduleDraftValidationResult.Success(
            new ScheduleDraft(
                draftId!,
                draftVersion!,
                period,
                demandSlots,
                availabilityEntries,
                Array.AsReadOnly(orderedAssignments),
                Array.AsReadOnly(orderedMarkers),
                Array.AsReadOnly(orderedLocks)));
    }

    private static void ValidateAssignments(
        SchedulePeriod period,
        DemandSlotSet demandSlots,
        AvailabilityEntrySet availabilityEntries,
        ScheduleAssignment[] assignments,
        List<ScheduleDraftValidationError> errors)
    {
        HashSet<DemandSlotId> knownSlotIds = demandSlots.Slots
            .Select(slot => slot.Id)
            .ToHashSet();

        errors.AddRange(
            assignments
                .GroupBy(assignment => assignment.Id)
                .Where(group => group.Count() > 1)
                .Select(group => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.DuplicateAssignmentIdentifier,
                    group.Key)));

        errors.AddRange(
            assignments
                .GroupBy(assignment => (assignment.EmployeeId, assignment.Date))
                .Where(group => group.Count() > 1)
                .Select(group => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.DuplicateEmployeeDateAssignment,
                    EmployeeId: group.Key.EmployeeId,
                    Date: group.Key.Date)));

        foreach (ScheduleAssignment assignment in assignments)
        {
            if (!period.Contains(assignment.Date))
            {
                errors.Add(new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.AssignmentOutsidePeriod,
                    assignment.Id,
                    assignment.EmployeeId,
                    assignment.Date));
            }

            if (availabilityEntries.Find(assignment.EmployeeId, assignment.Date) is not null)
            {
                errors.Add(new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.AssignmentOnUnavailableDay,
                    assignment.Id,
                    assignment.EmployeeId,
                    assignment.Date));
            }

            foreach (DemandSlotId slotId in assignment.Segments
                         .Select(segment => segment.AnchorSlotId)
                         .Concat(assignment.Coverages.Select(coverage => coverage.SlotId))
                         .Distinct())
            {
                if (!knownSlotIds.Contains(slotId))
                {
                    errors.Add(new ScheduleDraftValidationError(
                        ScheduleDraftValidationCode.AssignmentReferencesUnknownDemandSlot,
                        assignment.Id,
                        assignment.EmployeeId,
                        assignment.Date,
                        slotId));
                }
            }
        }

        errors.AddRange(
            assignments
                .SelectMany(
                    assignment => assignment.Coverages.Select(
                        coverage => (Assignment: assignment, coverage.SlotId)))
                .GroupBy(item => item.SlotId)
                .Where(group => group.Count() > 1)
                .Select(group => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.DuplicateDemandCoverage,
                    DemandSlotId: group.Key)));
    }

    private static void ValidateMarkers(
        SchedulePeriod period,
        AvailabilityEntrySet availabilityEntries,
        ScheduleAssignment[] assignments,
        GeneratedDayOffMarker[] markers,
        List<ScheduleDraftValidationError> errors)
    {
        errors.AddRange(
            markers
                .Where(marker => !period.Contains(marker.Date))
                .Select(marker => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.GeneratedDayOffOutsidePeriod,
                    EmployeeId: marker.EmployeeId,
                    Date: marker.Date)));

        errors.AddRange(
            markers
                .GroupBy(marker => (marker.EmployeeId, marker.Date))
                .Where(group => group.Count() > 1)
                .Select(group => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.DuplicateEmployeeDateDayState,
                    EmployeeId: group.Key.EmployeeId,
                    Date: group.Key.Date)));

        foreach (GeneratedDayOffMarker marker in markers)
        {
            if (availabilityEntries.Find(marker.EmployeeId, marker.Date) is not null)
            {
                errors.Add(new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.GeneratedDayOffConflictsAvailabilityEntry,
                    EmployeeId: marker.EmployeeId,
                    Date: marker.Date));
            }

            if (assignments.Any(assignment =>
                    assignment.EmployeeId == marker.EmployeeId
                    && assignment.Date == marker.Date))
            {
                errors.Add(new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.GeneratedDayOffConflictsAssignment,
                    EmployeeId: marker.EmployeeId,
                    Date: marker.Date));
            }
        }
    }

    private static void ValidateLocks(
        ScheduleAssignment[] assignments,
        AssignmentLock[] locks,
        List<ScheduleDraftValidationError> errors)
    {
        HashSet<ScheduleAssignmentId> assignmentIds = assignments
            .Select(assignment => assignment.Id)
            .ToHashSet();

        errors.AddRange(
            locks
                .Where(assignmentLock => !assignmentIds.Contains(assignmentLock.AssignmentId))
                .Select(assignmentLock => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.LockReferencesUnknownAssignment,
                    assignmentLock.AssignmentId)));

        errors.AddRange(
            locks
                .GroupBy(assignmentLock => assignmentLock.AssignmentId)
                .Where(group => group.Count() > 1)
                .Select(group => new ScheduleDraftValidationError(
                    ScheduleDraftValidationCode.DuplicateAssignmentLock,
                    group.Key)));
    }
}
