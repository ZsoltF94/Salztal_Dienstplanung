using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticScheduleRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomaticScheduleRuns",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SolverName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SolverVersion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ResultStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeLimitTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    ModelBuildDurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    NonAuxiliarySolveDurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    AuxiliarySolveDurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    ResultMappingDurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalDurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    SettingsPayload = table.Column<string>(type: "TEXT", nullable: false),
                    ObjectivePayload = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticScheduleRuns", x => x.DraftId);
                    table.CheckConstraint("CK_AutomaticScheduleRuns_Durations", "ModelBuildDurationTicks >= 0 AND NonAuxiliarySolveDurationTicks >= 0 AND AuxiliarySolveDurationTicks >= 0 AND ResultMappingDurationTicks >= 0 AND TotalDurationTicks >= 0");
                    table.CheckConstraint("CK_AutomaticScheduleRuns_ResultStatus", "ResultStatus >= 0 AND ResultStatus <= 1");
                    table.CheckConstraint("CK_AutomaticScheduleRuns_TimeLimit", "TimeLimitTicks > 0");
                    table.ForeignKey(
                        name: "FK_AutomaticScheduleRuns_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomaticScheduleRuns");
        }
    }
}
