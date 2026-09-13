using Microsoft.EntityFrameworkCore.Design;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ServiceCatalogDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ServiceCatalogDbContext>
{
    public ServiceCatalogDbContext CreateDbContext(string[] args)
    {
        string databasePath = Path.Combine(
            Path.GetTempPath(),
            "salztal-dienstplanung-design.db");

        return new ServiceCatalogDbContextFactory(databasePath).Create();
    }
}
