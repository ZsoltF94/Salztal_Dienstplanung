using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.StaffingDemands;

internal sealed class StaffingDemandDateExceptionEntityConfiguration
    : IEntityTypeConfiguration<StaffingDemandDateExceptionEntity>
{
    public void Configure(EntityTypeBuilder<StaffingDemandDateExceptionEntity> builder)
    {
        builder.ToTable(
            "StaffingDemandDateExceptions",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_StaffingDemandDateExceptions_Kind",
                    "Kind IN (0, 1, 2)");
                table.HasCheckConstraint(
                    "CK_StaffingDemandDateExceptions_ValueShape",
                    CreateValueShapeConstraint());
            });

        builder.HasKey(exception => exception.Id);
        builder.Property(exception => exception.Kind).HasConversion<int>();
        builder
            .HasIndex(exception => new
            {
                exception.Date,
                exception.WorkLocationId,
                exception.ShiftTypeId,
            })
            .IsUnique();

        builder
            .HasOne(exception => exception.WorkLocation)
            .WithMany()
            .HasForeignKey(exception => exception.WorkLocationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(exception => exception.ShiftType)
            .WithMany()
            .HasForeignKey(exception => exception.ShiftTypeId)
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
