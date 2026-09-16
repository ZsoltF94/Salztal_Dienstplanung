namespace Salztal.Dienstplanung.Domain.Scheduling;

public enum ScheduleDraftValidationCode
{
    IdentifierRequired,
    VersionMustBePositive,
    DuplicateAssignmentIdentifier,
    AssignmentOutsidePeriod,
    AssignmentReferencesUnknownDemandSlot,
    DuplicateEmployeeDateAssignment,
    AssignmentOnUnavailableDay,
    DuplicateDemandCoverage,
    GeneratedDayOffOutsidePeriod,
    DuplicateEmployeeDateDayState,
    GeneratedDayOffConflictsAvailabilityEntry,
    GeneratedDayOffConflictsAssignment,
    LockReferencesUnknownAssignment,
    DuplicateAssignmentLock,
}
