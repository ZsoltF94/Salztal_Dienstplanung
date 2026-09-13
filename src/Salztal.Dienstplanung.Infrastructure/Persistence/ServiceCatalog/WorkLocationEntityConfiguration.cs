using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class WorkLocationEntityConfiguration : IEntityTypeConfiguration<WorkLocationEntity>
{
    public void Configure(EntityTypeBuilder<WorkLocationEntity> builder)
    {
        builder.ToTable(
            "WorkLocations",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_WorkLocations_Name_NotEmpty",
                    "length(trim(Name)) > 0");
                table.HasCheckConstraint(
                    "CK_WorkLocations_ColorCode_NotEmpty",
                    "length(trim(ColorCode)) > 0");
            });

        builder.HasKey(location => location.Id);
        builder.Property(location => location.Name).IsRequired();
        builder.Property(location => location.ColorCode).IsRequired();

        builder.HasData(
            InitialWorkLocationCatalog.All.Select(
                location => new WorkLocationEntity
                {
                    Id = location.Id.Value,
                    Name = location.Name.Value,
                    ColorCode = location.Color.Code,
                }));
    }
}
