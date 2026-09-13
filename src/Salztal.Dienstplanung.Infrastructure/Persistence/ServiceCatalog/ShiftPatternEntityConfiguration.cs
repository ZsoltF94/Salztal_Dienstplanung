using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

internal sealed class ShiftPatternEntityConfiguration : IEntityTypeConfiguration<ShiftPatternEntity>
{
    public void Configure(EntityTypeBuilder<ShiftPatternEntity> builder)
    {
        builder.ToTable(
            "ShiftPatterns",
            table => table.HasCheckConstraint(
                "CK_ShiftPatterns_DifferentSegments",
                "FirstShiftTypeId <> SecondShiftTypeId"));

        builder.HasKey(pattern => pattern.Id);
        builder.HasIndex(pattern => pattern.Kind).IsUnique();
        builder.HasIndex(pattern => pattern.DisplayCode).IsUnique();
        builder.Property(pattern => pattern.Kind).HasConversion<int>();
        builder.Property(pattern => pattern.DisplayCode).IsRequired();
        builder.Property(pattern => pattern.AllowedDay).HasConversion<int?>();
        builder.Property(pattern => pattern.SwitchRule).HasConversion<int?>();

        builder
            .HasOne(pattern => pattern.FirstShiftType)
            .WithMany()
            .HasForeignKey(pattern => pattern.FirstShiftTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(pattern => pattern.SecondShiftType)
            .WithMany()
            .HasForeignKey(pattern => pattern.SecondShiftTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(CreateSplitShiftSeed(), CreateReliefShiftSeed());
    }

    private static ShiftPatternEntity CreateSplitShiftSeed()
    {
        SplitShiftPattern pattern = InitialShiftPatternCatalog.SplitShift;

        return new ShiftPatternEntity
        {
            Id = pattern.Id.Value,
            Kind = StoredShiftPatternKind.SplitShift,
            DisplayCode = pattern.DisplayCode,
            HasInterruption = true,
            FirstShiftTypeId = pattern.FirstShiftTypeId.Value,
            SecondShiftTypeId = pattern.SecondShiftTypeId.Value,
        };
    }

    private static ShiftPatternEntity CreateReliefShiftSeed()
    {
        ReliefShiftPattern pattern = InitialShiftPatternCatalog.ReliefShift;

        return new ShiftPatternEntity
        {
            Id = pattern.Id.Value,
            Kind = StoredShiftPatternKind.ReliefShift,
            DisplayCode = pattern.DisplayCode,
            DisplayColorCode = pattern.DisplayColorCode,
            AllowedDay = pattern.AllowedDay,
            SwitchRule = pattern.SwitchRule,
            HasInterruption = pattern.HasInterruption,
            FirstShiftTypeId = pattern.FirstShiftTypeId.Value,
            SecondShiftTypeId = pattern.SecondShiftTypeId.Value,
        };
    }
}
