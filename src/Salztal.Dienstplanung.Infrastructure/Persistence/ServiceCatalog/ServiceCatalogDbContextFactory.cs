using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ServiceCatalogDbContextFactory(string databasePath)
{
    private readonly string _connectionString = CreateConnectionString(databasePath);

    public ServiceCatalogDbContext Create()
    {
        DbContextOptions<ServiceCatalogDbContext> options =
            new DbContextOptionsBuilder<ServiceCatalogDbContext>()
                .UseSqlite(_connectionString)
                .Options;

        return new ServiceCatalogDbContext(options);
    }

    private static string CreateConnectionString(string databasePath)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Pooling = false,
        }.ToString();
    }
}
