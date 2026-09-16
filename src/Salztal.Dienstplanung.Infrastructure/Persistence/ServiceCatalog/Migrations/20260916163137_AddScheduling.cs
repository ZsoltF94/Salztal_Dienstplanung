using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    StartMonday = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndSunday = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PreparedSnapshotId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDrafts", x => x.Id);
                    table.CheckConstraint("CK_ScheduleDrafts_Period", "julianday(EndSunday) - julianday(StartMonday) = 20");
                    table.CheckConstraint("CK_ScheduleDrafts_Version", "Version > 0");
                });

            migrationBuilder.CreateTable(
                name: "PlanningSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DraftVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodMonday = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodSunday = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    RuleCatalogVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableAuxiliaryReliefShift = table.Column<bool>(type: "INTEGER", nullable: false),
                    HistoryCompleteness = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningSnapshots", x => x.Id);
                    table.CheckConstraint("CK_PlanningSnapshots_DraftVersion", "DraftVersion > 0");
                    table.CheckConstraint("CK_PlanningSnapshots_HistoryCompleteness", "HistoryCompleteness >= 0 AND HistoryCompleteness <= 2");
                    table.CheckConstraint("CK_PlanningSnapshots_Period", "julianday(PeriodSunday) - julianday(PeriodMonday) = 20");
                    table.CheckConstraint("CK_PlanningSnapshots_RuleCatalogVersion", "RuleCatalogVersion > 0");
                    table.ForeignKey(
                        name: "FK_PlanningSnapshots_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Origin = table.Column<int>(type: "INTEGER", nullable: false),
                    PatternId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WorkMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsProtectedFromAutomaticGeneration = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAssignments", x => x.Id);
                    table.CheckConstraint("CK_ScheduleAssignments_Kind", "Kind >= 0 AND Kind <= 4");
                    table.CheckConstraint("CK_ScheduleAssignments_Origin", "Origin >= 0 AND Origin <= 2");
                    table.CheckConstraint("CK_ScheduleAssignments_WorkMinutes", "WorkMinutes > 0");
                    table.ForeignKey(
                        name: "FK_ScheduleAssignments_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAvailabilityEntries",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAvailabilityEntries", x => new { x.DraftId, x.EmployeeId, x.Date });
                    table.CheckConstraint("CK_ScheduleAvailabilityEntries_Kind", "Kind >= 0 AND Kind <= 2");
                    table.ForeignKey(
                        name: "FK_ScheduleAvailabilityEntries_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleDemandSlots",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotKey = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    WorkLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualStart = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    ActualEnd = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDemandSlots", x => new { x.DraftId, x.SlotKey });
                    table.CheckConstraint("CK_ScheduleDemandSlots_Ordinal", "Ordinal > 0");
                    table.CheckConstraint("CK_ScheduleDemandSlots_Time", "ActualEnd > ActualStart");
                    table.ForeignKey(
                        name: "FK_ScheduleDemandSlots_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleGeneratedDaysOff",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleGeneratedDaysOff", x => new { x.DraftId, x.EmployeeId, x.Date });
                    table.ForeignKey(
                        name: "FK_ScheduleGeneratedDaysOff_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePeriodDays",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePeriodDays", x => x.Date);
                    table.ForeignKey(
                        name: "FK_SchedulePeriodDays_ScheduleDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "ScheduleDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningSnapshotComponents",
                columns: table => new
                {
                    SnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningSnapshotComponents", x => new { x.SnapshotId, x.Kind, x.Sequence });
                    table.CheckConstraint("CK_PlanningSnapshotComponents_Kind", "Kind >= 0 AND Kind <= 10");
                    table.CheckConstraint("CK_PlanningSnapshotComponents_Sequence", "Sequence >= 0");
                    table.ForeignKey(
                        name: "FK_PlanningSnapshotComponents_PlanningSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "PlanningSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAssignmentLocks",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAssignmentLocks", x => new { x.DraftId, x.AssignmentId });
                    table.ForeignKey(
                        name: "FK_ScheduleAssignmentLocks_ScheduleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ScheduleAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAssignmentSegments",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AnchorSlotKey = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    WorkLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActualStart = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    ActualEnd = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    WorkMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAssignmentSegments", x => new { x.AssignmentId, x.Sequence });
                    table.CheckConstraint("CK_ScheduleAssignmentSegments_Sequence", "Sequence >= 0");
                    table.CheckConstraint("CK_ScheduleAssignmentSegments_Time", "ActualEnd > ActualStart AND WorkMinutes > 0");
                    table.ForeignKey(
                        name: "FK_ScheduleAssignmentSegments_ScheduleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ScheduleAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleAssignmentSegments_ScheduleDemandSlots_DraftId_AnchorSlotKey",
                        columns: x => new { x.DraftId, x.AnchorSlotKey },
                        principalTable: "ScheduleDemandSlots",
                        principalColumns: new[] { "DraftId", "SlotKey" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleDemandCoverages",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    DraftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotKey = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    CoveredStart = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    CoveredEnd = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    CoveredMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDemandCoverages", x => new { x.AssignmentId, x.Sequence });
                    table.CheckConstraint("CK_ScheduleDemandCoverages_Kind", "Kind >= 0 AND Kind <= 1");
                    table.CheckConstraint("CK_ScheduleDemandCoverages_Sequence", "Sequence >= 0");
                    table.CheckConstraint("CK_ScheduleDemandCoverages_Time", "CoveredEnd > CoveredStart AND CoveredMinutes > 0");
                    table.ForeignKey(
                        name: "FK_ScheduleDemandCoverages_ScheduleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ScheduleAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleDemandCoverages_ScheduleDemandSlots_DraftId_SlotKey",
                        columns: x => new { x.DraftId, x.SlotKey },
                        principalTable: "ScheduleDemandSlots",
                        principalColumns: new[] { "DraftId", "SlotKey" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningSnapshots_DraftId",
                table: "PlanningSnapshots",
                column: "DraftId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignmentLocks_AssignmentId",
                table: "ScheduleAssignmentLocks",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_DraftId_EmployeeId_Date",
                table: "ScheduleAssignments",
                columns: new[] { "DraftId", "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignmentSegments_DraftId_AnchorSlotKey",
                table: "ScheduleAssignmentSegments",
                columns: new[] { "DraftId", "AnchorSlotKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDemandCoverages_DraftId_SlotKey",
                table: "ScheduleDemandCoverages",
                columns: new[] { "DraftId", "SlotKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDrafts_StartMonday",
                table: "ScheduleDrafts",
                column: "StartMonday",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePeriodDays_DraftId",
                table: "SchedulePeriodDays",
                column: "DraftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningSnapshotComponents");

            migrationBuilder.DropTable(
                name: "ScheduleAssignmentLocks");

            migrationBuilder.DropTable(
                name: "ScheduleAssignmentSegments");

            migrationBuilder.DropTable(
                name: "ScheduleAvailabilityEntries");

            migrationBuilder.DropTable(
                name: "ScheduleDemandCoverages");

            migrationBuilder.DropTable(
                name: "ScheduleGeneratedDaysOff");

            migrationBuilder.DropTable(
                name: "SchedulePeriodDays");

            migrationBuilder.DropTable(
                name: "PlanningSnapshots");

            migrationBuilder.DropTable(
                name: "ScheduleAssignments");

            migrationBuilder.DropTable(
                name: "ScheduleDemandSlots");

            migrationBuilder.DropTable(
                name: "ScheduleDrafts");
        }
    }
}
