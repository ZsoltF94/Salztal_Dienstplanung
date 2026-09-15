using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTypeManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "EmployeeTypeShiftEligibilities",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EmployeeTypes",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "AbsenceDayValueMinutes",
                table: "EmployeeTypes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsVacationAndSickness",
                table: "EmployeeTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PlanningRole",
                table: "EmployeeTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("554d2c92-d67e-439c-9bc4-7bbe564198b5"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { 420, true, 0 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("6197e678-38c8-465c-8d09-fed6812a6f2b"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { 480, true, 1 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("6f0feaed-65eb-4568-9c1f-a130cca65e44"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { 360, true, 0 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("86a88720-bcdb-4f56-9dc8-0acf702f00f7"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { null, false, 2 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("b4dd7b2a-2b7b-4a8d-b46b-ba1601a5a5a6"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { null, false, 2 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("b75fed95-1c2f-439f-9696-217bed8c4d8f"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { 300, true, 0 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("cc8921a3-83f0-419f-a39b-f8bb37c3d6ba"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { 360, true, 0 });

            migrationBuilder.UpdateData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("e12d71ce-5dca-45b3-bae5-eadebaad93dd"),
                columns: new[] { "AbsenceDayValueMinutes", "AllowsVacationAndSickness", "PlanningRole" },
                values: new object[] { 420, true, 0 });

            migrationBuilder.InsertData(
                table: "EmployeeTypes",
                columns: new[] { "Id", "AbsenceDayValueMinutes", "AllowsAutomaticAssignment", "AllowsVacationAndSickness", "Code", "ManualSuggestionPriority", "Name", "PlanningRole", "PreservesManualAssignmentsOnGeneration", "RequiresWeeklyManualAssignment", "WeeklyWorkTargetMinutes" },
                values: new object[,]
                {
                    { new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 300, true, true, "Typ25a", 0, "Alle Dienste - 25 Stunden", 0, false, false, 1500 },
                    { new Guid("71cc48ce-172a-4580-a6b2-e1acc77b94f6"), 240, true, true, "Typ20", 0, "Restaurant - 20 Stunden", 0, false, false, 1200 },
                    { new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 240, true, true, "Typ20a", 0, "Alle Dienste - 20 Stunden", 0, false, false, 1200 }
                });

            migrationBuilder.InsertData(
                table: "EmployeeTypeShiftEligibilities",
                columns: new[] { "Id", "Activation", "EmployeeTypeId", "Mode", "ShiftPatternId", "ShiftTypeId", "TargetKind" },
                values: new object[,]
                {
                    { 39, 0, new Guid("71cc48ce-172a-4580-a6b2-e1acc77b94f6"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 40, 0, new Guid("71cc48ce-172a-4580-a6b2-e1acc77b94f6"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 41, 0, new Guid("71cc48ce-172a-4580-a6b2-e1acc77b94f6"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 42, 0, new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 43, 0, new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 44, 0, new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 0, null, new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), 0 },
                    { 45, 0, new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 0, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 46, 0, new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 47, 0, new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"), 0, new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), null, 1 },
                    { 48, 0, new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 0, null, new Guid("914e5c77-18d6-4813-8212-8765cc0cd647"), 0 },
                    { 49, 0, new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 0, null, new Guid("573e723a-73c9-4b0f-9e91-ecb2462a8df4"), 0 },
                    { 50, 0, new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 0, null, new Guid("44fc4ea8-a839-4704-99ef-c8bf2b37f3e0"), 0 },
                    { 51, 0, new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 0, null, new Guid("4d6e10ea-a4b9-455c-a868-3a1ab992e856"), 0 },
                    { 52, 0, new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 0, new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"), null, 1 },
                    { 53, 0, new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"), 0, new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"), null, 1 }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeTypes_AbsencePolicy",
                table: "EmployeeTypes",
                sql: "(AllowsVacationAndSickness = 1 AND AbsenceDayValueMinutes IS NOT NULL AND AbsenceDayValueMinutes > 0 AND AbsenceDayValueMinutes <= 1440) OR (AllowsVacationAndSickness = 0 AND AbsenceDayValueMinutes IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeTypes_PlanningPolicy",
                table: "EmployeeTypes",
                sql: "(PlanningRole IN (0, 2) AND AllowsAutomaticAssignment = 1 AND RequiresWeeklyManualAssignment = 0 AND PreservesManualAssignmentsOnGeneration = 0 AND ManualSuggestionPriority = 0) OR (PlanningRole = 1 AND AllowsAutomaticAssignment = 0 AND RequiresWeeklyManualAssignment = 1 AND PreservesManualAssignmentsOnGeneration = 1 AND ManualSuggestionPriority = 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeTypes_PlanningRole",
                table: "EmployeeTypes",
                sql: "PlanningRole IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeTypes_AbsencePolicy",
                table: "EmployeeTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeTypes_PlanningPolicy",
                table: "EmployeeTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeTypes_PlanningRole",
                table: "EmployeeTypes");

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "EmployeeTypeShiftEligibilities",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("3c553205-5413-4f0c-ad1c-b4037066470c"));

            migrationBuilder.DeleteData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("71cc48ce-172a-4580-a6b2-e1acc77b94f6"));

            migrationBuilder.DeleteData(
                table: "EmployeeTypes",
                keyColumn: "Id",
                keyValue: new Guid("a06b4fcc-dbf1-4b3a-b975-07dc2d488157"));

            migrationBuilder.DropColumn(
                name: "AbsenceDayValueMinutes",
                table: "EmployeeTypes");

            migrationBuilder.DropColumn(
                name: "AllowsVacationAndSickness",
                table: "EmployeeTypes");

            migrationBuilder.DropColumn(
                name: "PlanningRole",
                table: "EmployeeTypes");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "EmployeeTypeShiftEligibilities",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "EmployeeTypes",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");
        }
    }
}
