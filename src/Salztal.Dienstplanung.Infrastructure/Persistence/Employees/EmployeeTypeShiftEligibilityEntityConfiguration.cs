using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal sealed class EmployeeTypeShiftEligibilityEntityConfiguration
    : IEntityTypeConfiguration<EmployeeTypeShiftEligibilityEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeTypeShiftEligibilityEntity> builder)
    {
        builder.ToTable(
            "EmployeeTypeShiftEligibilities",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_EmployeeTypeShiftEligibilities_Target",
                    "(TargetKind = 0 AND ShiftTypeId IS NOT NULL AND ShiftPatternId IS NULL) "
                    + "OR (TargetKind = 1 AND ShiftTypeId IS NULL AND ShiftPatternId IS NOT NULL)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypeShiftEligibilities_TargetKind",
                    "TargetKind IN (0, 1)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypeShiftEligibilities_Mode",
                    "Mode IN (0, 1)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypeShiftEligibilities_Activation",
                    "Activation IN (0, 1)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypeShiftEligibilities_ShiftTypeActivation",
                    "TargetKind <> 0 OR Activation = 0");
            });

        builder.HasKey(eligibility => eligibility.Id);
        builder.Property(eligibility => eligibility.Id).ValueGeneratedNever();
        builder.Property(eligibility => eligibility.TargetKind).HasConversion<int>();
        builder.Property(eligibility => eligibility.Mode).HasConversion<int>();
        builder.Property(eligibility => eligibility.Activation).HasConversion<int>();
        builder
            .HasIndex(eligibility => new
            {
                eligibility.EmployeeTypeId,
                eligibility.ShiftTypeId,
                eligibility.Mode,
            })
            .IsUnique()
            .HasFilter("TargetKind = 0");
        builder
            .HasIndex(eligibility => new
            {
                eligibility.EmployeeTypeId,
                eligibility.ShiftPatternId,
                eligibility.Mode,
                eligibility.Activation,
            })
            .IsUnique()
            .HasFilter("TargetKind = 1");

        builder
            .HasOne(eligibility => eligibility.EmployeeType)
            .WithMany()
            .HasForeignKey(eligibility => eligibility.EmployeeTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(eligibility => eligibility.ShiftType)
            .WithMany()
            .HasForeignKey(eligibility => eligibility.ShiftTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(eligibility => eligibility.ShiftPattern)
            .WithMany()
            .HasForeignKey(eligibility => eligibility.ShiftPatternId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(CreateSeedData());
    }

    private static IEnumerable<EmployeeTypeShiftEligibilityEntity> CreateSeedData()
    {
        int id = 0;

        foreach (EmployeeType employeeType in InitialEmployeeTypeCatalog.All)
        {
            foreach (EmployeeTypeShiftEligibility eligibility in employeeType.ShiftEligibilities)
            {
                yield return new EmployeeTypeShiftEligibilityEntity
                {
                    Id = ++id,
                    EmployeeTypeId = employeeType.Id.Value,
                    TargetKind = eligibility.TargetKind,
                    ShiftTypeId = eligibility.ShiftTypeId?.Value,
                    ShiftPatternId = eligibility.ShiftPatternId?.Value,
                    Mode = eligibility.Mode,
                    Activation = eligibility.Activation,
                };
            }
        }
    }
}
