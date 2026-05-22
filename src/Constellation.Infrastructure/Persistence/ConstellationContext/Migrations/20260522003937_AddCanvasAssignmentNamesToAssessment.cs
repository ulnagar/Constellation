using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddCanvasAssignmentNamesToAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CanvasAssignmentName",
                schema: "Assessments",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CanvasCourseName",
                schema: "Assessments",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasAssignmentName",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "CanvasCourseName",
                schema: "Assessments",
                table: "Assessments");
        }
    }
}
