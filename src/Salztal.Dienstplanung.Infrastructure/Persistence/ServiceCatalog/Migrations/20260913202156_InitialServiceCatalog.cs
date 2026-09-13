using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class InitialServiceCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ColorCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLocations", x => x.Id);
                    table.CheckConstraint("CK_WorkLocations_ColorCode_NotEmpty", "length(trim(ColorCode)) > 0");
                    table.CheckConstraint("CK_WorkLocations_Name_NotEmpty", "length(trim(Name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "ShiftTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    WorkLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: true),
                    StandardStartMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    StandardEndMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTypes", x => x.Id);
                    table.CheckConstraint("CK_ShiftTypes_Name_NotEmpty", "length(trim(Name)) > 0");
                    table.CheckConstraint("CK_ShiftTypes_StandardTime", "StandardStartMinutes >= 0 AND StandardStartMinutes < 1440 AND StandardEndMinutes > StandardStartMinutes AND StandardEndMinutes < 1440 AND StandardStartMinutes % 30 = 0 AND StandardEndMinutes % 30 = 0");
                    table.ForeignKey(
                        name: "FK_ShiftTypes_WorkLocations_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalTable: "WorkLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftPatterns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayCode = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayColorCode = table.Column<string>(type: "TEXT", nullable: true),
                    AllowedDay = table.Column<int>(type: "INTEGER", nullable: true),
                    SwitchRule = table.Column<int>(type: "INTEGER", nullable: true),
                    HasInterruption = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SecondShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftPatterns", x => x.Id);
                    table.CheckConstraint("CK_ShiftPatterns_DifferentSegments", "FirstShiftTypeId <> SecondShiftTypeId");
                    table.ForeignKey(
                        name: "FK_ShiftPatterns_ShiftTypes_FirstShiftTypeId",
                        column: x => x.FirstShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftPatterns_ShiftTypes_SecondShiftTypeId",
                        column: x => x.SecondShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "WorkLocations",
                columns: new[] { "Id", "ColorCode", "Name" },
                values: new object[,]
                {
                    { new Guid("2d2bf3b2-4744-4b56-8515-f33adf9f2161"), "red", "Restaurant" },
                    { new Guid("6d38f48b-f938-4aaa-82bd-c59372f599f4"), "yellow", "Cafeteria" }
                });

            migrationBuilder.InsertData(
                table: "ShiftTypes",
                columns: new[] { "Id", "Abbreviation", "DisplayKind", "Name", "StandardEndMinutes", "StandardStartMinutes", "WorkLocationId" },
                values: new object[,]
                {
                    { new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), null, 1, "Cafeteria-Dienst A", 1230, 810, new Guid("6d38f48b-f938-4aaa-82bd-c59372f599f4") },
                    { new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), null, 1, "Cafeteria-Dienst B", 1050, 810, new Guid("6d38f48b-f938-4aaa-82bd-c59372f599f4") },
                    { new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), "S", 0, "Spätdienst", 1170, 990, new Guid("2d2bf3b2-4744-4b56-8515-f33adf9f2161") },
                    { new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), "F", 0, "Frühdienst", 810, 390, new Guid("2d2bf3b2-4744-4b56-8515-f33adf9f2161") }
                });

            migrationBuilder.InsertData(
                table: "ShiftPatterns",
                columns: new[] { "Id", "AllowedDay", "DisplayCode", "DisplayColorCode", "FirstShiftTypeId", "HasInterruption", "Kind", "SecondShiftTypeId", "SwitchRule" },
                values: new object[,]
                {
                    { new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, "D", null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), true, 0, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), null },
                    { new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), 6, "Spr", "blue", new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), false, 1, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftPatterns_DisplayCode",
                table: "ShiftPatterns",
                column: "DisplayCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftPatterns_FirstShiftTypeId",
                table: "ShiftPatterns",
                column: "FirstShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftPatterns_Kind",
                table: "ShiftPatterns",
                column: "Kind",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftPatterns_SecondShiftTypeId",
                table: "ShiftPatterns",
                column: "SecondShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTypes_WorkLocationId",
                table: "ShiftTypes",
                column: "WorkLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftPatterns");

            migrationBuilder.DropTable(
                name: "ShiftTypes");

            migrationBuilder.DropTable(
                name: "WorkLocations");
        }
    }
}
