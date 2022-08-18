using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class TrackJobActivationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobActivations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InactiveUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobActivations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobActivations_JobName",
                table: "JobActivations",
                column: "JobName",
                unique: true,
                filter: "[JobName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobActivations");
        }
    }
}
