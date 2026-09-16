namespace Salztal.Dienstplanung.Domain.Scheduling;

public enum DemandSlotSetValidationCode
{
    DemandOutsidePeriod,
    UnknownWorkLocation,
    UnknownShiftType,
    DuplicateSlotIdentity,
}
