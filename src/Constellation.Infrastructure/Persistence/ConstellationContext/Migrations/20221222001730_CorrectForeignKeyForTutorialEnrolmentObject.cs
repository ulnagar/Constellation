using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class CorrectForeignKeyForTutorialEnrolmentObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Enrolment_GroupTutorials_Tutorial_GroupTutorialId",
                table: "GroupTutorials_Enrolment");

            migrationBuilder.DropIndex(
                name: "IX_GroupTutorials_Enrolment_GroupTutorialId",
                table: "GroupTutorials_Enrolment");

            migrationBuilder.DropColumn(
                name: "GroupTutorialId",
                table: "GroupTutorials_Enrolment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupTutorialId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Enrolment_GroupTutorialId",
                table: "GroupTutorials_Enrolment",
                column: "GroupTutorialId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Enrolment_GroupTutorials_Tutorial_GroupTutorialId",
                table: "GroupTutorials_Enrolment",
                column: "GroupTutorialId",
                principalTable: "GroupTutorials_Tutorial",
                principalColumn: "Id");
        }
    }
}
