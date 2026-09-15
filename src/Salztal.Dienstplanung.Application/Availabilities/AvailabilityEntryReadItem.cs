using Salztal.Dienstplanung.Domain.Availabilities;

namespace Salztal.Dienstplanung.Application.Availabilities;

public sealed record AvailabilityEntryReadItem(
    AvailabilityEntry Entry,
    long ChangeVersion);
