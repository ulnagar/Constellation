using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class CorrectForeignKeyForTutorialRollStudentObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_RollStudent_GroupTutorials_Roll_TutorialRollId1",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropIndex(
                name: "IX_GroupTutorials_RollStudent_TutorialRollId1",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropColumn(
                name: "TutorialRollId1",
                table: "GroupTutorials_RollStudent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TutorialRollId1",
                table: "GroupTutorials_RollStudent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_RollStudent_TutorialRollId1",
                table: "GroupTutorials_RollStudent",
                column: "TutorialRollId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_RollStudent_GroupTutorials_Roll_TutorialRollId1",
                table: "GroupTutorials_RollStudent",
                column: "TutorialRollId1",
                principalTable: "GroupTutorials_Roll",
                principalColumn: "Id");
        }
    }
}
