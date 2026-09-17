using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Desktop.Features.Scheduling;

internal sealed record AutomaticScheduleGenerationContext(
    Guid DraftId,
    long Version,
    DateOnly PeriodMonday,
    Guid? PreparedSnapshotId,
    SchedulePreparationStatus PreparationStatus,
    bool IsServiceManagementReady,
    AcceptedAutomaticScheduleSnapshot? AcceptedAutomaticSchedule);
