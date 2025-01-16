using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class TrackSubmissionDetailsInAttendancePlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                schema: "Attendance",
                table: "Plans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                schema: "Attendance",
                table: "Plans",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "Attendance",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                schema: "Attendance",
                table: "Plans");
        }
    }
}
