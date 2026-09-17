using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Scheduling;

internal static class SchedulingEntityConfigurations
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ConfigureDraft(modelBuilder.Entity<ScheduleDraftEntity>());
        ConfigurePeriodDay(modelBuilder.Entity<SchedulePeriodDayEntity>());
        ConfigureDemandSlot(modelBuilder.Entity<ScheduleDemandSlotEntity>());
        ConfigureAvailability(modelBuilder.Entity<ScheduleAvailabilityEntryEntity>());
        ConfigureAssignment(modelBuilder.Entity<ScheduleAssignmentEntity>());
        ConfigureSegment(modelBuilder.Entity<ScheduleAssignmentSegmentEntity>());
        ConfigureCoverage(modelBuilder.Entity<ScheduleDemandCoverageEntity>());
        ConfigureDayOff(modelBuilder.Entity<ScheduleGeneratedDayOffEntity>());
        ConfigureLock(modelBuilder.Entity<ScheduleAssignmentLockEntity>());
        ConfigureSnapshot(modelBuilder.Entity<PlanningSnapshotEntity>());
        ConfigureSnapshotComponent(modelBuilder.Entity<PlanningSnapshotComponentEntity>());
        ConfigureAutomaticRun(modelBuilder.Entity<AutomaticScheduleRunEntity>());
    }

    private static void ConfigureDraft(EntityTypeBuilder<ScheduleDraftEntity> builder)
    {
        builder.ToTable("ScheduleDrafts", table =>
        {
            table.HasCheckConstraint("CK_ScheduleDrafts_Version", "Version > 0");
            table.HasCheckConstraint(
                "CK_ScheduleDrafts_Period",
                "julianday(EndSunday) - julianday(StartMonday) = 20");
        });
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => entity.StartMonday).IsUnique();
    }

    private static void ConfigurePeriodDay(
        EntityTypeBuilder<SchedulePeriodDayEntity> builder)
    {
        builder.ToTable("SchedulePeriodDays");
        builder.HasKey(entity => entity.Date);
        builder.HasIndex(entity => entity.DraftId);
        builder.HasOne<ScheduleDraftEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureDemandSlot(
        EntityTypeBuilder<ScheduleDemandSlotEntity> builder)
    {
        builder.ToTable("ScheduleDemandSlots", table =>
        {
            table.HasCheckConstraint("CK_ScheduleDemandSlots_Ordinal", "Ordinal > 0");
            table.HasCheckConstraint(
                "CK_ScheduleDemandSlots_Time",
                "ActualEnd > ActualStart");
        });
        builder.HasKey(entity => new { entity.DraftId, entity.SlotKey });
        builder.Property(entity => entity.SlotKey).HasMaxLength(180);
        builder.HasOne<ScheduleDraftEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAvailability(
        EntityTypeBuilder<ScheduleAvailabilityEntryEntity> builder)
    {
        builder.ToTable("ScheduleAvailabilityEntries", table =>
            table.HasCheckConstraint(
                "CK_ScheduleAvailabilityEntries_Kind",
                "Kind >= 0 AND Kind <= 2"));
        builder.HasKey(entity => new
        {
            entity.DraftId,
            entity.EmployeeId,
            entity.Date,
        });
        builder.HasOne<ScheduleDraftEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAssignment(
        EntityTypeBuilder<ScheduleAssignmentEntity> builder)
    {
        builder.ToTable("ScheduleAssignments", table =>
        {
            table.HasCheckConstraint(
                "CK_ScheduleAssignments_Kind",
                "Kind >= 0 AND Kind <= 4");
            table.HasCheckConstraint(
                "CK_ScheduleAssignments_Origin",
                "Origin >= 0 AND Origin <= 2");
            table.HasCheckConstraint(
                "CK_ScheduleAssignments_WorkMinutes",
                "WorkMinutes > 0");
        });
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new
        {
            entity.DraftId,
            entity.EmployeeId,
            entity.Date,
        }).IsUnique();
        builder.HasOne<ScheduleDraftEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSegment(
        EntityTypeBuilder<ScheduleAssignmentSegmentEntity> builder)
    {
        builder.ToTable("ScheduleAssignmentSegments", table =>
        {
            table.HasCheckConstraint(
                "CK_ScheduleAssignmentSegments_Sequence",
                "Sequence >= 0");
            table.HasCheckConstraint(
                "CK_ScheduleAssignmentSegments_Time",
                "ActualEnd > ActualStart AND WorkMinutes > 0");
        });
        builder.HasKey(entity => new { entity.AssignmentId, entity.Sequence });
        builder.Property(entity => entity.AnchorSlotKey).HasMaxLength(180);
        builder.HasOne<ScheduleAssignmentEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ScheduleDemandSlotEntity>()
            .WithMany()
            .HasForeignKey(entity => new { entity.DraftId, entity.AnchorSlotKey })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCoverage(
        EntityTypeBuilder<ScheduleDemandCoverageEntity> builder)
    {
        builder.ToTable("ScheduleDemandCoverages", table =>
        {
            table.HasCheckConstraint(
                "CK_ScheduleDemandCoverages_Sequence",
                "Sequence >= 0");
            table.HasCheckConstraint(
                "CK_ScheduleDemandCoverages_Kind",
                "Kind >= 0 AND Kind <= 1");
            table.HasCheckConstraint(
                "CK_ScheduleDemandCoverages_Time",
                "CoveredEnd > CoveredStart AND CoveredMinutes > 0");
        });
        builder.HasKey(entity => new { entity.AssignmentId, entity.Sequence });
        builder.Property(entity => entity.SlotKey).HasMaxLength(180);
        builder.HasOne<ScheduleAssignmentEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ScheduleDemandSlotEntity>()
            .WithMany()
            .HasForeignKey(entity => new { entity.DraftId, entity.SlotKey })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDayOff(
        EntityTypeBuilder<ScheduleGeneratedDayOffEntity> builder)
    {
        builder.ToTable("ScheduleGeneratedDaysOff");
        builder.HasKey(entity => new
        {
            entity.DraftId,
            entity.EmployeeId,
            entity.Date,
        });
        builder.HasOne<ScheduleDraftEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureLock(
        EntityTypeBuilder<ScheduleAssignmentLockEntity> builder)
    {
        builder.ToTable("ScheduleAssignmentLocks");
        builder.HasKey(entity => new { entity.DraftId, entity.AssignmentId });
        builder.HasOne<ScheduleAssignmentEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSnapshot(
        EntityTypeBuilder<PlanningSnapshotEntity> builder)
    {
        builder.ToTable("PlanningSnapshots", table =>
        {
            table.HasCheckConstraint(
                "CK_PlanningSnapshots_DraftVersion",
                "DraftVersion > 0");
            table.HasCheckConstraint(
                "CK_PlanningSnapshots_Period",
                "julianday(PeriodSunday) - julianday(PeriodMonday) = 20");
            table.HasCheckConstraint(
                "CK_PlanningSnapshots_RuleCatalogVersion",
                "RuleCatalogVersion > 0");
            table.HasCheckConstraint(
                "CK_PlanningSnapshots_HistoryCompleteness",
                "HistoryCompleteness >= 0 AND HistoryCompleteness <= 2");
        });
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => entity.DraftId).IsUnique();
        builder.HasOne<ScheduleDraftEntity>()
            .WithOne()
            .HasForeignKey<PlanningSnapshotEntity>(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSnapshotComponent(
        EntityTypeBuilder<PlanningSnapshotComponentEntity> builder)
    {
        builder.ToTable("PlanningSnapshotComponents", table =>
        {
            table.HasCheckConstraint(
                "CK_PlanningSnapshotComponents_Kind",
                "Kind >= 0 AND Kind <= 10");
            table.HasCheckConstraint(
                "CK_PlanningSnapshotComponents_Sequence",
                "Sequence >= 0");
        });
        builder.HasKey(entity => new
        {
            entity.SnapshotId,
            entity.Kind,
            entity.Sequence,
        });
        builder.Property(entity => entity.Payload).IsRequired();
        builder.HasOne<PlanningSnapshotEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAutomaticRun(
        EntityTypeBuilder<AutomaticScheduleRunEntity> builder)
    {
        builder.ToTable("AutomaticScheduleRuns", table =>
        {
            table.HasCheckConstraint(
                "CK_AutomaticScheduleRuns_ResultStatus",
                "ResultStatus >= 0 AND ResultStatus <= 1");
            table.HasCheckConstraint(
                "CK_AutomaticScheduleRuns_TimeLimit",
                "TimeLimitTicks > 0");
            table.HasCheckConstraint(
                "CK_AutomaticScheduleRuns_Durations",
                "ModelBuildDurationTicks >= 0 "
                + "AND OptimizationDurationTicks >= 0 "
                + "AND LegacyPhaseDurationTicks >= 0 "
                + "AND ResultMappingDurationTicks >= 0 "
                + "AND TotalDurationTicks >= 0");
        });
        builder.HasKey(entity => entity.DraftId);
        builder.Property(entity => entity.SolverName).HasMaxLength(200);
        builder.Property(entity => entity.SolverVersion).HasMaxLength(100);
        builder.Property(entity => entity.SettingsPayload).IsRequired();
        builder.Property(entity => entity.ObjectivePayload).IsRequired();
        builder.HasOne<ScheduleDraftEntity>()
            .WithOne()
            .HasForeignKey<AutomaticScheduleRunEntity>(entity => entity.DraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
