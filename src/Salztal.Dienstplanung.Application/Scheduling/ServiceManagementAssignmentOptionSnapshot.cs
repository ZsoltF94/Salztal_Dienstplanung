namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record ServiceManagementAssignmentOptionSnapshot(
    Guid EmployeeId,
    ServiceManagementAssignmentSelectionKind Kind,
    ScheduleDemandSlotSnapshot FirstSlot,
    ScheduleDemandSlotSnapshot? SecondSlot);
