using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddStocktakeSightingDifferenceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Differences",
                schema: "Stocktake",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SightingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resolved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Differences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Differences_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "Stocktake",
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Differences_Sightings_SightingId",
                        column: x => x.SightingId,
                        principalSchema: "Stocktake",
                        principalTable: "Sightings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Differences_EventId",
                schema: "Stocktake",
                table: "Differences",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Differences_SightingId",
                schema: "Stocktake",
                table: "Differences",
                column: "SightingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Differences",
                schema: "Stocktake");
        }
    }
}
