using Microsoft.EntityFrameworkCore;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ServiceCatalogDbContext(DbContextOptions<ServiceCatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<WorkLocationEntity> WorkLocations => Set<WorkLocationEntity>();

    public DbSet<ShiftTypeEntity> ShiftTypes => Set<ShiftTypeEntity>();

    public DbSet<ShiftPatternEntity> ShiftPatterns => Set<ShiftPatternEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WorkLocationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftPatternEntityConfiguration());
    }
}
