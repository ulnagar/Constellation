using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AllowStocktakeSightingsToBeCancelled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationComment",
                table: "StocktakeSightings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "StocktakeSightings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "StocktakeSightings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "StocktakeSightings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationComment",
                table: "StocktakeSightings");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "StocktakeSightings");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "StocktakeSightings");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "StocktakeSightings");
        }
    }
}
