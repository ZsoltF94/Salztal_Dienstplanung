namespace Salztal.Dienstplanung.Application.ServiceCatalog;

public interface IServiceCatalogReader
{
    public Task<ServiceCatalogData> LoadAsync(CancellationToken cancellationToken);
}
