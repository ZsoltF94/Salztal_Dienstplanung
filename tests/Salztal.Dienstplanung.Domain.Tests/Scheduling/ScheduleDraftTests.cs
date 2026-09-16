using Salztal.Dienstplanung.Domain.Availabilities;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.Scheduling;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Domain.Tests.Scheduling;

public sealed class ScheduleDraftTests
{
    private static readonly EmployeeId FirstEmployeeId = CreateEmployeeId(1);
    private static readonly EmployeeId SecondEmployeeId = CreateEmployeeId(2);

    [Fact]
    public void CreateValidDraftKeepsImmutableOrderedPlanningState()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment laterAssignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday.AddDays(2),
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        ScheduleAssignment earlierAssignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.LateShift,
            AssignmentOrigin.AutomaticGeneration);
        AssignmentLock assignmentLock = new(laterAssignment.Id);

        ScheduleDraft draft = context.CreateDraft(
            assignments: [laterAssignment, earlierAssignment],
            locks: [assignmentLock]);

        Assert.Equal(context.Period, draft.Period);
        Assert.Equal(1, draft.Version.Value);
        Assert.Equal([earlierAssignment, laterAssignment], draft.Assignments);
        Assert.Equal(assignmentLock, Assert.Single(draft.AssignmentLocks));
        Assert.Empty(draft.GeneratedDayOffMarkers);
    }

    [Fact]
    public void CreateWithInvalidIdAndVersionReturnsStructuredErrors()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();

        ScheduleDraftValidationResult result = context.CreateDraftResult(
            id: Guid.Empty,
            version: 0);

        Assert.Equal(
            [
                ScheduleDraftValidationCode.IdentifierRequired,
                ScheduleDraftValidationCode.VersionMustBePositive,
            ],
            result.Errors.Select(error => error.Code));
    }

    [Fact]
    public void CreateWithTwoAssignmentsForEmployeeAndDateReturnsStructuredError()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        DateOnly date = context.Period.StartMonday;
        ScheduleAssignment early = context.CreateNormalAssignment(
            FirstEmployeeId,
            date,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        ScheduleAssignment late = context.CreateNormalAssignment(
            FirstEmployeeId,
            date,
            InitialShiftTypeCatalog.LateShift,
            AssignmentOrigin.ServiceManagement);

        ScheduleDraftValidationResult result = context.CreateDraftResult(
            assignments: [early, late]);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.DuplicateEmployeeDateAssignment);
    }

    [Fact]
    public void CreateWithAssignmentOnAvailabilityEntryReturnsStructuredError()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        DateOnly date = context.Period.StartMonday;
        AvailabilityEntry availabilityEntry = CreateAvailabilityEntry(
            FirstEmployeeId,
            date,
            AvailabilityEntryKind.FixedDayOff);
        ScheduleAssignment assignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            date,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);

        ScheduleDraftValidationResult result = context.CreateDraftResult(
            availabilityEntries: [availabilityEntry],
            assignments: [assignment]);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.AssignmentOnUnavailableDay);
    }

    [Fact]
    public void CreateWithTwoCoveringAssignmentsForSameSlotReturnsStructuredError()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        DemandSlot slot = context.FindSlot(
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            ordinal: 1);
        ScheduleAssignment first = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                FirstEmployeeId,
                slot,
                AssignmentOrigin.AutomaticGeneration).Value);
        ScheduleAssignment second = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                SecondEmployeeId,
                slot,
                AssignmentOrigin.AutomaticGeneration).Value);

        ScheduleDraftValidationResult result = context.CreateDraftResult(
            assignments: [first, second]);

        Assert.Contains(
            result.Errors,
            error => error.Code == ScheduleDraftValidationCode.DuplicateDemandCoverage
                && error.DemandSlotId == slot.Id);
    }

    [Fact]
    public void CreateAllowsOfficeTimeAndCoveringAssignmentForSameSlot()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        DemandSlot slot = context.FindSlot(
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            ordinal: 1);
        ScheduleAssignment office = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateOfficeTime(
                Guid.NewGuid(),
                FirstEmployeeId,
                slot).Value);
        ScheduleAssignment covering = Assert.IsType<ScheduleAssignment>(
            ScheduleAssignment.CreateNormal(
                Guid.NewGuid(),
                SecondEmployeeId,
                slot,
                AssignmentOrigin.AutomaticGeneration).Value);

        ScheduleDraft draft = context.CreateDraft(assignments: [office, covering]);

        Assert.Equal(2, draft.Assignments.Count);
        Assert.Single(draft.Assignments.SelectMany(assignment => assignment.Coverages));
    }

    [Fact]
    public void CreateWithGeneratedDayOffConflictsReturnsStructuredErrors()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        DateOnly date = context.Period.StartMonday;
        AvailabilityEntry availabilityEntry = CreateAvailabilityEntry(
            FirstEmployeeId,
            date,
            AvailabilityEntryKind.Vacation);
        ScheduleAssignment assignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            date,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        GeneratedDayOffMarker marker = new(FirstEmployeeId, date);

        ScheduleDraftValidationResult result = context.CreateDraftResult(
            availabilityEntries: [availabilityEntry],
            assignments: [assignment],
            markers: [marker, marker]);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.DuplicateEmployeeDateDayState);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.GeneratedDayOffConflictsAvailabilityEntry);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.GeneratedDayOffConflictsAssignment);
    }

    [Fact]
    public void CreateWithUnknownOrDuplicateLockReturnsStructuredErrors()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment assignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        ScheduleAssignmentId.TryCreate(Guid.NewGuid(), out ScheduleAssignmentId? unknownId);
        AssignmentLock knownLock = new(assignment.Id);
        AssignmentLock unknownLock = new(unknownId!);

        ScheduleDraftValidationResult result = context.CreateDraftResult(
            assignments: [assignment],
            locks: [knownLock, knownLock, unknownLock]);

        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.DuplicateAssignmentLock);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.LockReferencesUnknownAssignment
                && error.AssignmentId == unknownId);
    }

    private static AvailabilityEntry CreateAvailabilityEntry(
        EmployeeId employeeId,
        DateOnly date,
        AvailabilityEntryKind kind)
    {
        return Assert.IsType<AvailabilityEntry>(
            AvailabilityEntry.Create(employeeId.Value, date, kind).Value);
    }

    private static EmployeeId CreateEmployeeId(int suffix)
    {
        EmployeeId.TryCreate(
            new Guid($"30000000-0000-4000-8000-{suffix:000000000000}"),
            out EmployeeId? employeeId);
        return employeeId!;
    }
}
