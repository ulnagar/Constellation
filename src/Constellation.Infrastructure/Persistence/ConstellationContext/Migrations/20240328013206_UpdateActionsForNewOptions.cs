using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActionsForNewOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOccurred",
                table: "WorkFlows_Actions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "WorkFlows_Actions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "WorkFlows_Actions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "WorkFlows_Actions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOccurred",
                table: "WorkFlows_Actions");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "WorkFlows_Actions");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "WorkFlows_Actions");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "WorkFlows_Actions");
        }
    }
}
