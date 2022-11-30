using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class RemoveStaffIdColumnFromCoursesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Staff_StaffId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_StaffId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Courses");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffId",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_StaffId",
                table: "Courses",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Staff_StaffId",
                table: "Courses",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");
        }
    }
}
