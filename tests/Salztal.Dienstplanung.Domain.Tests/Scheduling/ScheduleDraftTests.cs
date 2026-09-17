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

    [Fact]
    public void ReplaceAutomaticGenerationExchangesReplaceableValuesAndAdvancesOnce()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment protectedAssignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        ScheduleAssignment oldAutomatic = context.CreateNormalAssignment(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(1),
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.AutomaticGeneration);
        GeneratedDayOffMarker oldMarker = new(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(2));
        ScheduleDraft draft = context.CreateDraft(
            assignments: [protectedAssignment, oldAutomatic],
            markers: [oldMarker]);
        ScheduleAssignment replacement = context.CreateNormalAssignment(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(3),
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.AutomaticGeneration);
        GeneratedDayOffMarker replacementMarker = new(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(4));

        ScheduleDraftValidationResult result = draft.ReplaceAutomaticGeneration(
            [replacement],
            [replacementMarker]);

        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(result.Value);
        Assert.Equal(draft.Version.Value + 1, updated.Version.Value);
        Assert.Equal([protectedAssignment, replacement], updated.Assignments);
        Assert.Equal(replacementMarker, Assert.Single(updated.GeneratedDayOffMarkers));
        Assert.DoesNotContain(oldAutomatic, updated.Assignments);
        Assert.DoesNotContain(oldMarker, updated.GeneratedDayOffMarkers);
        Assert.Equal(1, draft.Version.Value);
        Assert.Contains(oldAutomatic, draft.Assignments);
    }

    [Fact]
    public void ReplaceAutomaticGenerationPreservesLockedAutomaticAssignment()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment lockedAutomatic = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.AutomaticGeneration);
        AssignmentLock assignmentLock = new(lockedAutomatic.Id);
        ScheduleDraft draft = context.CreateDraft(
            assignments: [lockedAutomatic],
            locks: [assignmentLock]);

        ScheduleDraftValidationResult result = draft.ReplaceAutomaticGeneration([], []);

        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(result.Value);
        Assert.Equal(lockedAutomatic, Assert.Single(updated.Assignments));
        Assert.Equal(assignmentLock, Assert.Single(updated.AssignmentLocks));
    }

    [Fact]
    public void ReplaceAutomaticGenerationRejectsNonAutomaticReplacement()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleDraft draft = context.CreateDraft();
        ScheduleAssignment serviceManagementAssignment =
            context.CreateNormalAssignment(
                FirstEmployeeId,
                context.Period.StartMonday,
                InitialShiftTypeCatalog.EarlyShift,
                AssignmentOrigin.ServiceManagement);

        ScheduleDraftValidationResult result = draft.ReplaceAutomaticGeneration(
            [serviceManagementAssignment],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ScheduleDraftValidationCode.ReplacementAssignmentMustBeAutomatic,
            Assert.Single(result.Errors).Code);
        Assert.Empty(draft.Assignments);
        Assert.Equal(1, draft.Version.Value);
    }

    [Fact]
    public void ReplaceAutomaticGenerationRejectsConflictWithoutChangingOriginal()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment protectedAssignment = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        ScheduleDraft draft = context.CreateDraft(assignments: [protectedAssignment]);
        GeneratedDayOffMarker conflictingMarker = new(
            FirstEmployeeId,
            context.Period.StartMonday);

        ScheduleDraftValidationResult result = draft.ReplaceAutomaticGeneration(
            [],
            [conflictingMarker]);

        Assert.False(result.IsSuccess);
        Assert.Contains(
            result.Errors,
            error => error.Code
                == ScheduleDraftValidationCode.GeneratedDayOffConflictsAssignment);
        Assert.Equal(protectedAssignment, Assert.Single(draft.Assignments));
        Assert.Empty(draft.GeneratedDayOffMarkers);
        Assert.Equal(1, draft.Version.Value);
    }

    [Fact]
    public void ReplaceAutomaticGenerationRejectsExhaustedVersion()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleDraft draft = Assert.IsType<ScheduleDraft>(
            context.CreateDraftResult(version: int.MaxValue).Value);

        ScheduleDraftValidationResult result = draft.ReplaceAutomaticGeneration([], []);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ScheduleDraftValidationCode.VersionCannotAdvance,
            Assert.Single(result.Errors).Code);
        Assert.Equal(int.MaxValue, draft.Version.Value);
    }

    [Fact]
    public void DiscardAutomaticGenerationRemovesAllAutomaticValuesAndTheirLocks()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment serviceManagement = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.ServiceManagement);
        ScheduleAssignment manual = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday.AddDays(1),
            InitialShiftTypeCatalog.LateShift,
            AssignmentOrigin.ManualEdit);
        ScheduleAssignment automatic = context.CreateNormalAssignment(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(2),
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.AutomaticGeneration);
        AssignmentLock serviceManagementLock = new(serviceManagement.Id);
        AssignmentLock automaticLock = new(automatic.Id);
        GeneratedDayOffMarker marker = new(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(3));
        AvailabilityEntry vacation = CreateAvailabilityEntry(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(4),
            AvailabilityEntryKind.Vacation);
        AvailabilityEntry sickness = CreateAvailabilityEntry(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(5),
            AvailabilityEntryKind.Sickness);
        AvailabilityEntry fixedDayOff = CreateAvailabilityEntry(
            SecondEmployeeId,
            context.Period.StartMonday.AddDays(6),
            AvailabilityEntryKind.FixedDayOff);
        ScheduleDraft draft = context.CreateDraft(
            availabilityEntries: [vacation, sickness, fixedDayOff],
            assignments: [serviceManagement, manual, automatic],
            markers: [marker],
            locks: [serviceManagementLock, automaticLock]);

        ScheduleDraftValidationResult result = draft.DiscardAutomaticGeneration();

        ScheduleDraft updated = Assert.IsType<ScheduleDraft>(result.Value);
        Assert.Equal(draft.Version.Value + 1, updated.Version.Value);
        Assert.Equal([serviceManagement, manual], updated.Assignments);
        Assert.Equal(serviceManagementLock, Assert.Single(updated.AssignmentLocks));
        Assert.Empty(updated.GeneratedDayOffMarkers);
        Assert.Equal(
            [vacation, sickness, fixedDayOff],
            updated.AvailabilityEntries.Entries);
        Assert.Same(draft.DemandSlots, updated.DemandSlots);
        Assert.Same(draft.Period, updated.Period);
        Assert.Contains(automatic, draft.Assignments);
        Assert.Equal(marker, Assert.Single(draft.GeneratedDayOffMarkers));
    }

    [Fact]
    public void DiscardAutomaticGenerationRejectsExhaustedVersionWithoutMutation()
    {
        SchedulingTestContext context = SchedulingTestContext.Create();
        ScheduleAssignment automatic = context.CreateNormalAssignment(
            FirstEmployeeId,
            context.Period.StartMonday,
            InitialShiftTypeCatalog.EarlyShift,
            AssignmentOrigin.AutomaticGeneration);
        ScheduleDraft draft = Assert.IsType<ScheduleDraft>(
            context.CreateDraftResult(
                version: int.MaxValue,
                assignments: [automatic]).Value);

        ScheduleDraftValidationResult result = draft.DiscardAutomaticGeneration();

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ScheduleDraftValidationCode.VersionCannotAdvance,
            Assert.Single(result.Errors).Code);
        Assert.Equal(automatic, Assert.Single(draft.Assignments));
        Assert.Equal(int.MaxValue, draft.Version.Value);
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
