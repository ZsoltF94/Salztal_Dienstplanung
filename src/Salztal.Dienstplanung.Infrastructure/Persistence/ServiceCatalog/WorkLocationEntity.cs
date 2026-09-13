namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class WorkLocationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ColorCode { get; set; } = string.Empty;
}
