namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public enum StandardStaffingDemandRevisionSetValidationCode
{
    DuplicateKeyEffectiveMondayAndCorrectionSequence,
    NonContiguousCorrectionSequence,
    AdditionRequiresMissingStandard,
    ReplacementRequiresExistingStandard,
    RemovalRequiresExistingStandard,
}
