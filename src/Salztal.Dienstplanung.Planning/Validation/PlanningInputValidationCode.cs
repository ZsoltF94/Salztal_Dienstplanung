namespace Salztal.Dienstplanung.Planning.Validation;

internal enum PlanningInputValidationCode
{
    UnknownCatalogVersion,
    UnknownRule,
    RuleNotTranslated,
    DuplicateRule,
    RuleDefinitionMismatch,
    InvalidSnapshotIdentity,
    InvalidPeriod,
    InvalidEmployeeType,
    InvalidEmployee,
    InvalidServiceCatalog,
    InvalidAvailability,
    InvalidDemandSlot,
    InvalidHistory,
    ProtectedAssignmentConflict,
    ServiceManagementNotReady,
}
