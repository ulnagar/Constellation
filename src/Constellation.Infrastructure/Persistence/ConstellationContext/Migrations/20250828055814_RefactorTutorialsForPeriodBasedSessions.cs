using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTutorialsForPeriodBasedSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "EndTime",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Week",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.AddColumn<Guid>(
                name: "PeriodId",
                schema: "Tutorials",
                table: "Sessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PeriodId",
                schema: "Tutorials",
                table: "Sessions",
                column: "PeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Periods_PeriodId",
                schema: "Tutorials",
                table: "Sessions",
                column: "PeriodId",
                principalSchema: "Timetables",
                principalTable: "Periods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Periods_PeriodId",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_PeriodId",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "PeriodId",
                schema: "Tutorials",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "Day",
                schema: "Tutorials",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                schema: "Tutorials",
                table: "Sessions",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                schema: "Tutorials",
                table: "Sessions",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Week",
                schema: "Tutorials",
                table: "Sessions",
                type: "int",
                nullable: true);
        }
    }
}
