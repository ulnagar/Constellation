using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AddGroupTutorialMSTeamOperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TutorialId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TutorialId",
                table: "MSTeamOperations",
                column: "TutorialId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_GroupTutorials_Tutorial_TutorialId",
                table: "MSTeamOperations",
                column: "TutorialId",
                principalTable: "GroupTutorials_Tutorial",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_GroupTutorials_Tutorial_TutorialId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TutorialId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "TutorialId",
                table: "MSTeamOperations");
        }
    }
}
