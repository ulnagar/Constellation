using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAbsencesToIncludeTutorialAbsences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "Absences_Absences",
                newName: "SourceId");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Absences_Absences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE [dbo].[Absences_Absences]
                SET Source = 'Offering'
                WHERE Source is null;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Absences_Absences");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "Absences_Absences",
                newName: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
