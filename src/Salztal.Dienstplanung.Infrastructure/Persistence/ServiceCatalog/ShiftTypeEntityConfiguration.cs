using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salztal.Dienstplanung.Domain.ShiftTypes;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ShiftTypeEntityConfiguration : IEntityTypeConfiguration<ShiftTypeEntity>
{
    public void Configure(EntityTypeBuilder<ShiftTypeEntity> builder)
    {
        builder.ToTable(
            "ShiftTypes",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_ShiftTypes_Name_NotEmpty",
                    "length(trim(Name)) > 0");
                table.HasCheckConstraint(
                    "CK_ShiftTypes_StandardTime",
                    "StandardStartMinutes >= 0 AND StandardStartMinutes < 1440 "
                    + "AND StandardEndMinutes > StandardStartMinutes AND StandardEndMinutes < 1440 "
                    + "AND StandardStartMinutes % 30 = 0 AND StandardEndMinutes % 30 = 0");
            });

        builder.HasKey(shiftType => shiftType.Id);
        builder.Property(shiftType => shiftType.Name).IsRequired();
        builder.Property(shiftType => shiftType.DisplayKind).HasConversion<int>();

        builder
            .HasOne(shiftType => shiftType.WorkLocation)
            .WithMany()
            .HasForeignKey(shiftType => shiftType.WorkLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            InitialShiftTypeCatalog.All.Select(
                shiftType => new ShiftTypeEntity
                {
                    Id = shiftType.Id.Value,
                    Name = shiftType.Name.Value,
                    WorkLocationId = shiftType.WorkLocationId.Value,
                    DisplayKind = shiftType.Display.Kind,
                    Abbreviation = shiftType.Display.Abbreviation,
                    StandardStartMinutes = ToMinutes(shiftType.StandardTime.Start),
                    StandardEndMinutes = ToMinutes(shiftType.StandardTime.End),
                }));
    }

    private static int ToMinutes(TimeOnly value)
    {
        return (value.Hour * 60) + value.Minute;
    }
}
