using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorialRequestPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plan_Name",
                schema: "Tutorials",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plan_Periods",
                schema: "Tutorials",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Plan_StartDate",
                schema: "Tutorials",
                table: "Requests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Plan_TutorialId",
                schema: "Tutorials",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan_Name",
                schema: "Tutorials",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Plan_Periods",
                schema: "Tutorials",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Plan_StartDate",
                schema: "Tutorials",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Plan_TutorialId",
                schema: "Tutorials",
                table: "Requests");
        }
    }
}
