using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffingDemands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffingDemandDateExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    WorkLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualStartMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualEndMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    RequiredEmployeeCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffingDemandDateExceptions", x => x.Id);
                    table.CheckConstraint("CK_StaffingDemandDateExceptions_Kind", "Kind IN (0, 1, 2)");
                    table.CheckConstraint("CK_StaffingDemandDateExceptions_ValueShape", "(Kind = 2 AND ActualStartMinutes IS NULL AND ActualEndMinutes IS NULL AND RequiredEmployeeCount IS NULL) OR (Kind IN (0, 1) AND ActualStartMinutes IS NOT NULL AND ActualEndMinutes IS NOT NULL AND RequiredEmployeeCount IS NOT NULL AND ActualStartMinutes >= 0 AND ActualStartMinutes < 1440 AND ActualEndMinutes > ActualStartMinutes AND ActualEndMinutes < 1440 AND ActualStartMinutes % 30 = 0 AND ActualEndMinutes % 30 = 0 AND RequiredEmployeeCount > 0)");
                    table.ForeignKey(
                        name: "FK_StaffingDemandDateExceptions_ShiftTypes_ShiftTypeId",
                        column: x => x.ShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffingDemandDateExceptions_WorkLocations_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalTable: "WorkLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StandardStaffingDemandRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EffectiveFromMonday = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualStartMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualEndMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    RequiredEmployeeCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardStaffingDemandRevisions", x => x.Id);
                    table.CheckConstraint("CK_StandardStaffingDemandRevisions_DayOfWeek", "DayOfWeek >= 0 AND DayOfWeek <= 6");
                    table.CheckConstraint("CK_StandardStaffingDemandRevisions_EffectiveMonday", "strftime('%w', EffectiveFromMonday) = '1'");
                    table.CheckConstraint("CK_StandardStaffingDemandRevisions_Kind", "Kind IN (0, 1, 2)");
                    table.CheckConstraint("CK_StandardStaffingDemandRevisions_ValueShape", "(Kind = 2 AND ActualStartMinutes IS NULL AND ActualEndMinutes IS NULL AND RequiredEmployeeCount IS NULL) OR (Kind IN (0, 1) AND ActualStartMinutes IS NOT NULL AND ActualEndMinutes IS NOT NULL AND RequiredEmployeeCount IS NOT NULL AND ActualStartMinutes >= 0 AND ActualStartMinutes < 1440 AND ActualEndMinutes > ActualStartMinutes AND ActualEndMinutes < 1440 AND ActualStartMinutes % 30 = 0 AND ActualEndMinutes % 30 = 0 AND RequiredEmployeeCount > 0)");
                    table.ForeignKey(
                        name: "FK_StandardStaffingDemandRevisions_ShiftTypes_ShiftTypeId",
                        column: x => x.ShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StandardStaffingDemandRevisions_WorkLocations_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalTable: "WorkLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffingDemandDateExceptions_Date_WorkLocationId_ShiftTypeId",
                table: "StaffingDemandDateExceptions",
                columns: new[] { "Date", "WorkLocationId", "ShiftTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffingDemandDateExceptions_ShiftTypeId",
                table: "StaffingDemandDateExceptions",
                column: "ShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingDemandDateExceptions_WorkLocationId",
                table: "StaffingDemandDateExceptions",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardStaffingDemandRevisions_DayOfWeek_WorkLocationId_ShiftTypeId_EffectiveFromMonday",
                table: "StandardStaffingDemandRevisions",
                columns: new[] { "DayOfWeek", "WorkLocationId", "ShiftTypeId", "EffectiveFromMonday" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardStaffingDemandRevisions_ShiftTypeId",
                table: "StandardStaffingDemandRevisions",
                column: "ShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardStaffingDemandRevisions_WorkLocationId",
                table: "StandardStaffingDemandRevisions",
                column: "WorkLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffingDemandDateExceptions");

            migrationBuilder.DropTable(
                name: "StandardStaffingDemandRevisions");
        }
    }
}
