using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ServiceCatalogDbContext(DbContextOptions<ServiceCatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<WorkLocationEntity> WorkLocations => Set<WorkLocationEntity>();

    public DbSet<ShiftTypeEntity> ShiftTypes => Set<ShiftTypeEntity>();

    public DbSet<ShiftPatternEntity> ShiftPatterns => Set<ShiftPatternEntity>();

    public DbSet<EmployeeTypeEntity> EmployeeTypes => Set<EmployeeTypeEntity>();

    public DbSet<EmployeeTypeShiftEligibilityEntity> EmployeeTypeShiftEligibilities =>
        Set<EmployeeTypeShiftEligibilityEntity>();

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();

    public DbSet<StandardStaffingDemandRevisionEntity> StandardStaffingDemandRevisions =>
        Set<StandardStaffingDemandRevisionEntity>();

    public DbSet<StaffingDemandDateExceptionEntity> StaffingDemandDateExceptions =>
        Set<StaffingDemandDateExceptionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WorkLocationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftPatternEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTypeShiftEligibilityEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEntityConfiguration());
        modelBuilder.ApplyConfiguration(
            new StandardStaffingDemandRevisionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new StaffingDemandDateExceptionEntityConfiguration());
    }
}
