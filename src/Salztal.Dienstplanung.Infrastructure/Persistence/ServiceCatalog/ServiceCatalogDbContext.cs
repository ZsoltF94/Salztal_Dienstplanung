using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;
using Salztal.Dienstplanung.Infrastructure.Persistence.Employees;
using Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;
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

    public DbSet<AvailabilityEntryEntity> AvailabilityEntries =>
        Set<AvailabilityEntryEntity>();

    public DbSet<StandardStaffingDemandRevisionEntity> StandardStaffingDemandRevisions =>
        Set<StandardStaffingDemandRevisionEntity>();

    public DbSet<StaffingDemandDateExceptionEntity> StaffingDemandDateExceptions =>
        Set<StaffingDemandDateExceptionEntity>();

    public DbSet<ScheduleDraftEntity> ScheduleDrafts => Set<ScheduleDraftEntity>();

    public DbSet<SchedulePeriodDayEntity> SchedulePeriodDays =>
        Set<SchedulePeriodDayEntity>();

    public DbSet<ScheduleDemandSlotEntity> ScheduleDemandSlots =>
        Set<ScheduleDemandSlotEntity>();

    public DbSet<ScheduleAvailabilityEntryEntity> ScheduleAvailabilityEntries =>
        Set<ScheduleAvailabilityEntryEntity>();

    public DbSet<ScheduleAssignmentEntity> ScheduleAssignments =>
        Set<ScheduleAssignmentEntity>();

    public DbSet<ScheduleAssignmentSegmentEntity> ScheduleAssignmentSegments =>
        Set<ScheduleAssignmentSegmentEntity>();

    public DbSet<ScheduleDemandCoverageEntity> ScheduleDemandCoverages =>
        Set<ScheduleDemandCoverageEntity>();

    public DbSet<ScheduleGeneratedDayOffEntity> ScheduleGeneratedDaysOff =>
        Set<ScheduleGeneratedDayOffEntity>();

    public DbSet<ScheduleAssignmentLockEntity> ScheduleAssignmentLocks =>
        Set<ScheduleAssignmentLockEntity>();

    public DbSet<PlanningSnapshotEntity> PlanningSnapshots =>
        Set<PlanningSnapshotEntity>();

    public DbSet<PlanningSnapshotComponentEntity> PlanningSnapshotComponents =>
        Set<PlanningSnapshotComponentEntity>();

    public DbSet<AutomaticScheduleRunEntity> AutomaticScheduleRuns =>
        Set<AutomaticScheduleRunEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WorkLocationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftPatternEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTypeShiftEligibilityEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AvailabilityEntryEntityConfiguration());
        modelBuilder.ApplyConfiguration(
            new StandardStaffingDemandRevisionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new StaffingDemandDateExceptionEntityConfiguration());
        SchedulingEntityConfigurations.Apply(modelBuilder);
    }
}
