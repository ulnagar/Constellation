using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDeleteOfFamilyParents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_Parents_Families_Family_FamilyId",
                table: "Families_Parents");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Parents_Families_Family_FamilyId",
                table: "Families_Parents",
                column: "FamilyId",
                principalTable: "Families_Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_Parents_Families_Family_FamilyId",
                table: "Families_Parents");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Parents_Families_Family_FamilyId",
                table: "Families_Parents",
                column: "FamilyId",
                principalTable: "Families_Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
