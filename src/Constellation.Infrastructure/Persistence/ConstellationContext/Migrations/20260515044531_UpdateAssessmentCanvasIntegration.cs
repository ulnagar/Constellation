using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssessmentCanvasIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasId",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.AlterColumn<int>(
                name: "AllowedAttempts",
                schema: "Assessments",
                table: "Assessments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CanvasAssignmentId",
                schema: "Assessments",
                table: "Assessments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanvasCourse",
                schema: "Assessments",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ForwardDate",
                schema: "Assessments",
                table: "Assessments",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasAssignmentId",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "CanvasCourse",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "ForwardDate",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.AlterColumn<int>(
                name: "AllowedAttempts",
                schema: "Assessments",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CanvasId",
                schema: "Assessments",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
