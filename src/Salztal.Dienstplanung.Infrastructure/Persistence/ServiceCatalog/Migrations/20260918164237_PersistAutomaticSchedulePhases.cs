using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class PersistAutomaticSchedulePhases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhasesPayload",
                table: "AutomaticScheduleRuns",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhasesPayload",
                table: "AutomaticScheduleRuns");
        }
    }
}
