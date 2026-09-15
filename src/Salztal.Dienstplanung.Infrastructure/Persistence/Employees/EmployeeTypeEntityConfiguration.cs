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
                    "CK_EmployeeTypes_AbsencePolicy",
                    "(AllowsVacationAndSickness = 1 "
                    + "AND AbsenceDayValueMinutes IS NOT NULL "
                    + "AND AbsenceDayValueMinutes > 0 "
                    + "AND AbsenceDayValueMinutes <= 1440) "
                    + "OR (AllowsVacationAndSickness = 0 "
                    + "AND AbsenceDayValueMinutes IS NULL)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_PlanningRole",
                    "PlanningRole IN (0, 1, 2)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_ManualSuggestionPriority",
                    "ManualSuggestionPriority IN (0, 1)");
                table.HasCheckConstraint(
                    "CK_EmployeeTypes_PlanningPolicy",
                    "(PlanningRole IN (0, 2) "
                    + "AND AllowsAutomaticAssignment = 1 "
                    + "AND RequiresWeeklyManualAssignment = 0 "
                    + "AND PreservesManualAssignmentsOnGeneration = 0 "
                    + "AND ManualSuggestionPriority = 0) "
                    + "OR (PlanningRole = 1 "
                    + "AND AllowsAutomaticAssignment = 0 "
                    + "AND RequiresWeeklyManualAssignment = 1 "
                    + "AND PreservesManualAssignmentsOnGeneration = 1 "
                    + "AND ManualSuggestionPriority = 1)");
            });

        builder.HasKey(employeeType => employeeType.Id);
        builder.HasIndex(employeeType => employeeType.Code).IsUnique();
        builder.Property(employeeType => employeeType.Code).IsRequired().UseCollation("NOCASE");
        builder.Property(employeeType => employeeType.Name).IsRequired();
        builder.Property(employeeType => employeeType.PlanningRole).HasConversion<int>();
        builder.Property(employeeType => employeeType.ManualSuggestionPriority).HasConversion<int>();

        builder.HasData(
            EmployeeTypePersistenceSeedCatalog.All.Select(
                employeeType => new EmployeeTypeEntity
                {
                    Id = employeeType.Id.Value,
                    Code = employeeType.Code.Value,
                    Name = employeeType.Name.Value,
                    WeeklyWorkTargetMinutes = employeeType.WeeklyWorkTarget.Minutes,
                    AllowsVacationAndSickness =
                        employeeType.AbsencePolicy.AllowsVacationAndSickness,
                    AbsenceDayValueMinutes = employeeType.AbsencePolicy.DayValue?.Minutes,
                    PlanningRole = employeeType.PlanningPolicy.Role,
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
