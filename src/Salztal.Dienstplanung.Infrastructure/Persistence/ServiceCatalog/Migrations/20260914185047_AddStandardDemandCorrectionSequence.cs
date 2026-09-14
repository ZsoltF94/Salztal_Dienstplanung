using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddStandardDemandCorrectionSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StandardStaffingDemandRevisions_DayOfWeek_WorkLocationId_ShiftTypeId_EffectiveFromMonday",
                table: "StandardStaffingDemandRevisions");

            migrationBuilder.AddColumn<int>(
                name: "CorrectionSequence",
                table: "StandardStaffingDemandRevisions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_StandardStaffingDemandRevisions_DayOfWeek_WorkLocationId_ShiftTypeId_EffectiveFromMonday_CorrectionSequence",
                table: "StandardStaffingDemandRevisions",
                columns: new[] { "DayOfWeek", "WorkLocationId", "ShiftTypeId", "EffectiveFromMonday", "CorrectionSequence" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StandardStaffingDemandRevisions_CorrectionSequence",
                table: "StandardStaffingDemandRevisions",
                sql: "CorrectionSequence > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StandardStaffingDemandRevisions_DayOfWeek_WorkLocationId_ShiftTypeId_EffectiveFromMonday_CorrectionSequence",
                table: "StandardStaffingDemandRevisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StandardStaffingDemandRevisions_CorrectionSequence",
                table: "StandardStaffingDemandRevisions");

            migrationBuilder.DropColumn(
                name: "CorrectionSequence",
                table: "StandardStaffingDemandRevisions");

            migrationBuilder.CreateIndex(
                name: "IX_StandardStaffingDemandRevisions_DayOfWeek_WorkLocationId_ShiftTypeId_EffectiveFromMonday",
                table: "StandardStaffingDemandRevisions",
                columns: new[] { "DayOfWeek", "WorkLocationId", "ShiftTypeId", "EffectiveFromMonday" },
                unique: true);
        }
    }
}
