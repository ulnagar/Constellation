using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ModifyAssessmentDueDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasDueDate",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "CanvasLockDate",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "CanvasUnlockDate",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AvailableFrom",
                schema: "Assessments",
                table: "Assessments",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AvailableTo",
                schema: "Assessments",
                table: "Assessments",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueDate",
                schema: "Assessments",
                table: "Assessments",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "AvailableTo",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "Assessments",
                table: "Assessments");

            migrationBuilder.AddColumn<DateTime>(
                name: "CanvasDueDate",
                schema: "Assessments",
                table: "Assessments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CanvasLockDate",
                schema: "Assessments",
                table: "Assessments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CanvasUnlockDate",
                schema: "Assessments",
                table: "Assessments",
                type: "datetime2",
                nullable: true);
        }
    }
}
