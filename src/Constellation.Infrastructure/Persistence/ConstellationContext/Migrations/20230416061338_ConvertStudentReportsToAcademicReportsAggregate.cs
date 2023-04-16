using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStudentReportsToAcademicReportsAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentReports_StudentId",
                table: "StudentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentReports_Students_StudentId",
                table: "StudentReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentReports",
                table: "StudentReports");

            migrationBuilder.RenameTable(
                name: "StudentReports",
                newName: "Reports_AcademicReports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports_AcademicReports",
                table: "Reports_AcademicReports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AcademicReports_StudentId",
                table: "Reports_AcademicReports",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_AcademicReports_StudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports_AcademicReports",
                table: "Reports_AcademicReports");

            migrationBuilder.RenameTable(
                name: "Reports_AcademicReports",
                newName: "StudentReports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentReports",
                table: "StudentReports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentReports_Students_StudentId",
                table: "StudentReports",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_StudentId",
                table: "StudentReports",
                column: "StudentId");
        }
    }
}
