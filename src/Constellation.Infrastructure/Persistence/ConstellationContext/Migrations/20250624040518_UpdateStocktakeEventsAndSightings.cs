using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStocktakeEventsAndSightings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StocktakeSightings_StocktakeEvents_StocktakeEventId",
                table: "StocktakeSightings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StocktakeSightings",
                table: "StocktakeSightings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StocktakeEvents",
                table: "StocktakeEvents");

            migrationBuilder.EnsureSchema(
                name: "Stocktake");

            migrationBuilder.RenameTable(
                name: "StocktakeSightings",
                newName: "Sightings",
                newSchema: "Stocktake");

            migrationBuilder.RenameTable(
                name: "StocktakeEvents",
                newName: "Events",
                newSchema: "Stocktake");

            migrationBuilder.RenameIndex(
                name: "IX_StocktakeSightings_StocktakeEventId",
                schema: "Stocktake",
                table: "Sightings",
                newName: "IX_Sightings_StocktakeEventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sightings",
                schema: "Stocktake",
                table: "Sightings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                schema: "Stocktake",
                table: "Events",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sightings_Events_StocktakeEventId",
                schema: "Stocktake",
                table: "Sightings",
                column: "StocktakeEventId",
                principalSchema: "Stocktake",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sightings_Events_StocktakeEventId",
                schema: "Stocktake",
                table: "Sightings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sightings",
                schema: "Stocktake",
                table: "Sightings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                schema: "Stocktake",
                table: "Events");

            migrationBuilder.RenameTable(
                name: "Sightings",
                schema: "Stocktake",
                newName: "StocktakeSightings");

            migrationBuilder.RenameTable(
                name: "Events",
                schema: "Stocktake",
                newName: "StocktakeEvents");

            migrationBuilder.RenameIndex(
                name: "IX_Sightings_StocktakeEventId",
                table: "StocktakeSightings",
                newName: "IX_StocktakeSightings_StocktakeEventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StocktakeSightings",
                table: "StocktakeSightings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StocktakeEvents",
                table: "StocktakeEvents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StocktakeSightings_StocktakeEvents_StocktakeEventId",
                table: "StocktakeSightings",
                column: "StocktakeEventId",
                principalTable: "StocktakeEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
