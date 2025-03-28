using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RenameEnrolmentsToOfferingEnrolments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Students_StudentId",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_StudentId",
                table: "Enrolments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrolments",
                table: "Enrolments");

            migrationBuilder.RenameTable(
                name: "Enrolments",
                newName: "OfferingEnrolments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfferingEnrolments",
                table: "OfferingEnrolments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferingEnrolments_Offerings_Offerings_OfferingId",
                table: "OfferingEnrolments",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferingEnrolments_Students_StudentId",
                table: "OfferingEnrolments",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_OfferingEnrolments_OfferingId",
                table: "OfferingEnrolments",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferingEnrolments_StudentId",
                table: "OfferingEnrolments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferingEnrolments_Offerings_Offerings_OfferingId",
                table: "OfferingEnrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferingEnrolments_Students_StudentId",
                table: "OfferingEnrolments");

            migrationBuilder.DropIndex(
                name: "IX_OfferingEnrolments_OfferingId",
                table: "OfferingEnrolments");

            migrationBuilder.DropIndex(
                name: "IX_OfferingEnrolments_StudentId",
                table: "OfferingEnrolments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfferingEnrolments",
                table: "OfferingEnrolments");

            migrationBuilder.RenameTable(
                name: "OfferingEnrolments",
                newName: "Enrolments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrolments",
                table: "Enrolments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Students_StudentId",
                table: "Enrolments",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_OfferingId",
                table: "Enrolments",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_StudentId",
                table: "Enrolments",
                column: "StudentId");
        }
    }
}
