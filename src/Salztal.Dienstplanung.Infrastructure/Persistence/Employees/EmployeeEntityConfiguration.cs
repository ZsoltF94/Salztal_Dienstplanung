using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal sealed class EmployeeEntityConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.ToTable(
            "Employees",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Employees_FirstName_NotEmpty",
                    "length(trim(FirstName)) > 0");
                table.HasCheckConstraint(
                    "CK_Employees_LastName_NotEmpty",
                    "length(trim(LastName)) > 0");
            });

        builder.HasKey(employee => employee.Id);
        builder.Property(employee => employee.FirstName).IsRequired();
        builder.Property(employee => employee.LastName).IsRequired();
        builder
            .HasIndex(employee => employee.EmployeeTypeId)
            .IsUnique()
            .HasFilter(
                "IsActive = 1 AND lower(EmployeeTypeId) = '"
                + InitialEmployeeTypeCatalog.Type1.Id.Value.ToString("D")
                + "'");

        builder
            .HasOne(employee => employee.EmployeeType)
            .WithMany()
            .HasForeignKey(employee => employee.EmployeeTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
