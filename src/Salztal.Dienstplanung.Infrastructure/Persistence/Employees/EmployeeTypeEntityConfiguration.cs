using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal sealed class EmployeeTypeEntityConfiguration : IEntityTypeConfiguration<EmployeeTypeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeTypeEntity> builder)
    {
        builder.ToTable(
            "EmployeeTypes",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_Code_NotEmpty",
                    "length(trim(Code)) > 0");
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_Name_NotEmpty",
                    "length(trim(Name)) > 0");
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_WeeklyWorkTargetMinutes",
                    "WeeklyWorkTargetMinutes > 0 AND WeeklyWorkTargetMinutes <= 10080");
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_ManualSuggestionPriority",
                    "ManualSuggestionPriority IN (0, 1)");
            });

        builder.HasKey(employeeType => employeeType.Id);
        builder.HasIndex(employeeType => employeeType.Code).IsUnique();
        builder.Property(employeeType => employeeType.Code).IsRequired();
        builder.Property(employeeType => employeeType.Name).IsRequired();
        builder.Property(employeeType => employeeType.ManualSuggestionPriority).HasConversion<int>();

        builder.HasData(
            InitialEmployeeTypeCatalog.All.Select(
                employeeType => new EmployeeTypeEntity
                {
                    Id = employeeType.Id.Value,
                    Code = employeeType.Code.Value,
                    Name = employeeType.Name.Value,
                    WeeklyWorkTargetMinutes = employeeType.WeeklyWorkTarget.Minutes,
                    AllowsAutomaticAssignment =
                        employeeType.PlanningPolicy.AllowsAutomaticAssignment,
                    RequiresWeeklyManualAssignment =
                        employeeType.PlanningPolicy.RequiresWeeklyManualAssignment,
                    PreservesManualAssignmentsOnGeneration =
                        employeeType.PlanningPolicy.PreservesManualAssignmentsOnGeneration,
                    ManualSuggestionPriority =
                        employeeType.PlanningPolicy.ManualSuggestionPriority,
                }));
    }
}
