using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddSciencePracLessonRequirementFlagToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresSciencePracLesson",
                table: "Subjects_Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSciencePracLesson",
                schema: "Attendance",
                table: "PlanPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresSciencePracLesson",
                table: "Subjects_Courses");

            migrationBuilder.DropColumn(
                name: "RequiresSciencePracLesson",
                schema: "Attendance",
                table: "PlanPeriods");
        }
    }
}
