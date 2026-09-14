namespace Salztal.Dienstplanung.Application.StaffingDemands;

public sealed record RemoveStaffingDemandDateExceptionRequest(
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    Guid ExpectedExceptionId);
