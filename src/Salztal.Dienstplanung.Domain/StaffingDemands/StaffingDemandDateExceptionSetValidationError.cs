namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed record StaffingDemandDateExceptionSetValidationError(
    StaffingDemandDateExceptionSetValidationCode Code,
    StaffingDemandDateKey Key);
