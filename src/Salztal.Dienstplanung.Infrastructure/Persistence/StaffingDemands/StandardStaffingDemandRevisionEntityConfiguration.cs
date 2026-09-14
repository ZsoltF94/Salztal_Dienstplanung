using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

internal sealed class StandardStaffingDemandRevisionEntityConfiguration
    : IEntityTypeConfiguration<StandardStaffingDemandRevisionEntity>
{
    public void Configure(EntityTypeBuilder<StandardStaffingDemandRevisionEntity> builder)
    {
        builder.ToTable(
            "StandardStaffingDemandRevisions",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_StandardStaffingDemandRevisions_DayOfWeek",
                    "DayOfWeek >= 0 AND DayOfWeek <= 6");
                table.HasCheckConstraint(
                    "CK_StandardStaffingDemandRevisions_EffectiveMonday",
                    "strftime('%w', EffectiveFromMonday) = '1'");
                table.HasCheckConstraint(
                    "CK_StandardStaffingDemandRevisions_Kind",
                    "Kind IN (0, 1, 2)");
                table.HasCheckConstraint(
                    "CK_StandardStaffingDemandRevisions_CorrectionSequence",
                    "CorrectionSequence > 0");
                table.HasCheckConstraint(
                    "CK_StandardStaffingDemandRevisions_ValueShape",
                    CreateValueShapeConstraint());
            });

        builder.HasKey(revision => revision.Id);
        builder.Property(revision => revision.DayOfWeek).HasConversion<int>();
        builder.Property(revision => revision.Kind).HasConversion<int>();
        builder.Property(revision => revision.CorrectionSequence).HasDefaultValue(1);
        builder
            .HasIndex(revision => new
            {
                revision.DayOfWeek,
                revision.WorkLocationId,
                revision.ShiftTypeId,
                revision.EffectiveFromMonday,
                revision.CorrectionSequence,
            })
            .IsUnique();

        builder
            .HasOne(revision => revision.WorkLocation)
            .WithMany()
            .HasForeignKey(revision => revision.WorkLocationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(revision => revision.ShiftType)
            .WithMany()
            .HasForeignKey(revision => revision.ShiftTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static string CreateValueShapeConstraint()
    {
        const string timesAreValid =
            "ActualStartMinutes >= 0 AND ActualStartMinutes < 1440 "
            + "AND ActualEndMinutes > ActualStartMinutes AND ActualEndMinutes < 1440 "
            + "AND ActualStartMinutes % 30 = 0 AND ActualEndMinutes % 30 = 0 "
            + "AND RequiredEmployeeCount > 0";

        return "(Kind = 2 AND ActualStartMinutes IS NULL AND ActualEndMinutes IS NULL "
            + "AND RequiredEmployeeCount IS NULL) OR "
            + "(Kind IN (0, 1) AND ActualStartMinutes IS NOT NULL "
            + "AND ActualEndMinutes IS NOT NULL AND RequiredEmployeeCount IS NOT NULL AND "
            + timesAreValid
            + ")";
    }
}
