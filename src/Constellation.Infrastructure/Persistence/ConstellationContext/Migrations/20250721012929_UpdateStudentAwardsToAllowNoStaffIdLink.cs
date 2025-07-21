using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentAwardsToAllowNoStaffIdLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Members_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[Awards_StudentAwards]
                SET TeacherId = '00000000-0000-0000-0000-000000000000'
                WHERE TeacherId is null;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [dbo].[Awards_StudentAwards]
                SET TeacherId = null
                WHERE TeacherId = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Members_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
