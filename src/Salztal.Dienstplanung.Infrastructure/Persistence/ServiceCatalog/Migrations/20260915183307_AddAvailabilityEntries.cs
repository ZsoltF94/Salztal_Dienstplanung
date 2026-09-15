using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailabilityEntries",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityEntries", x => new { x.EmployeeId, x.Date });
                    table.CheckConstraint("CK_AvailabilityEntries_ChangeVersion", "ChangeVersion > 0");
                    table.CheckConstraint("CK_AvailabilityEntries_Kind", "Kind IN (0, 1, 2)");
                    table.ForeignKey(
                        name: "FK_AvailabilityEntries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityEntries_Date",
                table: "AvailabilityEntries",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityEntries");
        }
    }
}
