using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentAwards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAward_StudentId",
                table: "StudentAward");
            
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAward_Students_StudentId",
                table: "StudentAward");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentAward",
                table: "StudentAward");

            migrationBuilder.RenameTable(
                name: "StudentAward",
                newName: "Awards_StudentAwards");

            migrationBuilder.AddColumn<string>(
                name: "TeacherId",
                table: "Awards_StudentAwards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentId",
                table: "Awards_StudentAwards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Awards_StudentAwards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Awards_StudentAwards",
                table: "Awards_StudentAwards",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Staff_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Students_StudentId",
                table: "Awards_StudentAwards",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_IncidentId",
                table: "Awards_StudentAwards",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_StudentId",
                table: "Awards_StudentAwards",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropIndex(
                name: "IX_Awards_StudentAwards_StudentId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropIndex(
                name: "IX_Awards_StudentAwards_IncidentId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Students_StudentId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Staff_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Awards_StudentAwards",
                table: "Awards_StudentAwards");

            migrationBuilder.RenameTable(
                name: "Awards_StudentAwards",
                newName: "StudentAward");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "StudentAward");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "StudentAward");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "StudentAward");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentAward",
                table: "StudentAward",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAward_Students_StudentId",
                table: "StudentAward",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAward_StudentId",
                table: "StudentAward",
                column: "StudentId");
        }
    }
}
