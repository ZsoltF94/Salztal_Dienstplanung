using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RenameJointOptimizationDurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AutomaticScheduleRuns_Durations",
                table: "AutomaticScheduleRuns");

            migrationBuilder.RenameColumn(
                name: "NonAuxiliarySolveDurationTicks",
                table: "AutomaticScheduleRuns",
                newName: "OptimizationDurationTicks");

            migrationBuilder.RenameColumn(
                name: "AuxiliarySolveDurationTicks",
                table: "AutomaticScheduleRuns",
                newName: "LegacyPhaseDurationTicks");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AutomaticScheduleRuns_Durations",
                table: "AutomaticScheduleRuns",
                sql: "ModelBuildDurationTicks >= 0 AND OptimizationDurationTicks >= 0 AND LegacyPhaseDurationTicks >= 0 AND ResultMappingDurationTicks >= 0 AND TotalDurationTicks >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AutomaticScheduleRuns_Durations",
                table: "AutomaticScheduleRuns");

            migrationBuilder.RenameColumn(
                name: "OptimizationDurationTicks",
                table: "AutomaticScheduleRuns",
                newName: "NonAuxiliarySolveDurationTicks");

            migrationBuilder.RenameColumn(
                name: "LegacyPhaseDurationTicks",
                table: "AutomaticScheduleRuns",
                newName: "AuxiliarySolveDurationTicks");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AutomaticScheduleRuns_Durations",
                table: "AutomaticScheduleRuns",
                sql: "ModelBuildDurationTicks >= 0 AND NonAuxiliarySolveDurationTicks >= 0 AND AuxiliarySolveDurationTicks >= 0 AND ResultMappingDurationTicks >= 0 AND TotalDurationTicks >= 0");
        }
    }
}
