using Salztal.Dienstplanung.Domain.Scheduling;

namespace Salztal.Dienstplanung.Application.Scheduling;

public sealed record ScheduleDraftHeader(
    ScheduleDraftId DraftId,
    ScheduleDraftVersion Version,
    SchedulePeriod Period);
