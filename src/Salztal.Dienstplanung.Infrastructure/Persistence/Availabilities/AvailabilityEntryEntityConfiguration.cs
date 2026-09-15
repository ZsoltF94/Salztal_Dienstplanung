using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Availabilities;

internal sealed class AvailabilityEntryEntityConfiguration
    : IEntityTypeConfiguration<AvailabilityEntryEntity>
{
    public void Configure(EntityTypeBuilder<AvailabilityEntryEntity> builder)
    {
        builder.ToTable(
            "AvailabilityEntries",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_AvailabilityEntries_Kind",
                    "Kind IN (0, 1, 2)");
                table.HasCheckConstraint(
                    "CK_AvailabilityEntries_ChangeVersion",
                    "ChangeVersion > 0");
            });

        builder.HasKey(entry => new { entry.EmployeeId, entry.Date });
        builder.HasIndex(entry => entry.Date);
        builder
            .HasOne(entry => entry.Employee)
            .WithMany()
            .HasForeignKey(entry => entry.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
