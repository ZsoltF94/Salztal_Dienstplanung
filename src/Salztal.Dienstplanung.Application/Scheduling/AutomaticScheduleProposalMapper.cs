using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

internal static class AutomaticScheduleProposalMapper
{
    public static ScheduleAssignment[]? TryCreateAssignments(
        AutomaticScheduleProposal proposal,
        ScheduleDraft draft,
        ServiceCatalogData serviceCatalog)
    {
        List<ScheduleAssignment> assignments = [];
        foreach (ScheduleAssignmentSnapshot snapshot in proposal.Assignments)
        {
            ScheduleAssignment? assignment = TryCreateAssignment(
                snapshot,
                draft,
                serviceCatalog);
            if (assignment is null)
            {
                return null;
            }

            assignments.Add(assignment);
        }

        return assignments.ToArray();
    }

    private static ScheduleAssignment? TryCreateAssignment(
        ScheduleAssignmentSnapshot snapshot,
        ScheduleDraft draft,
        ServiceCatalogData serviceCatalog)
    {
        if (snapshot.Origin != ScheduleAssignmentOriginSnapshot.AutomaticGeneration
            || snapshot.IsProtectedFromAutomaticGeneration
            || !EmployeeId.TryCreate(snapshot.EmployeeId, out EmployeeId? employeeId))
        {
            return null;
        }

        DemandSlot[] slots = snapshot.Coverages
            .Select(coverage => FindSlot(draft, coverage))
            .OfType<DemandSlot>()
            .ToArray();
        if (slots.Length != snapshot.Coverages.Count)
        {
            return null;
        }

        ScheduleAssignment? assignment = snapshot.Kind switch
        {
            ScheduleAssignmentKindSnapshot.NormalDemand
                when slots.Length == 1 && snapshot.PatternId is null =>
                ScheduleAssignment.CreateNormal(
                    snapshot.AssignmentId,
                    employeeId,
                    slots[0],
                    AssignmentOrigin.AutomaticGeneration).Value,
            ScheduleAssignmentKindSnapshot.SplitShiftPattern
                when slots.Length == 2
                    && snapshot.PatternId == serviceCatalog.SplitShiftPattern.Id.Value =>
                ScheduleAssignment.CreateSplitShift(
                    snapshot.AssignmentId,
                    employeeId,
                    serviceCatalog.SplitShiftPattern,
                    slots[0],
                    slots[1],
                    AssignmentOrigin.AutomaticGeneration).Value,
            ScheduleAssignmentKindSnapshot.ReliefShiftPattern
                when slots.Length == 2
                    && snapshot.PatternId == serviceCatalog.ReliefShiftPattern.Id.Value =>
                ScheduleAssignment.CreateReliefShift(
                    snapshot.AssignmentId,
                    employeeId,
                    serviceCatalog.ReliefShiftPattern,
                    slots[0],
                    slots[1],
                    AssignmentOrigin.AutomaticGeneration).Value,
            _ => null,
        };

        return assignment is not null
            && Matches(snapshot, ScheduleSnapshotMapper.CreateAssignment(assignment))
                ? assignment
                : null;
    }

    private static DemandSlot? FindSlot(
        ScheduleDraft draft,
        ScheduleDemandCoverageSnapshot coverage)
    {
        return draft.DemandSlots.Slots.SingleOrDefault(slot =>
            slot.Id.SourceId.Value == coverage.DemandSourceId
            && slot.Id.Date == coverage.Date
            && slot.Id.WorkLocationId.Value == coverage.WorkLocationId
            && slot.Id.ShiftTypeId.Value == coverage.ShiftTypeId
            && slot.Id.Ordinal == coverage.Ordinal);
    }

    private static bool Matches(
        ScheduleAssignmentSnapshot expected,
        ScheduleAssignmentSnapshot actual)
    {
        return expected.AssignmentId == actual.AssignmentId
            && expected.EmployeeId == actual.EmployeeId
            && expected.Date == actual.Date
            && expected.Kind == actual.Kind
            && expected.Origin == actual.Origin
            && expected.PatternId == actual.PatternId
            && expected.WorkMinutes == actual.WorkMinutes
            && expected.IsProtectedFromAutomaticGeneration
                == actual.IsProtectedFromAutomaticGeneration
            && expected.Segments.SequenceEqual(actual.Segments)
            && expected.Coverages.SequenceEqual(actual.Coverages);
    }
}
