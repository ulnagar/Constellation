using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.Migrations
{
    public partial class IncludeCommentsInStocktakeSightings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "StocktakeSightings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "StocktakeSightings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCode",
                table: "StocktakeSightings",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "StocktakeSightings");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "StocktakeSightings");

            migrationBuilder.DropColumn(
                name: "UserCode",
                table: "StocktakeSightings");
        }
    }
}
