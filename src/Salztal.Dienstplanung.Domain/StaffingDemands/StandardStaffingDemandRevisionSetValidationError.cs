namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StandardStaffingDemandRevisionSetValidationError(
    StandardStaffingDemandRevisionSetValidationCode Code,
    StandardStaffingDemandKey Key,
    DateOnly EffectiveFromMonday);
