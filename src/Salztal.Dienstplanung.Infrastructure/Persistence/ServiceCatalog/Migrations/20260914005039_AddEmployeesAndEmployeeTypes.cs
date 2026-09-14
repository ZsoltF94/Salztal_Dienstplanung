using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeesAndEmployeeTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    WeeklyWorkTargetMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowsAutomaticAssignment = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresWeeklyManualAssignment = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreservesManualAssignmentsOnGeneration = table.Column<bool>(type: "INTEGER", nullable: false),
                    ManualSuggestionPriority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTypes", x => x.Id);
                    table.CheckConstraint("CK_EmployeeTypes_Code_NotEmpty", "length(trim(Code)) > 0");
                    table.CheckConstraint("CK_EmployeeTypes_ManualSuggestionPriority", "ManualSuggestionPriority IN (0, 1)");
                    table.CheckConstraint("CK_EmployeeTypes_Name_NotEmpty", "length(trim(Name)) > 0");
                    table.CheckConstraint("CK_EmployeeTypes_WeeklyWorkTargetMinutes", "WeeklyWorkTargetMinutes > 0 AND WeeklyWorkTargetMinutes <= 10080");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    EmployeeTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.CheckConstraint("CK_Employees_FirstName_NotEmpty", "length(trim(FirstName)) > 0");
                    table.CheckConstraint("CK_Employees_LastName_NotEmpty", "length(trim(LastName)) > 0");
                    table.ForeignKey(
                        name: "FK_Employees_EmployeeTypes_EmployeeTypeId",
                        column: x => x.EmployeeTypeId,
                        principalTable: "EmployeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTypeShiftEligibilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetKind = table.Column<int>(type: "INTEGER", nullable: false),
                    ShiftTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ShiftPatternId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    Activation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTypeShiftEligibilities", x => x.Id);
                    table.CheckConstraint("CK_EmployeeTypeShiftEligibilities_Activation", "Activation IN (0, 1)");
                    table.CheckConstraint("CK_EmployeeTypeShiftEligibilities_Mode", "Mode IN (0, 1)");
                    table.CheckConstraint("CK_EmployeeTypeShiftEligibilities_ShiftTypeActivation", "TargetKind <> 0 OR Activation = 0");
                    table.CheckConstraint("CK_EmployeeTypeShiftEligibilities_Target", "(TargetKind = 0 AND ShiftTypeId IS NOT NULL AND ShiftPatternId IS NULL) OR (TargetKind = 1 AND ShiftTypeId IS NULL AND ShiftPatternId IS NOT NULL)");
                    table.CheckConstraint("CK_EmployeeTypeShiftEligibilities_TargetKind", "TargetKind IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_EmployeeTypeShiftEligibilities_EmployeeTypes_EmployeeTypeId",
                        column: x => x.EmployeeTypeId,
                        principalTable: "EmployeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTypeShiftEligibilities_ShiftPatterns_ShiftPatternId",
                        column: x => x.ShiftPatternId,
                        principalTable: "ShiftPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTypeShiftEligibilities_ShiftTypes_ShiftTypeId",
                        column: x => x.ShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EmployeeTypes",
                columns: new[] { "Id", "AllowsAutomaticAssignment", "Code", "ManualSuggestionPriority", "Name", "PreservesManualAssignmentsOnGeneration", "RequiresWeeklyManualAssignment", "WeeklyWorkTargetMinutes" },
                values: new object[,]
                {
                    { new Guid("554d2c92-d67e-439c-9bc4-7bbe564198b5"), true, "Typ35", 0, "Restaurant - 35 Stunden", false, false, 2100 },
                    { new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), false, "Typ1", 1, "Serviceleitung", true, true, 2400 },
                    { new Guid("6f0feaed-65eb-4568-9c1f-a130cca65e44"), true, "Typ30", 0, "Restaurant - 30 Stunden", false, false, 1800 },
                    { new Guid("86a88720-bcdb-4f56-9dc8-0acf702f00f7"), true, "TypAH1", 0, "Restaurant-Spätdienst - 10 Stunden", false, false, 600 },
                    { new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"), true, "TypAH2", 0, "Restaurant, Cafeteria B und Doppeldienst - 10 Stunden", false, false, 600 },
                    { new Guid("b75fed95-1c2f-439f-9696-217bed8c4d8f"), true, "Typ25", 0, "Restaurant - 25 Stunden", false, false, 1500 },
                    { new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), true, "Typ30a", 0, "Alle Dienste - 30 Stunden", false, false, 1800 },
                    { new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), true, "Typ35a", 0, "Alle Dienste - 35 Stunden", false, false, 2100 }
                });

            migrationBuilder.InsertData(
                table: "EmployeeTypeShiftEligibilities",
                columns: new[] { "Id", "Activation", "EmployeeTypeId", "Mode", "ShiftPatternId", "ShiftTypeId", "TargetKind" },
                values: new object[,]
                {
                    { 1, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 2, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 3, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 0, null, new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), 0 },
                    { 4, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 0, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 5, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 6, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 0, new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), null, 1 },
                    { 7, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 1, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 8, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 1, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 9, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 1, null, new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), 0 },
                    { 10, 0, new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"), 1, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 11, 0, new Guid("b75fed95-1c2f-439f-9696-217bed8c4d8f"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 12, 0, new Guid("b75fed95-1c2f-439f-9696-217bed8c4d8f"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 13, 0, new Guid("b75fed95-1c2f-439f-9696-217bed8c4d8f"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 14, 0, new Guid("6f0feaed-65eb-4568-9c1f-a130cca65e44"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 15, 0, new Guid("6f0feaed-65eb-4568-9c1f-a130cca65e44"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 16, 0, new Guid("6f0feaed-65eb-4568-9c1f-a130cca65e44"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 17, 0, new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 18, 0, new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 19, 0, new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), 0, null, new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), 0 },
                    { 20, 0, new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), 0, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 21, 0, new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 22, 0, new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"), 0, new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), null, 1 },
                    { 23, 0, new Guid("554d2c92-d67e-439c-9bc4-7bbe564198b5"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 24, 0, new Guid("554d2c92-d67e-439c-9bc4-7bbe564198b5"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 25, 0, new Guid("554d2c92-d67e-439c-9bc4-7bbe564198b5"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 26, 0, new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 27, 0, new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 28, 0, new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), 0, null, new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), 0 },
                    { 29, 0, new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), 0, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 30, 0, new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 31, 0, new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"), 0, new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), null, 1 },
                    { 32, 0, new Guid("86a88720-bcdb-4f56-9dc8-0acf702f00f7"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 33, 0, new Guid("86a88720-bcdb-4f56-9dc8-0acf702f00f7"), 1, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 34, 0, new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 35, 0, new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"), 0, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 36, 0, new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 37, 0, new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"), 1, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 38, 1, new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"), 0, new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeTypeId",
                table: "Employees",
                column: "EmployeeTypeId",
                unique: true,
                filter: "IsActive = 1 AND lower(EmployeeTypeId) = '6197e678-38c8-465c-8d09-fed6812a6f2b'");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_Code",
                table: "EmployeeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypeShiftEligibilities_EmployeeTypeId_ShiftPatternId_Mode_Activation",
                table: "EmployeeTypeShiftEligibilities",
                columns: new[] { "EmployeeTypeId", "ShiftPatternId", "Mode", "Activation" },
                unique: true,
                filter: "TargetKind = 1");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypeShiftEligibilities_EmployeeTypeId_ShiftTypeId_Mode",
                table: "EmployeeTypeShiftEligibilities",
                columns: new[] { "EmployeeTypeId", "ShiftTypeId", "Mode" },
                unique: true,
                filter: "TargetKind = 0");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypeShiftEligibilities_ShiftPatternId",
                table: "EmployeeTypeShiftEligibilities",
                column: "ShiftPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypeShiftEligibilities_ShiftTypeId",
                table: "EmployeeTypeShiftEligibilities",
                column: "ShiftTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeTypeShiftEligibilities");

            migrationBuilder.DropTable(
                name: "EmployeeTypes");
        }
    }
}
