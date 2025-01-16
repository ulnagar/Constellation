using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddMissedAndFreePeriodsToAttendancePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FreePeriods",
                schema: "Attendance",
                table: "Plans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissedLessons",
                schema: "Attendance",
                table: "Plans",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreePeriods",
                schema: "Attendance",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "MissedLessons",
                schema: "Attendance",
                table: "Plans");
        }
    }
}
