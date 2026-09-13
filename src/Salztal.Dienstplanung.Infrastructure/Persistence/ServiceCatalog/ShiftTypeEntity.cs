using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ShiftTypeEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid WorkLocationId { get; set; }

    public ShiftTypeDisplayKind DisplayKind { get; set; }

    public string? Abbreviation { get; set; }

    public int StandardStartMinutes { get; set; }

    public int StandardEndMinutes { get; set; }

    public WorkLocationEntity WorkLocation { get; set; } = null!;
}
