using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports_AcademicReports",
                table: "Reports_AcademicReports");

            migrationBuilder.EnsureSchema(
                name: "Reports");

            migrationBuilder.RenameTable(
                name: "Reports_AcademicReports",
                newName: "AcademicReports",
                newSchema: "Reports");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_AcademicReports_StudentId",
                schema: "Reports",
                table: "AcademicReports",
                newName: "IX_AcademicReports_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicReports",
                schema: "Reports",
                table: "AcademicReports",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ExternalReports",
                schema: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalReports_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReports_StudentId",
                schema: "Reports",
                table: "ExternalReports",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicReports_Students_StudentId",
                schema: "Reports",
                table: "AcademicReports",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicReports_Students_StudentId",
                schema: "Reports",
                table: "AcademicReports");

            migrationBuilder.DropTable(
                name: "ExternalReports",
                schema: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicReports",
                schema: "Reports",
                table: "AcademicReports");

            migrationBuilder.RenameTable(
                name: "AcademicReports",
                schema: "Reports",
                newName: "Reports_AcademicReports");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicReports_StudentId",
                table: "Reports_AcademicReports",
                newName: "IX_Reports_AcademicReports_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports_AcademicReports",
                table: "Reports_AcademicReports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
