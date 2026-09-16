using Salztal.Dienstplanung.Application.Availabilities;

namespace Salztal.Dienstplanung.Application.Scheduling;

public enum ServiceManagementAssignmentSelectionKind
{
    NormalDemand,
    OfficeTime,
    SplitShiftPattern,
    ReliefShiftPattern,
}

public enum ScheduleReplacementConfirmation
{
    NotConfirmed,
    Confirmed,
}

public sealed record ScheduleDemandSlotSelection(
    Guid SourceId,
    ScheduleDemandSourceKindSnapshot SourceKind,
    DateOnly Date,
    Guid WorkLocationId,
    Guid ShiftTypeId,
    int Ordinal,
    TimeOnly ActualStart,
    TimeOnly ActualEnd);

public sealed record SetServiceManagementAssignmentRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday,
    Guid EmployeeId,
    ServiceManagementAssignmentSelectionKind Kind,
    ScheduleDemandSlotSelection FirstSlot,
    ScheduleDemandSlotSelection? SecondSlot,
    long? ExpectedDayEntryChangeVersion,
    ScheduleReplacementConfirmation DayEntryRemovalConfirmation);

public sealed record RemoveServiceManagementAssignmentRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday,
    Guid AssignmentId);

public sealed record ChangeScheduleDayEntryRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday,
    Guid EmployeeId,
    DateOnly Date,
    AvailabilityDayEntryKind Kind,
    long? ExpectedDayEntryChangeVersion,
    ScheduleReplacementConfirmation ReplacementConfirmation);

public sealed record RemoveScheduleDayEntryRequest(
    Guid DraftId,
    long ExpectedDraftVersion,
    DateOnly PeriodMonday,
    Guid EmployeeId,
    DateOnly Date,
    long ExpectedDayEntryChangeVersion,
    ScheduleReplacementConfirmation RemovalConfirmation);
